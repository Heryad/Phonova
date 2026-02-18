using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dyagnoz_Latest.Services;
using MaterialDesignThemes.Wpf;

namespace Dyagnoz_Latest
{
    public partial class SettingsWindow : Window
    {
        private readonly string _configPath;
        private Button? _activeNavButton;


        public SettingsWindow()
        {
            InitializeComponent();
            _configPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dyagnoz");
            if (!System.IO.Directory.Exists(_configPath)) System.IO.Directory.CreateDirectory(_configPath);
            _activeNavButton = NavDashboard;
            LoadSavedProfiles();
            LoadAndEnsureTestProfile();
            LoadTestListUI();
            LoadCommentsTable();
        }

        private void AutoWipeToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoShutdownToggle != null)
                AutoShutdownToggle.IsChecked = false;
        }

        private void AutoShutdownToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoWipeToggle != null)
                AutoWipeToggle.IsChecked = false;
        }

        private async void SaveTestFlow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = new Dictionary<string, bool>
                {
                    { "AutoWipe", AutoWipeToggle.IsChecked ?? false },
                    { "AutoPrint", AutoPrintToggle.IsChecked ?? false },
                    { "AutoShutdown", AutoShutdownToggle.IsChecked ?? false },
                    { "AutoInstall", AutoInstallToggle.IsChecked ?? false }
                };

                string filePath = System.IO.Path.Combine(_configPath, "GlobalSettings.json");
                string json = System.Text.Json.JsonSerializer.Serialize(settings);
                await System.IO.File.WriteAllTextAsync(filePath, json);
                MessageBox.Show("Test Flow settings saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving flow settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    using (var doc = System.Text.Json.JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("Test", out var testProp))
                        {
                            var parts = testProp.GetString()?.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            if (parts != null) foreach (var p in parts) enabledTests.Add(p.Trim());
                        }

                        // Read Params
                        foreach (var prop in root.EnumerateObject())
                        {
                            if (prop.Name != "Test")
                            {
                                paramsValues[prop.Name] = prop.Value.ToString();
                            }
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
                    using (var doc = System.Text.Json.JsonDocument.Parse(json))
                    {
                        foreach (var prop in doc.RootElement.EnumerateObject())
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
                string outJson = System.Text.Json.JsonSerializer.Serialize(finalObj);
                await System.IO.File.WriteAllTextAsync(filePath, outJson, System.Text.Encoding.UTF8);
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
				<string>WPA</string>
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

                string xml = template
                    .Replace("{PASSWORD}", password)
                    .Replace("{SSID}", ssid)
                    .Replace("{CLEAN_SSID}", cleanSsid)
                    .Replace("{CLEAN_SSID_UUID}", Guid.NewGuid().ToString())
                    .Replace("{UUID1}", payloadUuid1)
                    .Replace("{UUID2}", payloadUuid2);

                string fileName = "wifi-sys.mobileconfig";
                string filePath = System.IO.Path.Combine(_configPath, fileName);
                await System.IO.File.WriteAllTextAsync(filePath, xml, System.Text.Encoding.UTF8);
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
            PanelPrint.Visibility = Visibility.Collapsed;
            PanelWifi.Visibility = Visibility.Collapsed;
            PanelTestFlow.Visibility = Visibility.Collapsed;
            PanelLicense.Visibility = Visibility.Collapsed;
            PanelAbout.Visibility = Visibility.Collapsed;

            // Reset all nav buttons
            NavDashboard.Style = (Style)FindResource("NavBtn");
            NavReports.Style = (Style)FindResource("NavBtn");
            NavTests.Style = (Style)FindResource("NavBtn");
            NavComments.Style = (Style)FindResource("NavBtn");
            NavPrint.Style = (Style)FindResource("NavBtn");
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
                case "Print": PanelPrint.Visibility = Visibility.Visible; break;
                case "Wifi": PanelWifi.Visibility = Visibility.Visible; break;
                case "TestFlow": PanelTestFlow.Visibility = Visibility.Visible; break;
                case "License": PanelLicense.Visibility = Visibility.Visible; break;
                case "About": PanelAbout.Visibility = Visibility.Visible; break;
            }

            // Set active nav button
            navButton.Style = (Style)FindResource("NavBtnActive");
            _activeNavButton = navButton;

            if (panelName == "Comments") LoadCommentsTable();
            if (panelName == "Reports") LoadReportsTable();
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e) => ShowPanel("Dashboard", NavDashboard);
        private void NavReports_Click(object sender, RoutedEventArgs e) => ShowPanel("Reports", NavReports);
        private void NavTests_Click(object sender, RoutedEventArgs e) => ShowPanel("Tests", NavTests);
        private void NavComments_Click(object sender, RoutedEventArgs e) => ShowPanel("Comments", NavComments);
        private void NavPrint_Click(object sender, RoutedEventArgs e) => ShowPanel("Print", NavPrint);
        private void NavWifi_Click(object sender, RoutedEventArgs e) => ShowPanel("Wifi", NavWifi);
        private void NavTestFlow_Click(object sender, RoutedEventArgs e) => ShowPanel("TestFlow", NavTestFlow);
        private void NavLicense_Click(object sender, RoutedEventArgs e) => ShowPanel("License", NavLicense);
        private void NavAbout_Click(object sender, RoutedEventArgs e) => ShowPanel("About", NavAbout);

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
        
        // Comments CRUD
        private void LoadCommentsTable()
        {
            if (CommentsTablePanel == null) return;
            CommentsTablePanel.Children.Clear();

            var comments = App.Database.GetAllComments();
            foreach (var comment in comments)
            {
                var grid = new Grid { Margin = new Thickness(0, 0, 0, 1) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                var textBorder = new Border { Background = Brushes.White, Padding = new Thickness(12, 10, 12, 10) };
                textBorder.Child = new TextBlock { Text = comment, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
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

        private void AddCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            string newComment = NewCommentBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(newComment)) return;

            App.Database.AddCommentToLibrary(newComment);
            NewCommentBox.Text = "";
            LoadCommentsTable();
        }

        private void DeleteComment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string comment)
            {
                if (MessageBox.Show($"Are you sure you want to delete '{comment}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    App.Database.DeleteCommentFromLibrary(comment);
                    LoadCommentsTable();
                }
            }
        }
        
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

            var reports = App.Database.GetProcessedDevices(search, start, end);

            foreach (var report in reports)
            {
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
                int failed = report.KernelTests.Count(x => x.Value != "Pass") + report.AppTests.Count(x => x.Value != "Pass");
                var badge = new Border { CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 2, 8, 2), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                var badgeText = new TextBlock { FontWeight = FontWeights.Bold, FontSize = 11 };
                if (failed == 0) { badge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DEF7EC")); badgeText.Text = "PASS"; badgeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#03543F")); }
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
        private void LoadDashboardStats() { }
        private void DrawBarChart(List<(string Name, int Pass, int Fail)> data) { }
        private void DrawLineChart(List<KeyValuePair<DateTime, int>> data) { }
    }
}
