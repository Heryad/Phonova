using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using Dyagnoz_Latest.Services;
using System.Diagnostics;

namespace Dyagnoz_Latest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int DashboardCardCount = 20;
        private readonly Dictionary<int, DeviceCard> _portCards = new();

        public MainWindow()
        {
            InitializeComponent();
            InitializeDashboard();
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
                var selected = _portCards.Values.Count(c => c.IsSelected);
                SelectedCountText.Text = $"{selected} selected";
            }
            catch
            {
                // Ignore if UI not ready.
            }
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e) { }
        private void PrintAllBtn_Click(object sender, RoutedEventArgs e) { }
        private void RebootAllBtn_Click(object sender, RoutedEventArgs e) { }
        private void ShutdownAllBtn_Click(object sender, RoutedEventArgs e) { }
        private void WipeAllBtn_Click(object sender, RoutedEventArgs e) { }
        private void RestoreAllBtn_Click(object sender, RoutedEventArgs e) { }
        private void DisconnectAllBtn_Click(object sender, RoutedEventArgs e) { }
        private void InstallAppBtn_Click(object sender, RoutedEventArgs e) { }
        private void WifiProfileBtn_Click(object sender, RoutedEventArgs e) { }
        private void DashboardBtn_Click(object sender, RoutedEventArgs e) { }
        private void SettingsBtn_Click(object sender, RoutedEventArgs e) {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }
        private void PortmapBtn_Click(object sender, RoutedEventArgs e) { 
            PortMapWindow portMapWindow = new PortMapWindow();
            portMapWindow.Show();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
    }
}