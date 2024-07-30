using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class DiscoveryProtocolViewModel : ViewModelBase
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly DiscoveryProtocolCapture _discoveryProtocolCapture = new();
    private readonly bool _isLoading;
    private readonly Timer _remainingTimer;
    private int _secondsRemaining;

    private bool _firstRun = true;

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

    private List<DiscoveryProtocol> _protocols = new();

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

    private DiscoveryProtocol _selectedProtocol;

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

    private List<int> _durations;

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

    private int _selectedDuration;

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

    private bool _isCapturing;

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

    private string _timeRemainingMessage;

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

    private bool _isStatusMessageDisplayed;

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

    private string _statusMessage;

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

    private bool _discoveryPackageReceived;

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

    private DiscoveryProtocolPackageInfo _discoveryPackage;

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

    public DiscoveryProtocolViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

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

    public ICommand RestartAsAdminCommand => new RelayCommand(_ => RestartAsAdminAction().ConfigureAwait(false));

    private async Task RestartAsAdminAction()
    {
        try
        {
            (Application.Current.MainWindow as MainWindow)?.RestartApplication(true);
        }
        catch (Exception ex)
        {
            await _dialogCoordinator.ShowMessageAsync(this, Strings.Error, ex.Message,
                MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
        }
    }

    public ICommand CaptureCommand => new RelayCommand(_ => CaptureAction().ConfigureAwait(false));

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
            await _dialogCoordinator.ShowMessageAsync(this, Strings.Error, ex.Message,
                MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
        }
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    private async Task ExportAction()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.Export
        };

        var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType,
                        [DiscoveryPackage]);
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                        Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.DiscoveryProtocol_ExportFileType = instance.FileType;
                SettingsManager.Current.DiscoveryProtocol_ExportFilePath = instance.FilePath;
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], false, SettingsManager.Current.DiscoveryProtocol_ExportFileType,
            SettingsManager.Current.DiscoveryProtocol_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    #endregion

    #region Methods

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

    public void OnViewVisible()
    {
    }

    public void OnViewHide()
    {
    }

    #endregion

    #region Events

    private void DiscoveryProtocol_PackageReceived(object sender, DiscoveryProtocolPackageArgs e)
    {
        DiscoveryPackage = e.PackageInfo;

        DiscoveryPackageReceived = true;
    }

    private void DiscoveryProtocol_WarningReceived(object sender, DiscoveryProtocolWarningArgs e)
    {
        if (!string.IsNullOrEmpty(StatusMessage))
            StatusMessage += Environment.NewLine;

        StatusMessage += e.Message;

        IsStatusMessageDisplayed = true;
    }

    private void DiscoveryProtocol_ErrorReceived(object sender, DiscoveryProtocolErrorArgs e)
    {
        if (!string.IsNullOrEmpty(StatusMessage))
            StatusMessage += Environment.NewLine;

        StatusMessage += e.Message;

        IsStatusMessageDisplayed = true;
    }

    private void DiscoveryProtocol_Complete(object sender, EventArgs e)
    {
        _remainingTimer.Stop();
        IsCapturing = false;
    }

    #endregion
}