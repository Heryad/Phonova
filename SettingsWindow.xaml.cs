using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Phonova.Services;
using Phonova.Models;
using MaterialDesignThemes.Wpf;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Win32;
using DevExpress.XtraReports.UI;
using System.Diagnostics;

namespace Phonova
{
    public partial class SettingsWindow : Window
    {
        private readonly string _configPath;
        private Button? _activeNavButton;

        private string? _editingCustomerName = null;
        private string? _editingMmrComment = null;


        public SettingsWindow()
        {
            InitializeComponent();
            _configPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Phonova");
            if (!System.IO.Directory.Exists(_configPath)) System.IO.Directory.CreateDirectory(_configPath);
            _activeNavButton = NavDashboard;
            
            LoadAndEnsureTestProfile();
            LoadTestListUI();
            LoadSavedProfiles();
            SyncSettingsToUi();
            LoadDashboardStats();
        }

        private void SyncSettingsToUi()
        {
            var s = SettingsManager.Current;
            FullTestToggle.IsChecked = s.FullTest;
            ActivationOnlyToggle.IsChecked = s.ActivationOnly;
            MmrModeToggle.IsChecked = s.MmrMode;
            AutoPrintToggle.IsChecked = s.AutoPrint;
            AutoWipeToggle.IsChecked = s.AutoWipe;
            AutoShutdownToggle.IsChecked = s.AutoShutdown;
            PrintFailedPartsToggle.IsChecked = s.PrintFailedParts;
            PrintPartMessagesToggle.IsChecked = s.PrintPartMessages;
            PrintCustomerNameToggle.IsChecked = s.PrintCustomerName;
            PrintDeviceColorToggle.IsChecked = s.PrintDeviceColor;
            PrintTesterNameToggle.IsChecked = s.PrintTesterName;
            PrintPortNumberToggle.IsChecked = s.PrintPortNumber;
            PrintLogoToggle.IsChecked = s.PrintLogo;
            LabelFormatComboBox.SelectedIndex = s.LabelFormat == "Mobicare Label" ? 2 : (s.LabelFormat == "Simple Label" ? 1 : 0);
            WarrantyTextBox.Text = s.WarrantyText;

            // Enforce Permissions from ApiService.CurrentConfig
            var config = ApiService.CurrentConfig;
            if (config != null)
            {
                if (!config.canDoMMR)
                {
                    MmrModeToggle.IsChecked = false;
                    MmrModeToggle.IsEnabled = false;
                    NavMmrComments.Visibility = Visibility.Collapsed;
                    MmrModeRow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF2F2"));
                    MmrModeLockIcon.Visibility = Visibility.Visible;
                    if (s.MmrMode)
                    {
                        s.MmrMode = false;
                        SettingsManager.Save();
                    }
                }
                else
                {
                    MmrModeToggle.IsEnabled = true;
                    NavMmrComments.Visibility = Visibility.Visible;
                    MmrModeRow.Background = Brushes.Transparent;
                    MmrModeLockIcon.Visibility = Visibility.Collapsed;
                }

                if (!config.canFlashSoftware)
                {
                    AutoWipeToggle.IsChecked = false;
                    AutoWipeToggle.IsEnabled = false;
                    AutoWipeRow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF2F2"));
                    AutoWipeLockIcon.Visibility = Visibility.Visible;
                    if (s.AutoWipe)
                    {
                        s.AutoWipe = false;
                        SettingsManager.Save();
                    }
                }
                else
                {
                    AutoWipeToggle.IsEnabled = true;
                    AutoWipeRow.Background = Brushes.Transparent;
                    AutoWipeLockIcon.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MmrModeRow.Background = Brushes.Transparent;
                MmrModeLockIcon.Visibility = Visibility.Collapsed;
                AutoWipeRow.Background = Brushes.Transparent;
                AutoWipeLockIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void FlowToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender == FullTestToggle && FullTestToggle.IsChecked == true)
            {
                ActivationOnlyToggle.IsChecked = false;
                MmrModeToggle.IsChecked = false;
            }
            else if (sender == ActivationOnlyToggle && ActivationOnlyToggle.IsChecked == true)
            {
                FullTestToggle.IsChecked = false;
                MmrModeToggle.IsChecked = false;
            }
            else if (sender == MmrModeToggle && MmrModeToggle.IsChecked == true)
            {
                FullTestToggle.IsChecked = false;
                ActivationOnlyToggle.IsChecked = false;
            }

            UpdateSettingsAndSave();
        }

        private void SettingToggle_Click(object sender, RoutedEventArgs e)
        {
            // Mutually exclusive Wipe/Shutdown
            if (sender == AutoWipeToggle && AutoWipeToggle.IsChecked == true)
                AutoShutdownToggle.IsChecked = false;
            else if (sender == AutoShutdownToggle && AutoShutdownToggle.IsChecked == true)
                AutoWipeToggle.IsChecked = false;

            UpdateSettingsAndSave();
        }

        private void UpdateSettingsAndSave()
        {
            var s = SettingsManager.Current;
            s.FullTest = FullTestToggle.IsChecked ?? false;
            s.ActivationOnly = ActivationOnlyToggle.IsChecked ?? false;
            s.MmrMode = MmrModeToggle.IsChecked ?? false;
            s.AutoPrint = AutoPrintToggle.IsChecked ?? false;
            s.AutoWipe = AutoWipeToggle.IsChecked ?? false;
            s.AutoShutdown = AutoShutdownToggle.IsChecked ?? false;
            s.PrintFailedParts = PrintFailedPartsToggle.IsChecked ?? false;
            s.PrintPartMessages = PrintPartMessagesToggle.IsChecked ?? false;
            s.PrintCustomerName = PrintCustomerNameToggle.IsChecked ?? false;
            s.PrintDeviceColor = PrintDeviceColorToggle.IsChecked ?? false;
            s.PrintTesterName = PrintTesterNameToggle.IsChecked ?? false;
            s.PrintPortNumber = PrintPortNumberToggle.IsChecked ?? false;
            s.PrintLogo = PrintLogoToggle.IsChecked ?? false;

            SettingsManager.Save();
        }

        private void SettingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var s = SettingsManager.Current;
            s.LabelFormat = LabelFormatComboBox.SelectedIndex == 2 ? "Mobicare Label" : (LabelFormatComboBox.SelectedIndex == 1 ? "Simple Label" : "Standard");
            SettingsManager.Save();
        }

