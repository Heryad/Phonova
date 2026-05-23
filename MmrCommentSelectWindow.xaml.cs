using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dyagnoz_Latest.Services;

namespace Dyagnoz_Latest
{
    public partial class MmrCommentSelectWindow : Window
    {
        private List<string> _selectedMmrComments = new List<string>();
        
        public List<string> SelectedMmrComments => _selectedMmrComments;
        public bool Confirmed { get; private set; } = false;
        
        public MmrCommentSelectWindow(List<string>? existingComments = null)
        {
            InitializeComponent();
            LoadMmrComments(existingComments ?? new List<string>());
        }
        
        private void LoadMmrComments(List<string>? preselectedComments = null)
        {
            var currentlyChecked = new HashSet<string>();
            if (preselectedComments != null)
            {
                foreach (var c in preselectedComments) currentlyChecked.Add(c);
            }
            else
            {
                foreach (var child in MmrCommentsListPanel.Children)
                {
                    if (child is Border border && border.Child is CheckBox cb && cb.IsChecked == true && cb.Tag is string comment)
                    {
                        currentlyChecked.Add(comment);
                    }
                }
            }

            MmrCommentsListPanel.Children.Clear();
            
            var templates = App.Database.GetAllMmrComments();
            
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
                
                var checkBox = new CheckBox
                {
                    Content = template,
                    IsChecked = currentlyChecked.Contains(template),
                    Tag = template,
                    FontSize = 13
                };
                checkBox.Checked += (s, e) => UpdateCount();
                checkBox.Unchecked += (s, e) => UpdateCount();
                
                border.Child = checkBox;
                border.MouseLeftButtonUp += (s, e) => 
                {
                    checkBox.IsChecked = !checkBox.IsChecked;
                };
                
                MmrCommentsListPanel.Children.Add(border);
            }
            
            UpdateCount();
        }
        
        private void UpdateCount()
        {
            int count = 0;
            foreach (var child in MmrCommentsListPanel.Children)
            {
                if (child is Border border && border.Child is CheckBox cb && cb.IsChecked == true)
                {
                    count++;
                }
            }
            SelectedCountText.Text = $"{count} selected";
        }
        
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            _selectedMmrComments.Clear();
            foreach (var child in MmrCommentsListPanel.Children)
            {
                if (child is Border border && border.Child is CheckBox cb && cb.IsChecked == true && cb.Tag is string comment)
                {
                    _selectedMmrComments.Add(comment);
                }
            }
            Confirmed = true;
            Close();
        }
        
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
        
        private void AddQuickMmrCommentBtn_Click(object sender, RoutedEventArgs e) => AddQuickMmrComment();

        private void QuickMmrCommentBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AddQuickMmrComment();
            }
        }

        private void AddQuickMmrComment()
        {
            string comment = QuickMmrCommentBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(comment)) return;

            App.Database.AddMmrCommentToLibrary(comment);
            QuickMmrCommentBox.Text = "";
            LoadMmrComments();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
    }
}
