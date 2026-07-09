using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace Phonova
{
    public partial class LoginScreen : Window
    {
        private bool isPasswordVisible = false;

        public LoginScreen()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string companyEmail = CompanyEmailBox.Text;
            string password = isPasswordVisible ? VisiblePasswordBox.Text : PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(companyEmail) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter Company Email, Username, and Password.", 
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var button = sender as System.Windows.Controls.Button;
            object originalContent = null;
            if (button != null)
            {
                originalContent = button.Content;
                button.Content = "Signing in...";
                button.IsEnabled = false;
            }

            var response = await Phonova.Services.ApiService.LoginAsync(companyEmail, username, password);

            if (!string.IsNullOrEmpty(response.Token))
            {
                Phonova.Services.ApiService.Username = username;
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                if (button != null)
                {
                    button.Content = originalContent;
                    button.IsEnabled = true;
                }
                MessageBox.Show(response.Error ?? "Login failed.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            
            if (isPasswordVisible)
            {
                PasswordVisibilityIcon.Kind = PackIconKind.Eye;
                VisiblePasswordBox.Text = PasswordBox.Password;
                VisiblePasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                PasswordVisibilityIcon.Kind = PackIconKind.EyeOff;
                PasswordBox.Password = VisiblePasswordBox.Text;
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
            }
        }
    }
}
