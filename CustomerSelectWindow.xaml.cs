using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dyagnoz_Latest.Services;

namespace Dyagnoz_Latest
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
        
        private void LoadCustomers(string? preselected = null)
        {
            CustomersListPanel.Children.Clear();
            
            var templates = App.Database.GetAllCustomers();
            
            foreach (var template in templates)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12, 10, 12, 10),
                    Margin = new Thickness(0, 0, 0, 8),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                
                var radioButton = new RadioButton
                {
                    Content = template,
                    IsChecked = (template == preselected),
                    Tag = template,
                    FontSize = 13,
                    GroupName = "Customers"
                };
                
                radioButton.Checked += (s, e) => UpdateSelectedText(template);
                
                border.Child = radioButton;
                border.MouseLeftButtonUp += (s, e) => 
                {
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
            _selectedCustomer = customer;
            SelectedCustomerText.Text = $"Selected: {customer}";
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

        private void AddQuickCustomer()
        {
            string customer = QuickCustomerBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(customer)) return;

            App.Database.AddCustomerToLibrary(customer);
            QuickCustomerBox.Text = "";
            
            // Reload the list and preselect the newly added customer
            LoadCustomers(customer);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
    }
}
