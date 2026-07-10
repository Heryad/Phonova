using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Phonova.Models;

namespace Phonova.Services
{
    /// <summary>
    /// High-performance port mapping service with learning mode.
    /// Scaled up for large hubs (MAX_PORTS = 127).
    /// </summary>
    public sealed class PortMappingService : IDisposable
    {
        #region Constants

        private const string MAPPING_FILE_NAME = "usb-port-mapping.json";
        // INCREASED LIMIT as requested
        private const int MAX_PORTS = 127; 

        #endregion

        #region Fields

        private readonly string _mappingFilePath;
        private readonly ConcurrentDictionary<string, PortMappingEntry> _mappingsByLocation;
        private readonly ConcurrentDictionary<int, PortMappingEntry> _mappingsByPort;
        private readonly SemaphoreSlim _persistenceLock;
        private readonly object _learningLock = new object();

        private volatile bool _isLearningMode;
        private volatile int _nextPortNumber;
        private volatile bool _disposed;

        private HubInfo _hubInfo;

        #endregion

        #region Events

        public event EventHandler? MappingUpdated;
        public event EventHandler<bool>? LearningModeChanged;
        public event EventHandler<string>? ErrorOccurred;

        #endregion

        #region Properties

        public bool IsLearningMode => _isLearningMode;
        public int NextPortNumber => _nextPortNumber;
        public int MappingCount => _mappingsByLocation.Count;

        #endregion

        #region Constructor

        public PortMappingService()
        {
            // Use ProgramData for system-wide read/write access (Standard for installed apps)
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var dyagnozDataFolder = Path.Combine(programData, "Phonova");

            try
            {
                if (!Directory.Exists(dyagnozDataFolder))
                {
                    Directory.CreateDirectory(dyagnozDataFolder);
                }
            }
            catch (Exception ex)
            {
                RaiseError($"Failed to create data folder: {ex.Message}");
            }

            _mappingFilePath = Path.Combine(dyagnozDataFolder, MAPPING_FILE_NAME);

            _mappingsByLocation = new ConcurrentDictionary<string, PortMappingEntry>(StringComparer.OrdinalIgnoreCase);
            _mappingsByPort = new ConcurrentDictionary<int, PortMappingEntry>();
            _persistenceLock = new SemaphoreSlim(1, 1);
            _hubInfo = new HubInfo();
            _nextPortNumber = 1;
        }

        #endregion

        #region Loading & Saving

        public async Task<bool> LoadMappingAsync()
        {
            ThrowIfDisposed();

            await _persistenceLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!File.Exists(_mappingFilePath)) return true;

                var json = File.ReadAllText(_mappingFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    RaiseError("Mapping file is empty");
                    return false;
                }

                var config = JsonConvert.DeserializeObject<PortMappingConfiguration>(json);
                if (config == null || !ValidateConfiguration(config))
                {
                    RaiseError("Invalid mapping configuration");
                    return false;
                }

                _mappingsByLocation.Clear();
                _mappingsByPort.Clear();

                foreach (var mapping in config.Mappings)
                {
                    _mappingsByLocation[mapping.UsbLocationPath] = mapping;
                    _mappingsByPort[mapping.LogicalPort] = mapping;
                }

                _hubInfo = config.HubInfo ?? new HubInfo();
                
                // CRITICAL FIX: Update _nextPortNumber based on loaded mappings
                if (config.Mappings.Any())
                {
                    _nextPortNumber = config.Mappings.Max(m => m.LogicalPort) + 1;
                }
                else
                {
                    _nextPortNumber = 1;
                }

                RaiseMappingUpdated();

