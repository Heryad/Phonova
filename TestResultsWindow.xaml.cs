using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using Phonova.Services;
using Phonova.Models;
using MaterialDesignThemes.Wpf;

namespace Phonova
{
    public partial class TestResultsWindow : Window
    {
        private ProcessedDevice? _currentDevice;

        public TestResultsWindow()
        {
            InitializeComponent();
        }
        
        public void PopulateFromProcessedDevice(ProcessedDevice device)
        {
            _currentDevice = device;
            SetDeviceInfo(device.DeviceName ?? "Unknown", device.Serial ?? "-");
            SetModel(device.Model ?? "-");
            SetStorage(device.Storage ?? "-");
            SetImei(device.Imei ?? "-", "-");
            SetIosVersion(device.IosVersion ?? "-");
            SetBattery(device.BatteryHealth ?? "-", device.BatteryCycles ?? "-");
            SetRegion(device.Region ?? "-"); 
            SetColor(device.Color ?? "-", device.ProductType, device.EnclosureCode);
            SetLockStatus(device.IcloudStatus ?? "-", device.FmiStatus ?? "-", device.MdmStatus ?? "-", device.SimStatus ?? "-");
            
            ClearTestResults();

            var components = new List<(string Name, bool? Ok)>();
            int passCount = 0;
            int totalTests = 0;

            // Define which keys are components for categorization
            var componentKeys = new[] { "LCD", "Battery", "FaceID", "TouchID", "Camera", "Screen" };

            // Process Kernel Tests
            foreach (var test in device.KernelTests)
            {
                bool isComponent = componentKeys.Any(c => test.Key.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0);
                bool passed = test.Value.Equals("Pass", StringComparison.OrdinalIgnoreCase) || 
                              test.Value.Equals("Original", StringComparison.OrdinalIgnoreCase);

                if (isComponent)
                {
                    components.Add((test.Key, passed));
                }
                else
                {
                    totalTests++;
                    if (passed) passCount++;
                    AddTestResult(test.Key, passed);
                }
            }

            // Process App/Syslog Tests
            foreach (var test in device.AppTests)
            {
                // In syslog results, "0" means Pass
                bool passed = test.Value == "0" || test.Value.Equals("Pass", StringComparison.OrdinalIgnoreCase);
                
                totalTests++;
                if (passed) passCount++;
                AddTestResult(test.Key, passed);
            }

            SetComponentData(components);
            SetOverallStatus(passCount == totalTests && totalTests > 0, passCount, totalTests);
            SetComments(device.Comments);
            TestDateText.Text = device.DateTime.ToString("MMM dd, yyyy HH:mm");
            SetTester("-");
        }

        // Set device information
        public void SetDeviceInfo(string deviceName, string serial)
        {
            DeviceNameText.Text = deviceName;
            SerialText.Text = serial;
        }

        public void SetModel(string model) => ModelText.Text = model;
        public void SetStorage(string storage) => StorageText.Text = storage;
        public void SetImei(string imei1, string imei2)
        {
            Imei1Text.Text = imei1;
            Imei2Text.Text = imei2;
        }
        public void SetIosVersion(string version) => IosVersionText.Text = version;
        public void SetBattery(string percent, string cycles)
        {
            BatteryText.Text = percent;
            BatteryCyclesText.Text = cycles;
        }
        public void SetRegion(string region) => RegionText.Text = region;
        public void SetColor(string color, string? productType = null, string? enclosureCode = null)
        {
            ColorText.Text = color;
            try {
                // Use the shared mapping logic from DeviceColorMap
                // Prefer EnclosureCode if productType is present, otherwise fallback to marketing name
                string hex = Phonova.Models.DeviceColorMap.GetColorHex(productType, enclosureCode ?? color);
                
                if (!string.IsNullOrWhiteSpace(hex) && hex != "Transparent")
                {
                    ColorDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
                }
                else
                {
                    ColorDot.Fill = Brushes.Transparent;
                }
            } catch { 
                ColorDot.Fill = Brushes.Transparent;
            }
        }

        // Set lock statuses (accepts strings like "OFF", "FREE", "ON", "LOCK")
        public void SetLockStatus(string icloud, string fmi, string mdm, string sim)
        {
            bool icl = icloud.Equals("OFF", StringComparison.OrdinalIgnoreCase) || icloud.Equals("Clean", StringComparison.OrdinalIgnoreCase);
            bool f = fmi.Equals("OFF", StringComparison.OrdinalIgnoreCase) || fmi.Equals("Clean", StringComparison.OrdinalIgnoreCase);
            bool m = mdm.Equals("OFF", StringComparison.OrdinalIgnoreCase) || mdm.Equals("Clean", StringComparison.OrdinalIgnoreCase) || mdm.Equals("No", StringComparison.OrdinalIgnoreCase);
            
            SetStatusBadge(IcloudBadge, IcloudText, icloud, icl);
            SetStatusBadge(FmiBadge, FmiText, fmi, f);
            SetStatusBadge(MdmBadge, MdmText, mdm, m);

            // SIM Status (special case for eSIM/Normal/Physical)
            SimText.Text = sim;
            if (sim.IndexOf("eSIM", StringComparison.OrdinalIgnoreCase) >= 0 || 
                sim.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                sim.IndexOf("Physical", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                SimBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE")); // Blue
                SimText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E40AF"));
            }
            else
            {
                SetStatusBadge(SimBadge, SimText, sim, sim.Equals("FREE", StringComparison.OrdinalIgnoreCase) || sim.Equals("UNLOCKED", StringComparison.OrdinalIgnoreCase));
            }
        }

        // Set OEMR Parts Validation (Grid of 3)
        public void SetComponentData(List<(string Name, bool? Ok)> components)
        {
            ComponentPanel.Children.Clear();
            foreach (var comp in components)
            {
                ComponentPanel.Children.Add(CreateComponentItem(comp.Name, comp.Ok));
            }
        }

        private Border CreateComponentItem(string name, bool? isOk)
        {
            var badge = new Border
            {
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, 0, 8, 8),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isOk == true ? "#DEF7EC" : (isOk == false ? "#FEE2E2" : "#FEF3C7")))
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal };
            
