using NETworkManager.Utilities;
using NETworkManager.Models.Settings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NETworkManager.Properties;

namespace NETworkManager
{
    public partial class App
    {
        // Single instance identifier
        private const string GUID = "6A3F34B2-161F-4F70-A8BC-A19C40F79CFB";
        private Mutex _mutex;
        private DispatcherTimer _dispatcherTimer;

        private bool _singleInstanceClose;

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
                var processList = Process.GetProcesses();

                var process = processList.FirstOrDefault(x => x.Id == CommandLineManager.Current.RestartPid);

                process?.WaitForExit();
            }

            // Detect the current configuration
            ConfigurationManager.Detect();

            // Get assembly informations   
            AssemblyManager.Load();

            // Load application settings (profiles/Profiles/clients are loaded when needed)
            try
            {
                // Update integrated settings %LocalAppData%\NETworkManager\NETworkManager_GUID (custom settings path)
                if (Settings.Default.UpgradeRequired)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeRequired = false;
                }

                SettingsManager.Load();

                // Update settings (Default --> %AppData%\NETworkManager\Settings)
                Version assemblyVersion = AssemblyManager.Current.Version;
                Version settingsVersion = new Version(SettingsManager.Current.SettingsVersion);

                if (assemblyVersion > settingsVersion)
                {
                    SettingsManager.Update(assemblyVersion, settingsVersion);

                    try
                    {
                        ProfileManager.Upgrade(settingsVersion);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Failed to update profiles...\n\n" + ex.Message, "Profile Manager - Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                SettingsManager.InitDefault();
                ConfigurationManager.Current.ShowSettingsResetNoteOnStartup = true;
            }

            // Load localization (requires settings to be loaded first)
            LocalizationManager.Load();

            NETworkManager.Resources.Localization.Strings.Culture = LocalizationManager.Culture;

            if (CommandLineManager.Current.Help)
            {
                StartupUri = new Uri("/Views/CommandLineHelpWindow.xaml", UriKind.Relative);
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
            if (_singleInstanceClose || ImportExportManager.ForceRestart || CommandLineManager.Current.Help)
                return;

            _dispatcherTimer?.Stop();

            Save();
        }

        private void Save()
        {
            // Save local settings (custom settings path in AppData/Local)
            Settings.Default.Save();

            if (SettingsManager.Current.SettingsChanged) // This will also create the "Settings" folder, if it does not exist
                SettingsManager.Save();

            if (ProfileManager.ProfilesChanged)
                ProfileManager.Save();

            if (CredentialManager.CredentialsChanged)
                CredentialManager.Save();
        }
    }
}
