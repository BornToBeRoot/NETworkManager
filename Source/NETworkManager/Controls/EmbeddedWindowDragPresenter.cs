using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dragablz;
using NETworkManager.Models;

namespace NETworkManager.Controls;

/// <summary>
///     Hosts an embedded window (Remote Desktop, PowerShell, PuTTY, TigerVNC, WebView2) of a tab. While a window with
///     tabs of the application is dragged (<see cref="IsDragging" />, e.g. a tab is dragged out), the embedded window
///     is hidden: it is always drawn on top of WPF elements (airspace) and would cover the drop zones of Dragablz
///     (split the tabs). Instead of an empty area, a screenshot of the embedded window is shown, like the content of
///     the other tools (e.g. IP Scanner) stays visible during a drag. In the dragged window itself (just created, its
///     content is not on the screen yet) and if no screenshot can be taken, a placeholder with the icon of the
///     application and the title of the tab is shown.
///     The template is defined in <c>Resources/Styles/EmbeddedWindowDragPresenterStyles.xaml</c>.
/// </summary>
[TemplatePart(Name = PartContent, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PartScreenshot, Type = typeof(Image))]
[TemplatePart(Name = PartPlaceholder, Type = typeof(FrameworkElement))]
public class EmbeddedWindowDragPresenter : ContentControl
{
    private const string PartContent = "PART_Content";
    private const string PartScreenshot = "PART_Screenshot";
    private const string PartPlaceholder = "PART_Placeholder";

    public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.Register(nameof(IsDragging),
        typeof(bool), typeof(EmbeddedWindowDragPresenter), new FrameworkPropertyMetadata(false, OnIsDraggingChanged));

    public static readonly DependencyProperty ApplicationNameProperty = DependencyProperty.Register(
        nameof(ApplicationName), typeof(ApplicationName), typeof(EmbeddedWindowDragPresenter),
        new FrameworkPropertyMetadata(ApplicationName.None));

    private ContentPresenter _content;
    private Image _screenshot;
    private FrameworkElement _placeholder;

    /// <summary>
    ///     A window with tabs of the application is dragged (e.g.
    ///     <see cref="Settings.ConfigurationInfo.IsPuTTYWindowDragging" />).
    /// </summary>
    public bool IsDragging
    {
        get => (bool)GetValue(IsDraggingProperty);
        set => SetValue(IsDraggingProperty, value);
    }

    /// <summary>
    ///     Application of the tab, for the icon of the placeholder.
    /// </summary>
    public ApplicationName ApplicationName
    {
        get => (ApplicationName)GetValue(ApplicationNameProperty);
        set => SetValue(ApplicationNameProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _content = GetTemplateChild(PartContent) as ContentPresenter;
        _screenshot = GetTemplateChild(PartScreenshot) as Image;
        _placeholder = GetTemplateChild(PartPlaceholder) as FrameworkElement;

        Update(IsDragging);
    }

    private static void OnIsDraggingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EmbeddedWindowDragPresenter presenter)
            presenter.Update((bool)e.NewValue);
    }

    private void Update(bool isDragging)
    {
        // Template not applied (yet)
        if (_content == null || _screenshot == null || _placeholder == null)
            return;

        if (!isDragging)
        {
            _content.Visibility = Visibility.Visible;
            _screenshot.Visibility = Visibility.Hidden;
            _screenshot.Source = null;
            _screenshot.ClearValue(WidthProperty);
            _screenshot.ClearValue(HeightProperty);
            _placeholder.Visibility = Visibility.Hidden;

            return;
        }

        // Tab not shown (e.g. another tab is selected) - nothing to show, hide the embedded window as before
        if (IsVisible)
        {
            // The dragged window was just created, its content is not on the screen yet: a screenshot would show
            // what is behind it
            var screenshot = IsInDraggedWindow() ? null : AirspacePanel.CaptureScreenshot(_content);

            if (screenshot != null)
            {
                // Size of the content when the screenshot was taken (top left aligned), not stretched if the window
                // is resized during the drag
                _screenshot.Width = _content.ActualWidth;
                _screenshot.Height = _content.ActualHeight;
                _screenshot.Source = screenshot;
                _screenshot.Visibility = Visibility.Visible;
            }
            else
            {
                _placeholder.Visibility = Visibility.Visible;
            }
        }

        _content.Visibility = Visibility.Hidden;
    }

    /// <summary>
    ///     Whether this tab is in the window that is dragged (Dragablz sets <see cref="TabablzControl.IsDraggingWindow" />
    ///     on the tab control of the dragged window).
    /// </summary>
    private bool IsInDraggedWindow()
    {
        DependencyObject element = this;

        while (element != null)
        {
            if (element is TabablzControl { IsDraggingWindow: true })
                return true;

            element = VisualTreeHelper.GetParent(element);
        }

        return false;
    }
}
