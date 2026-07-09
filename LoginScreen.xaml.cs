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
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(companyEmail) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter Company Email, Username, and Password.", 
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var btnContent = ((System.Windows.Controls.Button)sender).Content;
            ((System.Windows.Controls.Button)sender).Content = "Signing in...";
            ((System.Windows.Controls.Button)sender).IsEnabled = false;

            var response = await Phonova.Services.ApiService.LoginAsync(companyEmail, username, password);

            if (!string.IsNullOrEmpty(response.Token))
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ((System.Windows.Controls.Button)sender).Content = btnContent;
                ((System.Windows.Controls.Button)sender).IsEnabled = true;
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
                // Note: Actual password visibility toggle would require a TextBox overlay
                // For now, we just change the icon as a visual indicator
            }
            else
            {
                PasswordVisibilityIcon.Kind = PackIconKind.EyeOff;
            }
        }
    }
}
