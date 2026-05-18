using Dyagnoz_Latest.Services;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Text;
using System.Windows.Media;
using System.Collections.Generic;
using materialDesign = MaterialDesignThemes.Wpf;
using System.Xml;
using DevExpress.XtraReports.UI;

namespace Dyagnoz_Latest
{
    public partial class DeviceCard : UserControl
    {
        // data: UDID + device location + port number.
        public string DeviceId { get; private set; } = string.Empty;
        public string DeviceLocationPath { get; private set; } = string.Empty;
        public int PortNumber { get; private set; }

        // Parsed device info from go-ios (only fields we care about).
        public string ProductType { get; set; } = string.Empty;
        public string DeviceEnclosureColor { get; set; } = string.Empty;
        public string TotalDiskCapacity { get; set; } = string.Empty;
        public string SIMStatus { get; set; } = string.Empty;
        public string SIMTrayStatus { get; set; } = string.Empty;
        public string RegionInfo { get; set; } = string.Empty;
        public string ModelNumber { get; set; } = string.Empty;
        public string InternationalMobileEquipmentIdentity { get; set; } = string.Empty;
        public string InternationalMobileEquipmentIdentity2 { get; set; } = string.Empty;
        public string BasebandStatus { get; set; } = string.Empty;
        public string ActivationState { get; set; } = string.Empty;
        public string ActivationStateAcknowledged { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string SetupDone { get; set; } = string.Empty;
        public string ProductVersion { get; set; } = string.Empty;
        public int? BatteryHealth { get; set; }
        public int? BatteryCycleCount { get; set; }
        public string FaceIdStatus { get; set; } = "—";
        public string LcdStatus { get; set; } = "—";
        public string BatteryStatus { get; set; } = "—";
        public string CameraStatus { get; set; } = "—";
        public string MdmStatus { get; set; } = "—";
        public string ICloudStatus { get; set; } = "—";
        public string FmiStatus { get; set; } = "—";
        public Dictionary<string, string> SyslogTestResults { get; set; } = new();
        public List<string> DeviceComments { get; set; } = new();


        private const string VPP_API_URL = "https://vpp.itunes.apple.com/mdm/manageVPPLicensesByAdamIdSrv";
        private const string VPP_ADAM_ID = "1547404030";
        private const string VPP_PRICING_PARAM = "STDQ";
        private const string VPP_STOKEN = "eyJleHBEYXRlIjoiMjAyNy0wMi0wMlQwODo0Njo1NiswMDAwIiwidG9rZW4iOiJRMDlxUGZKT3NSdmRUdDVLamFaMkRFZnBUOTdTRUgvMFJyOEVwVGpDTUdSUEVjZUw2b0RHOHJBNkZUQ3h4UlhyQTFCWm1TUWpHbys5ZzBVdzZCQnk3dE1nUUJya3B6L1dUdlRqbHIrbmZpMit5L2pIeWg4ZDhTU0pDcWFKMGpTOXdCaGMzNnQ3NGdpQ0t6eUgwOHJnTE9wZnZlblpGV2E4eVNNVVVUU2k2MEhXSVVzbVloQ0FzcDNVMHVQQkFOUDUiLCJvcmdOYW1lIjoiRFIgRk9ORVMgRlpDTyJ9";

        private readonly iOSCommander _iosCommander = new iOSCommander();
        private CancellationTokenSource? _pipelineCts;
        private string _lastKnownDeviceId = string.Empty;
        private bool _isSaved = false; // Track if we've already saved this session

        // Selection support for dashboard actions.
        public bool IsSelected
        {
            get => DeviceCheckbox.IsChecked ?? false;
            set => DeviceCheckbox.IsChecked = value;
        }

        public event EventHandler? OnSelectionChanged;

        public DeviceCard()
        {
            InitializeComponent();

            // Raise selection change whenever the checkbox is toggled.
            DeviceCheckbox.Checked += (_, _) => OnSelectionChanged?.Invoke(this, EventArgs.Empty);
            DeviceCheckbox.Unchecked += (_, _) => OnSelectionChanged?.Invoke(this, EventArgs.Empty);

            SetControlsEnabled(true);
        }

        public void setDevice(string deviceId, string locationPath, int portNumber)
        {
            // CRITICAL FIX: If we had a different device before, clean up its processes first
            if (!string.IsNullOrEmpty(_lastKnownDeviceId) && _lastKnownDeviceId != deviceId)
            {
                Debug.WriteLine($"[Port {portNumber}] Device SWAP detected: {_lastKnownDeviceId} → {deviceId}");
                iOSCommander.StopProcessFor(_lastKnownDeviceId);
                System.Threading.Thread.Sleep(150); // Give process cleanup time to complete
            }

            // Cancel any existing work for this card/slot.
            CancelPipeline();

            MainCardBorder.Background = (SolidColorBrush)FindResource("CardBg");
            
            _lastKnownDeviceId = deviceId; // Track for next potential swap
            DeviceId = deviceId;
            DeviceLocationPath = locationPath;
            PortNumber = portNumber;
            _isSaved = false; // Reset for new device
            
            Debug.WriteLine($"Device {DeviceId} Connected on Port {PortNumber}");

            DeviceNameText.Text = deviceId;
            portNumberText.Text = $"Port: {portNumber}";

            // Per-card pipeline (keeps concurrent devices isolated).
            SetControlsEnabled(false);

            _pipelineCts = new CancellationTokenSource();
            _ = ProcessDevicePipelineAsync(DeviceId, DeviceLocationPath, PortNumber, _pipelineCts.Token);

        }

        public void ClearDevice()
        {
            string udidToCleanup = _lastKnownDeviceId; // Capture before clearing
            
            Debug.WriteLine($"Device {udidToCleanup} Disconnected from Port {PortNumber}");

            // CRITICAL FIX: Stop processes for the device that's being cleared
            if (!string.IsNullOrEmpty(udidToCleanup))
            {
                iOSCommander.StopProcessFor(udidToCleanup);
                System.Threading.Thread.Sleep(100); // Let process cleanup complete
            }

            CancelPipeline();

            // 1. Reset backing properties
            _lastKnownDeviceId = string.Empty;
            DeviceId = string.Empty;
            DeviceLocationPath = string.Empty;
            ProductType = string.Empty;
            DeviceEnclosureColor = string.Empty;
            TotalDiskCapacity = string.Empty;
            SIMStatus = string.Empty;
            SIMTrayStatus = string.Empty;
            RegionInfo = string.Empty;
            ModelNumber = string.Empty;
            InternationalMobileEquipmentIdentity = string.Empty;
            InternationalMobileEquipmentIdentity2 = string.Empty;
            BasebandStatus = string.Empty;
            ActivationState = string.Empty;
            ActivationStateAcknowledged = string.Empty;
            SerialNumber = string.Empty;
            SetupDone = string.Empty;
            ProductVersion = string.Empty;
            BatteryHealth = null;
            BatteryCycleCount = null;
            FaceIdStatus = "—";
            LcdStatus = "—";
            BatteryStatus = "—";
            CameraStatus = "—";
            MdmStatus = "—";
            ICloudStatus = "—";
            FmiStatus = "—";
            SyslogTestResults.Clear();
            DeviceComments.Clear();


            MainCardBorder.Background = (SolidColorBrush)FindResource("CardBg");

            // 2. Reset UI elements on UI thread
            Dispatcher.InvokeAsync(() =>
            {
                DeviceNameText.Text = string.Empty;
                DeviceCheckbox.IsChecked = false;
                ModelText.Text = string.Empty;
                SerialText.Text = string.Empty;
                RegionText.Text = string.Empty;
                ColorText.Text = string.Empty;
                ColorDot.Fill = Brushes.Transparent;
                BatteryPercentText.Text = string.Empty;
                BatteryCycleText.Text = string.Empty;
                Imei1Text.Text = string.Empty;
                Imei2Text.Text = string.Empty;
                IosVersionText.Text = string.Empty;

                IcloudText.Text = "—";
                IcloudBadge.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                IcloudText.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9CA3AF"));

                FmiText.Text = "—";
                FmiBadge.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                FmiText.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9CA3AF"));

                MdmText.Text = "—";
                MdmBadge.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                MdmText.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9CA3AF"));

                SimlockText.Text = "—";
                SimlockBadge.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                SimlockText.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9CA3AF"));

                StatusText.Text = "Ready";
                StatusBadge.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981"));

                MainCardBorder.Background = (SolidColorBrush)FindResource("CardBg");
                CardCommentsText.Text = string.Empty;
                CardCommentsText.Visibility = Visibility.Collapsed;
                this.Foreground = (SolidColorBrush)FindResource("TextPrimary");

                var grayBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                var mutedBrush = (SolidColorBrush)FindResource("TextMuted");

                void ResetIcon(Border b)
                {
                    if (b == null) return;
                    b.Background = grayBrush;
                    if (b.Child is materialDesign.PackIcon icon) icon.Foreground = mutedBrush;
                }

                ResetIcon(ValFaceId);
                ResetIcon(ValScreen);
                ResetIcon(ValBattery);
                ResetIcon(ValCamera);
                ResetIcon(ValNfc);
            });

            SetControlsEnabled(false);
        }

        private void SetControlsEnabled(bool enabled)
        {
            Dispatcher.InvokeAsync(() =>
            {
                ViewBtn.IsEnabled = enabled;
                CommentBtn.IsEnabled = enabled;
                WifiBtn.IsEnabled = enabled;
                AppBtn.IsEnabled = enabled;
                RebootBtn.IsEnabled = enabled;
                ShutdownBtn.IsEnabled = enabled;
                WipeBtn.IsEnabled = enabled;
                PrintBtn.IsEnabled = enabled;
            });
        }

        private void CancelPipeline()
        {
            try { _pipelineCts?.Cancel(); } catch { }
            try { _pipelineCts?.Dispose(); } catch { }
            _pipelineCts = null;
        }

        private enum StepOutcome
        {
            Success,
            Retry,
            Fail
        }

        private async Task ProcessDevicePipelineAsync(string udid, string locationPath, int port, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(udid) || udid.Equals("Unknown UDID", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"[Port {port}] UDID missing, skipping pipeline.");
                    return;
                }

                // Step 1: Pair (with retries)
                var pairOutcome = await RunWithRetryAsync(
                    stepName: "Pairing",
                    maxAttempts: 100,
                    retryDelayMs: 1300,
                    step: (token) => PairStepAsync(udid, token),
                    ct: ct);
                if (pairOutcome != StepOutcome.Success)
                {
                    Debug.WriteLine($"[Port {port}] Pairing did not succeed (outcome: {pairOutcome}). Stopping pipeline.");
                    return;
                }

                // Step 2: Get Device Info (with retries)
                var deviceInfo = await RunWithRetryAsync(
                    stepName: "Info",
                    maxAttempts: 3,
                    retryDelayMs: 5000,
                    step: (token) => GetDeviceInfoStepAsync(udid, token),
                    ct: ct);
                if (deviceInfo != StepOutcome.Success)
                {
                    Debug.WriteLine($"[Port {port}] GetDeviceInfo did not succeed (outcome: {deviceInfo}). Stopping pipeline.");
                    return;
                }

                // Info parsed successfully; apply friendly name/color to UI on the UI thread.
                await Dispatcher.InvokeAsync(ApplyDeviceInfoToUi);

                var s = SettingsManager.Current;

                // Step 3: Check activation state and setup status
                bool isActivated = string.Equals(ActivationState, "Activated", StringComparison.OrdinalIgnoreCase);
                bool setupDone = string.Equals(SetupDone, "true", StringComparison.OrdinalIgnoreCase);
                if (!isActivated)
                {
                    Debug.WriteLine($"[Port {port}] Device not activated. Running activation...");
                    var activationOutcome = await RunWithRetryAsync("Activation", 3, 2000, (token) => ActivateDeviceStepAsync(udid, token), ct);
                    if (activationOutcome == StepOutcome.Success)
                    {
                        await Task.Delay(2000, ct).ConfigureAwait(false);
                        await RunWithRetryAsync("Setup", 4, 2000, (token) => SkipSetupStepAsync(udid, token), ct);
                    }
                }
                else if (isActivated && !setupDone)
                {
                    await RunWithRetryAsync("Setup", 4, 2000, (token) => SkipSetupStepAsync(udid, token), ct);
                }

                if (s.FullTest)
                {
                    // Step 4: MDM
                    await RunWithRetryAsync("MDM", 1, 0, (token) => GetDeviceMDMStatusAsync(udid, token), ct);
                    await Dispatcher.InvokeAsync(ApplyDeviceInfoToUi);

                    // Step 5: Battery
                    await RunWithRetryAsync("Bat + Cycle", 2, 2000, (token) => GetBatteryInfoStepAsync(udid, token), ct);

                    // Step 6: VPP
                    if (!string.IsNullOrEmpty(SerialNumber))
                        await RunWithRetryAsync("Scanning", 2, 3000, (token) => AssignVPPStepAsync(SerialNumber, token), ct);

                    // Step 7/8/9: Diagnostics
                    await RunWithRetryAsync("Kernel", 2, 2000, (token) => GetPearlInfoStepAsync(udid, token), ct);
                    await RunWithRetryAsync("Kernel", 2, 2000, (token) => GetLcdInfoStepAsync(udid, token), ct);
                    await RunWithRetryAsync("Kernel", 2, 2000, (token) => GetOemRInfoStepAsync(udid, token), ct);
                    await Dispatcher.InvokeAsync(ApplyDeviceInfoToUi);

                    // Step 10: WiFi
                    await RunWithRetryAsync("WiFi", 1, 0, (token) => PushWifiStepAsync(udid, token), ct);

                    // Step 11: App Installation
                    var appInstalledOutcome = await RunWithRetryAsync("Checking App", 1, 0, (token) => CheckIfAppInstalledStepAsync(udid, token), ct);
                    if (appInstalledOutcome != StepOutcome.Success)
                    {
                        await RunWithRetryAsync("Installing App", 1, 0, (token) => EnsureAppInstallationStepAsync(udid, token), ct);
                    }

                    // Step 12: App Config
                    await RunWithRetryAsync("App Config", 2, 100, (token) => PushTestConfigurationProfileStepAsync(udid, token), ct);
                }

                SetControlsEnabled(true);

                if (!s.FullTest)
                {
                    await Dispatcher.InvokeAsync(async () =>
                    {
                        StatusText.Text = "Finished";
                        // NO PRINT for activation-only flow
                        if (s.AutoWipe) await Task.Run(() => _iosCommander.WipeDevice(udid));
                        else if (s.AutoShutdown) await Task.Run(() => _iosCommander.ShutdownDevice(udid));
                    });
                    return;
                }


                // Step 13: Syslog Process to catch results
                var syslogOutcome = await RunWithRetryAsync(
                    stepName: "Syncing",
                    maxAttempts: 1,
                    retryDelayMs: 0,
                    step: (token) => HookSysLogStepAsync(udid, token),
                    ct: ct);

                if (syslogOutcome == StepOutcome.Success)
                {
                    // Decoupled Printing: Print immediately after Syslog success
                    await Dispatcher.InvokeAsync(() =>
                    {
                        // CRITICAL FIX: Ensure device wasn't cleared/unplugged resulting in empty data
                        if (s.AutoPrint && !string.IsNullOrEmpty(SerialNumber)) 
                            PrintBtn_Click(null, null);
                    });

                    // Attempt WiFi Removal (Cleanup)
                    var wifiRemovalOutcome = await RunWithRetryAsync(
                        stepName: "Removing Wifi",
                        maxAttempts: 2,
                        retryDelayMs: 2000,
                        step: (token) => RemoveWifiStepAsync(udid, token),
                        ct: ct);

                    // Finalize regardless of WiFi outcome so UI doesn't get stuck
                    await Dispatcher.InvokeAsync(async () =>
                    {
                        StatusText.Text = "Finished";
                        
                        // Execute cleanup actions even if WiFi removal failed
                        if (s.AutoWipe) await Task.Run(() => _iosCommander.WipeDevice(udid));
                        else if (s.AutoShutdown) await Task.Run(() => _iosCommander.ShutdownDevice(udid));
                    });
                    
                    // Final save for full test results
                    SaveDeviceToDatabase();
                    Debug.WriteLine($"[Port {port}] Syslog results captured and saved.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {port}] Pipeline error for {udid}: {ex.Message}");
            }
        }

