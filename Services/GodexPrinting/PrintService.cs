using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EzioDll;

namespace Dyagnoz.Services.Printing
{
    /// <summary>
    /// High-level printing service for device diagnostic labels.
    /// Manages printer connection, template loading, and label printing.
    /// </summary>
    public class PrintService : IDisposable
    {
        private readonly string _templatePath;
        GodexPrinter godexPrinter = new GodexPrinter();
        private string? _cachedTemplate;
        private bool _disposed;
        private readonly System.Threading.SemaphoreSlim _printSemaphore = new System.Threading.SemaphoreSlim(1, 1);

        public event EventHandler<string>? PrintError;
        public event EventHandler<string>? PrintSuccess;

        public PrintService()
        {
            try {
                _templatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dyagnoz", "label_template.txt");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] CRITICAL EXCEPTION IN CONSTRUCTOR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                RaiseError($"Initialization failed: {ex.Message}");
            }
            
        }

        /// <summary>
        /// Gets available USB printers
        /// </summary>
        public List<string> GetAvailablePrinters() => GodexPrinter.GetPrinter_USB();

        /// <summary>
        /// Connects to the first available USB printer
        /// </summary>
        public bool ConnectToFirstPrinter()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PRINT SERVICE] Attempting to connect to first available Godex printer...");

                var printers = GetAvailablePrinters();
                System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] Found {printers.Count} printers.");

                if (printers.Count == 0)
                {
                    RaiseError("No Godex printer found");
                    return false;
                }

                foreach (var p in printers)
                {
                    System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] Discovered printer: {p}");
                }

                System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] Connecting to {printers[0]}...");
                godexPrinter.OpenUSB(printers[0]);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] CRITICAL EXCEPTION IN ConnectToFirstPrinter: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                RaiseError($"Connection failed: {ex.Message}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Prints a device diagnostic label
        /// </summary>
        public async Task<bool> PrintDeviceLabelAsync(DeviceLabelData data)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] Starting print job for device: {data.DeviceName}, SN: {data.SerialNumber}");

                // Load template
                string template = await LoadTemplateAsync();
                if (string.IsNullOrEmpty(template))
                {
                    System.Diagnostics.Debug.WriteLine("[PRINT SERVICE] ERROR: Template is empty or could not be loaded.");
                    RaiseError("Template not loaded");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] Template loaded ({template.Length} chars).");

                // Build replacements
                // Helper to sanitize EZPL inputs (commas break command structure)
                string Sanitize(string? input)
                {
                    if (string.IsNullOrEmpty(input)) return "";
                    // Replace comma with space to prevent command truncation
                    // Replace newlines to prevent command splitting
                    return input.Replace(",", " ").Replace("\r", "").Replace("\n", "").Trim();
                }

                var replacements = new Dictionary<string, string>
                {
                    { "{DEVICE_NAME}", Sanitize(data.DeviceName) },
                    { "{STORAGE}", Sanitize(data.Storage) },
                    { "{MODEL_NUMBER}", Sanitize(data.ModelNumber) },
                    { "{ICLOUD_STATE}", Sanitize(data.ICloudState) },
                    { "{FMI_STATE}", Sanitize(data.FmiState) },
                    { "{SIM_STATE}", "Unlocked" }, // Hardcoded value, safe
                    { "{MDM_STATE}", Sanitize(data.MdmState) },
                    { "{COLOR}", Sanitize(data.Color) },
                    { "{IOS_VERSION}", Sanitize(data.IosVersion) },
                    { "{BATTERY}", Sanitize(data.Battery) },
                    { "{PORT}", Sanitize(data.Port) },
                    { "{IMEI}", Sanitize(data.Imei) },
                    { "{SERIAL_NUMBER}", Sanitize(data.SerialNumber) },
                    { "{DATE}", Sanitize(data.Date) },
                    { "{MESSAGES}", Sanitize(data.Messages) },
                    { "{COMMENTS}", Sanitize(data.Comments) },
                    { "{GRADE}", Sanitize(data.Grade) }
                };

                // Replace placeholders
                string labelData = template;
                foreach (var kvp in replacements)
                {
                    string replacementValue = kvp.Value;
                    labelData = labelData.Replace(kvp.Key, replacementValue);
                }

                System.Diagnostics.Debug.WriteLine("[PRINT SERVICE] Final Label Data to be sent:");
                System.Diagnostics.Debug.WriteLine("--------------------------------------------------");
                System.Diagnostics.Debug.WriteLine(labelData);
                System.Diagnostics.Debug.WriteLine("--------------------------------------------------");

                // Send to printer - ASYNC SEMAPHORE
                await _printSemaphore.WaitAsync();
                int result = 0;
                try
                {
                    System.Diagnostics.Debug.WriteLine("[PRINT SERVICE] Sending command to GodexPrinter.Command.Send...");
                    // Add a small delay to ensure printer buffer is clear
                    await Task.Delay(100);
                    result = godexPrinter.Command.Send(labelData);
                }
                finally
                {
                    _printSemaphore.Release();
                }

                System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] Printer Command Result: {result}");

                PrintSuccess?.Invoke(this, $"Label printed for {data.SerialNumber}");
                System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] Label printed for {data.SerialNumber}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] CRITICAL EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                RaiseError($"Print failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads the label template from disk (cached after first load)
        /// </summary>
        private async Task<string> LoadTemplateAsync()
        {
            if (!string.IsNullOrEmpty(_cachedTemplate))
            {
                System.Diagnostics.Debug.WriteLine("[PRINT SERVICE] Using cached template.");
                return _cachedTemplate;
            }

            System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] Loading template from: {_templatePath}");
            if (!File.Exists(_templatePath))
            {
                RaiseError($"Template file not found at: {_templatePath}");
                return string.Empty;
            }

            try
            {
                string[] lines = await File.ReadAllLinesAsync(_templatePath);
                _cachedTemplate = string.Join("\r\n", lines);
                System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE] Template loaded successfully. Total lines: {lines.Length}");
                return _cachedTemplate;
            }
            catch (Exception ex)
            {
                RaiseError($"Failed to read template file: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Clears the cached template (call if template file changes)
        /// </summary>
        public void ReloadTemplate() => _cachedTemplate = null;

        private void RaiseError(string message)
        {
            PrintError?.Invoke(this, message);
            System.Diagnostics.Debug.WriteLine($"[PRINT SERVICE ERROR] {message}");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                godexPrinter?.Close();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Data class for device label printing
    /// </summary>
    public class DeviceLabelData
    {
        public string? DeviceName { get; set; }
        public string? Storage { get; set; }
        public string? ModelNumber { get; set; }
        public string? ICloudState { get; set; }
        public string? FmiState { get; set; }
        public string? SimState { get; set; }
        public string? MdmState { get; set; }
        public string? Color { get; set; }
        public string? IosVersion { get; set; }
        public string? Battery { get; set; }
        public string? Port { get; set; }
        public string? Imei { get; set; }
        public string? SerialNumber { get; set; }
        public string? Date { get; set; }
        public string? Messages { get; set; }

        public string? Comments { get; set; }
        public string? Grade { get; set; }
    }
}