using System.Configuration;
using System.Data;
using System.Windows;
using Dyagnoz.Services.Printing;
using Dyagnoz_Latest.Services;
using EzioDll;

namespace Dyagnoz_Latest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Services.AppleDeviceDetector DeviceDetector { get; private set; }
        public static Services.PortMappingService PortMapper { get; private set; }
        public static Services.DatabaseService Database { get; private set; }
        public static PrintService PrintService { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize Services
            DeviceDetector = new Services.AppleDeviceDetector();
            PortMapper = new Services.PortMappingService();
            Database = new Services.DatabaseService();
            PrintService = new PrintService();

            await PortMapper.LoadMappingAsync();
            await DeviceDetector.StartAsync();
            try {
                PrintService.ConnectToFirstPrinter();
            } catch (Exception ex)
            {
                MessageBox.Show($"Error initializing services: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await DeviceDetector.StopAsync();
            
            DeviceDetector.Dispose();
            PortMapper.Dispose();
            PrintService.Dispose();

            base.OnExit(e);
        }
    }
}
