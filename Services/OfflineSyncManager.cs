using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Phonova.Models;

namespace Phonova.Services
{
    public class OfflineSyncManager
    {
        private static readonly Lazy<OfflineSyncManager> _instance = new Lazy<OfflineSyncManager>(() => new OfflineSyncManager());
        public static OfflineSyncManager Instance => _instance.Value;

        private readonly string _cachePath;
        private readonly ConcurrentQueue<ApiService.SyncResult> _pendingQueue = new ConcurrentQueue<ApiService.SyncResult>();
        private readonly SemaphoreSlim _syncSemaphore = new SemaphoreSlim(1, 1);
        private readonly object _fileLock = new object();

        public event Action<int>? QueueChanged;
        public event Action<int?>? SyncSucceeded;
        public event Action<int, int>? FuelExhausted;  // (required, available)

        public bool IsFuelExhausted { get; set; } = false;
        public int PendingCount => _pendingQueue.Count;

        private OfflineSyncManager()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var folder = Path.Combine(appData, "Phonova");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            _cachePath = Path.Combine(folder, "offline_sync.json");
            LoadQueueFromDisk();
        }

        public void QueueResult(ProcessedDevice device)
        {
            var syncItem = new ApiService.SyncResult
            {
                customerId = device.CustomerId, // null if no customer selected
                devicename = device.DeviceName,
                model = device.Model,
                serialnumber = device.Serial,
                imei1 = device.Imei ?? "",
                imei2 = device.Imei2 ?? "",
                color = device.Color,
                storage = device.Storage,
                battery = device.BatteryHealth != null ? $"{device.BatteryHealth}% (Cycles: {device.BatteryCycles})" : "--",
                icloud = device.IcloudStatus,
                mdm = device.MdmStatus,
                blacklist = "N/A",   // no blacklist check on client
                simlock = device.SimStatus,
                finalGrade = DetermineFinalGrade(device),
                comments = device.Comments ?? new System.Collections.Generic.List<string>(),
                mmrComments = new System.Collections.Generic.List<string>(),
                kernel_results = device.KernelTests,
                test_results = device.AppTests
            };

            // If MMR Mode is active, copy comments to mmrComments
            if (SettingsManager.Current.MmrMode)
            {
                syncItem.mmrComments = device.Comments ?? new List<string>();
                syncItem.comments = new List<string>();
            }

            _pendingQueue.Enqueue(syncItem);
            SaveQueueToDisk();
            QueueChanged?.Invoke(PendingCount);

            // Trigger sync in background
            _ = Task.Run(() => TrySyncAsync());
        }

        private string DetermineFinalGrade(ProcessedDevice device)
        {
            if (SettingsManager.Current.MmrMode) return "mmr";

            bool isPass = true;
            if (device.KernelTests != null)
            {
                foreach (var val in device.KernelTests.Values)
                {
                    if (val == "Fail" || val == "1") { isPass = false; break; }
                }
            }
            if (isPass && device.AppTests != null)
            {
                foreach (var val in device.AppTests.Values)
                {
                    if (val == "Fail" || val == "1") { isPass = false; break; }
                }
            }
            return isPass ? "passed" : "failed";
        }

        public async Task TrySyncAsync()
        {
            // Don't retry if we know fuel is exhausted — wait until fuel is topped up
            if (IsFuelExhausted) return;

            // Ensure only one sync operation runs at a time
            if (!await _syncSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                if (_pendingQueue.IsEmpty) return;

                // Take all currently pending items
                var itemsToSync = _pendingQueue.ToList();

                var response = await ApiService.SubmitResultsAsync(itemsToSync);
                if (response == null) return;

                if (response.HttpStatusCode == 402)
                {
                    // Insufficient fuel — hold items, stop retrying, raise banner event
                    IsFuelExhausted = true;
                    FuelExhausted?.Invoke(response.required ?? itemsToSync.Count, response.available ?? 0);
                    System.Diagnostics.Debug.WriteLine($"[OfflineSyncManager] Fuel exhausted. Required={response.required}, Available={response.available}. Holding {PendingCount} items.");
                }
                else if (string.IsNullOrEmpty(response.error))
                {
                    // Success! Clear fuel-exhausted state and dequeue synced items
                    IsFuelExhausted = false;
                    int syncedCount = response.syncedCount;
                    for (int i = 0; i < syncedCount; i++)
                    {
                        _pendingQueue.TryDequeue(out _);
                    }
                    SaveQueueToDisk();
                    QueueChanged?.Invoke(PendingCount);
                    SyncSucceeded?.Invoke(response.remainingFuel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OfflineSyncManager] Error syncing: {ex.Message}");
            }
            finally
            {
                _syncSemaphore.Release();
            }
        }

        private void SaveQueueToDisk()
        {
            lock (_fileLock)
            {
                try
                {
                    var items = _pendingQueue.ToList();
                    var json = JsonConvert.SerializeObject(items, Formatting.Indented);
                    File.WriteAllText(_cachePath, json, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[OfflineSyncManager] Error writing cache to disk: {ex.Message}");
                }
            }
        }

        private void LoadQueueFromDisk()
        {
            lock (_fileLock)
            {
                try
                {
                    if (File.Exists(_cachePath))
                    {
                        var json = File.ReadAllText(_cachePath, Encoding.UTF8);
                        var items = JsonConvert.DeserializeObject<List<ApiService.SyncResult>>(json);
                        if (items != null)
                        {
                            while (!_pendingQueue.IsEmpty)
                            {
                                _pendingQueue.TryDequeue(out _);
                            }
                            foreach (var item in items)
                            {
                                _pendingQueue.Enqueue(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[OfflineSyncManager] Error reading cache from disk: {ex.Message}");
                }
            }
        }
    }
}
