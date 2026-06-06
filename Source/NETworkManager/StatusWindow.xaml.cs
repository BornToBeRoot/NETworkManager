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

        _dispatcherTimerClose.Interval = TimeSpan.FromMilliseconds(33);
        _dispatcherTimerClose.Tick += DispatcherTimerTime_Tick;

        _networkConnectionView = new NetworkConnectionWidgetView();
        ContentControlNetworkConnection.Content = _networkConnectionView;

        // With SizeToContent="Height" the height is only known after layout; re-anchor to the
        // bottom-right whenever it changes so the window does not drift to the top of the screen.
        SizeChanged += (_, _) => UpdatePosition();
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

    public bool ShowTime
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

    public double TimeMax
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

    public double Time
    {
        get;
        set
        {
            // Same guard as NotificationWindow.TimeRemaining: ignore sub-0.001 changes so the
            // countdown updates at the same cadence in both windows.
            if (Math.Abs(value - field) < 0.001)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands & Actions

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
        // Position bottom-right on the primary screen. With SizeToContent="Height" the final
        // height is only known after layout, so this is refined again in SizeChanged.
        UpdatePosition();

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

    /// <summary>
    ///     Anchors the window to the bottom-right corner of the primary screen. Uses
    ///     <see cref="FrameworkElement.ActualHeight"/> (not Height, which is NaN while
    ///     SizeToContent is active) so the window stays bottom-anchored as its height changes.
    /// </summary>
    private void UpdatePosition()
    {
        // ToDo: User setting...
        if (Screen.PrimaryScreen == null)
            return;

        var scaleFactor = System.Windows.Media.VisualTreeHelper.GetDpi(this).DpiScaleX;

        Left = Screen.PrimaryScreen.WorkingArea.Right / scaleFactor - Width - 10;
        Top = Screen.PrimaryScreen.WorkingArea.Bottom / scaleFactor - ActualHeight - 10;
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
        // Use a local for the close decision — the Time setter now ignores sub-0.001 changes, so
        // reading the property back could stay stuck at a tiny positive value and never hide.
        var remaining = Math.Max(0.0, TimeMax - _stopwatch.Elapsed.TotalSeconds);
        Time = remaining;

        if (remaining > 0)
            return;

        _dispatcherTimerClose.Stop();
        _stopwatch.Stop();

        await Task.Delay(250);

        ShowTime = false;

        Hide();
    }

    #endregion
}
