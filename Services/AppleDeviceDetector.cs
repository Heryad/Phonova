using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Microsoft.Win32;
using Phonova.Models;
using System.Windows;

namespace Phonova.Services
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
        private readonly ConcurrentDictionary<string, string> _deviceLocationCache = new ConcurrentDictionary<string, string>();
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
                // DRFONES NATIVE METHOD: Fully eliminate WMI. 
                // We use USBLib to iterate all connected devices and find Apple devices (VID 05AC).
                var devices = USBLib.USB.GetConnectedDevices();
                if (devices == null) return;
                
                foreach (var device in devices)
                {
                    // USBDevice doesn't expose raw VID directly, but it exposes SerialNumber and HubDevicePath
                    // We can also just rely on Apple's standard 24 or 40 char hex serial numbers.
                    // Or if USBLib exposes something like "FriendlyName" or "DeviceID".
                    // The safest way is to invoke ProcessDeviceEventAsync with a mocked dbccName,
                    // or just fire ProcessDeviceEventAsync directly if we have the UDID.
                    
                    string udid = device.SerialNumber;
                    if (string.IsNullOrWhiteSpace(udid)) continue;
                    
                    // We know Apple UDIDs are either 40 chars or 24 chars (with or without dash)
                    bool isApple = udid.Length == 40 || udid.Length == 24 || udid.Length == 25;
                    
                    if (isApple)
                    {
                        // Mock the dbccName exactly like Windows does to push it to the main parser
                        // e.g. \\?\usb#vid_05ac&pid_12a8#00008101-000E04001A04001E#{guid}
                        string dbccName = $@"\\?\usb#vid_05ac&pid_12a8#{udid}#{{a5dcbf10-6530-11d2-901f-00c04fb951ed}}";
                        ThreadPool.QueueUserWorkItem(_ => ProcessDeviceEventAsync(dbccName, true));
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

                            string[] parts = dbccName.Split('#');
                            if (parts.Length > 1 && parts[1].IndexOf("vid_05ac", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                bool isConnected = (eventType == DBT_DEVICEARRIVAL);
                                ThreadPool.QueueUserWorkItem(_ => 
                                {
                                    ProcessDeviceEventAsync(dbccName, isConnected);
                                });
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
                if (parts[1].IndexOf("&mi_", StringComparison.OrdinalIgnoreCase) >= 0) return;

                string prefix = parts[0].StartsWith(@"\\?\") ? parts[0].Substring(4) : parts[0];
                string pnpDeviceId = $"{prefix}\\{parts[1]}\\{parts[2]}".ToUpperInvariant();

                // Extract UDID first so we can use it for USBLib lookup
                string udid = parts[2].ToLowerInvariant();
                if (udid.Length == 24 && udid.StartsWith("0000") && !udid.Contains("-"))
                {
                    udid = udid.Insert(8, "-");
                }
                
                string finalUdid = udid.Length > 25 ? udid.ToLowerInvariant() : udid.ToUpperInvariant();

                string locationPath;
                if (isConnected)
                {
                    Thread.Sleep(1200); // Let Apple drivers initialize to avoid USBLib race condition
                    locationPath = GetPhysicalLocationPath(pnpDeviceId, udid);
                    if (string.IsNullOrWhiteSpace(locationPath)) return;
                    _deviceLocationCache[finalUdid] = locationPath;
                }
                else
                {
                    if (!_deviceLocationCache.TryGetValue(finalUdid, out locationPath!) && 
                        !_deviceLocationCache.TryGetValue(pnpDeviceId, out locationPath!))
                    {
                        return;
                    }
                    _deviceLocationCache.TryRemove(finalUdid, out _);
                    _deviceLocationCache.TryRemove(pnpDeviceId, out _);
                }

                var device = new UsbDevice
                {
                    PnpDeviceId = pnpDeviceId,
                    LocationPath = locationPath,
                    Description = "Apple Mobile Device",
                    Manufacturer = "Apple Inc.",
                    Status = "OK",
                    IsAppleDevice = true,
                    DetectedAt = DateTime.Now,
                    Udid = finalUdid
                };

                // Process active locations debounce
                if (isConnected)
                {
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

        #region SetupAPI Location Lookup

        private string GetPhysicalLocationPath(string pnpDeviceId, string udid)
        {
            // 1:1 DRFONES REPLICA: Use USBLib exclusively to query physical topology.
            // NO WMI or SetupAPI fallbacks allowed, as they are unstable across PC restarts.
            try
            {
                if (!string.IsNullOrWhiteSpace(udid))
                {
                    string cleanUdid = udid.Replace("-", "").ToLowerInvariant();
                    
                    // The Apple driver sometimes takes a moment to initialize the COM port.
                    // DrFones often relies on the timing of WM_DEVICECHANGE. We will poll
                    // briefly to ensure we catch it without a fragile fallback.
                    for (int i = 0; i < 5; i++)
                    {
                        var devices1 = USBLib.USB.GetConnectedDevices(cleanUdid, "");
                        if (devices1 != null && devices1.Count > 0)
                            return devices1[0].HubDevicePath + devices1[0].PortNumber;

                        var devices2 = USBLib.USB.GetConnectedDevices(udid, "");
                        if (devices2 != null && devices2.Count > 0)
                            return devices2[0].HubDevicePath + devices2[0].PortNumber;
                            
                        Thread.Sleep(500); // Wait half a second and try again
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseError($"USBLib mapping error: {ex.Message}");
            }

            // If USBLib cannot find it, return empty. We absolutely do not fall back to WMI!
            return string.Empty;
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

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid classGuid;
            public uint devInst;
            public IntPtr reserved;
        }

        private const uint DIGCF_PRESENT = 0x00000002;
        private const uint DIGCF_ALLCLASSES = 0x00000004;
        private const uint SPDRP_LOCATION_PATHS = 0x00000023;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetupDiGetClassDevs(IntPtr ClassGuid, string Enumerator, IntPtr hwndParent, uint Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, uint Property, out uint PropertyRegDataType, IntPtr PropertyBuffer, uint PropertyBufferSize, out uint RequiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        #endregion
    }
}
