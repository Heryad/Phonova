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

namespace Phonova
{
    public partial class SettingsWindow : Window
    {
        private readonly string _configPath;
        private Button? _activeNavButton;
        private List<ProcessedDevice> _lastFetchedReports = new();
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
            LoadCommentsTable();
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
            LabelFormatComboBox.SelectedIndex = s.LabelFormat == "Simple Label" ? 1 : 0;
            WarrantyTextBox.Text = s.WarrantyText;
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
            s.LabelFormat = LabelFormatComboBox.SelectedIndex == 1 ? "Simple Label" : "Standard";
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
                { "Other", new List<string> { "NumberDial", "BatteryDrain" } } 
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
                string outJson = JsonConvert.SerializeObject(finalObj, Newtonsoft.Json.Formatting.Indented);
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
            PanelReports.Visibility = Visibility.Collapsed;
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
            NavReports.Style = (Style)FindResource("NavBtn");
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
                case "Reports": PanelReports.Visibility = Visibility.Visible; break;
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
            if (panelName == "Reports") LoadReportsTable();
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e) => ShowPanel("Dashboard", NavDashboard);
        private void NavReports_Click(object sender, RoutedEventArgs e) => ShowPanel("Reports", NavReports);
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
                if (s.LabelFormat == "Simple Label")
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
                        testerName: "Sample Tester"
                    );
                }
                label.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Test print failed: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
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
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                var textBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10) };
                textBorder.Child = new TextBlock { Text = comment.content, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(textBorder, 0);
                grid.Children.Add(textBorder);

                var actionBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10) };
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
                actionBorder.Child = deleteBtn;
                Grid.SetColumn(actionBorder, 1);
                grid.Children.Add(actionBorder);

                CommentsTablePanel.Children.Add(grid);
            }
        }

        private async void AddCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            string newComment = NewCommentBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(newComment)) return;

            await ApiService.AddCommentAsync(newComment);
            NewCommentBox.Text = "";
            LoadCommentsTable();
        }

        private async void DeleteComment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ApiService.CommentModel comment)
            {
                if (MessageBox.Show($"Are you sure you want to delete '{comment.content}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    await ApiService.DeleteCommentAsync(comment.id);
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

        
        // Reports table loading
        private void LoadReportsTable()
        {
            if (ReportsTablePanel == null) return;
            ReportsTablePanel.Children.Clear();

            string search = ReportSearchBox.Text;
            DateTime? start = StartDatePicker.SelectedDate;
            DateTime? end = EndDatePicker.SelectedDate;

            // Adjust end date to include the whole day
            if (end.HasValue) end = end.Value.Date.AddDays(1).AddTicks(-1);
            
            // Mock empty list instead of App.Database.GetProcessedDevices
            _lastFetchedReports = new List<Phonova.Models.ProcessedDevice>();
            var reports = _lastFetchedReports;

            int total = reports.Count;
            int passed = 0;
            int failed = 0;

            foreach (var report in reports)
            {
                // Calculate if device passed
                bool isPass = true;
                foreach (var test in report.KernelTests.Values)
                {
                    if (test == "Fail") { isPass = false; break; }
                }
                foreach (var test in report.AppTests.Values)
                {
                    if (test == "Fail") { isPass = false; break; }
                }

                if (isPass) passed++;
                else failed++;

                var grid = new Grid { Margin = new Thickness(0, 0, 0, 1) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                // Device Info
                var deviceInfo = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                deviceInfo.Children.Add(new TextBlock { Text = report.DeviceName ?? "Unknown Device", FontWeight = FontWeights.SemiBold, FontSize = 13 });
                deviceInfo.Children.Add(new TextBlock { Text = $"{report.Model} · {report.Storage}", FontSize = 11, Foreground = Brushes.Gray });
                var devBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 0, 10) }; devBorder.Child = deviceInfo;
                Grid.SetColumn(devBorder, 0); grid.Children.Add(devBorder);

                // IMEI
                var imeiBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10) };
                imeiBorder.Child = new TextBlock { Text = report.Imei ?? "-", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
                Grid.SetColumn(imeiBorder, 1); grid.Children.Add(imeiBorder);

                // Serial
                var serialBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10) };
                serialBorder.Child = new TextBlock { Text = report.Serial ?? "-", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
                Grid.SetColumn(serialBorder, 2); grid.Children.Add(serialBorder);

                // Date
                var dateBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10) };
                dateBorder.Child = new TextBlock { Text = report.DateTime.ToString("MMM dd, HH:mm"), VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
                Grid.SetColumn(dateBorder, 3); grid.Children.Add(dateBorder);

                // Status (Pass/Fail)
                var badge = new Border { CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 2, 8, 2), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                var badgeText = new TextBlock { FontWeight = FontWeights.Bold, FontSize = 11 };
                if (isPass) { badge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DEF7EC")); badgeText.Text = "PASS"; badgeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#03543F")); }
                else { badge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDE8E8")); badgeText.Text = "FAIL"; badgeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B1C1C")); }
                badge.Child = badgeText;
                var badgeContainer = new Border { Background = Brushes.White }; badgeContainer.Child = badge;
                Grid.SetColumn(badgeContainer, 4); grid.Children.Add(badgeContainer);

                // Action (View)
                var actionBorder = new Border { Background = Brushes.White };
                var viewBtn = new Button { 
                    Content = new PackIcon { Kind = PackIconKind.Eye, Width = 18, Height = 18 }, 
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Width = 32, 
                    Height = 32, 
                    Tag = report,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                viewBtn.Click += ViewReport_Click;
                actionBorder.Child = viewBtn;
                Grid.SetColumn(actionBorder, 5); grid.Children.Add(actionBorder);

                ReportsTablePanel.Children.Add(grid);
            }

            // Update Report Stat Cards
            ReportStatTotal.Text = total.ToString();
            ReportStatPassed.Text = passed.ToString();
            ReportStatFailed.Text = failed.ToString();
            ReportStatRatio.Text = total > 0 ? $"{(int)((double)passed / total * 100)}%" : "0%";
        }

        private void ExportReports_Click(object sender, RoutedEventArgs e)
        {
            if (_lastFetchedReports == null || _lastFetchedReports.Count == 0)
            {
                MessageBox.Show("No data to export. Please search first.", "Information");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Excel CSV (*.csv)|*.csv",
                FileName = $"Phonova_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv",
                Title = "Export Detailed Reports to Excel"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    // Professional Header
                    sb.AppendLine("REPORT DATE,DEVICE NAME,MODEL,STORAGE,SERIAL NUMBER,IMEI,iOS,REGION,BATTERY %,CYCLES,OVERALL STATUS,KERNEL FAILURES,APP FAILURES,COMMENTS");

                    foreach (var r in _lastFetchedReports)
                    {
                        var kernelFailures = new List<string>();
                        var syslogFailures = new List<string>();
                        bool isPass = true;

                        // Check Kernel Tests
                        foreach (var test in r.KernelTests)
                        {
                            bool failed = test.Value.Equals("Fail", StringComparison.OrdinalIgnoreCase) || 
                                         test.Value.Equals("1") || 
                                         test.Value.Equals("Replaced", StringComparison.OrdinalIgnoreCase);
                            
                            if (failed)
                            {
                                kernelFailures.Add(test.Key);
                                isPass = false;
                            }
                        }

                        // Check Syslog Tests
                        foreach (var test in r.AppTests)
                        {
                            bool failed = test.Value.Equals("Fail", StringComparison.OrdinalIgnoreCase) || 
                                         test.Value.Equals("1") || 
                                         test.Value.Equals("No", StringComparison.OrdinalIgnoreCase);
                            
                            if (failed)
                            {
                                syslogFailures.Add(test.Key);
                                isPass = false;
                            }
                        }

                        string status = isPass ? "PASSED" : "FAILED";
                        string kFailStr = kernelFailures.Count > 0 ? string.Join("; ", kernelFailures) : "None";
                        string sFailStr = syslogFailures.Count > 0 ? string.Join("; ", syslogFailures) : "None";
                        string comments = r.Comments != null ? string.Join(" | ", r.Comments).Replace(",", ";") : "";
                        
                        // Formatting CSV line
                        sb.AppendLine($"{r.DateTime:yyyy-MM-dd HH:mm}," +
                                      $"\"{r.DeviceName}\"," +
                                      $"\"{r.Model}\"," +
                                      $"\"{r.Storage}\"," +
                                      $"\"{r.Serial}\"," +
                                      $"\"{r.Imei}\"," +
                                      $"\"{r.IosVersion}\"," +
                                      $"\"{r.Region}\"," +
                                      $"\"{r.BatteryHealth}%\"," +
                                      $"\"{r.BatteryCycles}\"," +
                                      $"\"{status}\"," +
                                      $"\"{kFailStr}\"," +
                                      $"\"{sFailStr}\"," +
                                      $"\"{comments}\"");
                    }

                    // Write with UTF8 BOM
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Detailed report exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchReports_Click(object sender, RoutedEventArgs e) => LoadReportsTable();

        private void ResetReportFilters_Click(object sender, RoutedEventArgs e)
        {
            ReportSearchBox.Text = "";
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            LoadReportsTable();
        }

        private void ReportSearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) LoadReportsTable();
        }

        private void ViewReport_Click(object sender, RoutedEventArgs e) 
        {
            if (sender is Button btn && btn.Tag is ProcessedDevice report)
            {
                var detailsWindow = new TestResultsWindow();
                detailsWindow.PopulateFromProcessedDevice(report);
                detailsWindow.Owner = this;
                detailsWindow.ShowDialog();
            }
        }
        
        // Dashboard analytics
        private void LoadDashboardStats()
        {
            try
            {
                // Mock empty list until backend stats endpoint is implemented
                var reports = new List<Phonova.Models.ProcessedDevice>();
                int total = reports.Count;
                int passed = 0;
                int failed = 0;

                foreach (var r in reports)
                {
                    bool isPass = true;
                    // Check Kernel Tests
                    foreach (var test in r.KernelTests.Values)
                    {
                        if (test == "Fail" || test == "1") { isPass = false; break; }
                    }
                    if (isPass)
                    {
                        // Check App Tests
                        foreach (var test in r.AppTests.Values)
                        {
                            if (test == "Fail" || test == "1") { isPass = false; break; }
                        }
                    }

                    if (isPass) passed++;
                    else failed++;
                }

                StatTotalTests.Text = total.ToString();
                StatPassed.Text = passed.ToString();
                StatFailed.Text = failed.ToString();
                StatSuccessRate.Text = total > 0 ? $"{(int)((double)passed / total * 100)}%" : "0%";

                // Prepare Chart Data (Top 5 Devices)
                var chartData = reports.Take(5).Select(r => {
                    int p = r.KernelTests.Values.Count(v => v == "Pass" || v == "0") + r.AppTests.Values.Count(v => v == "Pass" || v == "0" || v == "Yes");
                    int f = r.KernelTests.Values.Count(v => v == "Fail" || v == "1") + r.AppTests.Values.Count(v => v == "Fail" || v == "1" || v == "No");
                    return (r.DeviceName ?? "Device", p, f);
                }).ToList();

                DrawBarChart(chartData);

                var lineData = reports.GroupBy(r => r.DateTime.Date)
                    .OrderBy(g => g.Key)
                    .Take(7)
                    .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count()))
                    .ToList();
                
                DrawLineChart(lineData);

                ChartEmptyState.Visibility = total > 0 ? Visibility.Collapsed : Visibility.Visible;
                LineChartEmptyState.Visibility = total > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading dashboard: " + ex.Message);
            }
        }

        private void DrawBarChart(List<(string Name, int Pass, int Fail)> data)
        {
            if (ChartCanvas == null) return;
            ChartCanvas.Children.Clear();
            if (data.Count == 0) return;

            double width = ChartCanvas.ActualWidth > 0 ? ChartCanvas.ActualWidth : 500;
            double height = 180; // Leave room for labels
            double barWidth = (width / data.Count) * 0.5;
            double spacing = (width / data.Count) * 0.5;
            double maxVal = data.Max(x => x.Pass + x.Fail);
            if (maxVal == 0) maxVal = 1;

            for (int i = 0; i < data.Count; i++)
            {
                double x = (i * (barWidth + spacing)) + (spacing / 2);
                double pHeight = (data[i].Pass / maxVal) * height;
                double fHeight = (data[i].Fail / maxVal) * height;

                // Pass Bar
                var rPass = new System.Windows.Shapes.Rectangle { Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")), Width = barWidth, Height = pHeight, RadiusX = 2, RadiusY = 2 };
                Canvas.SetLeft(rPass, x);
                Canvas.SetBottom(rPass, 25); // Leave space for X-axis label
                ChartCanvas.Children.Add(rPass);

                // Fail Bar (Stacked)
                var rFail = new System.Windows.Shapes.Rectangle { Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")), Width = barWidth, Height = fHeight, RadiusX = 2, RadiusY = 2 };
                Canvas.SetLeft(rFail, x);
                Canvas.SetBottom(rFail, 25 + pHeight);
                ChartCanvas.Children.Add(rFail);

                // Device Name Label
                var lbl = new TextBlock { Text = data[i].Name, FontSize = 10, Foreground = Brushes.Gray, Width = barWidth + spacing, TextAlignment = TextAlignment.Center };
                Canvas.SetLeft(lbl, x - (spacing / 4));
                Canvas.SetBottom(lbl, 5);
                ChartCanvas.Children.Add(lbl);

                // Count Label (Top)
                if (data[i].Pass + data[i].Fail > 0)
                {
                    var countLbl = new TextBlock { Text = (data[i].Pass + data[i].Fail).ToString(), FontSize = 9, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, Width = barWidth, TextAlignment = TextAlignment.Center };
                    Canvas.SetLeft(countLbl, x);
                    Canvas.SetBottom(countLbl, 25 + pHeight + fHeight + 2);
                    ChartCanvas.Children.Add(countLbl);
                }
            }
        }

        private void DrawLineChart(List<KeyValuePair<DateTime, int>> data)
        {
            if (LineChartCanvas == null) return;
            LineChartCanvas.Children.Clear();
            if (data.Count == 0) return;

            double width = LineChartCanvas.ActualWidth > 0 ? LineChartCanvas.ActualWidth : 500;
            double height = 120;
            double spacing = data.Count > 1 ? width / (data.Count - 1) : width;
            double maxVal = data.Max(x => x.Value);
            if (maxVal == 0) maxVal = 1;

            if (data.Count > 1)
            {
                var polyline = new System.Windows.Shapes.Polyline { Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#027dfe")), StrokeThickness = 2, StrokeLineJoin = PenLineJoin.Round };
                for (int i = 0; i < data.Count; i++)
                {
                    double x = i * spacing;
                    double y = height - ((data[i].Value / maxVal) * height);
                    polyline.Points.Add(new Point(x, y + 10)); // +10 for padding
                }
                LineChartCanvas.Children.Add(polyline);
            }

            for (int i = 0; i < data.Count; i++)
            {
                double x = i * spacing;
                double y = height - ((data[i].Value / maxVal) * height) + 10;

                // Point Circle
                var dot = new System.Windows.Shapes.Ellipse { Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#027dfe")), Width = 6, Height = 6 };
                Canvas.SetLeft(dot, x - 3);
                Canvas.SetTop(dot, y - 3);
                LineChartCanvas.Children.Add(dot);

                // Value Label
                var valLbl = new TextBlock { Text = data[i].Value.ToString(), FontSize = 9, FontWeight = FontWeights.Bold, Foreground = Brushes.SlateGray };
                Canvas.SetLeft(valLbl, x - 5);
                Canvas.SetTop(valLbl, y - 18);
                LineChartCanvas.Children.Add(valLbl);

                // Date Label
                var dateLbl = new TextBlock { Text = data[i].Key.ToString("MM/dd"), FontSize = 8, Foreground = Brushes.Gray };
                Canvas.SetLeft(dateLbl, x - 10);
                Canvas.SetBottom(dateLbl, -15);
                LineChartCanvas.Children.Add(dateLbl);
            }
        }
    }
}
