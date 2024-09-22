using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using log4net;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager;

public partial class StatusWindow : INotifyPropertyChanged
{
    #region Constructor

    public StatusWindow(MainWindow mainWindow)
    {
        InitializeComponent();
        DataContext = this;

        _mainWindow = mainWindow;

        _dispatcherTimerClose.Interval = new TimeSpan(0, 0, 0, 0, 250);
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
    
    // Set priority to make the ui smoother
    private readonly DispatcherTimer _dispatcherTimerClose = new(DispatcherPriority.Normal);

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

    private int _timeMax;

    public int TimeMax
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

    private int _time;

    public int Time
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
            Left = Screen.PrimaryScreen.WorkingArea.Right - Width - 10;
            Top = Screen.PrimaryScreen.WorkingArea.Bottom - Height - 10;
        }

        // Show the window
        Show();
        
        // Check the network connection
        Check();
        
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
        Time = SettingsManager.Current.Status_WindowCloseTime * 4;
        TimeMax = Time;

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

    private void DispatcherTimerTime_Tick(object sender, EventArgs e)
    {
        Time--;

        if (Time > 0)
            return;

        _dispatcherTimerClose.Stop();
        ShowTime = false;

        Hide();
    }

    #endregion
}