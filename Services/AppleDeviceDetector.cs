using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Microsoft.Win32;
using Dyagnoz_Latest.Models;
using System.Windows;

namespace Dyagnoz_Latest.Services
{
    /// <summary>
    /// Event arguments for Apple device connection/disconnection events
    /// </summary>
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

    /// <summary>
    /// High-performance Apple device detector using interrupt-driven WM_DEVICECHANGE.
    /// </summary>
    public sealed class AppleDeviceDetector : IDisposable
    {
        #region Constants

        private const string APPLE_VENDOR_ID = "vid_05ac";

        #endregion

        #region Fields

        private readonly ConcurrentDictionary<string, DateTime> _activeLocations;
        private readonly SemaphoreSlim _watcherLock;
        private volatile bool _isRunning;
        private volatile bool _disposed;
        
        private HwndSource? _hwndSource;
        private IntPtr _deviceNotifyHandle;

        #endregion

        #region Events

        public event EventHandler<AppleDeviceEventArgs>? DeviceConnected;
        public event EventHandler<AppleDeviceEventArgs>? DeviceDisconnected;
        public event EventHandler<string>? ErrorOccurred;

        #endregion

        #region Properties

        public bool IsRunning => _isRunning;
        public int ActiveDeviceCount => _activeLocations.Count;

        #endregion

        #region Constructor

        public AppleDeviceDetector()
        {
            _activeLocations = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            _watcherLock = new SemaphoreSlim(1, 1);
        }

        #endregion

        #region Start/Stop

        public async Task<bool> StartAsync(IntPtr mainWindowHandle)
        {
            ThrowIfDisposed();

            await _watcherLock.WaitAsync();
            try
            {
                if (_isRunning) return true;

                bool success = false;

                // Attach to the existing MainWindow's HwndSource so we are guaranteed to receive broadcast messages!
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _hwndSource = System.Windows.Interop.HwndSource.FromHwnd(mainWindowHandle);
                    _hwndSource.AddHook(WndProc);

                    // Register for device notifications
                    var dbi = new DEV_BROADCAST_DEVICEINTERFACE
                    {
                        dbcc_size = Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE)),
                        dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE,
                        dbcc_reserved = 0,
                        dbcc_classguid = GUID_DEVINTERFACE_USB_DEVICE
                    };

                    IntPtr buffer = Marshal.AllocHGlobal(dbi.dbcc_size);
                    Marshal.StructureToPtr(dbi, buffer, true);

                    _deviceNotifyHandle = RegisterDeviceNotification(_hwndSource.Handle, buffer, DEVICE_NOTIFY_WINDOW_HANDLE);
                    Marshal.FreeHGlobal(buffer);