        private async Task<StepOutcome> RunWithRetryAsync(
            string stepName,
            int maxAttempts,
            int retryDelayMs,
            Func<CancellationToken, Task<StepOutcome>> step,
            CancellationToken ct)
        {
            maxAttempts = Math.Max(1, maxAttempts);
            retryDelayMs = Math.Max(0, retryDelayMs);

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                // UI update must be marshalled to the UI thread.
                await Dispatcher.InvokeAsync(() =>
                {
                    StatusText.Text = $"{stepName}";
                });

                Debug.WriteLine($"[Port {PortNumber}] Step '{stepName}' attempt {attempt}/{maxAttempts}.");
                var outcome = await step(ct).ConfigureAwait(false);

                if (outcome == StepOutcome.Success)
                    return StepOutcome.Success;

                if (outcome == StepOutcome.Fail || attempt == maxAttempts)
                    return outcome;

                // Retry
                if (retryDelayMs > 0)
                    await Task.Delay(retryDelayMs, ct).ConfigureAwait(false);
            }

            return StepOutcome.Fail;
        }

        private async Task<StepOutcome> PairStepAsync(string udid, CancellationToken ct)
        {
            // iOSCommander is synchronous; run off the UI thread.
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.PairDevice(udid);
            }, ct).ConfigureAwait(false);

            if (result == "Paired")
                return StepOutcome.Success;

            if (result == "Error")
                return StepOutcome.Retry;

            return StepOutcome.Fail;
        }

        private async Task<StepOutcome> GetDeviceInfoStepAsync(string udid, CancellationToken ct)
        {
            // iOSCommander is synchronous; run off the UI thread.
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.GetDeviceInfo(udid);
            }, ct).ConfigureAwait(false);

            Debug.WriteLine($"[Port {PortNumber}] info result for {udid}: {result}");

            if (result == "Error")
            {
                // No device info yet – ask pipeline to retry this step.
                return StepOutcome.Retry;
            }

            try
            {
                ParseAndStoreDeviceInfo(result);

                // CRITICAL: Ensure we have all mandatory fields before considering this a success.
                // If incomplete, return Retry so the pipeline logic performs the requested retries.
                if (!AreDetailsComplete())
                {
                    Debug.WriteLine($"[Port {PortNumber}] Device info incomplete for {udid}. Triggering retry...");
                    return StepOutcome.Retry;
                }

                return StepOutcome.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] Failed to parse device info for {udid}: {ex.Message}");
                return StepOutcome.Fail;
            }
        }

        private async Task<StepOutcome> HookSysLogStepAsync(string udid, CancellationToken ct)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                MainCardBorder.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DBEAFE"));
            });

            try
            {
                IosCommandResult iosCommandResult;

                // 20 MINUTE TIMEOUT for human testing (adjustable safety net)
                int maxWaitMs = 20 * 60 * 1000; // 1,200,000 ms = 20 minutes

                using (ct.Register(() => iOSCommander.StopProcessFor(udid)))
                {
                    iosCommandResult = await Task.Run(() =>
                    {
                        ct.ThrowIfCancellationRequested();
                        IosCommandResult result;
                        _iosCommander.HookSysLogProcess(udid, maxWaitMs, out result);
                        return result;
                    }, ct).ConfigureAwait(false);
                }

                if (ct.IsCancellationRequested) return StepOutcome.Fail;

                if (iosCommandResult != null && !string.IsNullOrEmpty(iosCommandResult.Result))
                {
                    if (iosCommandResult.Result.Contains("-DrFonesResultEND"))
                    {
                        iosCommandResult.Result = this.Between(iosCommandResult.Result, "DrFonesResultSTART-", "-DrFonesResultEND");
                        iosCommandResult.Result = iosCommandResult.Result.Replace("[", "{").Replace("]", "}");
                        Debug.WriteLine($"[Port {PortNumber}] Syslog Result: {iosCommandResult.Result}");


                        try
                        {
                            var results = JsonSerializer.Deserialize<Dictionary<string, string>>(iosCommandResult.Result);
                            if (results != null)
                            {
                                foreach (var kv in results)
                                {
                                    SyslogTestResults[kv.Key] = kv.Value;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[Port {PortNumber}] Failed to parse Syslog Result JSON: {ex.Message}");
                        }
                    }

                    await Dispatcher.InvokeAsync(UpdatePassRateStatus);
                    return StepOutcome.Success;
                }


                return StepOutcome.Fail;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"[Port {PortNumber}] Syslog canceled for {udid}.");
                return StepOutcome.Fail;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] Syslog Error: {ex.Message}");
                return StepOutcome.Fail;
            }
        }

        private async Task<StepOutcome> PushWifiStepAsync(string udid, CancellationToken ct)
        {
            try
            {
                string configPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dyagnoz", "wifi-sys.mobileconfig");
                if (!System.IO.File.Exists(configPath))
                {
                    Debug.WriteLine($"[Port {PortNumber}] WiFi config file missing at: {configPath}");
                    return StepOutcome.Success; // Skip if not found
                }

                var result = await Task.Run(() =>
                {
                    ct.ThrowIfCancellationRequested();
                    return _iosCommander.InstallProfile(udid, configPath);
                }, ct).ConfigureAwait(false);

                Debug.WriteLine($"[Port {PortNumber}] WiFi Push Result: {result}");
                return StepOutcome.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] WiFi Push Error: {ex.Message}");
                return StepOutcome.Fail;
            }
        }

        private async Task<StepOutcome> GetBatteryInfoStepAsync(string udid, CancellationToken ct)
        {
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.GetBatteryInfo(udid);
            }, ct).ConfigureAwait(false);

            if (result == "Error")
            {
                return StepOutcome.Retry;
            }

            try
            {
                ParseAndStoreBatteryInfo(result);
                return StepOutcome.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] Failed to parse battery info for {udid}: {ex.Message}");
                return StepOutcome.Fail;
            }
        }

        private async Task<StepOutcome> GetOemRInfoStepAsync(string udid, CancellationToken ct)
        {
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.GetOemRData(udid);
            }, ct).ConfigureAwait(false);

            Debug.WriteLine($"[Port {PortNumber}] OemR result for {udid}");

            if (result == "Error")
            {
                return StepOutcome.Retry;
            }

            try
            {
                ParseAndStoreOemRInfo(result);
                return StepOutcome.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] Failed to parse OemR for {udid}: {ex.Message}");
                return StepOutcome.Fail;
            }
        }

        private void ParseAndStoreOemRInfo(string rawOemR)
        {
            if (string.IsNullOrWhiteSpace(rawOemR)) return;

            try
            {
                using var doc = JsonDocument.Parse(rawOemR);
                var root = doc.RootElement;
                if (root.TryGetProperty("PartHistory", out var history))
                {
                    if (history.TryGetProperty("Battery", out var batVal))
                    {
                        bool original = IsPartOriginal(batVal.GetString());
                        if (original && BatteryHealth.HasValue && BatteryHealth.Value < 80)
                        {
                            BatteryStatus = "Fail";
                        }
                        else
                        {
                            BatteryStatus = original ? "Pass" : "Fail";
                        }
                    }

                    if (history.TryGetProperty("Camera", out var camVal))
                        CameraStatus = IsPartOriginal(camVal.GetString()) ? "Pass" : "Fail";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] OemR Parse Issue: {ex.Message}");
            }
        }

        private bool IsPartOriginal(string? val)
        {
            if (string.IsNullOrWhiteSpace(val)) return true;
            var lower = val.ToLower();
            if (lower.Contains("unknown") || lower.Contains("non-genuine") || lower.Contains("not")) return false;
            return lower.Contains("original") || lower.Contains("orignal") || lower.Contains("genuine") || lower.Contains("authorized");
        }

        private async Task<StepOutcome> GetLcdInfoStepAsync(string udid, CancellationToken ct)
        {
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.GetLcdData(udid);
            }, ct).ConfigureAwait(false);

            Debug.WriteLine($"[Port {PortNumber}] LCD info result for {udid}");

            if (result == "Error")
            {
                return StepOutcome.Retry;
            }

            try
            {
                LcdStatus = ValidateLcdData(result);
                return StepOutcome.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] Failed to parse LCD info for {udid}: {ex.Message}");
                return StepOutcome.Fail;
            }
        }

        private string ValidateLcdData(string data)
        {
            try
            {
                if (data.Contains("raw-panel-serial-number"))
                {
                    string s = Between(data, "<key>raw-panel-serial-number</key>", "</data>")
                        .Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("<data>", "").Trim();
                    
                    return s.Contains("///////////") ? "Fail" : "Pass";
                }
            }
            catch { }
            return "Pass"; // Default to Pass if serial not found or parsing fails, but check required.
        }

        private async Task<StepOutcome> GetPearlInfoStepAsync(string udid, CancellationToken ct)
        {
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.GetPearlData(udid);
            }, ct).ConfigureAwait(false);

            Debug.WriteLine($"[Port {PortNumber}] Pearl info result for {udid}: {result}");

            if (result == "Error")
            {
                return StepOutcome.Retry;
            }

            try
            {
                FaceIdStatus = ValidatePearlData(result);
                return StepOutcome.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] Failed to parse pearl info for {udid}: {ex.Message}");
                return StepOutcome.Fail;
            }
        }

        private string ValidateMDMData(string data)
        {
            if (data.Contains("<key>IsSupervised</key>"))
            {
                int index = data.IndexOf("<key>IsSupervised</key>");
                string sub = data.Substring(index + "<key>IsSupervised</key>".Length);
                int nextTagStart = sub.IndexOf('<');
                if (nextTagStart != -1)
                {
                    int nextTagEnd = sub.IndexOf('>', nextTagStart);
                    if (nextTagEnd != -1)
                    {
                        string tag = sub.Substring(nextTagStart, nextTagEnd - nextTagStart + 1);
                        if (tag.Contains("true"))
                            return "ON";
                    }
                }
            }
            return "OFF";
        }

        private string ValidatePearlData(string data)
        {
            if (data.Contains("Unable to retrieve IORegistry from device"))
                return "Fail";

            string str1 = Between(data, "PearlSelfTestResult", "</integer>");

            if (str1 != string.Empty && str1.Replace("</key>", "").Replace("<integer>", "").Trim() != "0")
                return "Fail";

            return Between(data, "FrontIRCameraModuleSerialNumString", "</string>").Replace("<string>", "").Replace("</key>", "").Trim() == "00000000000000000" ? "Fail" : "Pass";
        }

        private string Between(string src, string findfrom, string findto)
        {
            try
            {
                int num1 = src.IndexOf(findfrom, StringComparison.Ordinal);
                if (num1 < 0) return string.Empty;
                int num2 = src.IndexOf(findto, num1 + findfrom.Length, StringComparison.Ordinal);
                return num2 < 0 ? string.Empty : src.Substring(num1 + findfrom.Length, num2 - num1 - findfrom.Length);
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<StepOutcome> AssignVPPStepAsync(string serial, CancellationToken ct)
        {
            try
            {
                Debug.WriteLine($"[VPP] Starting License Assignment for Serial: {serial}");
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                var requestBody = new
                {
                    associateSerialNumbers = serial,
                    adamIdStr = VPP_ADAM_ID,
                    pricingParam = VPP_PRICING_PARAM,
                    notifyDisassociation = true,
                    sToken = VPP_STOKEN
                };
                string json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "text/plain");
                var request = new HttpRequestMessage(HttpMethod.Post, VPP_API_URL) { Content = content };
                request.Headers.Add("Cookie", "POD=us~en");

                var response = await client.SendAsync(request, ct).ConfigureAwait(false);
                var respStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Debug.WriteLine($"[VPP] Response Code: {response.StatusCode}");
                Debug.WriteLine($"[VPP] Response Body: {respStr}");

                if (response.IsSuccessStatusCode) return StepOutcome.Success;
                return StepOutcome.Retry;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] VPP Exception: {ex.Message}");
                return StepOutcome.Retry;
            }
        }

        private async Task<StepOutcome> ActivateDeviceStepAsync(string udid, CancellationToken ct)
        {
            // iOSCommander is synchronous; run off the UI thread.
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.ActivateDevice(udid);
            }, ct).ConfigureAwait(false);

            Debug.WriteLine($"[Port {PortNumber}] Activation result for {udid}: {result}");

            if (result == "Activated")
                return StepOutcome.Success;

            // If activation fails, retry (might be temporary network/server issue)
            return StepOutcome.Retry;
        }

        private async Task<StepOutcome> SkipSetupStepAsync(string udid, CancellationToken ct)
        {
            // iOSCommander is synchronous; run off the UI thread.
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.SkipSetup(udid);
            }, ct).ConfigureAwait(false);


            if (result == "Skipped")
                return StepOutcome.Success;

            // If skip setup fails, retry (device might still be processing activation)
            return StepOutcome.Retry;
        }

        private async Task<StepOutcome> CheckIfAppInstalledStepAsync(string udid, CancellationToken ct)
        {
            // iOSCommander is synchronous; run off the UI thread.
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.CheckIfAppInstalled(udid);
            }, ct).ConfigureAwait(false);

            Debug.WriteLine($"[Port {PortNumber}] CheckIfAppInstalled result for {udid}: {result}");

            if (result == "Pass")
                return StepOutcome.Success;

            // If check fails, retry (device might still be processing activation)
            return StepOutcome.Retry;
        }

        private async Task<StepOutcome> GetDeviceMDMStatusAsync(string udid, CancellationToken ct)
        {
            // iOSCommander is synchronous; run off the UI thread.
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.GetDeviceMDMStatus(udid);
            }, ct).ConfigureAwait(false);

            if(result == "Error")
            {
                return StepOutcome.Retry;
            }

            try
            {
                MdmStatus = ValidateMDMData(result);
                return StepOutcome.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] Failed to parse MDM for {udid}: {ex.Message}");
                return StepOutcome.Fail;
            }
        }

        private async Task<StepOutcome> EnsureAppInstallationStepAsync(string udid, CancellationToken ct)
        {
            // iOSCommander is synchronous; run off the UI thread.
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.EnsureAppInstallation(udid);
            }, ct).ConfigureAwait(false);

            Debug.WriteLine($"[Port {PortNumber}] EnsureAppInstallation result for {udid}: {result}");

            if (result == "Pass")
                return StepOutcome.Success;

            // If check fails, retry (device might still be processing activation)
            return StepOutcome.Retry;
        }

        private async Task<StepOutcome> PushTestConfigurationProfileStepAsync(string udid, CancellationToken ct)
        {
            //We will wait 5-7 seconds before pushing the test configuration profile to give the device enough time to settle
            await Task.Delay(2200, ct).ConfigureAwait(false);
            // iOSCommander is synchronous; run off the UI thread.
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.PushTestConfigurationProfileAsync(udid);
            }, ct).ConfigureAwait(false);

            Debug.WriteLine($"[Port {PortNumber}] PushTestConfigurationProfile result for {udid}: {result}");

            if (result == "Pass")
                return StepOutcome.Success;

            // If check fails, retry (device might still be processing activation)
            return StepOutcome.Retry;
        }

        /// <summary>
        /// Parses the JSON object returned by go-ios and fills
        /// </summary>
        private void ParseAndStoreDeviceInfo(string rawInfo)
        {
            if (string.IsNullOrWhiteSpace(rawInfo))
                return;

            using var doc = JsonDocument.Parse(rawInfo);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return;

            string GetString(string name)
            {
                if (!root.TryGetProperty(name, out var prop))
                    return string.Empty;

                return prop.ValueKind switch
                {
                    JsonValueKind.String => prop.GetString() ?? string.Empty,
                    JsonValueKind.Number => prop.GetRawText(),
                    JsonValueKind.True or JsonValueKind.False => prop.GetBoolean().ToString(),
                    _ => prop.GetRawText()
                };
            }

            ProductType = GetString("ProductType");
            DeviceEnclosureColor = GetString("DeviceEnclosureColor");
            TotalDiskCapacity = GetString("TotalDiskCapacity");
            SIMStatus = GetString("SIMStatus");
            SIMTrayStatus = GetString("SIMTrayStatus");
            RegionInfo = GetString("RegionInfo");
            ModelNumber = GetString("ModelNumber");
            InternationalMobileEquipmentIdentity = GetString("InternationalMobileEquipmentIdentity");
            InternationalMobileEquipmentIdentity2 = GetString("InternationalMobileEquipmentIdentity2");
            BasebandStatus = GetString("BasebandStatus");
            ActivationState = GetString("ActivationState");
            ActivationStateAcknowledged = GetString("ActivationStateAcknowledged");
            SerialNumber = GetString("SerialNumber");
            SetupDone = GetString("SetupDone");
            ProductVersion = GetString("ProductVersion");
            string fmip = GetString("FMIP");
            if (!string.IsNullOrEmpty(fmip))
            {
                FmiStatus = fmip.Equals("true", StringComparison.OrdinalIgnoreCase) ? "ON" : "OFF";
                ICloudStatus = FmiStatus;
            }
        }

    private bool AreDetailsComplete()
    {
        return !string.IsNullOrWhiteSpace(SerialNumber) &&
               !string.IsNullOrWhiteSpace(InternationalMobileEquipmentIdentity) &&
               !string.IsNullOrWhiteSpace(ProductType) &&
               !string.IsNullOrWhiteSpace(ProductVersion) &&
               !string.IsNullOrWhiteSpace(TotalDiskCapacity) &&
               !string.IsNullOrWhiteSpace(DeviceEnclosureColor);
    }

        private void ParseAndStoreBatteryInfo(string rawBatteryInfo)
        {
            if (string.IsNullOrWhiteSpace(rawBatteryInfo))
                return;

            try
            {
                int designCap = ExtractIntFromXml(rawBatteryInfo, "DesignCapacity");
                int currentCap = ExtractIntFromXml(rawBatteryInfo, "NominalChargeCapacity");
                int cycleCount = ExtractIntFromXml(rawBatteryInfo, "CycleCount");

                if (designCap > 0)
                {
                    int healthInt = (int)((double)currentCap / (double)designCap * 100.0);
                    if (healthInt > 100)
                        healthInt = 100;
                    else if (healthInt < 0)
                        healthInt = 0;

                    BatteryHealth = healthInt;
                    BatteryCycleCount = cycleCount;

                    if (healthInt < 80)
                    {
                        BatteryStatus = "Fail";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] Error calculating battery details: {ex.Message}");
            }
        }

        private int ExtractIntFromXml(string xml, string key)
        {
            string pattern = $"<key>{key}</key>";
            int keyIndex = xml.IndexOf(pattern);
            if (keyIndex == -1) return 0;

            int integerStart = xml.IndexOf("<integer>", keyIndex);
            if (integerStart == -1) return 0;

            int integerEnd = xml.IndexOf("</integer>", integerStart);
            if (integerEnd == -1) return 0;

            string valueStr = xml.Substring(integerStart + 9, integerEnd - integerStart - 9);
            if (int.TryParse(valueStr, out int result))
                return result;

            return 0;
        }

        private async Task<StepOutcome> RemoveWifiStepAsync(string udid, CancellationToken ct)
        {
            // iOSCommander is synchronous; run off the UI thread.
            var result = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _iosCommander.RemoveWifi(udid);
            }, ct).ConfigureAwait(false);

            if (result == "Pass")
                return StepOutcome.Success;

            if (result == "Fail")
                return StepOutcome.Fail;

            return StepOutcome.Retry;
        }

        /// <summary>
        /// Push parsed device info into the existing UI.
        /// </summary>
        private void ApplyDeviceInfoToUi()
        {
            // Device name (header) + storage
            try
            {
                var friendlyName = Dyagnoz.Models.DeviceModelMap.GetShortDeviceName(ProductType);
                var storageLabel = GetRoundedStorageLabel(TotalDiskCapacity);
                
                if (!string.IsNullOrWhiteSpace(friendlyName))
                {
                    if (!string.IsNullOrWhiteSpace(storageLabel))
                        DeviceNameText.Text = $"{friendlyName} · {storageLabel}";
                    else
                        DeviceNameText.Text = friendlyName;
                }
            }
            catch
            {
                // Keep existing text (likely UDID) if mapping fails.
            }

            // Model = ModelNumber + RegionInfo (+ storage tier)
            try
            {
                var parts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(ModelNumber)) parts.Add(ModelNumber);
                if (!string.IsNullOrWhiteSpace(RegionInfo)) parts.Add(RegionInfo);

                if (parts.Count > 0)
                    ModelText.Text = string.Join("", parts);
            }
            catch { }

            // Serial number
            if (!string.IsNullOrWhiteSpace(SerialNumber))
                SerialText.Text = SerialNumber;

            // Region
            if (!string.IsNullOrWhiteSpace(RegionInfo))
                RegionText.Text = RegionInfo;

            // Color (marketing name) based on ProductType + enclosure code
            try
            {
                var colorName = Dyagnoz.Models.DeviceColorMap.GetColorName(ProductType, DeviceEnclosureColor);
                if (!string.IsNullOrWhiteSpace(colorName))
                    ColorText.Text = colorName;

                var hex = Dyagnoz.Models.DeviceColorMap.GetColorHex(ProductType, DeviceEnclosureColor);
                if (!string.IsNullOrWhiteSpace(hex) && hex != "Transparent")
                {
                    try
                    {
                        ColorDot.Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex));
                    }
                    catch { ColorDot.Fill = System.Windows.Media.Brushes.Transparent; }
                }
                else
                {
                    ColorDot.Fill = System.Windows.Media.Brushes.Transparent;
                }
            }
            catch { }


            // IMEIs
            if (!string.IsNullOrWhiteSpace(InternationalMobileEquipmentIdentity))
                Imei1Text.Text = InternationalMobileEquipmentIdentity;
            if (!string.IsNullOrWhiteSpace(InternationalMobileEquipmentIdentity2))
                Imei2Text.Text = InternationalMobileEquipmentIdentity2;

            String simInfo = iPhone14AndAbove(ProductType) ? "ESIM" : "Normal";
            if (!string.IsNullOrWhiteSpace(SIMStatus))
                SimlockText.Text = simInfo;

            if (!string.IsNullOrWhiteSpace(ProductVersion))
                IosVersionText.Text = $"iOS {ProductVersion}";

            // Battery info
            if (BatteryHealth.HasValue)
                BatteryPercentText.Text = $"{BatteryHealth}%";
            if (BatteryCycleCount.HasValue)
                BatteryCycleText.Text = BatteryCycleCount.ToString();

            // Baseband status: treat BBInfoAvailable as PASS
            if (!string.IsNullOrWhiteSpace(BasebandStatus) &&
                string.Equals(BasebandStatus, "BBInfoAvailable", StringComparison.OrdinalIgnoreCase))
            {
                StatusText.Text = "PASS";
            }

            // FaceID Info
            var faceIdIcon = ValFaceId.Child as materialDesign.PackIcon;
            if (FaceIdStatus == "Pass")
            {
                ValFaceId.Background = (SolidColorBrush)FindResource("StatusOk");
                if (faceIdIcon != null) faceIdIcon.Foreground = System.Windows.Media.Brushes.White;
            }
            else if (FaceIdStatus == "Fail")
            {
                ValFaceId.Background = (SolidColorBrush)FindResource("StatusBad");
                if (faceIdIcon != null) faceIdIcon.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                ValFaceId.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                if (faceIdIcon != null) faceIdIcon.Foreground = (SolidColorBrush)FindResource("TextMuted");
            }

            // LCD Info
            var lcdIcon = ValScreen.Child as materialDesign.PackIcon;
            if (LcdStatus == "Pass")
            {
                ValScreen.Background = (SolidColorBrush)FindResource("StatusOk");
                if (lcdIcon != null) lcdIcon.Foreground = System.Windows.Media.Brushes.White;
            }
            else if (LcdStatus == "Fail")
            {
                ValScreen.Background = (SolidColorBrush)FindResource("StatusBad");
                if (lcdIcon != null) lcdIcon.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                ValScreen.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                if (lcdIcon != null) lcdIcon.Foreground = (SolidColorBrush)FindResource("TextMuted");
            }

            // Battery Info
            var valBatteryIcon = ValBattery.Child as materialDesign.PackIcon;
            if (BatteryStatus == "Pass")
            {
                ValBattery.Background = (SolidColorBrush)FindResource("StatusOk");
                if (valBatteryIcon != null) valBatteryIcon.Foreground = System.Windows.Media.Brushes.White;
            }
            else if (BatteryStatus == "Fail")
            {
                ValBattery.Background = (SolidColorBrush)FindResource("StatusBad");
                if (valBatteryIcon != null) valBatteryIcon.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                ValBattery.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                if (valBatteryIcon != null) valBatteryIcon.Foreground = (SolidColorBrush)FindResource("TextMuted");
            }

            // Camera Info
            var valCameraIcon = ValCamera.Child as materialDesign.PackIcon;
            if (CameraStatus == "Pass")
            {
                ValCamera.Background = (SolidColorBrush)FindResource("StatusOk");
                if (valCameraIcon != null) valCameraIcon.Foreground = System.Windows.Media.Brushes.White;
            }
            else if (CameraStatus == "Fail")
            {
                ValCamera.Background = (SolidColorBrush)FindResource("StatusBad");
                if (valCameraIcon != null) valCameraIcon.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                ValCamera.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                if (valCameraIcon != null) valCameraIcon.Foreground = (SolidColorBrush)FindResource("TextMuted");
            }

            // MDM Status
            if (MdmStatus == "OFF")
            {
                MdmText.Text = "OFF";
                MdmBadge.Background = (SolidColorBrush)FindResource("StatusOk");
                MdmText.Foreground = System.Windows.Media.Brushes.White;
            }
            else if (MdmStatus == "ON")
            {
                MdmText.Text = "ON";
                MdmBadge.Background = (SolidColorBrush)FindResource("StatusBad");
                MdmText.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                MdmText.Text = "—";
                MdmBadge.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                MdmText.Foreground = (SolidColorBrush)FindResource("TextMuted");
            }

            // iCloud & FMI Status
            if (ICloudStatus == "OFF")
            {
                IcloudText.Text = "OFF";
                IcloudBadge.Background = (SolidColorBrush)FindResource("StatusOk");
                IcloudText.Foreground = System.Windows.Media.Brushes.White;

                FmiText.Text = "OFF";
                FmiBadge.Background = (SolidColorBrush)FindResource("StatusOk");
                FmiText.Foreground = System.Windows.Media.Brushes.White;
            }
            else if (ICloudStatus == "ON")
            {
                IcloudText.Text = "ON";
                IcloudBadge.Background = (SolidColorBrush)FindResource("StatusBad");
                IcloudText.Foreground = System.Windows.Media.Brushes.White;

                FmiText.Text = "ON";
                FmiBadge.Background = (SolidColorBrush)FindResource("StatusBad");
                FmiText.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                IcloudText.Text = "—";
                IcloudBadge.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                IcloudText.Foreground = (SolidColorBrush)FindResource("TextMuted");

                FmiText.Text = "—";
                FmiBadge.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                FmiText.Foreground = (SolidColorBrush)FindResource("TextMuted");
            }

            UpdatePassRateStatus();
        }


        private void UpdatePassRateStatus()
        {
            // Only update pass rate if we are far enough in the pipeline (past initial info)
            // or if we already have some test results.
            if (string.IsNullOrEmpty(SerialNumber)) return;

            int passed = 0;
            int total = 0;

            void CheckStatus(string status)
            {
                if (status == "Pass") { passed++; total++; }
                else if (status == "Fail") { total++; }
            }

            CheckStatus(FaceIdStatus);
            CheckStatus(LcdStatus);
            CheckStatus(BatteryStatus);
            CheckStatus(CameraStatus);

            // iCloud/FMI (Counted as one critical test)
            if (ICloudStatus == "OFF" || ICloudStatus == "ON")
            {
                total++;
                if (ICloudStatus == "OFF") passed++;
            }

            // MDM
            if (MdmStatus == "OFF" || MdmStatus == "ON")
            {
                total++;
                if (MdmStatus == "OFF") passed++;
            }

            // Syslog tests
            foreach (var test in SyslogTestResults)
            {
                total++;
                string v = test.Value?.Trim() ?? "";
                if (v == "0" || v.Equals("Pass", StringComparison.OrdinalIgnoreCase) || v.Equals("Yes", StringComparison.OrdinalIgnoreCase)) 
                    passed++;
            }

            if (total == 0) return;

            int failed = total - passed;
            StatusText.Text = failed == 0 ? "PASS" : $"{passed}/{total} PASS";

            if (failed == 0)
            {
                StatusBadge.Background = (SolidColorBrush)FindResource("StatusOk");
                
                // State-based background for 0 failures:
                // 1. If Finished -> Green
                // 2. If currently Syncing/Testing -> Blue (alert user)
                // 3. Otherwise -> Neutral
                if (StatusText.Text == "Finished")
                    MainCardBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DEF7EC"));
                else if (StatusText.Text == "Syncing" || StatusText.Text == "Removing Wifi")
                    MainCardBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
                else
                    MainCardBorder.Background = (SolidColorBrush)FindResource("CardBg");
            }
            else if (failed <= 2)
            {
                StatusBadge.Background = (SolidColorBrush)FindResource("StatusWarn");
                MainCardBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDF6B2")); // Amber
            }
            else
            {
                StatusBadge.Background = (SolidColorBrush)FindResource("StatusBad");
                MainCardBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDE8E8")); // Red
            }
        }


        /// <summary>
        /// Convert raw TotalDiskCapacity (bytes) into a rounded marketing tier label
        /// like "64 GB", "128 GB", "256 GB", "512 GB", "1 TB", "2 TB".
        /// </summary>
        private static string GetRoundedStorageLabel(string totalDiskCapacityRaw)
        {
            if (string.IsNullOrWhiteSpace(totalDiskCapacityRaw))
                return string.Empty;

            if (!double.TryParse(totalDiskCapacityRaw, System.Globalization.NumberStyles.Any,
                                 System.Globalization.CultureInfo.InvariantCulture, out double bytes))
                return string.Empty;

            if (bytes <= 0)
                return string.Empty;

            double gb = bytes / 1_000_000_000d; // marketing GB

            int[] tiers = new[] { 64, 128, 256, 512, 1024, 2048, 4096 };
            int closest = tiers[0];
            double bestDiff = Math.Abs(gb - closest);

            for (int i = 1; i < tiers.Length; i++)
            {
                double diff = Math.Abs(gb - tiers[i]);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    closest = tiers[i];
                }
            }

            if (closest >= 1024)
            {
                int tb = closest / 1024;
                return $"{tb} TB";
            }

            return $"{closest} GB";
        }

        public static bool iPhone14AndAbove(string productType)
        {
            switch (productType)
            {
                case "iPhone14,7":
                case "iPhone14,8":
                case "iPhone15,2":
                case "iPhone15,3":
                case "iPhone15,4":
                case "iPhone15,5":
                case "iPhone16,1":
                case "iPhone16,2":
                case "iPhone17,1":
                case "iPhone17,2":
                case "iPhone17,3":
                case "iPhone17,4":
                    return true;
                default:
                    return false;
            }
        }

        //<zone> Event handlers for UI interactions (buttons, header click for selection, etc.)
        private void Header_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DeviceCheckbox.IsChecked = !DeviceCheckbox.IsChecked;
            OnSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            // CRITICAL FIX: Guard against printing if device data is cleared (race condition on unplug)
            if (string.IsNullOrEmpty(SerialNumber)) 
                return;

            try
            {
                var friendlyName = Dyagnoz.Models.DeviceModelMap.GetShortDeviceName(ProductType);
                var storageLabel = GetRoundedStorageLabel(TotalDiskCapacity);
                string productTitle = !string.IsNullOrWhiteSpace(friendlyName) ? $"{friendlyName} {storageLabel}" : ProductType;

                var s = Services.SettingsManager.Current;

                string notes = DeviceComments != null ? string.Join(", ", DeviceComments) : "";

                // Parse failed parts based on settings
                if (s.PrintFailedParts)
                {
                    var failed = new System.Collections.Generic.List<string>();

                    // Kernel Tests (Red/Green Icons)
                    if (FaceIdStatus == "Fail" || FaceIdStatus == "Fixed") failed.Add("FaceID MSG");
                    if (LcdStatus == "Fail" || LcdStatus == "Fixed") failed.Add("Display MSG");
                    if (BatteryStatus == "Fail" || BatteryStatus == "Fixed") failed.Add("Battery MSG");
                    if (CameraStatus == "Fail" || CameraStatus == "Fixed") failed.Add("Camera MSG");

                    // Syslog/App Tests
                    if (SyslogTestResults != null)
                    {
                        foreach (var kv in SyslogTestResults)
                        {
                            string val = kv.Value?.Trim() ?? "";
                            // 1 = Failure, 0 = Success in these syslogs
                            if (val.Equals("Fail", StringComparison.OrdinalIgnoreCase) || 
                                val.Equals("No", StringComparison.OrdinalIgnoreCase) ||
                                val == "1") 
                            {
                                string cleanKey = kv.Key.Replace(" Button", "").Replace(" Mic", "Mic");
                                failed.Add(cleanKey); // Just the name, no status message
                            }
                        }
                    }

                    if (failed.Count > 0)
                    {
                        string failedText = string.Join(", ", failed);
                        if (string.IsNullOrEmpty(notes)) notes = failedText;
                        else notes = notes.TrimEnd() + " | " + failedText;
                    }
                }

                // Map Color and SIM Name
                var displayColor = Dyagnoz.Models.DeviceColorMap.GetColorName(ProductType, DeviceEnclosureColor);
                var displaySim = SIMStatus == "kCTSIMSupportSIMStatusReady" ? "Unlocked" : "Locked";

                // Create the label report with merged data
                var report = new HorizontalLabel(
                    InternationalMobileEquipmentIdentity,
                    SerialNumber,
                    ModelText.Text,
                    productTitle,
                    displayColor,
                    ProductVersion,
                    BatteryHealth?.ToString() + "%",
                    ICloudStatus,
                    FmiStatus,
                    MdmStatus,
                    displaySim,
                    PortNumber.ToString("D2"),
                    notes,
                    MainWindow.SelectedCustomer ?? ""
                );
                report.Print();

                Debug.WriteLine($"[Print] Label printed for Serial: {SerialNumber}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing label: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewBtn_Click(object sender, RoutedEventArgs e) 
        {
            try
            {
                var friendlyName = Dyagnoz.Models.DeviceModelMap.GetShortDeviceName(ProductType);
                var storageLabel = GetRoundedStorageLabel(TotalDiskCapacity);
                string deviceName = !string.IsNullOrWhiteSpace(friendlyName) ? $"{friendlyName}" : DeviceId;

                var kernelTests = new Dictionary<string, string>
                {
                    { "FaceID", FaceIdStatus },
                    { "LCD", LcdStatus },
                    { "Battery", BatteryStatus },
                    { "Camera", CameraStatus }
                };

                var device = new ProcessedDevice
                {
                    DeviceName = deviceName,
                    Model = ModelText.Text,
                    Color = ColorText.Text,
                    Storage = storageLabel,
                    Serial = SerialNumber,
                    Imei = InternationalMobileEquipmentIdentity,
                    IcloudStatus = ICloudStatus,
                    FmiStatus = FmiStatus,
                    SimStatus = SimlockText.Text,
                    MdmStatus = MdmStatus,
                    BatteryHealth = BatteryHealth?.ToString(),
                    BatteryCycles = BatteryCycleCount?.ToString(),
                    ProductType = ProductType,
                    EnclosureCode = DeviceEnclosureColor,
                    IosVersion = ProductVersion,
                    Region = RegionInfo,
                    KernelTests = kernelTests,
                    AppTests = SyslogTestResults,
                    Comments = DeviceComments,
                    Customer = MainWindow.SelectedCustomer,
                    DateTime = DateTime.Now
                };

                var detailsWindow = new TestResultsWindow();
                detailsWindow.PopulateFromProcessedDevice(device);
                detailsWindow.Owner = Window.GetWindow(this);
                detailsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[View] Error: {ex.Message}");
            }
        }

        private void CommentBtn_Click(object sender, RoutedEventArgs e)
        {
            var commentWindow = new CommentSelectWindow(DeviceComments);
            commentWindow.Owner = Window.GetWindow(this);
            commentWindow.ShowDialog();

            if (commentWindow.Confirmed)
            {
                DeviceComments = commentWindow.SelectedComments;
                Debug.WriteLine($"[Port {PortNumber}] Comments updated: {string.Join(", ", DeviceComments)}");
                UpdateCommentsUi();
            }
        }

        private void UpdateCommentsUi()
        {
            Dispatcher.Invoke(() =>
            {
                if (DeviceComments != null && DeviceComments.Count > 0)
                {
                    CardCommentsText.Text = string.Join(", ", DeviceComments);
                    CardCommentsText.Visibility = Visibility.Visible;
                }
                else
                {
                    CardCommentsText.Text = string.Empty;
                    CardCommentsText.Visibility = Visibility.Collapsed;
                }
            });
        }

        private async void WifiBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DeviceId)) return;

            string configPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dyagnoz", "wifi-sys.mobileconfig");
            if (!System.IO.File.Exists(configPath))
            {
                MessageBox.Show("WiFi config file not found in ProgramData\\Dyagnoz\\", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                StatusText.Text = "Pushing WiFi...";
                var res = await Task.Run(() => _iosCommander.InstallProfile(DeviceId, configPath));
                Debug.WriteLine($"[Manual WiFi] Result: {res}");
                StatusText.Text = "WiFi Pushed";
            }
            catch (Exception ex)
            {
                MessageBox.Show("WiFi Push Failed: " + ex.Message);
                StatusText.Text = "WiFi Failed";
            }
        }
        private async void RebootBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DeviceId)) return;
            StatusText.Text = "Rebooting...";
            await Task.Run(() => _iosCommander.RebootDevice(DeviceId));
        }

        private async void ShutdownBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DeviceId)) return;
            StatusText.Text = "Shutting Down...";
            await Task.Run(() => _iosCommander.ShutdownDevice(DeviceId));
        }

        private async void WipeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DeviceId)) return;
            if (MessageBox.Show("Are you sure you want to WIPE this device? All data will be lost.", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                StatusText.Text = "Wiping...";
                await Task.Run(() => _iosCommander.WipeDevice(DeviceId));
            }
        }
        private void AppBtn_Click(object sender, RoutedEventArgs e) { }

        private void SaveDeviceToDatabase(bool isManual = false)
        {
            try
            {
                // If this is a manual save (from UI) and we already auto-saved, skip it
                if (isManual && _isSaved)
                {
                    Debug.WriteLine($"[Port {PortNumber}] Skipping manual save as it was already saved.");
                    return;
                }
                var friendlyName = Dyagnoz.Models.DeviceModelMap.GetShortDeviceName(ProductType);
                var storageLabel = GetRoundedStorageLabel(TotalDiskCapacity);
                string deviceName = !string.IsNullOrWhiteSpace(friendlyName) ? $"{friendlyName}" : DeviceId;

                var kernelTests = new Dictionary<string, string>
                {
                    { "FaceID", FaceIdStatus },
                    { "LCD", LcdStatus },
                    { "Battery", BatteryStatus },
                    { "Camera", CameraStatus }
                };

                var device = new ProcessedDevice
                {
                    DeviceName = deviceName,
                    Model = ModelText.Text,
                    Color = ColorText.Text,
                    Storage = storageLabel,
                    Serial = SerialNumber,
                    Imei = InternationalMobileEquipmentIdentity,
                    IcloudStatus = ICloudStatus,
                    FmiStatus = FmiStatus,
                    SimStatus = SimlockText.Text,
                    MdmStatus = MdmStatus,
                    BatteryHealth = BatteryHealth?.ToString(),
                    BatteryCycles = BatteryCycleCount?.ToString(),
                    ProductType = ProductType,
                    EnclosureCode = DeviceEnclosureColor,
                    IosVersion = ProductVersion,
                    Region = RegionInfo,
                    KernelTests = kernelTests,
                    AppTests = SyslogTestResults,
                    Comments = DeviceComments,
                    Customer = MainWindow.SelectedCustomer,
                    DateTime = DateTime.Now
                };

                App.Database.SaveProcessedDevice(device);
                _isSaved = true; // Mark as saved
                Debug.WriteLine($"[Port {PortNumber}] Device saved to database successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Port {PortNumber}] Failed to save device to database: {ex.Message}");
            }
        }
    }
}

