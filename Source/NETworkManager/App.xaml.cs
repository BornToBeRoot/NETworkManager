using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using log4net;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager;

/*
 * Class: App
 * 1) Get command line args
 * 2) Detect current configuration
 * 3) Get assembly info 
 * 4) Load system-wide policies
 * 5) Load local settings
 * 6) Load settings
 * 7) Load localization / language
 *
 * Class: MainWindow
 * 8) Load appearance
 * 9) Load profiles
 */

public partial class App
{
    // Single instance identifier
    private const string Guid = "6A3F34B2-161F-4F70-A8BC-A19C40F79CFB";
    private static readonly ILog Log = LogManager.GetLogger(typeof(App));
    private DispatcherTimer _dispatcherTimer;
    private Mutex _mutex;

    private bool _singleInstanceClose;

    public App()
    {
        ShutdownMode = ShutdownMode.OnMainWindowClose;
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var startLog = $@"
  _   _ _____ _____                    _    __  __                                   
 | \ | | ____|_   _|_      _____  _ __| | _|  \/  | __ _ _ __   __ _  __ _  ___ _ __ 
 |  \| |  _|   | | \ \ /\ / / _ \| '__| |/ / |\/| |/ _` | '_ \ / _` |/ _` |/ _ \ '__|
 | |\  | |___  | |  \ V  V / (_) | |  |   <| |  | | (_| | | | | (_| | (_| |  __/ |   
 |_| \_|_____| |_|   \_/\_/ \___/|_|  |_|\_\_|  |_|\__,_|_| |_|\__,_|\__, |\___|_|   
                                                                     |___/        
            
                                               by BornToBeRoot
                                               GitHub.com/BornToBeRoot/NETworkManager

                                               Version: {AssemblyManager.Current.Version}
";
        Log.Info(startLog);

        // Catch unhandled exception globally
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            Log.Fatal("Unhandled exception occured!");

            Log.Fatal($"Exception raised by: {args.ExceptionObject}");
        };

        // Wait until the previous instance has been closed (restart from ui)
        if (CommandLineManager.Current.RestartPid != -1)
        {
            Log.Info(
                $"Waiting for another NETworkManager process with Pid {CommandLineManager.Current.RestartPid} to exit...");

            var processList = Process.GetProcesses();
            var process = processList.FirstOrDefault(x => x.Id == CommandLineManager.Current.RestartPid);
            process?.WaitForExit();

            Log.Info($"NETworkManager process with Pid {CommandLineManager.Current.RestartPid} has been exited.");
        }

        // Load system-wide policies
        PolicyManager.Load();

        // Load (or initialize) local settings
        LocalSettingsManager.Load();

        // Load (or initialize) settings
        try
        {
            if (CommandLineManager.Current.ResetSettings)
                SettingsManager.Initialize();
            else
                SettingsManager.Load();
        }
        catch (InvalidOperationException ex)
        {
            Log.Error("Could not load application settings!", ex);

            HandleCorruptedSettingsFile();
        }
        catch (JsonException ex)
        {
            Log.Error("Could not load application settings! JSON file is corrupted or invalid.", ex);

            HandleCorruptedSettingsFile();
        }

        // Upgrade settings if necessary
        var settingsVersion = Version.Parse(SettingsManager.Current.Version);

        if (settingsVersion < AssemblyManager.Current.Version)
            SettingsManager.Upgrade(settingsVersion, AssemblyManager.Current.Version);
        else
            Log.Info($"Application settings are already on version {AssemblyManager.Current.Version}.");

        // Initialize localization
        var localizationManager = LocalizationManager.GetInstance(SettingsManager.Current.Localization_CultureCode);

        Strings.Culture = localizationManager.Culture;

        Log.Info(
            $"Application localization culture has been set to {localizationManager.Current.Code} (Settings value is \"{SettingsManager.Current.Localization_CultureCode}\").");

        // Show help window
        if (CommandLineManager.Current.Help)
        {
            Log.Info("Set StartupUri to CommandLineWindow.xaml...");
            StartupUri = new Uri("CommandLineWindow.xaml", UriKind.Relative);

            return;
        }

        // Create mutex (to detect single instance)
        Log.Info($"Try to acquire mutex with GUID {Guid} for single instance detection...");

        _mutex = new Mutex(true, "{" + Guid + "}");
        var mutexIsAcquired = _mutex.WaitOne(TimeSpan.Zero, true);

        Log.Info($"Mutex value for {Guid} is {mutexIsAcquired}");

        // Release mutex
        if (mutexIsAcquired)
            _mutex.ReleaseMutex();

        // If another instance is running, bring it to the foreground
        if (!mutexIsAcquired && !SettingsManager.Current.Window_MultipleInstances)
        {
            // Bring the already running application into the foreground
            Log.Info(
                "Another NETworkManager process is already running. Trying to bring the window to the foreground...");
            SingleInstance.PostMessage(SingleInstance.HWND_BROADCAST, SingleInstance.WM_SHOWME, IntPtr.Zero,
                IntPtr.Zero);

            // Close the application                
            _singleInstanceClose = true;
            Shutdown();

            return;
        }


        // Setup background job
        if (SettingsManager.Current.General_BackgroundJobInterval != 0)
        {
            Log.Info(
                $"Setup background job with interval {SettingsManager.Current.General_BackgroundJobInterval} minute(s)...");

            _dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(SettingsManager.Current.General_BackgroundJobInterval)
            };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }
        else
        {
            Log.Info("Background job is disabled.");
        }

        // Setup ThreadPool for the application
        ThreadPool.GetMaxThreads(out var workerThreadsMax, out var completionPortThreadsMax);
        ThreadPool.GetMinThreads(out var workerThreadsMin, out var completionPortThreadsMin);

        var workerThreadsMinNew = workerThreadsMin + SettingsManager.Current.General_ThreadPoolAdditionalMinThreads;
        var completionPortThreadsMinNew = completionPortThreadsMin +
                                          SettingsManager.Current.General_ThreadPoolAdditionalMinThreads;

        if (workerThreadsMinNew > workerThreadsMax)
            workerThreadsMinNew = workerThreadsMax;

        if (completionPortThreadsMinNew > completionPortThreadsMax)
            completionPortThreadsMinNew = completionPortThreadsMax;

        if (ThreadPool.SetMinThreads(workerThreadsMinNew, completionPortThreadsMinNew))
            Log.Info(
                $"ThreadPool min threads set to: workerThreads: {workerThreadsMinNew}, completionPortThreads: {completionPortThreadsMinNew}");
        else
            Log.Warn(
                $"ThreadPool min threads could not be set to workerThreads: {workerThreadsMinNew}, completionPortThreads: {completionPortThreadsMinNew}");

        // Show splash screen
        if (SettingsManager.Current.SplashScreen_Enabled)
        {
            Log.Info("Show SplashScreen while application is loading...");
            new SplashScreen(@"SplashScreen.png").Show(true, true);
        }

        // Show main window
        Log.Info("Set StartupUri to MainWindow.xaml...");
        StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
    }

    /// <summary>
    ///     Handles a corrupted settings file by creating a backup and initializing default settings.
    /// </summary>
    private void HandleCorruptedSettingsFile()
    {
        // Create backup of corrupted file
        var destinationFile =
            $"{TimestampHelper.GetTimestamp()}_corrupted_" + SettingsManager.GetSettingsFileName();

        File.Copy(SettingsManager.GetSettingsFilePath(),
            Path.Combine(SettingsManager.GetSettingsFolderLocation(), destinationFile));

        Log.Info($"A backup of the corrupted settings file has been saved under {destinationFile}");

        // Initialize default application settings
        Log.Info("Initialize default application settings...");

        SettingsManager.Initialize();
        ConfigurationManager.Current.ShowSettingsResetNoteOnStartup = true;
    }

    /// <summary>
    /// Handles the tick event of the dispatcher timer to trigger a background job and save data.
    /// </summary>
    /// <param name="sender">The source of the event, typically the dispatcher timer instance.</param>
    /// <param name="e">The event data associated with the timer tick.</param>
    private void DispatcherTimer_Tick(object sender, EventArgs e)
    {
        Log.Info("Run background job...");

        Save();
    }

    /// <summary>
    /// Handles the session ending event by canceling the session termination and initiating application shutdown.
    /// </summary>
    /// <remarks>This method overrides the default session ending behavior to prevent the session from ending
    /// automatically. Instead, it cancels the session termination and shuts down the application gracefully. Use this
    /// override to implement custom shutdown logic when the user logs off or shuts down the system.</remarks>
    /// <param name="e">The event data for the session ending event. Provides information about the session ending request and allows
    /// cancellation of the event.</param>
    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
        base.OnSessionEnding(e);

        e.Cancel = true;

        Shutdown();
    }