            var icon = new PackIcon 
            { 
                Kind = isOk == true ? PackIconKind.Check : (isOk == false ? PackIconKind.Close : PackIconKind.Help),
                Width = 14, Height = 14, VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isOk == true ? "#10B981" : (isOk == false ? "#EF4444" : "#F59E0B")))
            };

            var text = new TextBlock
            {
                Text = $"{name}: {(isOk == true ? "ORIGINAL" : (isOk == false ? "REPLACED" : "N/A"))}",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(6, 0, 0, 0),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isOk == true ? "#10B981" : (isOk == false ? "#EF4444" : "#F59E0B")))
            };

            stack.Children.Add(icon);
            stack.Children.Add(text);
            badge.Child = stack;
            return badge;
        }


        // Add dynamic test results
        public void AddTestResult(string testName, bool passed)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 6)
            };
            
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            var nameText = new TextBlock
            {
                Text = testName,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A2E")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameText, 0);
            
            string status = passed ? "PASS" : "FAIL";
            string bgColor = passed ? "#DEF7EC" : "#FEE2E2";
            string fgColor = passed ? "#10B981" : "#EF4444";
            
            var badge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 4, 10, 4)
            };
            Grid.SetColumn(badge, 1);
            
            var statusText = new TextBlock
            {
                Text = status,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgColor))
            };
            
            badge.Child = statusText;
            grid.Children.Add(nameText);
            grid.Children.Add(badge);
            border.Child = grid;
            
            TestResultsPanel.Children.Add(border);
        }

        // Clear all test results
        public void ClearTestResults()
        {
            TestResultsPanel.Children.Clear();
        }

        private void SetStatusBadge(Border badge, TextBlock text, string status, bool isGood)
        {
            text.Text = status;
            if (isGood)
            {
                badge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DEF7EC"));
                text.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            }
            else
            {
                badge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                text.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
            }
        }

        // Set overall status
        public void SetOverallStatus(bool allPassed, int passCount, int totalCount)
        {
            if (allPassed)
            {
                OverallStatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DEF7EC"));
                OverallStatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
                OverallStatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                OverallStatusText.Text = "ALL TESTS PASSED";
                OverallStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                OverallStatusSubtext.Text = "Device is in excellent condition";
            }
            else
            {
                OverallStatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                OverallStatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.AlertCircle;
                OverallStatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                OverallStatusText.Text = $"{passCount}/{totalCount} TESTS PASSED";
                OverallStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                OverallStatusSubtext.Text = "Some tests failed - review results";
            }
        }

        public void SetTester(string tester)
        {
            TesterText.Text = tester;
        }

        // Button handlers
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }



        private async void CertificateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDevice == null) return;

            // Show Loading
            LoadingOverlay.Visibility = Visibility.Visible;
            CertificateBtn.IsEnabled = false;

            try
            {
                // Temporarily disable Topmost so the preview can come to the front
                bool wasTopmost = this.Topmost;
                this.Topmost = false;

                // Move heavy report generation to a background thread
                var report = await Task.Run(() => new Labels.DeviceCertificate(_currentDevice));
                
                var tool = new DevExpress.XtraReports.UI.ReportPrintTool(report);
                tool.ShowPreviewDialog();

                // Restore Topmost status
                this.Topmost = wasTopmost;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating certificate: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Hide Loading
                LoadingOverlay.Visibility = Visibility.Collapsed;
                CertificateBtn.IsEnabled = true;
            }
        }
        
        // Set comments section
        public void SetComments(List<string>? comments)
        {
            CommentsListPanel.Children.Clear();
            
            if (comments == null || comments.Count == 0)
            {
                CommentsSection.Visibility = Visibility.Collapsed;
                return;
            }
            
            CommentsSection.Visibility = Visibility.Visible;
            
            foreach (var comment in comments)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush(Colors.White),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(10, 8, 10, 8),
                    Margin = new Thickness(0, 0, 0, 6)
                };
                
                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                var bullet = new TextBlock
                {
                    Text = "•",
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97706")),
                    Margin = new Thickness(0, 0, 8, 0)
                };
                var text = new TextBlock
                {
                    Text = comment,
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#78350F")),
                    TextWrapping = TextWrapping.Wrap
                };
                sp.Children.Add(bullet);
                sp.Children.Add(text);
                border.Child = sp;
                
                CommentsListPanel.Children.Add(border);
            }
        }
    }
}
