using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Phonova.Models;

namespace Phonova.Services
{
    public sealed class AppleDeviceEventArgs : EventArgs
    {
        public UsbDevice Device { get; }
        public bool IsConnected { get; }

        public AppleDeviceEventArgs(UsbDevice device, bool isConnected)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            IsConnected = isConnected;
        }
    }

    public sealed class AppleDeviceDetector : IDisposable
    {
        private readonly ConcurrentDictionary<string, UsbDevice> _activeDevices;
        private volatile bool _isRunning;
        private volatile bool _disposed;
        private CancellationTokenSource _cts;

        private readonly string _toolboxPath;
        private readonly string IDEVICE_ID = "idevice_id.exe";

        public event EventHandler<AppleDeviceEventArgs>? DeviceConnected;
        public event EventHandler<AppleDeviceEventArgs>? DeviceDisconnected;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsRunning => _isRunning;
        public int ActiveDeviceCount => _activeDevices.Count;

        public AppleDeviceDetector()
        {
            _activeDevices = new ConcurrentDictionary<string, UsbDevice>(StringComparer.OrdinalIgnoreCase);
            _toolboxPath = Path.Combine(Environment.CurrentDirectory, "src_set");
        }

        public Task<bool> StartAsync(IntPtr mainWindowHandle)
        {
            ThrowIfDisposed();

            if (_isRunning) return Task.FromResult(true);

            _isRunning = true;
            _cts = new CancellationTokenSource();

            // TRUE DRFONES REPLICA: Infinite polling loop in background thread
            Task.Run(() => ProcessAppleDeviceLoop(_cts.Token));

            return Task.FromResult(true);
        }

        public Task StopAsync()
        {
            if (!_isRunning) return Task.CompletedTask;

            _isRunning = false;
            _cts?.Cancel();
            _activeDevices.Clear();

            return Task.CompletedTask;
        }

        private void ProcessAppleDeviceLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 1. Get Connected Devices via idevice_id -l exactly like DrFones AppleInternals.GetConnectedDevices()
                    var currentScannedUdids = GetConnectedDevicesCli();
                    
                    var currentScannedHashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    // 2. Process Additions
                    foreach (var udidRaw in currentScannedUdids)
                    {
                        if (string.IsNullOrWhiteSpace(udidRaw)) continue;
                        
                        string udid = udidRaw.Trim();
                        if (udid.Length == 24 && !udid.Contains("-"))
                        {
                            udid = udid.Insert(8, "-");
                        }
                        
                        string finalUdid = udid.Length > 25 ? udid.ToLowerInvariant() : udid.ToUpperInvariant();
                        currentScannedHashSet.Add(finalUdid);

                        if (!_activeDevices.ContainsKey(finalUdid))
                        {
                            // 3. Port Map via USBLib exactly like DrFones PortMappList.getPortNumber() -> MatchListUSBDevice_SerialNumber
                            string locationPath = GetUniqueUsbPath(udidRaw);

                            if (string.IsNullOrEmpty(locationPath))
                            {
                                // Retry physical lookup
                                Thread.Sleep(200);
                                locationPath = GetUniqueUsbPath(udidRaw);
                            }

                            var newDevice = new UsbDevice
                            {
                                PnpDeviceId = "N/A",
                                LocationPath = locationPath,
                                Description = "Apple Mobile Device",
                                Manufacturer = "Apple Inc.",
                                Status = "OK",
                                IsAppleDevice = true,
                                DetectedAt = DateTime.Now,
                                Udid = finalUdid
                            };

                            if (_activeDevices.TryAdd(finalUdid, newDevice))
                            {
                                DeviceConnected?.Invoke(this, new AppleDeviceEventArgs(newDevice, true));
                            }
                        }
                    }

                    // 4. Process Removals
                    var activeUdids = _activeDevices.Keys.ToList();
                    foreach (var activeUdid in activeUdids)
                    {
                        if (!currentScannedHashSet.Contains(activeUdid))
                        {
                            if (_activeDevices.TryRemove(activeUdid, out var removedDevice))
                            {
                                DeviceDisconnected?.Invoke(this, new AppleDeviceEventArgs(removedDevice, false));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    RaiseError($"Scan error: {ex.Message}");
                }

                // Sleep exactly 1.5s like DrFones Thread.Sleep(1500)
                Thread.Sleep(1500);
            }
        }

        private List<string> GetConnectedDevicesCli()
        {
            try
            {
                string exePath = Path.Combine(_toolboxPath, IDEVICE_ID);
                if (!File.Exists(exePath)) return new List<string>();

                string output = LaunchExternalExecutable(exePath, "-l");
                if (string.IsNullOrWhiteSpace(output)) return new List<string>();

                return output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private string GetUniqueUsbPath(string serial)
        {
            try
            {
                string udid = serial;
                if (udid.Contains("-"))
                    udid = udid.Replace("-", "");

                // Exactly mirroring DrFones PortMapping.getUsbPath
                List<USBLib.USB.USBDevice> list1 = MatchListUSBDevice_SerialNumber(udid.ToLower());
                if (list1.Count > 0)
                    return list1[0].HubDevicePath + list1[0].PortNumber;

                List<USBLib.USB.USBDevice> list2 = MatchListUSBDevice_SerialNumber(udid);
                if (list2.Count > 0)
                    return list2[0].HubDevicePath + list2[0].PortNumber;
                
                return "";
            }
            catch (Exception ex)
            {
                RaiseError($"USBLib mapping error: {ex.Message}");
                return "";
            }
        }

        private List<USBLib.USB.USBDevice> MatchListUSBDevice_SerialNumber(string aSerialNumber)
        {
            List<USBLib.USB.USBDevice> usbDeviceList = new List<USBLib.USB.USBDevice>();
            
            // EXACT MATCH to DrFones: USB.GetConnectedDevices(aSerialNumber, "")
            var connectedDevices = USBLib.USB.GetConnectedDevices(aSerialNumber, "");
            if (connectedDevices != null)
            {
                foreach (USBLib.USB.USBDevice connectedDevice in connectedDevices)
                {
                    if (connectedDevice.Manufacturer != "Apple Inc.") continue;
                    
                    string dfuSerial = connectedDevice.SerialNumber;
                    if (dfuSerial.Contains("ECID:"))
                    {
                        try
                        {
                            string[] parts = dfuSerial.Split(' ');
                            foreach (string p in parts)
                            {
                                if (p.StartsWith("SRNM:"))
                                {
                                    dfuSerial = p.Replace("SRNM:", "").Replace("]", "").Replace("[", "").Trim();
                                }
                            }
                        }
                        catch { }
                    }

                    if (dfuSerial == aSerialNumber)
                    {
                        usbDeviceList.Add(connectedDevice);
                    }
                }
            }
            return usbDeviceList;
        }

        private string LaunchExternalExecutable(string executablePath, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = executablePath,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = arguments,
                RedirectStandardOutput = true
            };

            using (Process process = Process.Start(startInfo))
            {
                var outputTask = process.StandardOutput.ReadToEndAsync();
                if (process.WaitForExit(5000))
                {
                    return outputTask.Result;
                }
                else
                {
                    try { if (!process.HasExited) process.Kill(); } catch { }
                    return "";
                }
            }
        }

        private void RaiseError(string message)
        {
            try { ErrorOccurred?.Invoke(this, message); } catch { }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AppleDeviceDetector));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { StopAsync().Wait(TimeSpan.FromSeconds(5)); } catch { }
            _cts?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
