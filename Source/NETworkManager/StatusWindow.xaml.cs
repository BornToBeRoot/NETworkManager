using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager;

public partial class StatusWindow : INotifyPropertyChanged
{
    #region Constructor

    public StatusWindow(MainWindow mainWindow)
    {
        InitializeComponent();
        DataContext = this;

        Title = $"NETworkManager {AssemblyManager.Current.Version} - {Localization.Resources.Strings.NetworkStatus}";

        _mainWindow = mainWindow;

        _dispatcherTimerClose.Interval = TimeSpan.FromMilliseconds(16);
        _dispatcherTimerClose.Tick += DispatcherTimerTime_Tick;

        _networkConnectionView = new NetworkConnectionWidgetView();
        ContentControlNetworkConnection.Content = _networkConnectionView;
    }

    #endregion

    #region PropertyChangedEventHandler

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Variables

    private readonly DispatcherTimer _dispatcherTimerClose = new(DispatcherPriority.Render);
    private readonly Stopwatch _stopwatch = new();

    private readonly MainWindow _mainWindow;
    private readonly NetworkConnectionWidgetView _networkConnectionView;

    private bool _showTime;

    public bool ShowTime
    {
        get => _showTime;
        set
        {
            if (value == _showTime)
                return;

            _showTime = value;
            OnPropertyChanged();
        }
    }

    private double _timeMax;

    public double TimeMax
    {
        get => _timeMax;
        private set
        {
            if (value == _timeMax)
                return;

            _timeMax = value;
            OnPropertyChanged();
        }
    }

    private double _time;

    public double Time
    {
        get => _time;
        set
        {
            if (value == _time)
                return;

            _time = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands & Actions

    public ICommand ReloadCommand => new RelayCommand(_ => ReloadAction());

    private void ReloadAction()
    {
        Check();
    }

    public ICommand ShowMainWindowCommand => new RelayCommand(_ => ShowMainWindowAction());

    private void ShowMainWindowAction()
    {
        Hide();

        if (_mainWindow.ShowWindowCommand.CanExecute(null))
            _mainWindow.ShowWindowCommand.Execute(null);
    }

    public ICommand CloseCommand => new RelayCommand(_ => CloseAction());

    private void CloseAction()
    {
        Hide();
    }

    #endregion

    #region Methods

    private void Check()
    {
        _networkConnectionView.Check();
    }

    /// <summary>
    ///     Show the window on the screen.
    /// </summary>
    /// <param name="enableCloseTimer">Automatically close the window after a certain time.</param>
    public void ShowWindow(bool enableCloseTimer = false)
    {
        // Set window position on primary screen
        // ToDo: User setting...
        if (Screen.PrimaryScreen != null)
        {
            var scaleFactor = System.Windows.Media.VisualTreeHelper.GetDpi(this).DpiScaleX;

            Left = Screen.PrimaryScreen.WorkingArea.Right / scaleFactor - Width - 10;
            Top = Screen.PrimaryScreen.WorkingArea.Bottom / scaleFactor - Height - 10;
        }

        // Show the window
        Show();

        // Check the network connection
        Check();

        enableCloseTimer = true;

        // Close the window after a certain time
        if (enableCloseTimer)
        {
            SetupCloseTimer();
            return;
        }

        // Focus the window
        Activate();
    }

    private void SetupCloseTimer()
    {
        TimeMax = SettingsManager.Current.Status_WindowCloseTime;
        Time = TimeMax;

        _stopwatch.Restart();
        ShowTime = true;
        _dispatcherTimerClose.Start();
    }

    #endregion

    #region Events

    private void MetroWindow_Deactivated(object sender, EventArgs e)
    {
        Hide();
    }

    private void MetroWindow_Closing(object sender, CancelEventArgs e)
    {
        e.Cancel = true;

        Hide();
    }

    private async void DispatcherTimerTime_Tick(object sender, EventArgs e)
    {
        Time = Math.Max(0.0, TimeMax - _stopwatch.Elapsed.TotalSeconds);

        if (Time > 0)
            return;

        _dispatcherTimerClose.Stop();
        _stopwatch.Stop();

        await Task.Delay(250);

        ShowTime = false;

        Hide();
    }

    #endregion
}
