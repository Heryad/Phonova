using System.Configuration;
using System.Data;
using System.Windows;
using Phonova.Services;

namespace Phonova
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Services.AppleDeviceDetector DeviceDetector { get; private set; }
        public static Services.PortMappingService PortMapper { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize Services
            DeviceDetector = new Services.AppleDeviceDetector();
            PortMapper = new Services.PortMappingService();

            await PortMapper.LoadMappingAsync();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await DeviceDetector.StopAsync();
            
            // Kill any background processes (like syslog) before closing
            iOSCommander.StopAllProcesses();

            DeviceDetector.Dispose();
            PortMapper.Dispose();

            base.OnExit(e);
        }
    }
}