                    if (_deviceNotifyHandle != IntPtr.Zero)
                    {
                        success = true;
                    }
                });

                if (!success)
                {
                    RaiseError("Failed to register device notification.");
                    return false;
                }

                _isRunning = true;

                // Run a one-time initial scan for devices that are already plugged in
                _ = Task.Run(() => PerformInitialScan());

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

        private void PerformInitialScan()
        {
            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT DeviceID FROM Win32_PnPEntity WHERE DeviceID LIKE '%VID_05AC%'");
                
                foreach (System.Management.ManagementBaseObject managementBaseObject in searcher.Get())
                {
                    if (managementBaseObject is System.Management.ManagementObject obj)
                    {
                        var deviceId = obj["DeviceID"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(deviceId))
                        {
                            // Convert standard USB\VID format to the dbccName format expected by ProcessDeviceEventAsync
                            string dbccName = @"\\?\" + deviceId.Replace('\\', '#');
                            ThreadPool.QueueUserWorkItem(_ => ProcessDeviceEventAsync(dbccName, true));
                        }
                        obj.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseError($"Initial device scan failed: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            await _watcherLock.WaitAsync();
            try
            {
                if (!_isRunning) return;

                CleanupWatchers();
                _activeLocations.Clear();
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
                if (_deviceNotifyHandle != IntPtr.Zero)
                {
                    UnregisterDeviceNotification(_deviceNotifyHandle);
                    _deviceNotifyHandle = IntPtr.Zero;
                }
                if (_hwndSource != null)
                {
                    _hwndSource.RemoveHook(WndProc);
                    // Do NOT dispose _hwndSource because it belongs to MainWindow!
                    _hwndSource = null;
                }
            });
        }

        #endregion

        #region WM_DEVICECHANGE Processor

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DEVICECHANGE)
            {
                int eventType = wParam.ToInt32();
                if (eventType == DBT_DEVICEARRIVAL || eventType == DBT_DEVICEREMOVECOMPLETE)
                {
                    if (lParam != IntPtr.Zero)
                    {
                        var hdr = Marshal.PtrToStructure<DEV_BROADCAST_HDR>(lParam);
                        if (hdr.dbcc_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
                        {
                            // Safely extract the variable-length string from offset 28
                            IntPtr namePtr = new IntPtr(lParam.ToInt64() + 28);
                            string dbccName = Marshal.PtrToStringAuto(namePtr);

                            if (!string.IsNullOrEmpty(dbccName) && dbccName.Contains(APPLE_VENDOR_ID, StringComparison.OrdinalIgnoreCase))
                            {
                                bool isConnected = (eventType == DBT_DEVICEARRIVAL);
                                ThreadPool.QueueUserWorkItem(_ => ProcessDeviceEventAsync(dbccName, isConnected));
                            }
                        }
                    }
                }
            }
            return IntPtr.Zero;
        }

        private void ProcessDeviceEventAsync(string dbccName, bool isConnected)
        {
            try
            {
                // dbccName looks like \\?\usb#vid_05ac&pid_12a8#00008101-000E04001A04001E#{a5dcbf10-6530-11d2-901f-00c04fb951ed}
                string[] parts = dbccName.Split('#');
                if (parts.Length < 3) return;

                // Ignore MI_ interfaces to avoid duplicate trigger
                if (parts[1].Contains("&mi_", StringComparison.OrdinalIgnoreCase)) return;

                string prefix = parts[0].StartsWith(@"\\?\") ? parts[0].Substring(4) : parts[0];
                string pnpDeviceId = $"{prefix}\\{parts[1]}\\{parts[2]}".ToUpperInvariant();

                string locationPath = GetPhysicalLocationPath(pnpDeviceId);
                if (string.IsNullOrWhiteSpace(locationPath)) return;

                var device = new UsbDevice
                {
                    PnpDeviceId = pnpDeviceId,
                    LocationPath = locationPath,
                    Description = "Apple Mobile Device",
                    Manufacturer = "Apple Inc.",
                    Status = "OK",
                    IsAppleDevice = true,
                    DetectedAt = DateTime.Now
                };

                // Extract UDID from the last part of PnpDeviceId
                string potentialUdid = parts[2].ToLowerInvariant();
                if (potentialUdid.Length == 24 && potentialUdid.StartsWith("0000") && !potentialUdid.Contains("-"))
                {
                    potentialUdid = potentialUdid.Insert(8, "-");
                }
                if (potentialUdid.Length > 25)
                {
                    device.Udid = potentialUdid.ToLower();
                }
                else
                {
                    device.Udid = potentialUdid.ToUpper();
                }

                if (isConnected)
                {
                    // Smart Debounce logic that auto-recovers from missed disconnects
                    if (_activeLocations.TryGetValue(locationPath, out var lastConnectTime))
                    {
                        if ((DateTime.UtcNow - lastConnectTime).TotalSeconds < 2) return;
                    }
                    _activeLocations[locationPath] = DateTime.UtcNow;
                    DeviceConnected?.Invoke(this, new AppleDeviceEventArgs(device, true));
                }
                else
                {
                    _activeLocations.TryRemove(locationPath, out _);
                    DeviceDisconnected?.Invoke(this, new AppleDeviceEventArgs(device, false));
                }
            }
            catch (Exception ex)
            {
                RaiseError($"Error processing device event: {ex.Message}");
            }
        }

        #endregion

        #region Registry Location Lookup

        private string GetPhysicalLocationPath(string pnpDeviceId)
        {
            if (string.IsNullOrWhiteSpace(pnpDeviceId)) return string.Empty;

            try
            {
                string? location = GetLocationFromRegistry(@"SYSTEM\CurrentControlSet\Enum\" + pnpDeviceId);
                if (!string.IsNullOrWhiteSpace(location)) return location;

                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\" + pnpDeviceId))
                {
                    var parentId = key?.GetValue("ParentIdPrefix")?.ToString();
                    if (!string.IsNullOrWhiteSpace(parentId))
                    {
                        location = FindLocationByParentId(parentId);
                        if (!string.IsNullOrWhiteSpace(location)) return location;
                    }
                }
                return string.Empty;
            }
            catch { return string.Empty; }
        }

        private string? GetLocationFromRegistry(string registryPath)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (key == null) return null;

                    // Strictly use LocationPaths to guarantee physical hardware ports only
                    var locationPaths = key.GetValue("LocationPaths");
                    if (locationPaths is string[] paths && paths.Length > 0) return paths[0];
                    else if (locationPaths is string singlePath) return singlePath;
                }
            }
            catch { }
            return null;
        }

        private string? FindLocationByParentId(string parentId)
        {
            try
            {
                string usbEnumPath = @"SYSTEM\CurrentControlSet\Enum\USB";
                using (var usbKey = Registry.LocalMachine.OpenSubKey(usbEnumPath))
                {
                    if (usbKey == null) return null;

                    foreach (var vidPidKeyName in usbKey.GetSubKeyNames())
                    {
                        using (var vidPidKey = usbKey.OpenSubKey(vidPidKeyName))
                        {
                            if (vidPidKey == null) continue;

                            foreach (var instanceKeyName in vidPidKey.GetSubKeyNames())
                            {
                                if (instanceKeyName.StartsWith(parentId, StringComparison.OrdinalIgnoreCase))
                                {
                                    string fullPath = $@"SYSTEM\CurrentControlSet\Enum\USB\{vidPidKeyName}\{instanceKeyName}";
                                    return GetLocationFromRegistry(fullPath);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        #endregion

        #region Helper & Dispose

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

        #endregion

        #region P/Invoke Definitions

        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVTYP_DEVICEINTERFACE = 5;
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;

        // GUID for USB devices (A5DCBF10-6530-11D2-901F-00C04FB951ED)
        private static readonly Guid GUID_DEVINTERFACE_USB_DEVICE = new Guid(0xA5DCBF10, 0x6530, 0x11D2, 0x90, 0x1F, 0x00, 0xC0, 0x4F, 0xB9, 0x51, 0xED);

        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_HDR
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            public char dbcc_name; // Declared as single char to prevent PtrToStructure AccessViolation bounds error
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        #endregion
    }
}