                return true;
            }
            catch (Exception ex)
            {
                RaiseError($"Failed to load mapping: {ex.Message}");
                return false;
            }
            finally
            {
                _persistenceLock.Release();
            }
        }

        public async Task<bool> SaveMappingAsync()
        {
            ThrowIfDisposed();

            await _persistenceLock.WaitAsync().ConfigureAwait(false);
            try
            {
                _hubInfo.LastCalibrated = DateTime.Now;
                _hubInfo.TotalPorts = MAX_PORTS;

                var config = new PortMappingConfiguration
                {
                    HubInfo = _hubInfo,
                    Mappings = _mappingsByLocation.Values.ToList()
                };

                var json = JsonConvert.SerializeObject(config, Formatting.Indented);

                var tempPath = _mappingFilePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(_mappingFilePath)) File.Delete(_mappingFilePath);
                File.Move(tempPath, _mappingFilePath);

                RaiseMappingUpdated();
                return true;
            }
            catch (Exception ex)
            {
                RaiseError($"Failed to save mapping: {ex.Message}");
                return false;
            }
            finally
            {
                _persistenceLock.Release();
            }
        }

        public async Task ResetMappingAsync()
        {
            ThrowIfDisposed();

            await _persistenceLock.WaitAsync().ConfigureAwait(false);
            try
            {
                _mappingsByLocation.Clear();
                _mappingsByPort.Clear();
                _nextPortNumber = 1;
                
                // Clear file content immediately
                if (File.Exists(_mappingFilePath))
                {
                    File.Delete(_mappingFilePath);
                }
                
                RaiseMappingUpdated();
            }
            catch (Exception ex)
            {
                RaiseError($"Failed to reset mapping: {ex.Message}");
            }
            finally
            {
                _persistenceLock.Release();
            }
        }

        #endregion

        #region Learning Mode

        public Task<bool> StartLearningModeAsync()
        {
            ThrowIfDisposed();

            lock (_learningLock)
            {
                if (_isLearningMode) return Task.FromResult(true);

                // Clear existing mappings for multiple rounds of learning? 
                // Usually we want to clear. User can re-scan everything.
                _mappingsByLocation.Clear();
                _mappingsByPort.Clear();
                _nextPortNumber = 1;
                _isLearningMode = true;
            }

            RaiseLearningModeChanged(true);
            RaiseMappingUpdated();
            return Task.FromResult(true);
        }

        public Task StopLearningModeAsync()
        {
            ThrowIfDisposed();

            lock (_learningLock)
            {
                if (!_isLearningMode) return Task.CompletedTask;
                _isLearningMode = false;
            }

            RaiseLearningModeChanged(false);
            return Task.CompletedTask;
        }

        public Task<int?> AssignPortAsync(string usbLocationPath)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(usbLocationPath))
            {
                RaiseError("USB location path cannot be empty");
                return Task.FromResult<int?>(null);
            }

            lock (_learningLock)
            {
                if (!_isLearningMode)
                {
                    RaiseError("Cannot assign port: not in learning mode");
                    return Task.FromResult<int?>(null);
                }

                // If already assigned, return existing
                if (_mappingsByLocation.TryGetValue(usbLocationPath, out var existing))
                {
                    return Task.FromResult<int?>(existing.LogicalPort);
                }

                int limit = MAX_PORTS;
                if (ApiService.CurrentConfig != null && !ApiService.CurrentConfig.isUnlimitedTesting)
                {
                    limit = ApiService.CurrentConfig.maxConcurrentDevices;
                }

                if (_mappingsByLocation.Count >= limit || _nextPortNumber > limit)
                {
                    RaiseError($"Mapping Limit Reached! Your license only allows mapping up to {limit} concurrent devices.");
                    return Task.FromResult<int?>(null);
                }

                var mapping = new PortMappingEntry
                {
                    LogicalPort = _nextPortNumber,
                    UsbLocationPath = usbLocationPath,
                    AssignedAt = DateTime.Now,
                    IsConnected = true
                };

                _mappingsByLocation[usbLocationPath] = mapping;
                _mappingsByPort[_nextPortNumber] = mapping;

                var assignedPort = _nextPortNumber;
                _nextPortNumber++;

                RaiseMappingUpdated();
                
                _ = Task.Run(() => SaveMappingAsync());

                return Task.FromResult<int?>(assignedPort);
            }
        }

        #endregion

        #region Port Lookups

        public Task<int?> GetPortNumberAsync(string usbLocationPath)
        {
            if (string.IsNullOrWhiteSpace(usbLocationPath)) return Task.FromResult<int?>(null);

            if (_mappingsByLocation.TryGetValue(usbLocationPath, out var mapping))
            {
                return Task.FromResult<int?>(mapping.LogicalPort);
            }
            return Task.FromResult<int?>(null);
        }

        public PortMappingEntry? GetMappingByPort(int portNumber)
        {
            _mappingsByPort.TryGetValue(portNumber, out var mapping);
            return mapping;
        }

        public Task<bool> UpdatePortStatusAsync(string usbLocationPath, bool isConnected)
        {
            if (string.IsNullOrWhiteSpace(usbLocationPath)) return Task.FromResult(false);

            if (_mappingsByLocation.TryGetValue(usbLocationPath, out var mapping))
            {
                mapping.IsConnected = isConnected;
                RaiseMappingUpdated();
                // If in learning mode, we might want to do something here, 
                // but usually we just listen for connections to assign.
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public List<PortMappingEntry> GetAllMappings()
        {
            return _mappingsByLocation.Values
                .OrderBy(m => m.LogicalPort)
                .ToList();
        }

        public Task<List<PortMappingEntry>> GetAllMappingsAsync()
        {
            return Task.FromResult(GetAllMappings());
        }

        #endregion

        #region Helper & Dispose

        private bool ValidateConfiguration(PortMappingConfiguration config)
        {
            if (config.Mappings == null) return false;
            // Basic dupe checks
            var portNumbers = config.Mappings.Select(m => m.LogicalPort).ToList();
            if (portNumbers.Count != portNumbers.Distinct().Count()) return false;
            return true;
        }

        private void RaiseMappingUpdated() { try { MappingUpdated?.Invoke(this, EventArgs.Empty); } catch { } }
        private void RaiseLearningModeChanged(bool isActive) { try { LearningModeChanged?.Invoke(this, isActive); } catch { } }
        private void RaiseError(string message) { try { ErrorOccurred?.Invoke(this, message); } catch { } }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PortMappingService));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _persistenceLock.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
