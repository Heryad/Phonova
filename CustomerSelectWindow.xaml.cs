using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Phonova.Services;

namespace Phonova
{
    public partial class CustomerSelectWindow : Window
    {
        private string? _selectedCustomer = null;
        
        public string? SelectedCustomer => _selectedCustomer;
        public bool Confirmed { get; private set; } = false;
        
        public CustomerSelectWindow(string? existingCustomer = null)
        {
            InitializeComponent();
            LoadCustomers(existingCustomer);
        }
        
        private string? _editingCustomerId = null;
        private string? _editingCustomer = null;

        private async void LoadCustomers(string? preselected = null)
        {
            CustomersListPanel.Children.Clear();
            
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
                Content = "None (No Customer)",
                IsChecked = string.IsNullOrEmpty(preselected),
                Tag = "",
                FontSize = 13,
                GroupName = "Customers",
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
            CustomersListPanel.Children.Add(noneBorder);

            var customers = await ApiService.GetCustomersAsync();
            
            foreach (var template in customers)
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
                    Content = template.name,
                    IsChecked = (template.name == preselected),
                    Tag = template.name,
                    FontSize = 13,
                    GroupName = "Customers",
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                radioButton.Checked += (s, e) => UpdateSelectedText(template.name);
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
                editBtn.Click += EditCustomer_Click;
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
                deleteBtn.Click += DeleteCustomer_Click;
                Grid.SetColumn(deleteBtn, 2);
                grid.Children.Add(deleteBtn);

                border.Child = grid;
                border.MouseLeftButtonUp += (s, e) => 
                {
                    if (e.OriginalSource is RadioButton || e.OriginalSource is Button || e.OriginalSource is MaterialDesignThemes.Wpf.PackIcon) return;
                    radioButton.IsChecked = true;
                };
                
                CustomersListPanel.Children.Add(border);
            }
            
            if (string.IsNullOrEmpty(preselected))
            {
                SelectedCustomerText.Text = "No customer selected";
            }
            else
            {
                _selectedCustomer = preselected;
                SelectedCustomerText.Text = $"Selected: {preselected}";
            }
        }
        
        private void UpdateSelectedText(string customer)
        {
            _selectedCustomer = string.IsNullOrEmpty(customer) ? null : customer;
            SelectedCustomerText.Text = string.IsNullOrEmpty(customer) ? "No customer selected" : $"Selected: {customer}";
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
        
        private void AddQuickCustomerBtn_Click(object sender, RoutedEventArgs e) => AddQuickCustomer();

        private void QuickCustomerBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AddQuickCustomer();
            }
        }

        private async void AddQuickCustomer()
        {
            string customerName = QuickCustomerBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(customerName)) return;

            if (!string.IsNullOrEmpty(_editingCustomerId))
            {
                await ApiService.UpdateCustomerAsync(_editingCustomerId, customerName);
                string? prevSelected = (_selectedCustomer == _editingCustomer) ? customerName : _selectedCustomer;
                _editingCustomer = null;
                _editingCustomerId = null;
                QuickCustomerBox.Text = "";
                AddQuickCustomerBtn.Content = "Add";
                CancelEditBtn.Visibility = Visibility.Collapsed;
                LoadCustomers(prevSelected);
            }
            else
            {
                await ApiService.AddCustomerAsync(customerName);
                QuickCustomerBox.Text = "";
                LoadCustomers(customerName);
            }
        }

        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ApiService.CustomerModel customer)
            {
                _editingCustomer = customer.name;
                _editingCustomerId = customer.id;
                QuickCustomerBox.Text = customer.name;
                AddQuickCustomerBtn.Content = "Update";
                CancelEditBtn.Visibility = Visibility.Visible;
                QuickCustomerBox.Focus();
            }
        }

        private void CancelEditBtn_Click(object sender, RoutedEventArgs e)
        {
            _editingCustomer = null;
            _editingCustomerId = null;
            QuickCustomerBox.Text = "";
            AddQuickCustomerBtn.Content = "Add";
            CancelEditBtn.Visibility = Visibility.Collapsed;
        }

        private async void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ApiService.CustomerModel customer)
            {
                if (MessageBox.Show($"Are you sure you want to delete '{customer.name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    await ApiService.DeleteCustomerAsync(customer.id);
                    if (_selectedCustomer == customer.name)
                    {
                        _selectedCustomer = null;
                        SelectedCustomerText.Text = "No customer selected";
                    }
                    LoadCustomers(_selectedCustomer);
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
