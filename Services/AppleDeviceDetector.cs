using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Phonova.Models;
using System.Windows;

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
        private readonly SemaphoreSlim _watcherLock;
        private volatile bool _isRunning;
        private volatile bool _disposed;
        
        private HwndSource? _hwndSource;
        private USBClass? _usbClass;
        private object _scanLock = new object();

        public event EventHandler<AppleDeviceEventArgs>? DeviceConnected;
        public event EventHandler<AppleDeviceEventArgs>? DeviceDisconnected;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsRunning => _isRunning;
        public int ActiveDeviceCount => _activeDevices.Count;

        public AppleDeviceDetector()
        {
            _activeDevices = new ConcurrentDictionary<string, UsbDevice>(StringComparer.OrdinalIgnoreCase);
            _watcherLock = new SemaphoreSlim(1, 1);
        }

        public async Task<bool> StartAsync(IntPtr mainWindowHandle)
        {
            ThrowIfDisposed();

            await _watcherLock.WaitAsync();
            try
            {
                if (_isRunning) return true;

                bool success = false;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _hwndSource = System.Windows.Interop.HwndSource.FromHwnd(mainWindowHandle);
                    _hwndSource.AddHook(WndProc);

                    _usbClass = new USBClass();
                    _usbClass.USBDeviceAttached += OnUSBDeviceAttached;
                    _usbClass.USBDeviceRemoved += OnUSBDeviceRemoved;
                    
                    // Register exactly like DrFones
                    success = _usbClass.RegisterForDeviceChange(true, _hwndSource.Handle);
                });

                if (!success)
                {
                    RaiseError("Failed to register device notification using USBClass.");
                    return false;
                }

                _isRunning = true;
                
                // DRFONES NATIVE: Perform an initial full scan
                _ = Task.Run(() => PerformScan());

                return true;
            }
            catch (Exception ex)
            {
                RaiseError($"Failed to start detector: {ex.Message}");
                CleanupWatchers();
                return false;
            }
            finally
            {
                _watcherLock.Release();
            }
        }

        public async Task StopAsync()
        {
            await _watcherLock.WaitAsync();
            try
            {
                if (!_isRunning) return;

                CleanupWatchers();
                _activeDevices.Clear();
                _isRunning = false;
            }
            catch (Exception ex)
            {
                RaiseError($"Error stopping detector: {ex.Message}");
            }
            finally
            {
                _watcherLock.Release();
            }
        }

        private void CleanupWatchers()
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                if (_usbClass != null)
                {
                    _usbClass.RegisterForDeviceChange(false, IntPtr.Zero);
                    _usbClass.USBDeviceAttached -= OnUSBDeviceAttached;
                    _usbClass.USBDeviceRemoved -= OnUSBDeviceRemoved;
                    _usbClass = null;
                }
                
                if (_hwndSource != null)
                {
                    _hwndSource.RemoveHook(WndProc);
                    _hwndSource = null;
                }
            });
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_usbClass != null)
            {
                // Feed messages straight to DrFones USBClass
                _usbClass.ProcessWindowsMessage(msg, wParam, lParam, ref handled);
            }
            return IntPtr.Zero;
        }

        private void OnUSBDeviceAttached(object sender, USBClass.USBDeviceEventArgs e)
        {
            // Add a small 1-second delay so Apple mobile drivers initialize exactly like DrFones timing
            Task.Run(() => 
            {
                Thread.Sleep(1000);
                PerformScan();
            });
        }

        private void OnUSBDeviceRemoved(object sender, USBClass.USBDeviceEventArgs e)
        {
            Task.Run(() => 
            {
                PerformScan();
            });
        }

        private void PerformScan()
        {
            lock (_scanLock)
            {
                try
                {
                    // Full scan using USBLib precisely like DrFones RetailDashboard does
                    var devices = USBLib.USB.GetConnectedDevices();
                    if (devices == null) return;
                    
                    var currentScannedUdids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var device in devices)
                    {
                        string udid = device.SerialNumber;
                        if (string.IsNullOrWhiteSpace(udid)) continue;
                        
                        string finalUdid = udid.Length > 25 ? udid.ToLowerInvariant() : udid.ToUpperInvariant();
                        currentScannedUdids.Add(finalUdid);

                        if (!_activeDevices.ContainsKey(finalUdid))
                        {
                            string locationPath = device.HubDevicePath + device.PortNumber;

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

                    // Diff removals
                    var activeUdids = _activeDevices.Keys.ToList();
                    foreach (var activeUdid in activeUdids)
                    {
                        if (!currentScannedUdids.Contains(activeUdid))
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
            _watcherLock.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
