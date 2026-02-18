using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dyagnoz_Latest.Services;

namespace Dyagnoz_Latest
{
    public partial class PortMapWindow : Window
    {
        private Dictionary<int, Border> portTiles = new Dictionary<int, Border>();
        private Dictionary<int, TextBlock> portStatusLabels = new Dictionary<int, TextBlock>();
        private bool _mappingChangedSinceOpen = false;
        
        // Colors
        private readonly SolidColorBrush ConnectedColor = CreateFrozenBrush("#10B981");
        private readonly SolidColorBrush MappedColor = CreateFrozenBrush("#3B82F6");
        private readonly SolidColorBrush UnmappedColor = CreateFrozenBrush("#9CA3AF");

        private static SolidColorBrush CreateFrozenBrush(string hex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }

        public PortMapWindow()
        {
            InitializeComponent();
            InitializePortTiles();
            
            // Initial UI Sync
            RefreshPortStatus();

            // Event Subscriptions
            App.PortMapper.MappingUpdated += OnMappingUpdated;
            App.PortMapper.LearningModeChanged += OnLearningModeChanged;
            App.DeviceDetector.DeviceConnected += OnDeviceConnectedEvent;
        }

        private void InitializePortTiles()
        {
            portTiles[1] = Port1; portStatusLabels[1] = Port1Status;
            portTiles[2] = Port2; portStatusLabels[2] = Port2Status;
            portTiles[3] = Port3; portStatusLabels[3] = Port3Status;
            portTiles[4] = Port4; portStatusLabels[4] = Port4Status;
            portTiles[5] = Port5; portStatusLabels[5] = Port5Status;
            portTiles[6] = Port6; portStatusLabels[6] = Port6Status;
            portTiles[7] = Port7; portStatusLabels[7] = Port7Status;
            portTiles[8] = Port8; portStatusLabels[8] = Port8Status;
            portTiles[9] = Port9; portStatusLabels[9] = Port9Status;
            portTiles[10] = Port10; portStatusLabels[10] = Port10Status;
            portTiles[11] = Port11; portStatusLabels[11] = Port11Status;
            portTiles[12] = Port12; portStatusLabels[12] = Port12Status;
            portTiles[13] = Port13; portStatusLabels[13] = Port13Status;
            portTiles[14] = Port14; portStatusLabels[14] = Port14Status;
            portTiles[15] = Port15; portStatusLabels[15] = Port15Status;
            portTiles[16] = Port16; portStatusLabels[16] = Port16Status;
            portTiles[17] = Port17; portStatusLabels[17] = Port17Status;
            portTiles[18] = Port18; portStatusLabels[18] = Port18Status;
            portTiles[19] = Port19; portStatusLabels[19] = Port19Status;
            portTiles[20] = Port20; portStatusLabels[20] = Port20Status;

            // Determine if we are already learning
             UpdateButtonState(App.PortMapper.IsLearningMode);
        }

        private void RefreshPortStatus()
        {
            var mappings = App.PortMapper.GetAllMappings();

            // Reset visuals first
            foreach (var tile in portTiles.Values) tile.Background = UnmappedColor;
            foreach (var lbl in portStatusLabels.Values) lbl.Text = "—";

            // Apply mappings
            foreach (var m in mappings)
            {
                if (portTiles.ContainsKey(m.LogicalPort))
                {
                    // Prioritize Mapped Color (Blue) as requested
                    portTiles[m.LogicalPort].Background = MappedColor;
                    portStatusLabels[m.LogicalPort].Text = "✓";
                }
            }
        }

        // --- Event Handlers ---

        private void OnMappingUpdated(object? sender, EventArgs e)
        {
            // Use BeginInvoke to avoid blocking the background thread that might be holding a lock
            Dispatcher.BeginInvoke(new Action(RefreshPortStatus));
        }
        private void OnLearningModeChanged(object? sender, bool isLearning)
        {
             Dispatcher.Invoke(() => UpdateButtonState(isLearning));
        }

        private async void OnDeviceConnectedEvent(object? sender, AppleDeviceEventArgs e)
        {
            // If we are learning, assign this new path to the next port
            if (App.PortMapper.IsLearningMode)
            {
                var assignedPort = await App.PortMapper.AssignPortAsync(e.Device.LocationPath);
                if (assignedPort.HasValue)
                {
                    Dispatcher.Invoke(() => 
                    {
                        _mappingChangedSinceOpen = true;
                        UpdateStatus($"Mapped Port {assignedPort} successfully!", true);
                        // Force a refresh to ensure UI catches up
                        RefreshPortStatus();
                    });                    
                }
            }
        }

        private void UpdateButtonState(bool isLearning)
        {
            // Always enable the button so we can toggle it
            StartLearningBtn.IsEnabled = true;
            
            var content = StartLearningBtn.Content as StackPanel;
            if (content != null && content.Children.Count > 1)
            {
                var textBlock = content.Children[1] as TextBlock;
                var icon = content.Children[0] as MaterialDesignThemes.Wpf.PackIcon;

                if (isLearning)
                {
                    // STATE: Learning (Click to Stop)
                    if (textBlock != null) textBlock.Text = "Stop Learning";
                    if (icon != null) icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;
                    
                    // Optional: Change button color to Red/Orange to indicate active state
                    StartLearningBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));

                    UpdateStatus("Plug in devices one by one to map ports...", true);
                }
                else
                {
                    // STATE: Idle (Click to Start)
                    if (textBlock != null) textBlock.Text = "Start Learning";
                    if (icon != null) icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    
                    // Revert to original Green
                    StartLearningBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7FB679")); // BrandGreen

                    UpdateStatus("Ready", false);
                }
            }
        }

        public void UpdateStatus(string message, bool isActive = false)
        {
            StatusText.Text = message;
            if (isActive)
            {
                StatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CircleOutline;
                StatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
            }
            else
            {
                StatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Information;
                StatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all port mappings?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await App.PortMapper.ResetMappingAsync();
                _mappingChangedSinceOpen = true;
                UpdateStatus("All mappings reset.", false);
            }
        }

        private async void StartLearningBtn_Click(object sender, RoutedEventArgs e)
        {
            if (App.PortMapper.IsLearningMode)
            {
                await App.PortMapper.StopLearningModeAsync();
                if (_mappingChangedSinceOpen)
                {
                    RestartOverlay.Visibility = Visibility.Visible;
                }
            }
            else
            {
                await App.PortMapper.StartLearningModeAsync();
            }
        }

        private void RestartLater_Click(object sender, RoutedEventArgs e)
        {
            RestartOverlay.Visibility = Visibility.Collapsed;
        }

        private void RestartNow_Click(object sender, RoutedEventArgs e)
        {
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            System.Diagnostics.Process.Start(exePath);
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe to prevent leaks
            App.PortMapper.MappingUpdated -= OnMappingUpdated;
            App.PortMapper.LearningModeChanged -= OnLearningModeChanged;
            App.DeviceDetector.DeviceConnected -= OnDeviceConnectedEvent;
            
            if (App.PortMapper.IsLearningMode)
            {
                 App.PortMapper.StopLearningModeAsync();
            }

            base.OnClosed(e);
        }
    }
}