    /// <summary>
    /// Handles the application exit event to perform cleanup operations such as stopping background tasks and saving
    /// settings.
    /// </summary>
    /// <remarks>This method is intended to be called when the application is shutting down. It ensures that
    /// any running background jobs are stopped and application settings are saved, unless the application is closed due
    /// to a single instance or help command scenario.</remarks>
    /// <param name="sender">The source of the event, typically the application instance.</param>
    /// <param name="e">The event data associated with the application exit event.</param>
    private void Application_Exit(object sender, ExitEventArgs e)
    {
        Log.Info("Exiting NETworkManager...");

        // Save settings, when the application is normally closed
        if (_singleInstanceClose || CommandLineManager.Current.Help)
            return;

        Log.Info("Stop background job, if it exists...");

        _dispatcherTimer?.Stop();

        Save();

        Log.Info("Bye!");
    }

    /// <summary>
    /// Saves application settings and profile data if changes have been detected.
    /// </summary>
    /// <remarks>This method saves settings only if they have been modified. Profile data is saved if a
    /// profile file is loaded, its data is available, and changes have been made. If no profile file is loaded or the
    /// file is encrypted and not unlocked, profile data will not be saved and a warning is logged.</remarks>
    private void Save()
    {
        // Save local settings if they have changed
        if (LocalSettingsManager.Current.SettingsChanged)
        {
            Log.Info("Save local settings...");
            LocalSettingsManager.Save();
        }

        // Save settings if they have changed
        if (SettingsManager.Current.SettingsChanged)
        {
            Log.Info("Save application settings...");
            SettingsManager.Save();
        }

        // Save profiles if they have changed
        if (ProfileManager.LoadedProfileFile != null && ProfileManager.LoadedProfileFileData != null)
        {
            if (ProfileManager.LoadedProfileFileData.ProfilesChanged)
            {
                Log.Info($"Save current profile file \"{ProfileManager.LoadedProfileFile.Name}\"...");
                ProfileManager.Save();
            }
        }
        else
        {
            Log.Warn("Cannot save profiles because no profile file is loaded or the profile file is encrypted and not yet unlocked.");
        }
    }
}