        private void SettingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var s = SettingsManager.Current;
            s.WarrantyText = WarrantyTextBox.Text;
            SettingsManager.Save();
        }

        private void LoadAndEnsureTestProfile()
        {
            try 
            {
                string filePath = System.IO.Path.Combine(_configPath, "CustomTestList.json");
                
                if (!System.IO.File.Exists(filePath))
                {
                    // Create with full list? Or empty list?
                    // User prompt had the JSON with FULL LIST. So default is FULL.
                    string defaultJson = "{\"Test\":\"WiFi,TouchID,DeviceMics,Device Mics Frequency,RearCamera,FrontCamera,Digitizer,DeviceButtons,Device Vibration,Proximity,Telephoto Camera,Accelerometer,Audio Output,TrueDepthFaceID,Ultra Wide Camera,AutoSnapFront,AutoSnapRear,Mic Quality,Home Button,Volume Up Button,Volume Down Button,Flip Switch,Power Button,Auto Camera Tests,NFC,LCDV,\",\"NumberDial\":\"\",\"BatteryDrain\":\"\",\"RearMicFrequency\":\"3\",\"BottomMicFrequency\":\"3\",\"EarpieceFrequency\":\"3\",\"FrontMicFrequency\":\"3\",\"RearMicAmplitude\":\"0.2\",\"FrontMicAmplitude\":\"0.2\",\"BottomMicAmplitude\":\"0.2\",\"EarpieceAmplitude\":\"0.2\"}";
                    System.IO.File.WriteAllText(filePath, defaultJson, System.Text.Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Error initializing test profile: {ex.Message}");
            }
        }

        private void LoadTestListUI()
        {
            TestConfigPanel.Children.Clear();

            // 1. Definition of Categories
            var categories = new Dictionary<string, List<string>>
            {
                { "Cameras", new List<string> { "RearCamera", "FrontCamera", "Ultra Wide Camera", "Telephoto Camera", "AutoSnapFront", "AutoSnapRear", "Auto Camera Tests" } },
                { "Audio", new List<string> { "DeviceMics", "Device Mics Frequency", "Mic Quality", "Audio Output" } },
                { "Buttons", new List<string> { "DeviceButtons", "Home Button", "Volume Up Button", "Volume Down Button", "Flip Switch", "Power Button" } },
                { "Sensors & Biometrics", new List<string> { "TouchID", "TrueDepthFaceID", "Accelerometer", "Proximity", "Device Vibration", "NFC" } },
                { "Screen & Touch", new List<string> { "Digitizer", "LCDV" } },
                { "Connectivity", new List<string> { "WiFi" } },
                { "Other", new List<string> { "NumberDial", "BatteryDrain" } },
                { "AI Tests", new List<string> { "AILCD", "AICallTest", "AIMultiTouch", "AIButtons", "AIMicQuality", "AIDigitizer", "AIGrading", "Face ID Auto" } }
            };

            // 2. Parse Current Config
            HashSet<string> enabledTests = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> paramsValues = new Dictionary<string, string>();
            
            try 
            {
                string filePath = System.IO.Path.Combine(_configPath, "CustomTestList.json");
                if (System.IO.File.Exists(filePath))
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    var root = Newtonsoft.Json.Linq.JObject.Parse(json);
                    if (root["Test"] != null)
                    {
                        var parts = root["Test"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts != null) foreach (var p in parts) enabledTests.Add(p.Trim());
                    }

                    // Read Params
                    foreach (var prop in root.Properties())
                    {
                        if (prop.Name != "Test")
                        {
                            paramsValues[prop.Name] = prop.Value.ToString();
                        }
                    }
                }
            }
            catch {}


            // 3. Render Categories (Toggles)
            foreach (var cat in categories)
            {
                // Header
                TestConfigPanel.Children.Add(new TextBlock 
                { 
                    Text = cat.Key, 
                    FontWeight = FontWeights.Bold, 
                    FontSize = 14,
                    Margin = new Thickness(0, 16, 0, 8),
                    Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary")
                });

                // Grid
                var uniGrid = new System.Windows.Controls.Primitives.UniformGrid { Columns = 2 };
                
                foreach (var test in cat.Value)
                {
                    var p = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,0,8,8) };
                    
                    var toggle = new System.Windows.Controls.Primitives.ToggleButton 
                    { 
                        Style = (Style)FindResource("ToggleStyle"),
                        IsChecked = enabledTests.Contains(test),
                        Tag = $"TEST:{test}",
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    
                    var lbl = new TextBlock 
                    { 
                        Text = test, 
                        VerticalAlignment = VerticalAlignment.Center, 
                        Margin = new Thickness(12,0,0,0) 
                    };

                    p.Children.Add(toggle);
                    p.Children.Add(lbl);
                    uniGrid.Children.Add(p);
                }

                TestConfigPanel.Children.Add(uniGrid);
                TestConfigPanel.Children.Add(new Border { Height = 1, Background = (System.Windows.Media.Brush)FindResource("BorderColor"), Margin = new Thickness(0,8,0,0) });
            }

            // 4. Render Parameters (Sliders)
            TestConfigPanel.Children.Add(new TextBlock 
            { 
                Text = "Audio Parameters", 
                FontWeight = FontWeights.Bold, 
                FontSize = 14,
                Margin = new Thickness(0, 16, 0, 8),
                Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary")
            });

            var paramGrid = new System.Windows.Controls.Primitives.UniformGrid { Columns = 2 };

            // Pairs of (Name, IsFreq)
            var configParams = new List<(string Name, bool IsFreq)>
            {
                ("RearMicFrequency", true), ("RearMicAmplitude", false),
                ("FrontMicFrequency", true), ("FrontMicAmplitude", false),
                ("BottomMicFrequency", true), ("BottomMicAmplitude", false),
                ("EarpieceFrequency", true), ("EarpieceAmplitude", false)
            };

            foreach (var cp in configParams)
            {
                var sp = new StackPanel { Margin = new Thickness(0,0,16,16) };
                
                // Get Val
                double val = 0;
                if (paramsValues.ContainsKey(cp.Name)) double.TryParse(paramsValues[cp.Name], out val);
                else val = cp.IsFreq ? 3 : 0.2; // Defaults

                var topRow = new DockPanel();
                topRow.Children.Add(new TextBlock { Text = cp.Name, FontWeight = FontWeights.SemiBold });
                var valTxt = new TextBlock { Text = val.ToString("0.0"), HorizontalAlignment = HorizontalAlignment.Right, Foreground = System.Windows.Media.Brushes.Gray };
                topRow.Children.Add(valTxt);
                sp.Children.Add(topRow);

                var slider = new Slider 
                { 
                    Minimum = 0, 
                    Maximum = cp.IsFreq ? 10 : 1, 
                    Value = val, 
                    SmallChange = 0.1, 
                    LargeChange = 0.5,
                    Tag = $"PARAM:{cp.Name}",
                    Margin = new Thickness(0,4,0,0),
                    TickFrequency = cp.IsFreq ? 1 : 0.1,
                    IsSnapToTickEnabled = true
                };
                
                // Event to update val text
                slider.ValueChanged += (s, e) => { valTxt.Text = e.NewValue.ToString("0.0"); };

                sp.Children.Add(slider);
                paramGrid.Children.Add(sp);
            }
            TestConfigPanel.Children.Add(paramGrid);
        }

