using System;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace Dyagnoz_Latest.Services
{
    public class AppSettings
    {
        // Test Flow
        public bool FullTest { get; set; } = true;
        public bool ActivationOnly { get; set; } = false;

        // Post Test
        public bool AutoPrint { get; set; } = true;
        public bool AutoWipe { get; set; } = false;
        public bool AutoShutdown { get; set; } = false;

        // Print Settings
        public bool PrintFailedParts { get; set; } = true;
        public bool PrintPartMessages { get; set; } = true;
        public bool PrintCustomerName { get; set; } = true;
        public bool PrintDeviceColor { get; set; } = true;
        public bool PrintTesterName { get; set; } = true;
        public bool PrintPortNumber { get; set; } = true;
        public bool PrintLogo { get; set; } = true;
    }

    public class SettingsManager
    {
        private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dyagnoz", "GlobalSettings.json");
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
                    _current = JsonSerializer.Deserialize<AppSettings>(json);
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

                string json = JsonSerializer.Serialize(_current, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving settings: " + ex.Message);
            }
        }
    }
}
