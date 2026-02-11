using System.Collections.Concurrent;
using System.Management;
using Microsoft.Win32;
using Dyagnoz_Latest.Models;
using System.Diagnostics;

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
    /// High-performance Apple device detector using WMI.
    /// </summary>
    public sealed class AppleDeviceDetector : IDisposable
    {
        #region Constants

        private const string APPLE_VENDOR_ID = "05AC";

        #endregion

        #region Fields

        private ManagementEventWatcher? _connectionWatcher;
        private ManagementEventWatcher? _disconnectionWatcher;
        private readonly ConcurrentDictionary<string, DateTime> _activeLocations;
        private readonly SemaphoreSlim _watcherLock;
        private volatile bool _isRunning;
        private volatile bool _disposed;

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

        public async Task<bool> StartAsync()
        {
            ThrowIfDisposed();

            await _watcherLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_isRunning) return true;

                if (!SetupConnectionWatcher()) return false;

                if (!SetupDisconnectionWatcher())
                {
                    CleanupWatchers();
                    return false;
                }

                _isRunning = true;
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
            await _watcherLock.WaitAsync().ConfigureAwait(false);
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
            if (_connectionWatcher != null)
            {
                try { _connectionWatcher.Stop(); _connectionWatcher.Dispose(); } catch { }
                _connectionWatcher = null;
            }

            if (_disconnectionWatcher != null)
            {
                try { _disconnectionWatcher.Stop(); _disconnectionWatcher.Dispose(); } catch { }
                _disconnectionWatcher = null;
            }
        }

        #endregion

        #region WMI Watcher Setup

        private bool SetupConnectionWatcher()
        {
            try
            {
                var query = new WqlEventQuery(
                    "__InstanceCreationEvent",
                    TimeSpan.FromSeconds(1),
                    $"TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.PNPDeviceID LIKE 'USB\\\\VID_{APPLE_VENDOR_ID}%'"
                );

                _connectionWatcher = new ManagementEventWatcher(query);
                _connectionWatcher.EventArrived += OnWmiConnectionEvent;
                _connectionWatcher.Start();

                return true;
            }
            catch (Exception ex)
            {
                RaiseError($"Failed to setup connection watcher: {ex.Message}");
                return false;
            }
        }

        private bool SetupDisconnectionWatcher()
        {
            try
            {
                var query = new WqlEventQuery(
                    "__InstanceDeletionEvent",
                    TimeSpan.FromSeconds(1),
                    $"TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.PNPDeviceID LIKE 'USB\\\\VID_{APPLE_VENDOR_ID}%'"
                );

                _disconnectionWatcher = new ManagementEventWatcher(query);
                _disconnectionWatcher.EventArrived += OnWmiDisconnectionEvent;
                _disconnectionWatcher.Start();

                return true;
            }
            catch (Exception ex)
            {
                RaiseError($"Failed to setup disconnection watcher: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region WMI Event Handlers (Non-Blocking)

        private void OnWmiConnectionEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject? targetInstance = null;
            try { targetInstance = e?.NewEvent?["TargetInstance"] as ManagementBaseObject; } catch { return; }

            if (targetInstance == null) return;

            var deviceData = ExtractDeviceData(targetInstance);
            if (deviceData == null) return;

            ThreadPool.QueueUserWorkItem(_ => ProcessConnectionAsync(deviceData.Value));
        }

        private void OnWmiDisconnectionEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject? targetInstance = null;
            try { targetInstance = e?.NewEvent?["TargetInstance"] as ManagementBaseObject; } catch { return; }

            if (targetInstance == null) return;

            var deviceData = ExtractDeviceData(targetInstance);
            if (deviceData == null) return;

            ThreadPool.QueueUserWorkItem(_ => ProcessDisconnectionAsync(deviceData.Value));
        }

        #endregion

        #region Device Processing

        private readonly struct DeviceData
        {
            public readonly string PnpDeviceId;
            public readonly string Description;
            public readonly string Manufacturer;
            public readonly string Status;
            public readonly string LocationPath;

            public DeviceData(string pnpDeviceId, string description, string manufacturer, string status, string locationPath)
            {
                PnpDeviceId = pnpDeviceId;
                Description = description;
                Manufacturer = manufacturer;
                Status = status;
                LocationPath = locationPath;
            }
        }

        private DeviceData? ExtractDeviceData(ManagementBaseObject obj)
        {
            try
            {
                var pnpDeviceId = obj["PNPDeviceID"]?.ToString();
                if (string.IsNullOrWhiteSpace(pnpDeviceId)) return null;

                var description = obj["Description"]?.ToString() ?? "";

                if (pnpDeviceId.Contains("&MI_", StringComparison.OrdinalIgnoreCase)) return null;

                bool isAppleMobile = description.Contains("Apple Mobile Device", StringComparison.OrdinalIgnoreCase) ||
                                     description.Contains("Apple iPhone", StringComparison.OrdinalIgnoreCase) ||
                                     description.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
                                     description.Contains("iPad", StringComparison.OrdinalIgnoreCase);

                if (!isAppleMobile) return null;

                var locationPath = GetPhysicalLocationPath(pnpDeviceId);
                if (string.IsNullOrWhiteSpace(locationPath)) return null;

                return new DeviceData(
                    pnpDeviceId,
                    description,
                    obj["Manufacturer"]?.ToString() ?? "Apple Inc.",
                    obj["Status"]?.ToString() ?? "OK",
                    locationPath
                );
            }
            catch (Exception ex)
            {
                RaiseError($"Error extracting device data: {ex.Message}");
                return null;
            }
        }

        private void ProcessConnectionAsync(DeviceData data)
        {
            try
            {
                if (!_activeLocations.TryAdd(data.LocationPath, DateTime.UtcNow)) return;

                var device = new UsbDevice
                {
                    PnpDeviceId = data.PnpDeviceId,
                    LocationPath = data.LocationPath,
                    Description = data.Description,
                    Manufacturer = data.Manufacturer,
                    Status = data.Status,
                    IsAppleDevice = true,
                    DetectedAt = DateTime.Now
                };

                if (!string.IsNullOrEmpty(data.PnpDeviceId))
                {
                    var lastSlashIndex = data.PnpDeviceId.LastIndexOf('\\');
                    if (lastSlashIndex >= 0 && lastSlashIndex < data.PnpDeviceId.Length - 1)
                    {
                        var potentialUdid = data.PnpDeviceId.Substring(lastSlashIndex + 1);
                        
                        if (potentialUdid.Length == 24 && potentialUdid.StartsWith("0000") && !potentialUdid.Contains("-"))
                        {
                            potentialUdid = potentialUdid.Insert(8, "-");
                        }

                        // Basic validation: UDIDs are alphanumeric and reasonably long
                        if (potentialUdid.Length > 25)
                        {
                            device.Udid = potentialUdid.ToLower();
                        }
                        else
                        {
                            device.Udid = potentialUdid.ToUpper();
                        }
                    }
                }

                DeviceConnected?.Invoke(this, new AppleDeviceEventArgs(device, true));
            }
            catch (Exception ex)
            {
                RaiseError($"Error processing connection: {ex.Message}");
            }
        }

        private void ProcessDisconnectionAsync(DeviceData data)
        {
            try
            {
                if (!_activeLocations.TryRemove(data.LocationPath, out _)) return;

                var device = new UsbDevice
                {
                    PnpDeviceId = data.PnpDeviceId,
                    LocationPath = data.LocationPath,
                    Description = data.Description,
                    Manufacturer = data.Manufacturer,
                    Status = data.Status,
                    IsAppleDevice = true,
                    DetectedAt = DateTime.Now
                };

                DeviceDisconnected?.Invoke(this, new AppleDeviceEventArgs(device, false));
            }
            catch (Exception ex)
            {
                RaiseError($"Error processing disconnection: {ex.Message}");
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

                    var locationPaths = key.GetValue("LocationPaths");
                    if (locationPaths is string[] paths && paths.Length > 0) return paths[0];
                    else if (locationPaths is string singlePath) return singlePath;

                    var locationInfo = key.GetValue("LocationInformation")?.ToString();
                    if (!string.IsNullOrWhiteSpace(locationInfo)) return locationInfo;
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
    }
}
