using MahApps.Metro.Controls;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager;

public partial class StatusWindow : MetroWindow, INotifyPropertyChanged
{
    #region PropertyChangedEventHandler
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Variables
    // Set prio to make the ui smoother
    private readonly DispatcherTimer _dispatcherTimerClose = new(DispatcherPriority.Normal);

    private readonly MainWindow _mainWindow;
    private readonly NetworkConnectionView _networkConnectionView;

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
        set
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

    #region Constructor
    public StatusWindow(MainWindow mainWindow)
    {
        InitializeComponent();
        DataContext = this;

        _mainWindow = mainWindow;

        _dispatcherTimerClose.Interval = new System.TimeSpan(0, 0, 0, 0, 250);
        _dispatcherTimerClose.Tick += DispatcherTimerTime_Tick;

        _networkConnectionView = new NetworkConnectionView();
        ContentControlNetworkConnection.Content = _networkConnectionView;
    }
    #endregion

    #region ICommands & Actions
    public ICommand ReloadCommand => new RelayCommand(p => ReloadAction());

    private void ReloadAction()
    {
        Reload();
    }

    public ICommand ShowMainWindowCommand => new RelayCommand(p => ShowMainWindowAction());

    private void ShowMainWindowAction()
    {
        Hide();

        if (_mainWindow.ShowWindowCommand.CanExecute(null))
            _mainWindow.ShowWindowCommand.Execute(null);
    }

    public ICommand CloseCommand => new RelayCommand(p => CloseAction());

    private void CloseAction()
    {
        Hide();
    }

    #endregion

    #region Methods
    private void Reload()
    {
        _networkConnectionView.Reload();
    }

    /// <summary>
    /// Show the window on the screen.
    /// </summary>
    /// <param name="fromNetworkChangeEvent">Focus the window (will automatically hide if the focus is lost).</param>
    public void ShowWindow(bool fromNetworkChangeEvent)
    {
        // Show on primary screen in left/bottom corner
        // ToDo: User setting...
        Left = Screen.PrimaryScreen.WorkingArea.Right - Width - 10;
        Top = Screen.PrimaryScreen.WorkingArea.Bottom - Height - 10;

        Show();

        if (fromNetworkChangeEvent)
            Activate();
        else
            SetupCloseTimer();

        Topmost = true;
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
    private void MetroWindow_Deactivated(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void MetroWindow_Closing(object sender, CancelEventArgs e)
    {
        e.Cancel = true;

        Hide();
    }

    private void DispatcherTimerTime_Tick(object sender, System.EventArgs e)
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
