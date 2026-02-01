using log4net;
using MahApps.Metro.SimpleChildWindow;
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
/// View model for the discovery protocol view.
/// </summary>
public class DiscoveryProtocolViewModel : ViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(DiscoveryProtocolViewModel));

    /// <summary>
    /// The discovery protocol capture instance.
    /// </summary>
    private readonly DiscoveryProtocolCapture _discoveryProtocolCapture = new();

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
    /// Backing field for <see cref="FirstRun"/>.
    /// </summary>
    private bool _firstRun = true;

    /// <summary>
    /// Gets or sets a value indicating whether this is the first run.
    /// </summary>
    public bool FirstRun
    {
        get => _firstRun;
        set
        {
            if (value == _firstRun)
                return;

            _firstRun = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="Protocols"/>.
    /// </summary>
    private List<DiscoveryProtocol> _protocols = new();

    /// <summary>
    /// Gets the list of available discovery protocols.
    /// </summary>
    public List<DiscoveryProtocol> Protocols
    {
        get => _protocols;
        private set
        {
            if (value == _protocols)
                return;

            _protocols = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="SelectedProtocol"/>.
    /// </summary>
    private DiscoveryProtocol _selectedProtocol;

    /// <summary>
    /// Gets or sets the selected discovery protocol.
    /// </summary>
    public DiscoveryProtocol SelectedProtocol
    {
        get => _selectedProtocol;
        set
        {
            if (value == _selectedProtocol)
                return;

            if (!_isLoading)
                SettingsManager.Current.DiscoveryProtocol_Protocol = value;

            _selectedProtocol = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="Durations"/>.
    /// </summary>
    private List<int> _durations;

    /// <summary>
    /// Gets the list of available durations.
    /// </summary>
    public List<int> Durations
    {
        get => _durations;
        private set
        {
            if (value == _durations)
                return;

            _durations = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="SelectedDuration"/>.
    /// </summary>
    private int _selectedDuration;

    /// <summary>
    /// Gets or sets the selected duration.
    /// </summary>
    public int SelectedDuration
    {
        get => _selectedDuration;
        set
        {
            if (value == _selectedDuration)
                return;

            if (!_isLoading)
                SettingsManager.Current.DiscoveryProtocol_Duration = value;

            _selectedDuration = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="IsCapturing"/>.
    /// </summary>
    private bool _isCapturing;

    /// <summary>
    /// Gets or sets a value indicating whether the capture is running.
    /// </summary>
    public bool IsCapturing
    {
        get => _isCapturing;
        set
        {
            if (value == _isCapturing)
                return;

            _isCapturing = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="TimeRemainingMessage"/>.
    /// </summary>
    private string _timeRemainingMessage;

    /// <summary>
    /// Gets the message for the remaining time.
    /// </summary>
    public string TimeRemainingMessage
    {
        get => _timeRemainingMessage;
        private set
        {
            if (value == _timeRemainingMessage)
                return;

            _timeRemainingMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="IsStatusMessageDisplayed"/>.
    /// </summary>
    private bool _isStatusMessageDisplayed;

    /// <summary>
    /// Gets or sets a value indicating whether the status message is displayed.
    /// </summary>
    public bool IsStatusMessageDisplayed
    {
        get => _isStatusMessageDisplayed;
        set
        {
            if (value == _isStatusMessageDisplayed)
                return;

            _isStatusMessageDisplayed = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="StatusMessage"/>.
    /// </summary>
    private string _statusMessage;

    /// <summary>
    /// Gets the status message.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (value == _statusMessage)
                return;

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="DiscoveryPackageReceived"/>.
    /// </summary>
    private bool _discoveryPackageReceived;

    /// <summary>
    /// Gets or sets a value indicating whether a discovery package has been received.
    /// </summary>
    public bool DiscoveryPackageReceived
    {
        get => _discoveryPackageReceived;
        set
        {
            if (value == _discoveryPackageReceived)
                return;

            _discoveryPackageReceived = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="DiscoveryPackage"/>.
    /// </summary>
    private DiscoveryProtocolPackageInfo _discoveryPackage;

    /// <summary>
    /// Gets the received discovery package.
    /// </summary>
    public DiscoveryProtocolPackageInfo DiscoveryPackage
    {
        get => _discoveryPackage;
        private set
        {
            if (value == _discoveryPackage)
                return;

            _discoveryPackage = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, LoadSettings

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoveryProtocolViewModel"/> class.
    /// </summary>
    /// <param name="instance">The dialog coordinator instance.</param>
    public DiscoveryProtocolViewModel()
    {
        _isLoading = true;

        _discoveryProtocolCapture.PackageReceived += DiscoveryProtocol_PackageReceived;
        _discoveryProtocolCapture.ErrorReceived += DiscoveryProtocol_ErrorReceived;
        _discoveryProtocolCapture.WarningReceived += DiscoveryProtocol_WarningReceived;
        _discoveryProtocolCapture.Complete += DiscoveryProtocol_Complete;

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
        Protocols = Enum.GetValues(typeof(DiscoveryProtocol)).Cast<DiscoveryProtocol>().OrderBy(x => x.ToString())
            .ToList();
        SelectedProtocol = Protocols.FirstOrDefault(x => x == SettingsManager.Current.DiscoveryProtocol_Protocol);
        Durations = new List<int> { 15, 30, 60, 90, 120 };
        SelectedDuration = Durations.FirstOrDefault(x => x == SettingsManager.Current.DiscoveryProtocol_Duration);
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to restart the application as administrator.
    /// </summary>
    public ICommand RestartAsAdminCommand => new RelayCommand(_ => RestartAsAdminAction().ConfigureAwait(false));

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
    public ICommand CaptureCommand => new RelayCommand(_ => CaptureAction().ConfigureAwait(false));

    /// <summary>
    /// Action to start the capture.
    /// </summary>
    private async Task CaptureAction()
    {
        if (FirstRun)
            FirstRun = false;

        IsStatusMessageDisplayed = false;
        StatusMessage = string.Empty;

        DiscoveryPackageReceived = false;

        IsCapturing = true;

        var duration = SelectedDuration + 2; // Capture 2 seconds more than the user chose

        _secondsRemaining = duration + 1; // Init powershell etc. takes some time... 

        TimeRemainingMessage = string.Format(Strings.XXSecondsRemainingDots, _secondsRemaining);

        _remainingTimer.Start();

        try
        {
            _discoveryProtocolCapture.CaptureAsync(duration, SelectedProtocol);
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
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

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

            SettingsManager.Current.DiscoveryProtocol_ExportFileType = instance.FileType;
            SettingsManager.Current.DiscoveryProtocol_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        },
            [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], false, SettingsManager.Current.DiscoveryProtocol_ExportFileType,
            SettingsManager.Current.DiscoveryProtocol_ExportFilePath);

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
    /// Handles the PackageReceived event of the discovery protocol capture.
    /// </summary>
    private void DiscoveryProtocol_PackageReceived(object sender, DiscoveryProtocolPackageArgs e)
    {
        DiscoveryPackage = e.PackageInfo;

        DiscoveryPackageReceived = true;
    }

    /// <summary>
    /// Handles the WarningReceived event of the discovery protocol capture.
    /// </summary>
    private void DiscoveryProtocol_WarningReceived(object sender, DiscoveryProtocolWarningArgs e)
    {
        if (!string.IsNullOrEmpty(StatusMessage))
            StatusMessage += Environment.NewLine;

        StatusMessage += e.Message;

        IsStatusMessageDisplayed = true;
    }

    /// <summary>
    /// Handles the ErrorReceived event of the discovery protocol capture.
    /// </summary>
    private void DiscoveryProtocol_ErrorReceived(object sender, DiscoveryProtocolErrorArgs e)
    {
        if (!string.IsNullOrEmpty(StatusMessage))
            StatusMessage += Environment.NewLine;

        StatusMessage += e.Message;

        IsStatusMessageDisplayed = true;
    }

    /// <summary>
    /// Handles the Complete event of the discovery protocol capture.
    /// </summary>
    private void DiscoveryProtocol_Complete(object sender, EventArgs e)
    {
        _remainingTimer.Stop();
        IsCapturing = false;
    }

    #endregion
}