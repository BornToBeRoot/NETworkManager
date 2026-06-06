using MahApps.Metro.IconPacks;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager;

/// <summary>
/// A generic, stackable notification popup shown in the bottom-right corner of the primary
/// screen. Contains no feature-specific knowledge — icon, color, title and message are all
/// supplied by the caller (via <see cref="NotificationManager"/>).
/// </summary>
public partial class NotificationWindow : INotifyPropertyChanged
{
    #region PropertyChangedEventHandler

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Variables

    private readonly DispatcherTimer _timer = new(DispatcherPriority.Render);
    private readonly Stopwatch _stopwatch = new();

    // Bound properties — all set once in the constructor, no change notifications needed.
    // Note: the header property is named NotificationTitle (not Title) to avoid clashing with
    // the inherited Window.Title property, which a "{Binding Title}" would otherwise resolve to.
    public PackIconMaterialKind IconKind { get; }
    public string IconColor { get; }
    public string NotificationTitle { get; }
    public string Message { get; }

    // Timestamp shown in the header — captured when the notification is created, which is the
    // moment the status change occurred.
    public string Time { get; }

    public double TimeMax { get; }

    public double TimeRemaining
    {
        get;
        private set
        {
            if (Math.Abs(value - field) < 0.001)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    // Command — initialized once in the constructor, not recreated on every binding access
    public ICommand CloseCommand { get; }

    #endregion

    #region Constructor

    public NotificationWindow(PackIconMaterialKind iconKind, string iconColor, string title, string message, int closeTimeSeconds)
    {
        InitializeComponent();
        DataContext = this;

        IconKind = iconKind;
        IconColor = iconColor;
        NotificationTitle = title;
        Message = message;
        Time = DateTime.Now.ToString("HH:mm:ss");
        TimeMax = closeTimeSeconds;
        TimeRemaining = closeTimeSeconds;

        CloseCommand = new RelayCommand(_ => CloseWindow());

        // The window auto-sizes its height to the content (SizeToContent=Height), so when the
        // message wraps to more lines the whole stack must re-anchor to the bottom edge.
        SizeChanged += (_, _) => NotificationManager.RepositionAll();

        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += Timer_Tick;
    }

    #endregion

    #region ICommands & Actions

    private void ShowMainWindow()
    {
        CloseWindow();

        if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow &&
            mainWindow.ShowWindowCommand.CanExecute(null))
            mainWindow.ShowWindowCommand.Execute(null);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Called by <see cref="NotificationManager"/> when the stack changes (a sibling closes or
    /// resizes) and this window may need to move.
    /// </summary>
    internal void Reposition()
    {
        ApplyPosition();
    }

    private void ApplyPosition()
    {
        if (Screen.PrimaryScreen == null)
            return;

        var scale = System.Windows.Media.VisualTreeHelper.GetDpi(this).DpiScaleX;
        var area = Screen.PrimaryScreen.WorkingArea;

        // Offset = total height (incl. margins) of all windows stacked below this one. Using the
        // actual heights keeps variable-height popups (wrapped messages) bottom-anchored.
        var offset = NotificationManager.GetStackOffset(this);

        Left = area.Right / scale - Width - NotificationManager.WindowMargin;
        Top = area.Bottom / scale - ActualHeight - NotificationManager.WindowMargin - offset;
    }

    private void CloseWindow()
    {
        _timer.Stop();
        _stopwatch.Stop();

        Closing -= MetroWindow_Closing;
        Close(); // fires Window.Closed → NotificationManager removes from stack and repositions
    }

    #endregion

    #region Events

    // Lifecycle — the HWND exists here, so DPI is safe to read for positioning
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        ApplyPosition();

        _stopwatch.Restart();
        _timer.Start();
    }

    // Clicking anywhere on the popup opens the main window. The close button handles its own
    // mouse events, so clicking [×] does not bubble up here.
    private void Root_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ShowMainWindow();
    }

    // Intercept Alt+F4 / OS close so the stack cleanup always runs through CloseWindow()
    private void MetroWindow_Closing(object sender, CancelEventArgs e)
    {
        e.Cancel = true;
        CloseWindow();
    }

    private async void Timer_Tick(object sender, EventArgs e)
    {
        // Use a local for the close decision — the TimeRemaining setter ignores sub-0.001 changes
        // (to throttle the progress bar), so reading the property back could stay stuck at a tiny
        // positive value and the window would never close.
        var remaining = Math.Max(0.0, TimeMax - _stopwatch.Elapsed.TotalSeconds);
        TimeRemaining = remaining;

        if (remaining > 0)
            return;

        _timer.Stop();
        _stopwatch.Stop();

        await Task.Delay(250); // let the bar visually reach zero before closing

        CloseWindow();
    }

    #endregion
}
