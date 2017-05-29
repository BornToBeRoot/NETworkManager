using NETworkManager.Models.Settings;
using System.Windows;

namespace NETworkManager
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ShutdownMode = ShutdownMode.OnLastWindowClose;

            // Parse the command line arguments and store them in the current configuration
            CommandLineManager.Parse();

            // Detect the current configuration
            ConfigurationManager.Detect();
        }
    }
}
