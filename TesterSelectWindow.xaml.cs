using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dyagnoz_Latest.Services;

namespace Dyagnoz_Latest
{
    public partial class TesterSelectWindow : Window
    {
        private string? _selectedTester = null;
        
        public string? SelectedTester => _selectedTester;
        public bool Confirmed { get; private set; } = false;
        
        public TesterSelectWindow(string? existingTester = null)
        {
            InitializeComponent();
            LoadTesters(existingTester);
        }
        
        private string? _editingTester = null;

        private void LoadTesters(string? preselected = null)
        {
            TestersListPanel.Children.Clear();
            
            // Add a "None / Clear Selection" option at the very top
            var noneBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 0, 8),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            
            var noneGrid = new Grid();
            var noneRadioButton = new RadioButton
            {
                Content = "None (No Tester)",
                IsChecked = string.IsNullOrEmpty(preselected),
                Tag = "",
                FontSize = 13,
                GroupName = "Testers",
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center
            };
            noneRadioButton.Checked += (s, e) => UpdateSelectedText("");
            noneGrid.Children.Add(noneRadioButton);
            noneBorder.Child = noneGrid;
            noneBorder.MouseLeftButtonUp += (s, e) => 
            {
                if (e.OriginalSource is RadioButton) return;
                noneRadioButton.IsChecked = true;
            };
            TestersListPanel.Children.Add(noneBorder);

            var templates = App.Database.GetAllTesters();
            
            foreach (var template in templates)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12, 6, 12, 6),
                    Margin = new Thickness(0, 0, 0, 8),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var radioButton = new RadioButton
                {
                    Content = template,
                    IsChecked = (template == preselected),
                    Tag = template,
                    FontSize = 13,
                    GroupName = "Testers",
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                radioButton.Checked += (s, e) => UpdateSelectedText(template);
                Grid.SetColumn(radioButton, 0);
                grid.Children.Add(radioButton);

                // Edit Button
                var editBtn = new Button
                {
                    Content = new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.Pencil, Width = 14, Height = 14 },
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Width = 28,
                    Height = 28,
                    Padding = new Thickness(0),
                    Tag = template,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Margin = new Thickness(0, 0, 4, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                editBtn.Click += EditTester_Click;
                Grid.SetColumn(editBtn, 1);
                grid.Children.Add(editBtn);

                // Delete Button
                var deleteBtn = new Button
                {
                    Content = new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.Delete, Width = 14, Height = 14 },
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Width = 28,
                    Height = 28,
                    Padding = new Thickness(0),
                    Tag = template,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    VerticalAlignment = VerticalAlignment.Center
                };
                deleteBtn.Click += DeleteTester_Click;
                Grid.SetColumn(deleteBtn, 2);
                grid.Children.Add(deleteBtn);

                border.Child = grid;
                border.MouseLeftButtonUp += (s, e) => 
                {
                    if (e.OriginalSource is RadioButton || e.OriginalSource is Button || e.OriginalSource is MaterialDesignThemes.Wpf.PackIcon) return;
                    radioButton.IsChecked = true;
                };
                
                TestersListPanel.Children.Add(border);
            }
            
            if (string.IsNullOrEmpty(preselected))
            {
                SelectedTesterText.Text = "No tester selected";
            }
            else
            {
                _selectedTester = preselected;
                SelectedTesterText.Text = $"Selected: {preselected}";
            }
        }
        
        private void UpdateSelectedText(string tester)
        {
            _selectedTester = string.IsNullOrEmpty(tester) ? null : tester;
            SelectedTesterText.Text = string.IsNullOrEmpty(tester) ? "No tester selected" : $"Selected: {tester}";
        }
        
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = true;
            Close();
        }
        
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
        
        private void AddQuickTesterBtn_Click(object sender, RoutedEventArgs e) => AddQuickTester();

        private void QuickTesterBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AddQuickTester();
            }
        }

        private void AddQuickTester()
        {
            string tester = QuickTesterBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(tester)) return;

            if (!string.IsNullOrEmpty(_editingTester))
            {
                App.Database.UpdateTesterInLibrary(_editingTester, tester);
                string? prevSelected = (_selectedTester == _editingTester) ? tester : _selectedTester;
                _editingTester = null;
                QuickTesterBox.Text = "";
                AddQuickTesterBtn.Content = "Add";
                CancelEditBtn.Visibility = Visibility.Collapsed;
                LoadTesters(prevSelected);
            }
            else
            {
                App.Database.AddTesterToLibrary(tester);
                QuickTesterBox.Text = "";
                LoadTesters(tester);
            }
        }

        private void EditTester_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tester)
            {
                _editingTester = tester;
                QuickTesterBox.Text = tester;
                AddQuickTesterBtn.Content = "Update";
                CancelEditBtn.Visibility = Visibility.Visible;
                QuickTesterBox.Focus();
            }
        }

        private void CancelEditBtn_Click(object sender, RoutedEventArgs e)
        {
            _editingTester = null;
            QuickTesterBox.Text = "";
            AddQuickTesterBtn.Content = "Add";
            CancelEditBtn.Visibility = Visibility.Collapsed;
        }

        private void DeleteTester_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tester)
            {
                if (MessageBox.Show($"Are you sure you want to delete '{tester}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    App.Database.DeleteTesterFromLibrary(tester);
                    if (_selectedTester == tester)
                    {
                        _selectedTester = null;
                        SelectedTesterText.Text = "No tester selected";
                    }
                    LoadTesters(_selectedTester);
                }
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
    }
}
