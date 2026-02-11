using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Dyagnoz_Latest
{
    /// <summary>
    /// A spectacular animated splash screen for Dyagnoz application
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

        public SplashScreen()
        {
            InitializeComponent();
            StartLoadingSequence();
        }

        private async void StartLoadingSequence()
        {
            try
            {
                // Simulate loading with status updates
                for (int i = 0; i < _loadingMessages.Length; i++)
                {
                    StatusText.Text = _loadingMessages[i];
                    await Task.Delay(600); // Adjust timing as needed
                }

                // Brief pause before transitioning
                await Task.Delay(300);

                // Fade out animation before closing
                await FadeOutAndClose();
            }
            catch (Exception ex)
            {
                // Log error if needed, then proceed to login window
                System.Diagnostics.Debug.WriteLine($"Splash screen error: {ex.Message}");
                OpenLoginScreen();
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
            var loginScreen = new MainWindow();
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
