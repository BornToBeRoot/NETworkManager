using NETworkManager.Models.Settings;
using System;
using System.Diagnostics;
using System.Linq;
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

            // Load settings
            SettingsManager.Load();
            TemplateManager.LoadNetworkInterfaceConfigTemplates();
            TemplateManager.LoadWakeOnLANTemplates();

            StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);

            e.Cancel = true;

            Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (!ImportExportManager.ForceRestart)
            {
                // Save templates
                if (TemplateManager.NetworkInterfaceConfigTemplatesChanged)
                    TemplateManager.SaveNetworkInterfaceConfigTemplates();

                if (TemplateManager.WakeOnLANTemplatesChanged)
                    TemplateManager.SaveWakeOnLANTemplates();

                // Save settings
                if (SettingsManager.Current.SettingsChanged)
                    SettingsManager.Save();
            }
        }
    }
}
