using MahApps.Metro.IconPacks;
using NETworkManager.Localization;
using NETworkManager.Models;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
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
    // Opened when the notification is clicked (the tool or its tab)
    private readonly ApplicationRequest _request;

    // Header: the tool that sent the notification
    public Brush ApplicationIconBrush { get; }
    public string ApplicationTitle { get; }

    public PackIconMaterialKind IconKind { get; }
    public Brush IconBrush { get; }
    public string NotificationTitle { get; }
    public IReadOnlyList<NotificationLineItem> Lines { get; }

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

    public NotificationWindow(NotificationInfo notification)
    {
        InitializeComponent();
        DataContext = this;

        _request = notification.Request ?? new ApplicationRequest(notification.Application);

        ApplicationIconBrush = new VisualBrush(ApplicationManager.GetIcon(notification.Application))
        {
            Stretch = Stretch.Uniform
        };
        ApplicationTitle = ResourceTranslator.Translate(ResourceIdentifier.ApplicationName, notification.Application);

        (IconKind, IconBrush) = GetStatusIcon(notification.Status);
        NotificationTitle = notification.Title;
        Lines = [.. notification.Lines.Select(x => new NotificationLineItem(
            string.IsNullOrEmpty(x.Text) ? string.Empty : x.Text + NotificationLine.Separator, x.Status,
            GetStatusBrush(x.StatusKind)))];
        Time = DateTime.Now.ToString("HH:mm:ss");
        TimeMax = notification.CloseTimeSeconds;
        TimeRemaining = notification.CloseTimeSeconds;

        CloseCommand = new RelayCommand(_ => CloseWindow());

        // The window auto-sizes its height to the content (SizeToContent=Height), so when the
        // message wraps to more lines the whole stack must re-anchor to the bottom edge.
        SizeChanged += (_, _) => NotificationManager.RepositionAll();

        _timer.Interval = TimeSpan.FromMilliseconds(33);
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

        // The tool (and the tab) that sent the notification
        NavigationManager.Open(_request);
    }

    #endregion

    #region Methods

    // Colors of the application (status icons, "Since last scan" of the scanners)
    private static readonly Color UpColor = (Color)ColorConverter.ConvertFromString("#badc58");
    private static readonly Color DownColor = (Color)ColorConverter.ConvertFromString("#b95353");
    private static readonly Color ChangedColor = (Color)ColorConverter.ConvertFromString("#f1b856");

    /// <summary>
    /// Gets the icon and its brush of a status: up green, down red, up and down half green / half red (split along the
    /// diagonal), changed orange.
    /// </summary>
    /// <param name="status">Status of the notification.</param>
    /// <returns>Icon and brush.</returns>
    private static (PackIconMaterialKind kind, Brush brush) GetStatusIcon(NotificationStatus status)
    {
        var brush = GetStatusBrush(status);

        var kind = status switch
        {
            NotificationStatus.Up => PackIconMaterialKind.LanConnect,
            NotificationStatus.Down => PackIconMaterialKind.LanDisconnect,
            NotificationStatus.UpAndDown => PackIconMaterialKind.Lan,
            _ => PackIconMaterialKind.LanPending
        };

        return (kind, brush);
    }

    /// <summary>
    /// Gets the brush of a status (status icon, status text of a line): up green, down red, up and down half green /
    /// half red (split along the diagonal), changed orange.
    /// </summary>
    /// <param name="status">Status.</param>
    /// <returns>Frozen brush.</returns>
    private static Brush GetStatusBrush(NotificationStatus status)
    {
        Brush brush = status switch
        {
            NotificationStatus.Up => new SolidColorBrush(UpColor),
            NotificationStatus.Down => new SolidColorBrush(DownColor),
            NotificationStatus.UpAndDown => new LinearGradientBrush(new GradientStopCollection
            {
                // Two stops at the same offset: a sharp diagonal edge instead of a gradient
                new(UpColor, 0), new(UpColor, 0.5), new(DownColor, 0.5), new(DownColor, 1)
            }, new Point(0, 0), new Point(1, 1)),
            _ => new SolidColorBrush(ChangedColor)
        };

        brush.Freeze();

        return brush;
    }

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

/// <summary>
/// A line of the notification as shown: the text with the separator (empty if the line has only a status), the status
/// and its brush (see <see cref="NotificationLine" />).
/// </summary>
/// <param name="Prefix">Text of the line with the separator, e.g. "10.0.0.5 (server) · ".</param>
/// <param name="Status">Status text, e.g. "New".</param>
/// <param name="StatusBrush">Brush of the status text.</param>
public sealed record NotificationLineItem(string Prefix, string Status, Brush StatusBrush);
