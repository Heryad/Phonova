using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Threading;
using System.Diagnostics;

namespace Phonova
{
    /// <summary>
    /// A spectacular animated splash screen for Phonova application
    /// </summary>
    public partial class SplashScreen : Window
    {
        private readonly string[] _loadingMessages = new[]
        {
            "Initializing components...",
            "Loading diagnostic modules...",
            "Preparing analysis engine...",
            "Configuring user interface...",
            "Almost ready..."
        };

        private static Mutex _mutex;
        private bool _isDuplicate;

        public SplashScreen()
        {
            InitializeComponent();
            CheckInstances();
            if (!_isDuplicate)
            {
                StartLoadingSequence();
            }
        }

        private void CheckInstances()
        {
            const string mutexName = "Global\\Phonova-SingleInstance-Mutex";
            _mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                _isDuplicate = true;
                DuplicateOverlay.Visibility = Visibility.Visible;
            }
        }

        private void ExitDuplicateBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private class HealthResponse
        {
            [Newtonsoft.Json.JsonProperty("status")]
            public string Status { get; set; }
            [Newtonsoft.Json.JsonProperty("database")]
            public string Database { get; set; }
        }

        private async void StartLoadingSequence()
        {
            try
            {
                // Ensure any leftover tokens from previous sessions are revoked on startup
                Phonova.Services.ApiService.Logout();

                StatusText.Text = "Connecting to server...";
                
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    string url = Phonova.Services.SettingsManager.Current.ApiBaseUrl.TrimEnd('/') + "/health";
                    var response = await client.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var health = Newtonsoft.Json.JsonConvert.DeserializeObject<HealthResponse>(json);
                        
                        if (health != null && health.Status == "ok" && health.Database == "ok")
                        {
                            StatusText.Text = "Connected successfully. Launching...";
                            await Task.Delay(500); // Brief pause to show success
                            await FadeOutAndClose();
                            return;
                        }
                    }
                }

                // If we get here, connection failed or status wasn't ok
                StatusText.Text = "Server connection failed.";
                MessageBox.Show("Unable to connect to the Phonova API Server. Please check your internet connection and try again.", 
                                "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                StatusText.Text = "Server connection error.";
                System.Diagnostics.Debug.WriteLine($"Splash screen error: {ex.Message}");
                MessageBox.Show($"Could not reach the server: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private async Task FadeOutAndClose()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var tcs = new TaskCompletionSource<bool>();
            fadeOut.Completed += (s, e) => tcs.SetResult(true);

            MainContainer.BeginAnimation(OpacityProperty, fadeOut);
            await tcs.Task;

            OpenLoginScreen();
        }

        private void OpenLoginScreen()
        {
            var loginScreen = new LoginScreen();
            loginScreen.Show();
            this.Close();
        }

        /// <summary>
        /// Updates the loading status text displayed on the splash screen
        /// </summary>
        /// <param name="message">The status message to display</param>
        public void UpdateStatus(string message)
        {
            if (StatusText != null)
            {
                StatusText.Text = message;
            }
        }

        /// <summary>
        /// Updates the progress bar to a specific percentage
        /// </summary>
        /// <param name="percentage">Progress percentage (0-100)</param>
        public void UpdateProgress(double percentage)
        {
            if (ProgressIndicator != null)
            {
                double maxWidth = 400; // Match the Grid width
                ProgressIndicator.Width = (percentage / 100) * maxWidth;
            }
        }
    }
}