        private async void SaveTestConfig_Click(object sender, RoutedEventArgs e)
        {
            var enabledTests = new List<string>();
            var paramValues = new Dictionary<string, string>();

            // Recursive finder or just iterate expected structure?
            // TestConfigPanel -> Children (TextBlocks, UniformGrids, Borders).
            // Better to use a recursive helper or known structure.
            // I'll scan visual tree of TestConfigPanel.
            
            FindConfigData(TestConfigPanel, enabledTests, paramValues);

            // Construct JSON
            // Preserve "NumberDial" and "BatteryDrain" if they were read? 
            // I should ideally preserve existing keys that are not in UI.
            // But simplifying: I'll just write what I know + current defaults for others?
            // I will Re-Read the file to preserve "NumberDial" etc, then overwrite.
            
            var finalObj = new Dictionary<string, string>();
            try 
            {
                string filePath = System.IO.Path.Combine(_configPath, "CustomTestList.json");
                if (System.IO.File.Exists(filePath))
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    var root = Newtonsoft.Json.Linq.JObject.Parse(json);
                    foreach (var prop in root.Properties())
                    {
                        finalObj[prop.Name] = prop.Value.ToString();
                    }
                }
            } catch {}

            // Update Tests
            finalObj["Test"] = string.Join(",", enabledTests) + ","; // User format has trailing comma
            
            // Update Params
            foreach (var kvp in paramValues) finalObj[kvp.Key] = kvp.Value;

            try 
            {
                string filePath = System.IO.Path.Combine(_configPath, "CustomTestList.json");
                string outJson = JsonConvert.SerializeObject(finalObj, Newtonsoft.Json.Formatting.None);
                System.IO.File.WriteAllText(filePath, outJson, System.Text.Encoding.UTF8);
                MessageBox.Show("Configuration Saved!", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving: " + ex.Message);
            }
        }

