using Dyagnoz_Latest.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyagnoz_Latest.Services
{
    internal class iOSCommander
    {
        private static readonly ConcurrentDictionary<string, Process> RunningProcess = new ConcurrentDictionary<string, Process>();

        private String toolboxPath = Environment.CurrentDirectory + @"\src_set\";
        private String newToolboxPath = Environment.CurrentDirectory + @"\src_set\tools\";

        private String IDEVICE_PAIR = @"idevicepair.exe";
        private String IDEVICE_INFO = @"go-ios.exe";
        private String IDEVICE_ACTIVATE = @"ideviceactivation.exe";
        private String IDEVICE_SKIPSETUP = @"ideviceconfiguration.exe";
        private String IDEVICE_DIAGNOSTICS = @"idevicediagnostics.exe";
        private String IDEVICE_INSTALLER = @"ideviceinstaller.exe";
        private String BUNDLE_ID = @"com.drfones.to";
        private String IPA_FILENAME = @"Velocity-X2.ipa";
        private String IDEVICE_PROVISION = @"ideviceprovision.exe";
        private String IDEVICE_SYSLOG = @"idevicesyslog.exe";
        private String IDEVICE_WIFI = @"ideviceprovision.exe";
        private string WIFI_PROFILE_NAME
        {
            get
            {
                try
                {
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dyagnoz", "wifi-sys.mobileconfig");
                    if (File.Exists(filePath))
                    {
                        string content = File.ReadAllText(filePath);
                        string marker = "<key>SSID_STR</key>";
                        int idx = content.IndexOf(marker);
                        if (idx != -1)
                        {
                            int startStr = content.IndexOf("<string>", idx);
                            int endStr = content.IndexOf("</string>", startStr);
                            if (startStr != -1 && endStr != -1)
                            {
                                string ssid = content.Substring(startStr + 8, endStr - (startStr + 8));
                                return ssid.Replace(" ", "");
                            }
                        }
                    }
                }
                catch { }
                return "TODO";
            }
        }

        public string PairDevice(string deviceUDID)
        {
            string PairingResulr = LaunchExternalExecutable(toolboxPath + IDEVICE_PAIR, $"pair --udid {deviceUDID}");
            if (PairingResulr.Contains("SUCCESS"))
                return "Paired";
            else if (PairingResulr.Contains("ERROR"))
            return "Error";
            else
                return "Unexpected response from pairing process: " + PairingResulr;
        }

        public string GetDeviceInfo(string deviceUDID)
        {
            string DeviceInfoResult = LaunchExternalExecutable(toolboxPath + IDEVICE_INFO, $" --udid {deviceUDID} info");
            if (DeviceInfoResult == "No device found with udid")
                return "Error";
            else
                return DeviceInfoResult;
        }

        public string ActivateDevice(string deviceUDID)
        {
            string ActivationResult = LaunchExternalExecutable(newToolboxPath + IDEVICE_ACTIVATE, $" -u {deviceUDID} activate");
            if (ActivationResult.Contains("session_mode create activation info success"))
                return "Activated";
            else
                return "Unexpected response from activation process: " + ActivationResult;
        }

        public string SkipSetup(string deviceUDID)
        {
            string SkipSetupResult = LaunchExternalExecutable(newToolboxPath + IDEVICE_SKIPSETUP, $" -u {deviceUDID} testprepare");
            if (SkipSetupResult.Contains("Prepare-Success"))
                return "Skipped";
            else
                return "Unexpected response from skip setup process: " + SkipSetupResult;
        }

        public string GetDeviceMDMStatus(string deviceUDID)
        {
            string DeviceMDMStatusResult = LaunchExternalExecutable(newToolboxPath + IDEVICE_SKIPSETUP, $" -u {deviceUDID} CloudConfig");
            if (DeviceMDMStatusResult != "")
                return DeviceMDMStatusResult;
            else
                return "Error";
        }

        public string GetBatteryInfo(string deviceUDID)
        {
            string BatteryResult = LaunchExternalExecutable(toolboxPath + IDEVICE_DIAGNOSTICS, $"-u {deviceUDID} InternalInfo Battery mesaDump");
            if (string.IsNullOrWhiteSpace(BatteryResult) || !BatteryResult.Contains("DesignCapacity"))
                return "Error";
            else
                return BatteryResult;
        }

        public string GetPearlData(string deviceUDID)
        {
            // idevicediagnostics.exe -u {udid} Pearl PP
            string PearlResult = LaunchExternalExecutable(toolboxPath + IDEVICE_DIAGNOSTICS, $"-u {deviceUDID} Pearl PP");
            if (string.IsNullOrWhiteSpace(PearlResult) || PearlResult.Contains("Unable to retrieve IORegistry from device"))
                return "Error";
            else
                return PearlResult;
        }

        public string GetLcdData(string deviceUDID)
        {
            // idevicediagnostics.exe -u {udid} ioregentry product
            string LcdResult = LaunchExternalExecutable(toolboxPath + IDEVICE_DIAGNOSTICS, $"-u {deviceUDID} ioregentry product");
            if (string.IsNullOrWhiteSpace(LcdResult))
                return "Error";
            else
                return LcdResult;
        }

        public string GetOemRData(string deviceUDID)
        {
            // go-ios --udid={udid} OemR
            string OemResult = LaunchExternalExecutable(toolboxPath + IDEVICE_INFO, $"--udid {deviceUDID} OemR");
            if (string.IsNullOrWhiteSpace(OemResult))
                return "Error";
            else
                return OemResult;
        }

        public string InstallProfile(string deviceUDID, string profilePath)
        {
            // ideviceprovision.exe -u {udid} install {path}
            string result = LaunchExternalExecutable(toolboxPath + IDEVICE_PROVISION, $"-u {deviceUDID} install \"{profilePath}\"");
            return result;
        }

        public string RebootDevice(string deviceUDID)
        {
            // go-ios --udid={udid} reboot
            return LaunchExternalExecutable(toolboxPath + IDEVICE_INFO, $"--udid {deviceUDID} reboot");
        }

        public string ShutdownDevice(string deviceUDID)
        {
            // idevicediagnostics.exe -u {udid} shutdown
            return LaunchExternalExecutable(toolboxPath + IDEVICE_DIAGNOSTICS, $"-u {deviceUDID} shutdown");
        }

        public string WipeDevice(string deviceUDID)
        {
            // go-ios --udid={udid} erase
            return LaunchExternalExecutable(toolboxPath + IDEVICE_INFO, $"--udid {deviceUDID} erase");
        }

        public string CheckIfAppInstalled(string deviceUDID)
        {
            // go-ios --udid={udid} listapps
            string result = LaunchExternalExecutable(toolboxPath + IDEVICE_INFO, $"--udid {deviceUDID} apps --list");
            if (result.Contains(BUNDLE_ID))
                return "Pass";
            else
                return "Fail";
        }

        public string EnsureAppInstallation(string deviceUDID)
        {
            // go-ios --udid={udid} listapps
            string tempIPAPath = Path.Combine(Environment.CurrentDirectory, "src_set", IPA_FILENAME);
            string result = LaunchExternalExecutable(toolboxPath + IDEVICE_INSTALLER, $"-u {deviceUDID} -i \"{tempIPAPath}\"");
            Debug.WriteLine($"Installation output for device {deviceUDID}: {result}");
            if (result.Contains("Install: Complete"))
                return "Pass";
            else
                return "Fail";
        }

        public async Task<string> PushTestConfigurationProfileAsync(string deviceUDID)
        {
            string testConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dyagnoz", "CustomTestList.json");
            if (!File.Exists(testConfigPath))
            {   
                Debug.WriteLine("[WARN] Config file not found, skipping push.");
                return "Fail";
            }

            string jsonContent = File.ReadAllText(testConfigPath);
            string base64Config = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonContent));

            string result = LaunchExternalExecutable(toolboxPath + IDEVICE_INFO, $"--udid={deviceUDID} fsync --app={BUNDLE_ID} push --srcPath=\"{base64Config}\" --dstPath=\"/Documents/CustomTestList.json\"");
            Debug.WriteLine($"Push config output for device {deviceUDID}: {result}");
            if (result.Contains("Push: Complete"))
                return "Pass";
            else
                return "Fail";
        }

        public string RemoveWifi(string deviceUDID)
        {
            // idevicewifi.exe -u {udid} remove
            string result = LaunchExternalExecutable(toolboxPath + IDEVICE_WIFI, $"-u {deviceUDID} Delete com.Phoenix.wifiProfile.{WIFI_PROFILE_NAME}");
            if (result.Contains("Done..."))
                return "Pass";
            else
                return "Fail";
        }

        private string LaunchExternalExecutable(string executablePath, string arguments)
        {
            if (string.IsNullOrWhiteSpace(executablePath))
                throw new ArgumentNullException("Path is not valid. LaunchExternalExecutable called with invalid argument: executablePath was empty.");

            string output = "";
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = executablePath,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = arguments,
                RedirectStandardOutput = true
            };

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    output = process.StandardOutput.ReadToEnd();
                }
            }
            catch (SystemException ex)
            {
                throw new Exception($"LaunchExternalExecutable - Device failed to launch a tool with executable path {executablePath}. {ex}");
            }

            // Return raw trimmed output; do NOT replace braces so JSON/arrays stay intact.
            return output.Trim().TrimEnd(Environment.NewLine.ToCharArray());
        }

        public int HookSysLogProcess(string udid, int timeout, out IosCommandResult result)
        {
            int num = 0;
            result = new IosCommandResult();
            StringBuilder standardOutputBuilder = new StringBuilder();
            StringBuilder standardErrorBuilder = new StringBuilder();
            
            // CRITICAL FIX 1: Clean up any existing process for this UDID first
            StopProcessFor(udid);
            System.Threading.Thread.Sleep(200); // Give OS time to release handles
            
            using (Process process = new Process())
            {
                // CRITICAL FIX 2: Use TryAdd to detect conflicts
                if (!RunningProcess.TryAdd(udid, process))
                {
                    Debug.WriteLine($"[ERROR] Failed to register process for {udid} - already exists!");
                    // Force cleanup and retry once
                    StopProcessFor(udid);
                    System.Threading.Thread.Sleep(500);
                    
                    if (!RunningProcess.TryAdd(udid, process))
                    {
                        result.Exception = "Failed to register syslog process - UDID conflict";
                        return -1;
                    }
                }
                
                try
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.FileName = newToolboxPath + IDEVICE_SYSLOG;
                    process.StartInfo.Arguments = $"-u {udid} -m DrFonesResultSTART-";
                    
                    process.OutputDataReceived += (DataReceivedEventHandler)((sender, args) =>
                    {
                        if (args.Data == null)
                            return;
                        standardOutputBuilder.AppendLine(args.Data);
                        if (args.Data.Contains("-DrFonesResultEND"))
                        {
                            try
                            {
                                if (!process.HasExited)
                                    process?.Kill();
                            }
                            catch { }
                        }
                    });
                    
                    process.ErrorDataReceived += (DataReceivedEventHandler)((sender, args) =>
                    {
                        if (args.Data == null)
                            return;
                        standardErrorBuilder.AppendLine(args.Data);
                    });
                    
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    
                    if (process.WaitForExit(timeout))
                    {
                        num = process.ExitCode;
                    }
                    else
                    {
                        // Timeout reached
                        try
                        {
                            if (!process.HasExited)
                                process.Kill();
                        }
                        catch { }
                        result.Exception = $"Timeout after {timeout}ms";
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = $"Process error: {ex.Message}";
                }
                finally
                {
                    // CRITICAL FIX 3: Always remove from dictionary and dispose
                    RunningProcess.TryRemove(udid, out _);
                    
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                        process.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[WARN] Cleanup error for {udid}: {ex.Message}");
                    }
                }
            }
            
            result.Result = standardOutputBuilder.ToString();
            if (string.IsNullOrEmpty(result.Exception))
                result.Exception = standardErrorBuilder.ToString();
            
            return num;
        }

        public static void StopProcessFor(string udid)
        {
            if (string.IsNullOrEmpty(udid)) return;
            
            try
            {
                if (RunningProcess.TryRemove(udid, out Process process))
                {
                    Debug.WriteLine($"[CLEANUP] Stopping process for {udid}");
                    
                    try
                    {
                        if (process != null && !process.HasExited)
                        {
                            // Try graceful kill first
                            process.Kill();
                            
                            // Wait up to 2 seconds for exit
                            if (!process.WaitForExit(2000))
                            {
                                Debug.WriteLine($"[WARN] Process for {udid} did not exit gracefully");
                                // Force kill if still running (shouldn't happen but safety net)
                                try { process.Kill(); } catch { }
                            }
                        }
                        
                        // Always dispose to release handles
                        process?.Dispose();
                        Debug.WriteLine($"[CLEANUP] Process for {udid} stopped and disposed");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ERROR] Failed to kill process for {udid}: {ex.Message}");
                        // Still try to dispose
                        try { process?.Dispose(); } catch { }
                    }
                }
                else
                {
                    Debug.WriteLine($"[CLEANUP] No process found for {udid}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] StopProcessFor outer exception for {udid}: {ex.Message}");
            }
        }

        public static void StopAllProcesses()
        {
            try
            {
                var udids = RunningProcess.Keys.ToList();
                foreach (var udid in udids)
                {
                    StopProcessFor(udid);
                }
            }
            catch { }
        }
    }
}
