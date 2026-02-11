using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace Dyagnoz_Latest
{
    public partial class LoginScreen : Window
    {
        private bool isPasswordVisible = false;

        public LoginScreen()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string selectedTestor = TestorComboBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(selectedTestor) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter Username, Password and select a Tester to proceed.", 
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // TODO: Validate credentials against database
            
            // Open main window and close login
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
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
