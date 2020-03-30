using NETworkManager.Utilities;
using NETworkManager.Settings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NETworkManager.Profiles;
using NETworkManager.Localization;

namespace NETworkManager
{
    /* 
     * Class: App
     * 1) Get command line args
     * 2) Detect current configuration
     * 3) Get assembly info
     * 4) Load settings
     * 5) Load localization / language
     * 
     * Class: MainWindow
     * 6) Load appearance
     * 7) Load profiles                 
     */

    public partial class App
    {
        // Single instance identifier
        private const string GUID = "6A3F34B2-161F-4F70-A8BC-A19C40F79CFB";
        private Mutex _mutex;
        private DispatcherTimer _dispatcherTimer;

        private bool _singleInstanceClose;

        public App()
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // If we have restart our application... wait until it has finished
            if (CommandLineManager.Current.RestartPid != -1)
            {
                var processList = Process.GetProcesses();

                var process = processList.FirstOrDefault(x => x.Id == CommandLineManager.Current.RestartPid);

                process?.WaitForExit();
            }

            // Update integrated settings %LocalAppData%\NETworkManager\NETworkManager_GUID (custom settings path)
            if (LocalSettingsManager.UpgradeRequired)
            {
                LocalSettingsManager.Upgrade();
                LocalSettingsManager.UpgradeRequired = false;
            }

            // Load settings
            try
            {
                SettingsManager.Load();
            }
            catch (InvalidOperationException)
            {
                SettingsManager.InitDefault();

                ConfigurationManager.Current.ShowSettingsResetNoteOnStartup = true;
            }

            // Init the location with the culture code...
            Localization.Resources.Strings.Culture = LocalizationManager.GetInstance(SettingsManager.Current.Localization_CultureCode).Culture;

            if (CommandLineManager.Current.Help)
            {
                StartupUri = new Uri("CommandLineWindow.xaml", UriKind.Relative);
                return;
            }

            // Create mutex
            _mutex = new Mutex(true, "{" + GUID + "}");
            var mutexIsAcquired = _mutex.WaitOne(TimeSpan.Zero, true);

            // Release mutex
            if (mutexIsAcquired)
                _mutex.ReleaseMutex();

            if (SettingsManager.Current.Window_MultipleInstances || mutexIsAcquired)
            {
                if (SettingsManager.Current.General_BackgroundJobInterval != 0)
                {
                    _dispatcherTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMinutes(SettingsManager.Current.General_BackgroundJobInterval)
                    };

                    _dispatcherTimer.Tick += DispatcherTimer_Tick;

                    _dispatcherTimer.Start();
                }

                StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
            }
            else
            {
                // Bring the already running application into the foreground
                SingleInstance.PostMessage((IntPtr)SingleInstance.HWND_BROADCAST, SingleInstance.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);

                _singleInstanceClose = true;
                Shutdown();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Save();
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
            if (_singleInstanceClose || ConfigurationManager.Current.ForceRestart || CommandLineManager.Current.Help)
                return;

            _dispatcherTimer?.Stop();

            Save();
        }

        private void Save()
        {
            // Save local settings (custom settings path in AppData/Local)
            LocalSettingsManager.Save();

            if (SettingsManager.Current.SettingsChanged) // This will also create the "Settings" folder, if it does not exist
                SettingsManager.Save();

            if (ProfileManager.ProfilesChanged)
                ProfileManager.Save();
        }
    }
}
