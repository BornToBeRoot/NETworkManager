using log4net;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the LLDP / CDP Capture view.
/// </summary>
public class LLDPCDPCaptureViewModel : ViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(LLDPCDPCaptureViewModel));

    /// <summary>
    /// The LLDP / CDP capture instance.
    /// </summary>
    private readonly LLDPCDPCapture _lldpCdpCapture = new();

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// The timer for the remaining time.
    /// </summary>
    private readonly Timer _remainingTimer;

    /// <summary>
    /// The seconds remaining for the capture.
    /// </summary>
    private int _secondsRemaining;

    /// <summary>
    /// Gets or sets a value indicating whether this is the first run.
    /// </summary>
    public bool FirstRun
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    /// <summary>
    /// Gets the list of available discovery protocols.
    /// </summary>
    public List<LLDPCDPProtocol> Protocols
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    /// <summary>
    /// Gets or sets the selected discovery protocol.
    /// </summary>
    public LLDPCDPProtocol SelectedProtocol
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.LLDPCDPCapture_Protocol = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the list of available durations.
    /// </summary>
    public List<int> Durations
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected duration.
    /// </summary>
    public int SelectedDuration
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.LLDPCDPCapture_Duration = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the capture is running.
    /// </summary>
    public bool IsCapturing
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the message for the remaining time.
    /// </summary>
    public string TimeRemainingMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the status message is displayed.
    /// </summary>
    public bool IsStatusMessageDisplayed
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the status message.
    /// </summary>
    public string StatusMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Level of the status message (icon and color, see <see cref="StatusMessageText" />).
    /// </summary>
    public StatusLevel StatusMessageLevel
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a discovery package has been received.
    /// </summary>
    public bool DiscoveryPackageReceived
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the received discovery package.
    /// </summary>
    public LLDPCDPPackageInfo DiscoveryPackage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, LoadSettings

    /// <summary>
    /// Initializes a new instance of the <see cref="LLDPCDPCaptureViewModel"/> class.
    /// </summary>
    /// <param name="instance">The dialog coordinator instance.</param>
    public LLDPCDPCaptureViewModel()
    {
        _isLoading = true;

        _lldpCdpCapture.PackageReceived += LLDPCDPCapture_PackageReceived;
        _lldpCdpCapture.ErrorReceived += LLDPCDPCapture_ErrorReceived;
        _lldpCdpCapture.WarningReceived += LLDPCDPCapture_WarningReceived;
        _lldpCdpCapture.Complete += LLDPCDPCapture_Complete;

        _remainingTimer = new Timer
        {
            Interval = 1000
        };

        _remainingTimer.Elapsed += Timer_Elapsed;

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        Protocols = Enum.GetValues(typeof(LLDPCDPProtocol)).Cast<LLDPCDPProtocol>().OrderBy(x => x.ToString())
            .ToList();
        SelectedProtocol = Protocols.FirstOrDefault(x => x == SettingsManager.Current.LLDPCDPCapture_Protocol);
        Durations = new List<int> { 15, 30, 60, 90, 120 };
        SelectedDuration = Durations.FirstOrDefault(x => x == SettingsManager.Current.LLDPCDPCapture_Duration);
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to restart the application as administrator.
    /// </summary>
    public ICommand RestartAsAdminCommand => new RelayCommand(parameter => { _ = RestartAsAdminAction(); });

    /// <summary>
    /// Action to restart the application as administrator.
    /// </summary>
    private async Task RestartAsAdminAction()
    {
        try
        {
            (Application.Current.MainWindow as MainWindow)?.RestartApplication(true);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error, ex.Message, ChildWindowIcon.Error);
        }
    }

    /// <summary>
    /// Gets the command to start the capture.
    /// </summary>
    public ICommand CaptureCommand => new RelayCommand(parameter => { _ = CaptureAction(); }, Capture_CanExecute);

    private bool Capture_CanExecute(object _) => ConfigurationManager.Current.IsAdmin && !IsCapturing;

    /// <summary>
    /// Action to start the capture.
    /// </summary>
    private async Task CaptureAction()
    {
        if (FirstRun)
            FirstRun = false;

        IsStatusMessageDisplayed = false;
        StatusMessage = string.Empty;
        StatusMessageLevel = StatusLevel.None;

        DiscoveryPackageReceived = false;

        IsCapturing = true;

        var duration = SelectedDuration + 2; // Capture 2 seconds more than the user chose

        _secondsRemaining = duration + 1; // Init powershell etc. takes some time... 

        TimeRemainingMessage = string.Format(Strings.XXSecondsRemainingDots, _secondsRemaining);

        _remainingTimer.Start();

        try
        {
            _lldpCdpCapture.CaptureAsync(duration, SelectedProtocol);
        }
        catch (Exception ex)
        {
            Log.Error("Error while trying to capture", ex);

            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error, ex.Message, ChildWindowIcon.Error);
        }
    }

    /// <summary>
    /// Gets the command to export the result.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(parameter => { _ = ExportAction(); });

    /// <summary>
    /// Action to export the result.
    /// </summary>
    private Task ExportAction()
    {
        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    [DiscoveryPackage]);
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.LLDPCDPCapture_ExportFileType = instance.FileType;
            SettingsManager.Current.LLDPCDPCapture_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        },
            [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], false, SettingsManager.Current.LLDPCDPCapture_ExportFileType,
            SettingsManager.Current.LLDPCDPCapture_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handles the elapsed event of the remaining time timer.
    /// </summary>
    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
        {
            TimeRemainingMessage =
                string.Format(Strings.XXSecondsRemainingDots, _secondsRemaining);

            if (_secondsRemaining > 0)
                _secondsRemaining--;
        }));
    }

    /// <summary>
    /// Called when the view becomes visible.
    /// </summary>
    public void OnViewVisible()
    {
    }

    /// <summary>
    /// Called when the view is hidden.
    /// </summary>
    public void OnViewHide()
    {
    }

    #endregion

    #region Events

    /// <summary>
    /// Handles the PackageReceived event of the LLDP / CDP capture.
    /// </summary>
    private void LLDPCDPCapture_PackageReceived(object sender, LLDPCDPPackageArgs e)
    {
        DiscoveryPackage = e.PackageInfo;

        DiscoveryPackageReceived = true;
    }

    /// <summary>
    /// Handles the WarningReceived event of the LLDP / CDP capture.
    /// </summary>
    private void LLDPCDPCapture_WarningReceived(object sender, LLDPCDPWarningArgs e)
    {
        if (!string.IsNullOrEmpty(StatusMessage))
            StatusMessage += Environment.NewLine;

        StatusMessage += e.Message;

        StatusMessageLevel = StatusMessageLevel.MostSevere(StatusLevel.Warning);

        IsStatusMessageDisplayed = true;
    }

    /// <summary>
    /// Handles the ErrorReceived event of the LLDP / CDP capture.
    /// </summary>
    private void LLDPCDPCapture_ErrorReceived(object sender, LLDPCDPErrorArgs e)
    {
        if (!string.IsNullOrEmpty(StatusMessage))
            StatusMessage += Environment.NewLine;

        StatusMessage += e.Message;
        StatusMessageLevel = StatusMessageLevel.MostSevere(StatusLevel.Error);

        IsStatusMessageDisplayed = true;
    }

    /// <summary>
    /// Handles the Complete event of the LLDP / CDP capture.
    /// </summary>
    private void LLDPCDPCapture_Complete(object sender, EventArgs e)
    {
        _remainingTimer.Stop();
        IsCapturing = false;
    }

    #endregion
}