        private void FindConfigData(DependencyObject parent, List<string> tests, Dictionary<string, string> paramsVal)
        {
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for(int i=0; i<count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is System.Windows.Controls.Primitives.ToggleButton tb && tb.Tag is string tStr && tStr.StartsWith("TEST:"))
                {
                     if (tb.IsChecked == true) tests.Add(tStr.Substring(5));
                }
                else if (child is Slider sl && sl.Tag is string pStr && pStr.StartsWith("PARAM:"))
                {
                     paramsVal[pStr.Substring(6)] = sl.Value.ToString("0.0"); // Keep 1 decimal format? "3" becomes "3.0". User had "3". I'll use G format? "0.###"?
                     // User had "3", "0.2". 
                     // I'll use generic format.
                     paramsVal[pStr.Substring(6)] = sl.Value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    FindConfigData(child, tests, paramsVal);
                }
            }
        }

        private async void SaveWifiBtn_Click(object sender, RoutedEventArgs e)
        {
            string ssid = WifiSsidBox.Text;
            string password = WifiPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(ssid))
            {
                MessageBox.Show("Please enter an SSID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool result = await GenerateWifiProfileAsync(ssid, password);
            if (result)
            {
                MessageBox.Show("WiFi Profile Generated Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSavedProfiles();
            }
            else
            {
                MessageBox.Show("Failed to generate profile.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSavedProfiles()
        {
            WifiProfilesPanel.Children.Clear();
            string filePath = System.IO.Path.Combine(_configPath, "wifi-sys.mobileconfig");
            
            if (System.IO.File.Exists(filePath))
            {
                string ssid = "Unknown SSID";
                try {
                    string content = System.IO.File.ReadAllText(filePath);
                    string marker = "<key>SSID_STR</key>";
                    int idx = content.IndexOf(marker);
                    if (idx != -1) {
                        int startStr = content.IndexOf("<string>", idx);
                        int endStr = content.IndexOf("</string>", startStr);
                        if (startStr != -1 && endStr != -1) {
                            ssid = content.Substring(startStr + 8, endStr - (startStr + 8));
                        }
                    }
                } catch {}

                // Build UI
                var border = new Border 
                { 
                    Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F9FAFB")), 
                    CornerRadius = new CornerRadius(6), 
                    Padding = new Thickness(16), 
                    Margin = new Thickness(0,0,0,8) 
                };
                
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                
                var sp = new StackPanel();
                sp.Children.Add(new TextBlock { Text = ssid, FontWeight = FontWeights.SemiBold });
                sp.Children.Add(new TextBlock { Text = "Ready to deploy", Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981")), FontSize = 11 });
                
                Grid.SetColumn(sp, 0);
                grid.Children.Add(sp);
                
                // Icon
                var badge = new Border { Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981")), CornerRadius = new CornerRadius(4), Padding = new Thickness(8,4,8,4) };
                 var icon = new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 16, Height = 16, Foreground = System.Windows.Media.Brushes.White };
                 badge.Child = icon;
                 
                 Grid.SetColumn(badge, 1);
                 grid.Children.Add(badge);
                 
                 border.Child = grid;
                 WifiProfilesPanel.Children.Add(border);
            }
            else
            {
                 WifiProfilesPanel.Children.Add(new TextBlock { Text = "No profiles saved", Foreground = System.Windows.Media.Brushes.Gray, FontStyle = FontStyles.Italic, Margin = new Thickness(0,10,0,0) });
            }
        }

        public async Task<bool> GenerateWifiProfileAsync(string ssid, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ssid)) return false;

                string cleanSsid = ssid.Replace(" ", "");
                string payloadUuid1 = Guid.NewGuid().ToString();
                string payloadUuid2 = Guid.NewGuid().ToString();

                string template = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
	<dict>
		<key>HasRemovalPasscode</key>
		<false/>
		<key>PayloadContent</key>
		<array>
			<dict>
				<key>AutoJoin</key>
				<true/>
				<key>CaptiveBypass</key>
				<false/>
				<key>EncryptionType</key>
				<string>{ENCRYPTION_TYPE}</string>
				<key>HIDDEN_NETWORK</key>
				<false/>
				<key>IsHotspot</key>
				<false/>
				<key>Password</key>
				<string>{PASSWORD}</string>
				<key>PayloadDescription</key>
				<string>Configures Wi-Fi settings</string>
				<key>PayloadDisplayName</key>
				<string>Wi-Fi</string>
				<key>PayloadIdentifier</key>
				<string>com.Phoenix.wifiProfile.{CLEAN_SSID_UUID}</string>
				<key>PayloadType</key>
				<string>com.apple.wifi.managed</string>
				<key>PayloadUUID</key>
				<string>{UUID1}</string>
				<key>PayloadVersion</key>
				<integer>1</integer>
				<key>ProxyType</key>
				<string>None</string>
				<key>SSID_STR</key>
				<string>{SSID}</string>
			</dict>
		</array>
		<key>PayloadDisplayName</key>
		<string>Wifi</string>
		<key>PayloadIdentifier</key>
		<string>com.Phoenix.wifiProfile.{CLEAN_SSID}</string>
		<key>PayloadRemovalDisallowed</key>
		<false/>
		<key>PayloadType</key>
		<string>Configuration</string>
		<key>PayloadUUID</key>
		<string>{UUID2}</string>
		<key>PayloadVersion</key>
		<integer>1</integer>
	</dict>
</plist>";

                string encryptionType = string.IsNullOrEmpty(password) ? "None" : "WPA";
                string xml = template
                    .Replace("{PASSWORD}", password)
                    .Replace("{SSID}", ssid)
                    .Replace("{ENCRYPTION_TYPE}", encryptionType)
                    .Replace("{CLEAN_SSID}", cleanSsid)
                    .Replace("{CLEAN_SSID_UUID}", Guid.NewGuid().ToString())
                    .Replace("{UUID1}", payloadUuid1)
                    .Replace("{UUID2}", payloadUuid2);

                string fileName = "wifi-sys.mobileconfig";
                string filePath = System.IO.Path.Combine(_configPath, fileName);
                System.IO.File.WriteAllText(filePath, xml, System.Text.Encoding.UTF8);
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }

        private void ShowPanel(string panelName, Button navButton)
        {
            // Hide all panels
            PanelDashboard.Visibility = Visibility.Collapsed;

            PanelTests.Visibility = Visibility.Collapsed;
            PanelComments.Visibility = Visibility.Collapsed;
            PanelMmrComments.Visibility = Visibility.Collapsed;
            PanelCustomers.Visibility = Visibility.Collapsed;

            PanelWifi.Visibility = Visibility.Collapsed;
            PanelTestFlow.Visibility = Visibility.Collapsed;
            PanelLicense.Visibility = Visibility.Collapsed;
            PanelAbout.Visibility = Visibility.Collapsed;

            // Reset all nav buttons
            NavDashboard.Style = (Style)FindResource("NavBtn");

            NavTests.Style = (Style)FindResource("NavBtn");
            NavComments.Style = (Style)FindResource("NavBtn");
            NavMmrComments.Style = (Style)FindResource("NavBtn");
            NavCustomers.Style = (Style)FindResource("NavBtn");

            NavWifi.Style = (Style)FindResource("NavBtn");
            NavTestFlow.Style = (Style)FindResource("NavBtn");
            NavLicense.Style = (Style)FindResource("NavBtn");
            NavAbout.Style = (Style)FindResource("NavBtn");

            // Show selected panel
            switch (panelName)
            {
                case "Dashboard": PanelDashboard.Visibility = Visibility.Visible; break;

                case "Tests": PanelTests.Visibility = Visibility.Visible; break;
                case "Comments": PanelComments.Visibility = Visibility.Visible; break;
                case "MmrComments": PanelMmrComments.Visibility = Visibility.Visible; break;
                case "Customers": PanelCustomers.Visibility = Visibility.Visible; break;

                case "Wifi": PanelWifi.Visibility = Visibility.Visible; break;
                case "TestFlow": PanelTestFlow.Visibility = Visibility.Visible; break;
                case "License": PanelLicense.Visibility = Visibility.Visible; break;
                case "About": PanelAbout.Visibility = Visibility.Visible; break;
            }

            // Set active nav button
            navButton.Style = (Style)FindResource("NavBtnActive");
            _activeNavButton = navButton;

            if (panelName == "Comments") LoadCommentsTable();
            if (panelName == "MmrComments") LoadMmrCommentsTable();
            if (panelName == "Customers") LoadCustomersTable();

            if (panelName == "License") LoadLicenseData();
            if (panelName == "Dashboard") LoadDashboardStats();
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e) => ShowPanel("Dashboard", NavDashboard);

        private void NavTests_Click(object sender, RoutedEventArgs e) => ShowPanel("Tests", NavTests);
        private void NavComments_Click(object sender, RoutedEventArgs e) => ShowPanel("Comments", NavComments);
        private void NavMmrComments_Click(object sender, RoutedEventArgs e) => ShowPanel("MmrComments", NavMmrComments);
        private void NavCustomers_Click(object sender, RoutedEventArgs e) => ShowPanel("Customers", NavCustomers);

        private void NavWifi_Click(object sender, RoutedEventArgs e) => ShowPanel("Wifi", NavWifi);
        private void NavTestFlow_Click(object sender, RoutedEventArgs e) => ShowPanel("TestFlow", NavTestFlow);
        private void NavLicense_Click(object sender, RoutedEventArgs e) => ShowPanel("License", NavLicense);
        private void NavAbout_Click(object sender, RoutedEventArgs e) => ShowPanel("About", NavAbout);

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

        private void TestPrintBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var s = SettingsManager.Current;
                XtraReport label;
                if (s.LabelFormat == "Mobicare Label")
                {
                    label = new MobicareLabel(
                        imei:         "353456789012345",
                        serial:       "C8QKF2ABCD12",
                        model:        "iPhone 14 Pro",
                        product:      "iPhone 14 Pro 256GB",
                        color:        "Deep Purple",
                        version:      "16.7.2",
                        battery:      "94%",
                        icloud:       "Off",
                        fmi:          "Off",
                        mdm:          "Off",
                        sim:          "Unlocked",
                        port:         "1",
                        notes:        "Test Print",
                        customerName: "Sample Customer",
                        testerName: "Sample Tester",
                        isSynced: true
                    );
                }
                else if (s.LabelFormat == "Simple Label")
                {
                    label = new SimpleLabel(
                        imei: "353456789012345",
                        product: "iPhone 14 Pro",
                        battery: "94%",
                        notes: "Test Print"
                    );
                }
                else
                {
                    label = new HorizontalLabel(
                        imei:         "353456789012345",
                        serial:       "C8QKF2ABCD12",
                        model:        "iPhone 14 Pro",
                        product:      "iPhone 14 Pro 256GB",
                        color:        "Deep Purple",
                        version:      "16.7.2",
                        battery:      "94%",
                        icloud:       "Off",
                        fmi:          "Off",
                        mdm:          "Off",
                        sim:          "Unlocked",
                        port:         "1",
                        notes:        "Test Print",
                        customerName: "Sample Customer",
                        testerName: "Sample Tester",
                        isSynced: true
                    );
                }
                label.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Test print failed: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private string? _editingCommentId = null;
        private string? _editingComment = null;

        // Comments CRUD
        private async void LoadCommentsTable()
        {
            if (CommentsTablePanel == null) return;
            CommentsTablePanel.Children.Clear();

            var comments = await ApiService.GetCommentsAsync();
            foreach (var comment in comments)
            {
                var grid = new Grid { Margin = new Thickness(0, 0, 0, 1) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                var textBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10) };
                textBorder.Child = new TextBlock { Text = comment.content, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(textBorder, 0);
                grid.Children.Add(textBorder);

                var actionBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10), HorizontalAlignment = HorizontalAlignment.Center };
                var actionStack = new StackPanel { Orientation = Orientation.Horizontal };

                // Edit Button
                var editBtn = new Button
                {
                    Content = new PackIcon { Kind = PackIconKind.Pencil, Width = 18, Height = 18 },
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Width = 32,
                    Height = 32,
                    Padding = new Thickness(0),
                    Tag = comment,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                editBtn.Click += EditComment_Click;
                actionStack.Children.Add(editBtn);

                // Delete Button
                var deleteBtn = new Button
                {
                    Content = new PackIcon { Kind = PackIconKind.Delete, Width = 18, Height = 18 },
                    Foreground = Brushes.Red,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Width = 32,
                    Height = 32,
                    Padding = new Thickness(0),
                    Tag = comment,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                deleteBtn.Click += DeleteComment_Click;
                actionStack.Children.Add(deleteBtn);

                actionBorder.Child = actionStack;
                Grid.SetColumn(actionBorder, 1);
                grid.Children.Add(actionBorder);

                CommentsTablePanel.Children.Add(grid);
            }
        }

        private async void AddCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            string newComment = NewCommentBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(newComment)) return;

            if (!string.IsNullOrEmpty(_editingCommentId))
            {
                await ApiService.UpdateCommentAsync(_editingCommentId, newComment);
                _editingComment = null;
                _editingCommentId = null;
                NewCommentBox.Text = "";
                AddCommentBtnText.Text = "Add";
                CancelEditCommentBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                await ApiService.AddCommentAsync(newComment);
                NewCommentBox.Text = "";
            }
            LoadCommentsTable();
        }

        private void EditComment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ApiService.CommentModel comment)
            {
                _editingComment = comment.content;
                _editingCommentId = comment.id;
                NewCommentBox.Text = comment.content;
                AddCommentBtnText.Text = "Update";
                CancelEditCommentBtn.Visibility = Visibility.Visible;
                NewCommentBox.Focus();
            }
        }

        private void CancelEditCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            _editingComment = null;
            _editingCommentId = null;
            NewCommentBox.Text = "";
            AddCommentBtnText.Text = "Add";
            CancelEditCommentBtn.Visibility = Visibility.Collapsed;
        }

        private async void DeleteComment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ApiService.CommentModel comment)
            {
                if (MessageBox.Show($"Are you sure you want to delete '{comment.content}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    await ApiService.DeleteCommentAsync(comment.id);
                    if (_editingCommentId == comment.id)
                    {
                        CancelEditCommentBtn_Click(sender, e);
                    }
                    LoadCommentsTable();
                }
            }
        }

        private string? _editingMmrCommentId = null;

        private async void LoadMmrCommentsTable()
        {
            if (MmrCommentsTablePanel == null) return;
            MmrCommentsTablePanel.Children.Clear();

            var comments = await ApiService.GetMmrCommentsAsync();
            foreach (var comment in comments)
            {
                var grid = new Grid { Margin = new Thickness(0, 0, 0, 1) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                var textBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10) };
                textBorder.Child = new TextBlock { Text = comment.content, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(textBorder, 0);
                grid.Children.Add(textBorder);

                var actionBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10), HorizontalAlignment = HorizontalAlignment.Center };
                var actionStack = new StackPanel { Orientation = Orientation.Horizontal };

                // Edit Button
                var editBtn = new Button
                {
                    Content = new PackIcon { Kind = PackIconKind.Pencil, Width = 18, Height = 18 },
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Width = 32,
                    Height = 32,
                    Padding = new Thickness(0),
                    Tag = comment,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                editBtn.Click += EditMmrComment_Click;
                actionStack.Children.Add(editBtn);

                // Delete Button
                var deleteBtn = new Button
                {
                    Content = new PackIcon { Kind = PackIconKind.Delete, Width = 18, Height = 18 },
                    Foreground = Brushes.Red,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Width = 32,
                    Height = 32,
                    Padding = new Thickness(0),
                    Tag = comment,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                deleteBtn.Click += DeleteMmrComment_Click;
                actionStack.Children.Add(deleteBtn);

                actionBorder.Child = actionStack;
                Grid.SetColumn(actionBorder, 1);
                grid.Children.Add(actionBorder);

                MmrCommentsTablePanel.Children.Add(grid);
            }
        }

        private async void AddMmrCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            string newComment = NewMmrCommentBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(newComment)) return;

            if (!string.IsNullOrEmpty(_editingMmrCommentId))
            {
                await ApiService.UpdateMmrCommentAsync(_editingMmrCommentId, newComment);
                _editingMmrComment = null;
                _editingMmrCommentId = null;
                NewMmrCommentBox.Text = "";
                AddMmrCommentBtnText.Text = "Add";
                CancelEditMmrCommentBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                await ApiService.AddMmrCommentAsync(newComment);
                NewMmrCommentBox.Text = "";
            }
            LoadMmrCommentsTable();
        }

        private void EditMmrComment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ApiService.CommentModel comment)
            {
                _editingMmrComment = comment.content;
                _editingMmrCommentId = comment.id;
                NewMmrCommentBox.Text = comment.content;
                AddMmrCommentBtnText.Text = "Update";
                CancelEditMmrCommentBtn.Visibility = Visibility.Visible;
                NewMmrCommentBox.Focus();
            }
        }

        private void CancelEditMmrCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            _editingMmrComment = null;
            _editingMmrCommentId = null;
            NewMmrCommentBox.Text = "";
            AddMmrCommentBtnText.Text = "Add";
            CancelEditMmrCommentBtn.Visibility = Visibility.Collapsed;
        }

        private async void DeleteMmrComment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ApiService.CommentModel comment)
            {
                if (MessageBox.Show($"Are you sure you want to delete '{comment.content}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    await ApiService.DeleteMmrCommentAsync(comment.id);
                    if (_editingMmrCommentId == comment.id)
                    {
                        CancelEditMmrCommentBtn_Click(sender, e);
                    }
                    LoadMmrCommentsTable();
                }
            }
        }
        
        // Customers CRUD
        private string? _editingCustomerId = null;

        private async void LoadCustomersTable()
        {
            if (CustomersTablePanel == null) return;
            CustomersTablePanel.Children.Clear();

            var customers = await ApiService.GetCustomersAsync();
            foreach (var customer in customers)
            {
                var grid = new Grid { Margin = new Thickness(0, 0, 0, 1) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                var textBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10) };
                textBorder.Child = new TextBlock { Text = customer.name, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(textBorder, 0);
                grid.Children.Add(textBorder);

                var actionBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10), HorizontalAlignment = HorizontalAlignment.Center };
                
                var actionStack = new StackPanel { Orientation = Orientation.Horizontal };

                // Edit Button
                var editBtn = new Button
                {
                    Content = new PackIcon { Kind = PackIconKind.Pencil, Width = 18, Height = 18 },
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Width = 32,
                    Height = 32,
                    Padding = new Thickness(0),
                    Tag = customer,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                editBtn.Click += EditCustomer_Click;
                actionStack.Children.Add(editBtn);

                // Delete Button
                var deleteBtn = new Button
                {
                    Content = new PackIcon { Kind = PackIconKind.Delete, Width = 18, Height = 18 },
                    Foreground = Brushes.Red,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Width = 32,
                    Height = 32,
                    Padding = new Thickness(0),
                    Tag = customer,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                deleteBtn.Click += DeleteCustomer_Click;
                actionStack.Children.Add(deleteBtn);

                actionBorder.Child = actionStack;
                Grid.SetColumn(actionBorder, 1);
                grid.Children.Add(actionBorder);

                CustomersTablePanel.Children.Add(grid);
            }
        }

        private async void AddCustomerBtn_Click(object sender, RoutedEventArgs e)
        {
            string customerName = NewCustomerBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(customerName)) return;

            if (!string.IsNullOrEmpty(_editingCustomerId))
            {
                // Update mode
                await ApiService.UpdateCustomerAsync(_editingCustomerId, customerName);
                _editingCustomerName = null;
                _editingCustomerId = null;
                NewCustomerBox.Text = "";
                AddCustomerBtnText.Text = "Add";
                CancelEditCustomerBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Add mode
                await ApiService.AddCustomerAsync(customerName);
                NewCustomerBox.Text = "";
            }
            LoadCustomersTable();
        }

        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ApiService.CustomerModel customer)
            {
                _editingCustomerName = customer.name;
                _editingCustomerId = customer.id;
                NewCustomerBox.Text = customer.name;
                AddCustomerBtnText.Text = "Update";
                CancelEditCustomerBtn.Visibility = Visibility.Visible;
                NewCustomerBox.Focus();
            }
        }

        private void CancelEditCustomerBtn_Click(object sender, RoutedEventArgs e)
        {
            _editingCustomerName = null;
            _editingCustomerId = null;
            NewCustomerBox.Text = "";
            AddCustomerBtnText.Text = "Add";
            CancelEditCustomerBtn.Visibility = Visibility.Collapsed;
        }

        private async void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ApiService.CustomerModel customer)
            {
                if (MessageBox.Show($"Are you sure you want to delete '{customer.name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    await ApiService.DeleteCustomerAsync(customer.id);
                    if (_editingCustomerId == customer.id)
                    {
                        CancelEditCustomerBtn_Click(sender, e);
                    }
                    LoadCustomersTable();
                }
            }
        }

        // Testers CRUD

        

        
        // Dashboard analytics
        private async void LoadDashboardStats()
        {
            try
            {
                var response = await ApiService.GetKpiAsync();
                if (response != null)
                {
                    StatTodayTests.Text = response.kpis.todayDevices.ToString();
                    StatTotalTests.Text = response.kpis.totalDevices.ToString();
                    StatPassRate.Text = response.kpis.passRate;
                    StatFailRate.Text = response.kpis.failRate;
                    StatMmrRate.Text = response.kpis.mmrRate;

                    DrawBarChart(response.Chart.TestsPerMonth);
                    ChartEmptyState.Visibility = response.kpis.totalDevices > 0 ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    StatTodayTests.Text = "0";
                    StatTotalTests.Text = "0";
                    StatPassRate.Text = "0%";
                    StatFailRate.Text = "0%";
                    StatMmrRate.Text = "0%";
                    ChartCanvas?.Children.Clear();
                    if (ChartEmptyState != null) ChartEmptyState.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading dashboard: " + ex.Message);
            }
        }

        private void DrawBarChart(System.Collections.Generic.List<ApiService.MonthChartPoint> data)
        {
            if (ChartCanvas == null) return;
            ChartCanvas.Children.Clear();
            if (data == null || data.Count == 0) return;

            double width = ChartCanvas.ActualWidth > 0 ? ChartCanvas.ActualWidth : 500;
            double height = 150; // Leave room for labels
            double barWidth = (width / data.Count) * 0.5;
            double spacing = (width / data.Count) * 0.5;
            double maxVal = data.Max(x => x.Count);
            if (maxVal == 0) maxVal = 1;

            for (int i = 0; i < data.Count; i++)
            {
                double x = (i * (barWidth + spacing)) + (spacing / 2);
                double bHeight = ((double)data[i].Count / maxVal) * height;

                // Month Bar
                var rBar = new System.Windows.Shapes.Rectangle { 
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBC101")), 
                    Width = barWidth, 
                    Height = bHeight, 
                    RadiusX = 4, 
                    RadiusY = 4 
                };
                Canvas.SetLeft(rBar, x);
                Canvas.SetBottom(rBar, 25); // Leave space for X-axis label
                ChartCanvas.Children.Add(rBar);

                // Month Name Label (Format: "2026-01" -> "Jan")
                string monthLabel = data[i].Month;
                try
                {
                    if (monthLabel.Length == 7)
                    {
                        var date = DateTime.ParseExact(monthLabel, "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
                        monthLabel = date.ToString("MMM");
                    }
                }
                catch { }

                var lbl = new TextBlock { Text = monthLabel, FontSize = 10, Foreground = Brushes.Gray, Width = barWidth + spacing, TextAlignment = TextAlignment.Center };
                Canvas.SetLeft(lbl, x - (spacing / 4));
                Canvas.SetBottom(lbl, 5);
                ChartCanvas.Children.Add(lbl);

                // Count Label (Top)
                if (data[i].Count > 0)
                {
                    var countLbl = new TextBlock { Text = data[i].Count.ToString(), FontSize = 9, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, Width = barWidth, TextAlignment = TextAlignment.Center };
                    Canvas.SetLeft(countLbl, x);
                    Canvas.SetBottom(countLbl, 25 + bHeight + 2);
                    ChartCanvas.Children.Add(countLbl);
                }
            }
        }


        private async void LoadLicenseData()
        {
            try
            {
                var config = await ApiService.GetConfigAsync();
                if (config != null)
                {
                    ApiService.CurrentConfig = config.Client;

                    // If fuel was previously exhausted, check if it's now been topped up
                    if (Phonova.Services.OfflineSyncManager.Instance.IsFuelExhausted)
                    {
                        bool hasFuel = config.Client.isUnlimitedTesting || config.Client.fuel > 0;
                        if (hasFuel)
                        {
                            // Reset and retry sync — the fuel banner will auto-hide on success
                            Phonova.Services.OfflineSyncManager.Instance.IsFuelExhausted = false;
                            _ = System.Threading.Tasks.Task.Run(() => Phonova.Services.OfflineSyncManager.Instance.TrySyncAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reloading config in License tab: {ex.Message}");
            }

            UpdateLicenseUi();
        }

        private void UpdateLicenseUi()
        {
            var config = ApiService.CurrentConfig;
            if (config != null)
            {
                LicenseCompanyNameText.Text = config.companyName;

                if (!string.IsNullOrEmpty(config.logoUrl))
                {
                    try
                    {
                        var uri = new Uri(config.logoUrl, UriKind.Absolute);
                        var bmi = new System.Windows.Media.Imaging.BitmapImage(uri);
                        LicenseCompanyLogoImage.Source = bmi;
                        LicenseCompanyLogoImage.Visibility = Visibility.Visible;
                        LicenseCompanyLogoPlaceholder.Visibility = Visibility.Collapsed;
                    }
                    catch
                    {
                        LicenseCompanyLogoImage.Visibility = Visibility.Collapsed;
                        LicenseCompanyLogoPlaceholder.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    LicenseCompanyLogoImage.Visibility = Visibility.Collapsed;
                    LicenseCompanyLogoPlaceholder.Visibility = Visibility.Visible;
                }

                LicenseCompanyStatusText.Text = config.isUnlimitedTesting ? "Premium Enterprise Partner" : "Verified Partner";
                LicenseCompanyStatusText.Foreground = config.isUnlimitedTesting 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1")) 
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));

                FuelValueText.Text = config.isUnlimitedTesting ? "Unlimited" : config.fuel.ToString();
                ConcurrentLimitValueText.Text = $"{config.maxConcurrentDevices} Devices";
                
                if (config.isUnlimitedTesting)
                {
                    ExpirationValueText.Text = !string.IsNullOrEmpty(config.unlimitedTestingEndDate) 
                        ? DateTime.Parse(config.unlimitedTestingEndDate).ToString("MMM dd, yyyy") 
                        : "Never";
                }
                else
                {
                    ExpirationValueText.Text = "N/A";
                }

                DetailStatusText.Text = config.isUnlimitedTesting ? "Unlimited Plan" : "Fuel Plan";
                DetailStatusBorder.Background = config.isUnlimitedTesting 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E7FF")) 
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DEF7EC"));
                DetailStatusText.Foreground = config.isUnlimitedTesting 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1")) 
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));

                DetailFuelText.Text = config.fuel.ToString();
                DetailConcurrentText.Text = config.maxConcurrentDevices.ToString();
            }
            else
            {
                LicenseCompanyNameText.Text = "Unknown Client";
                LicenseCompanyStatusText.Text = "No Connection";
                LicenseCompanyStatusText.Foreground = Brushes.Red;
                FuelValueText.Text = "--";
                ConcurrentLimitValueText.Text = "--";
                ExpirationValueText.Text = "--";
                DetailStatusText.Text = "--";
                DetailFuelText.Text = "--";
                DetailConcurrentText.Text = "--";
            }
        }
    }
}
