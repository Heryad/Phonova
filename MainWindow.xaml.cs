using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using Phonova.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Phonova
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int DashboardCardCount = 20;
        private readonly Dictionary<int, DeviceCard> _portCards = new();
        private readonly iOSCommander _iosCommander = new();
        public static string? SelectedCustomer { get; set; } = null;
        public static string? SelectedTester { get; set; } = null;
        public static string? SelectedMmrComment { get; set; } = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDashboard();
            StartInternetCheck();
        }

        protected override async void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            await App.DeviceDetector.StartAsync(hwnd);
        }

        private async void InitializeDashboard()
        {
            App.DeviceDetector.DeviceConnected += OnDeviceConnected;
            App.DeviceDetector.DeviceDisconnected += OnDeviceDisconnected;
            App.PortMapper.MappingUpdated += OnMappingUpdated;

            // Always show a fixed number of slots/cards on the dashboard.
            EnsureDeviceCards(DashboardCardCount);
            UpdateDeviceCount();    
            UpdateSelectedCount();  
            UpdateCustomerHeaderUi();  
            UpdateMmrHeaderVisibility();
        }

        private void OnDeviceConnected(object? sender, AppleDeviceEventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                var port = await App.PortMapper.GetPortNumberAsync(e.Device.LocationPath);
                if (port.HasValue && _portCards.TryGetValue(port.Value, out var card))
                {
                    // Only assign UDID to the card – no extra details.
                    card.setDevice(e.Device.Udid ?? "Unknown UDID", e.Device.LocationPath, port.Value);
                }

                UpdateDeviceCount();
            });
        }

        private void OnDeviceDisconnected(object? sender, AppleDeviceEventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                // Use port mapping (same as connect) so we always find the right slot,
                // even if UDID is missing or has already been cleared.
                var port = await App.PortMapper.GetPortNumberAsync(e.Device.LocationPath);
                if (port.HasValue && _portCards.TryGetValue(port.Value, out var card))
                {
                    card.ClearDevice();
                }

                UpdateDeviceCount();
            });
        }

        private void OnMappingUpdated(object? sender, EventArgs e)
        {
            // Re-render to ensure we still have 20 slots after mapping changes.
            EnsureDeviceCards(DashboardCardCount);
            UpdateDeviceCount();
        }

        private void UpdateDeviceCount()
        {
            DeviceCountText.Text = App.DeviceDetector.ActiveDeviceCount.ToString();
        }

        private void EnsureDeviceCards(int count)
        {
            if (count <= 0) return;

            // Recreate deterministically (simple + keeps UI in sync).
            _portCards.Clear();
            DeviceCardsPanel.Children.Clear();

            for (int port = 1; port <= count; port++)
            {
                var card = new DeviceCard
                {
                    Margin = new Thickness(6, 5, 6, 5)
                };

                // Assign the fixed port number for this dashboard slot.
                card.setDevice(string.Empty, string.Empty, port);

                // For now we only care about UDID assignment when a real device appears.
                card.OnSelectionChanged += (_, _) => UpdateSelectedCount();

                _portCards[port] = card;
                DeviceCardsPanel.Children.Add(card);
            }
        }

        private void UpdateSelectedCount()
        {
            try
            {
                var selectedDevices = _portCards.Values.Where(c => c.IsSelected && !string.IsNullOrEmpty(c.DeviceId) && c.DeviceId != "Unknown UDID").ToList();
                var count = selectedDevices.Count;
                SelectedCountText.Text = $"{count} selected";

                // Enable/Disable buttons based on whether any real device is selected
                RebootBatchBtn.IsEnabled = count > 0;
                ShutdownBatchBtn.IsEnabled = count > 0;
                WipeBatchBtn.IsEnabled = count > 0;
            }
            catch
            {
                // Ignore if UI not ready.
            }
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var card in _portCards.Values)
            {
                if (!string.IsNullOrEmpty(card.DeviceId) && card.DeviceId != "Unknown UDID")
                {
                    card.IsSelected = true;
                }
            }
            UpdateSelectedCount();
        }
 
        private void DeselectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var card in _portCards.Values)
            {
                card.IsSelected = false;
            }
            UpdateSelectedCount();
        }
 
        private void PrintAllBtn_Click(object sender, RoutedEventArgs e) { }
        
        private async void RebootAllBtn_Click(object sender, RoutedEventArgs e) 
        {
            var selectedUdids = GetSelectedUdids();
            if (!selectedUdids.Any()) return;

            await ProcessInBatches(selectedUdids, udid => _iosCommander.RebootDevice(udid));
        }

        private async void ShutdownAllBtn_Click(object sender, RoutedEventArgs e) 
        {
            var selectedUdids = GetSelectedUdids();
            if (!selectedUdids.Any()) return;

            await ProcessInBatches(selectedUdids, udid => _iosCommander.ShutdownDevice(udid));
        }

        private async void WipeAllBtn_Click(object sender, RoutedEventArgs e) 
        {
            var selectedUdids = GetSelectedUdids();
            if (!selectedUdids.Any()) return;

            var result = MessageBox.Show($"Are you sure you want to WIPE {selectedUdids.Count()} devices? This cannot be undone.", 
                "Confirm Wipe", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                await ProcessInBatches(selectedUdids, udid => _iosCommander.WipeDevice(udid));
            }
        }

        private IEnumerable<string> GetSelectedUdids()
        {
            return _portCards.Values
                .Where(c => c.IsSelected && !string.IsNullOrEmpty(c.DeviceId) && c.DeviceId != "Unknown UDID")
                .Select(c => c.DeviceId);
        }

        private async Task ProcessInBatches(IEnumerable<string> udids, Func<string, string> action)
        {
            var udidList = udids.ToList();
            int batchSize = 3;

            for (int i = 0; i < udidList.Count; i += batchSize)
            {
                var batch = udidList.Skip(i).Take(batchSize);
                var tasks = batch.Select(udid => Task.Run(() => action(udid)));
                await Task.WhenAll(tasks);
            }
            
            MessageBox.Show("Operation completed for selected devices.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void RestoreAllBtn_Click(object sender, RoutedEventArgs e) { }
        private void DisconnectAllBtn_Click(object sender, RoutedEventArgs e) { }
        private void InstallAppBtn_Click(object sender, RoutedEventArgs e) { }
        private void WifiProfileBtn_Click(object sender, RoutedEventArgs e) { }
        private void DashboardBtn_Click(object sender, RoutedEventArgs e) { }
        private void SettingsBtn_Click(object sender, RoutedEventArgs e) {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Closed += (s, ev) => {
                SettingsManager.Load();
                UpdateMmrHeaderVisibility();
            };
            settingsWindow.Show();
        }
        private void PortmapBtn_Click(object sender, RoutedEventArgs e) { 
            PortMapWindow portMapWindow = new PortMapWindow();
            portMapWindow.Show();
        }

         private void MinimizeBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

        private void UpdateCustomerHeaderUi()
        {
            if (string.IsNullOrEmpty(SelectedCustomer))
            {
                HeaderCustomerText.Text = "Select Customer";
                HeaderCustomerClearBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                HeaderCustomerText.Text = SelectedCustomer;
                HeaderCustomerClearBtn.Visibility = Visibility.Visible;
            }
        }

        private void HeaderCustomerBtn_Click(object sender, RoutedEventArgs e)
        {
            var customerWindow = new CustomerSelectWindow(SelectedCustomer);
            customerWindow.Owner = this;
            customerWindow.ShowDialog();

            if (customerWindow.Confirmed)
            {
                SelectedCustomer = customerWindow.SelectedCustomer;
                UpdateCustomerHeaderUi();
                Debug.WriteLine($"[MainWindow] Selected Customer updated globally to: {SelectedCustomer ?? "None"}");
            }
        }

        private void HeaderCustomerClearBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedCustomer = null;
            UpdateCustomerHeaderUi();
            Debug.WriteLine("[MainWindow] Selected Customer cleared globally.");
        }

        private void UpdateTesterHeaderUi()
        {
            if (string.IsNullOrEmpty(SelectedTester))
            {
                HeaderTesterText.Text = "Select Tester";
                HeaderTesterClearBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                HeaderTesterText.Text = SelectedTester;
                HeaderTesterClearBtn.Visibility = Visibility.Visible;
            }
        }

        private void HeaderTesterBtn_Click(object sender, RoutedEventArgs e)
        {
            var testerWindow = new TesterSelectWindow(SelectedTester);
            testerWindow.Owner = this;
            testerWindow.ShowDialog();

            if (testerWindow.Confirmed)
            {
                SelectedTester = testerWindow.SelectedTester;
                UpdateTesterHeaderUi();
                Debug.WriteLine($"[MainWindow] Selected Tester updated globally to: {SelectedTester ?? "None"}");
            }
        }

        private void HeaderTesterClearBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedTester = null;
            UpdateTesterHeaderUi();
            Debug.WriteLine("[MainWindow] Selected Tester cleared globally.");
        }
 
        private System.Windows.Threading.DispatcherTimer? _internetCheckTimer;
 
        private void StartInternetCheck()
        {
            _internetCheckTimer = new System.Windows.Threading.DispatcherTimer();
            _internetCheckTimer.Interval = TimeSpan.FromSeconds(5);
            _internetCheckTimer.Tick += async (s, e) => await CheckInternetStatusAsync();
            _internetCheckTimer.Start();
            
            _ = CheckInternetStatusAsync();
        }
 
        private async Task CheckInternetStatusAsync()
        {
            bool isConnected = false;
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(2);
                    using (var response = await client.GetAsync("https://www.google.com", System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                    {
                        isConnected = response.IsSuccessStatusCode;
                    }
                }
            }
            catch
            {
                isConnected = false;
            }
 
            Dispatcher.Invoke(() =>
            {
                if (isConnected)
                {
                    InternetStatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Wifi;
                    InternetStatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    InternetStatusBorder.ToolTip = "Internet Connected";
                }
                else
                {
                    InternetStatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WifiOff;
                    InternetStatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    InternetStatusBorder.ToolTip = "Internet Offline";
                }
            });
        }

        private void UpdateMmrHeaderVisibility()
        {
            var isMmrMode = SettingsManager.Current.MmrMode;
            if (isMmrMode)
            {
                HeaderMmrCommentBtn.Visibility = Visibility.Visible;
                UpdateMmrCommentHeaderUi();
            }
            else
            {
                HeaderMmrCommentBtn.Visibility = Visibility.Collapsed;
                HeaderMmrCommentClearBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateMmrCommentHeaderUi()
        {
            if (string.IsNullOrEmpty(SelectedMmrComment))
            {
                HeaderMmrCommentText.Text = "Select MMR Comment";
                HeaderMmrCommentClearBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                HeaderMmrCommentText.Text = SelectedMmrComment;
                HeaderMmrCommentClearBtn.Visibility = Visibility.Visible;
            }
        }

        private void HeaderMmrCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            var existing = string.IsNullOrEmpty(SelectedMmrComment) ? null : new List<string> { SelectedMmrComment };
            var mmrSelectWindow = new MmrCommentSelectWindow(existing);
            mmrSelectWindow.Owner = this;
            mmrSelectWindow.ShowDialog();

            if (mmrSelectWindow.Confirmed)
            {
                SelectedMmrComment = mmrSelectWindow.SelectedMmrComments.FirstOrDefault();
                UpdateMmrCommentHeaderUi();
                Debug.WriteLine($"[MainWindow] Selected MMR Comment updated globally to: {SelectedMmrComment ?? "None"}");
            }
        }

        private void HeaderMmrCommentClearBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedMmrComment = null;
            UpdateMmrCommentHeaderUi();
            Debug.WriteLine("[MainWindow] Selected MMR Comment cleared globally.");
        }
    }
}