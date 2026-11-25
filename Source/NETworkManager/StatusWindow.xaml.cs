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

    /// <summary>
    ///     Incremented whenever the close timer is started or stopped. The expired timer only hides the window if it is
    ///     still the same run (the user may have opened the window again while the timer waited before hiding it).
    /// </summary>
    private int _closeTimerRun;

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
        HideWindow();

        if (_mainWindow.ShowWindowCommand.CanExecute(null))
            _mainWindow.ShowWindowCommand.Execute(null);
    }

    public ICommand CloseCommand => new RelayCommand(_ => CloseAction());

    private void CloseAction()
    {
        HideWindow();
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

        // Opened on a network change: don't take the focus from the application the user is working in (the window
        // closes itself, see the close timer). Clicking into it stops the close timer (Activated).
        ShowActivated = !enableCloseTimer;

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

        // Opened by the user, cancel a running close timer (e.g. if the window was opened on a network change)
        StopCloseTimer();

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
        var workingArea = Screen.PrimaryScreen.WorkingArea;

        // Not wider than the working area (e.g. 1280 x 720 with 150 % scaling is 853 px wide), otherwise the window
        // would start outside the screen on the left
        MaxWidth = Math.Max(0, workingArea.Width / scaleFactor - 20);

        Left = Math.Max(workingArea.Left / scaleFactor + 10,
            workingArea.Right / scaleFactor - Math.Min(Width, MaxWidth) - 10);
        Top = workingArea.Bottom / scaleFactor - ActualHeight - 10;
    }

    private void SetupCloseTimer()
    {
        _closeTimerRun++;

        TimeMax = SettingsManager.Current.Status_WindowCloseTime;
        Time = TimeMax;

        _stopwatch.Restart();
        ShowTime = true;
        _dispatcherTimerClose.Start();
    }

    /// <summary>
    ///     Stops the close timer (e.g. the window is opened by the user or hidden). A pending close of the expired timer
    ///     is canceled, see <see cref="_closeTimerRun" />.
    /// </summary>
    private void StopCloseTimer()
    {
        _closeTimerRun++;

        _dispatcherTimerClose.Stop();
        _stopwatch.Stop();
        ShowTime = false;
    }

    /// <summary>
    ///     Hides the window and stops the close timer, so it doesn't keep running in the background.
    /// </summary>
    private void HideWindow()
    {
        StopCloseTimer();

        Hide();
    }

    #endregion

    #region Events

    private void MetroWindow_Activated(object sender, EventArgs e)
    {
        // The user clicked into the window that was opened on a network change: keep it open (like opened by the user)
        if (_dispatcherTimerClose.IsEnabled)
            StopCloseTimer();
    }

    private void MetroWindow_Deactivated(object sender, EventArgs e)
    {
        HideWindow();
    }

    private void MetroWindow_Closing(object sender, CancelEventArgs e)
    {
        e.Cancel = true;

        HideWindow();
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

        var run = _closeTimerRun;

        await Task.Delay(250);

        // Opened again (by the user or another network change) in the meantime
        if (run != _closeTimerRun)
            return;

        HideWindow();
    }

    #endregion
}
