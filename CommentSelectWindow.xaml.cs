using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Phonova.Services;


namespace Phonova
{
    public partial class CommentSelectWindow : Window
    {
        private List<string> _selectedComments = new List<string>();
        
        public List<string> SelectedComments => _selectedComments;
        public bool Confirmed { get; private set; } = false;
        
        public CommentSelectWindow(List<string>? existingComments = null)
        {
            InitializeComponent();
            LoadComments(existingComments ?? new List<string>());
        }
        
        private async void LoadComments(List<string>? preselectedComments = null)
        {
            var currentlyChecked = new HashSet<string>();
            if (preselectedComments != null)
            {
                foreach (var c in preselectedComments) currentlyChecked.Add(c);
            }
            else
            {
                foreach (var child in CommentsListPanel.Children)
                {
                    if (child is Border border && border.Child is CheckBox cb && cb.IsChecked == true && cb.Tag is string comment)
                    {
                        currentlyChecked.Add(comment);
                    }
                }
            }

            CommentsListPanel.Children.Clear();
            
            var models = await ApiService.GetCommentsAsync();
            var templates = new List<string>();
            foreach (var m in models) templates.Add(m.content);
            
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
                
                CommentsListPanel.Children.Add(border);
            }
            
            UpdateCount();
        }
        
        private void UpdateCount()
        {
            int count = 0;
            foreach (var child in CommentsListPanel.Children)
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
            _selectedComments.Clear();
            foreach (var child in CommentsListPanel.Children)
            {
                if (child is Border border && border.Child is CheckBox cb && cb.IsChecked == true && cb.Tag is string comment)
                {
                    _selectedComments.Add(comment);
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
        
        private void AddQuickCommentBtn_Click(object sender, RoutedEventArgs e) => AddQuickComment();

        private void QuickCommentBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AddQuickComment();
            }
        }

        private async void AddQuickComment()
        {
            string comment = QuickCommentBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(comment)) return;

            await ApiService.AddCommentAsync(comment);
            QuickCommentBox.Text = "";
            LoadComments();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
    }
}

