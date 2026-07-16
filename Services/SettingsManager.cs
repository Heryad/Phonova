using System;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Phonova.Services
{
    public class AppSettings
    {
        // API Configuration
        public string ApiBaseUrl { get; set; } = "https://phonova-api.vercel.app";
        public string SavedCompanyEmail { get; set; } = string.Empty;
        public string SavedUsername { get; set; } = string.Empty;
        public string SavedPassword { get; set; } = string.Empty;

        // Test Flow
        public bool FullTest { get; set; } = true;
        public bool ActivationOnly { get; set; } = false;
        public bool MmrMode { get; set; } = false;

        // Post Test
        public bool AutoPrint { get; set; } = true;
        public bool AutoWipe { get; set; } = false;
        public bool AutoShutdown { get; set; } = false;

        // Print Settings
        public bool PrintFailedParts { get; set; } = true;
        public bool PrintPartMessages { get; set; } = true;
        public bool PrintCustomerName { get; set; } = true;
        public bool PrintTesterName { get; set; } = true;
        public bool PrintDeviceColor { get; set; } = true;
        public bool PrintPortNumber { get; set; } = true;
        public bool PrintLogo { get; set; } = true;
        public string LabelFormat { get; set; } = "Standard";
        public string WarrantyText { get; set; } = "30 Days";
    }

    public class SettingsManager
    {
        private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Phonova", "GlobalSettings.json");
        private static AppSettings _current;

        public static AppSettings Current
        {
            get
            {
                if (_current == null) Load();
                return _current;
            }
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    _current = JsonConvert.DeserializeObject<AppSettings>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading settings: " + ex.Message);
            }

            if (_current == null) _current = new AppSettings();
        }

        public static void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string json = JsonConvert.SerializeObject(_current, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving settings: " + ex.Message);
            }
        }
    }
}
