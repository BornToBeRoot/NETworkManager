using NETworkManager.Helpers;
using NETworkManager.Models.Settings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace NETworkManager
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        // Single instance unique identifier
        private const string Guid = "6A3F34B2-161F-4F70-A8BC-A19C40F79CFB";
        Mutex _mutex;

        private bool _singleInstanceClose = false;

        public App()
        {
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Parse the command line arguments and store them in the current configuration
            CommandLineManager.Parse();

            // If we have restart our application... wait until it has finished
            if (CommandLineManager.Current.RestartPid != 0)
            {
                Process[] processList = Process.GetProcesses();

                Process process = processList.FirstOrDefault(x => x.Id == CommandLineManager.Current.RestartPid);

                if (process != null)
                    process.WaitForExit();
            }

            // Detect the current configuration
            ConfigurationManager.Detect();

            // Get assembly informations   
            AssemblyManager.Load();

            // Load settings
            SettingsManager.Load();

            // Load localization (requires settings to be loaded first)
            LocalizationManager.Load();

            if (CommandLineManager.Current.Help)
            {
                StartupUri = new Uri("/Views/Help/HelpCommandLineWindow.xaml", UriKind.Relative);
                return;
            }

            // Single instance
            _mutex = new Mutex(true, "{" + Guid + "}");

            if (SettingsManager.Current.Window_MultipleInstances || _mutex.WaitOne(TimeSpan.Zero, true))
            {
                StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
                _mutex.ReleaseMutex();
            }
            else
            {
                // Bring the already running application into the foreground
                SingleInstanceHelper.PostMessage((IntPtr)SingleInstanceHelper.HWND_BROADCAST, SingleInstanceHelper.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);

                _singleInstanceClose = true;
                Shutdown();
            }
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);

            e.Cancel = true;

            Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Save settings, when the application is normally closed
            if (!_singleInstanceClose && !ImportExportManager.ForceRestart && !CommandLineManager.Current.Help)
            {
                if (SettingsManager.Current.SettingsChanged)
                    SettingsManager.Save();
            }
        }
    }
}
