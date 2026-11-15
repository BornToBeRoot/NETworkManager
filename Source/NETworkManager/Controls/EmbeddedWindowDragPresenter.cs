using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Dragablz;
using Dragablz.Dockablz;
using NETworkManager.Converters;
using NETworkManager.Models;

namespace NETworkManager.Controls;

/// <summary>
///     Hosts an embedded window (Remote Desktop, PowerShell, PuTTY, TigerVNC, WebView2) of a tab. While a window with
///     tabs of the application is dragged (<see cref="IsDragging" />, e.g. a tab is dragged out), the embedded window
///     is hidden: it is always drawn on top of WPF elements (airspace) and would cover the drop zones of Dragablz
///     (split the tabs). Instead of an empty area, a placeholder with the icon of the application and the title of the
///     tab is shown.
///
///     A screenshot of the embedded window was tried instead of the placeholder, but it is taken when the dragged
///     window is already shown above the tab and the embedded window is not always drawn: parts of the dragged
///     window, offset and blurry (DPI scaling) pictures.
///
///     The template is defined in <c>Resources/Styles/EmbeddedWindowDragPresenterStyles.xaml</c>.
/// </summary>
[TemplatePart(Name = PartContent, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PartPlaceholder, Type = typeof(FrameworkElement))]
public class EmbeddedWindowDragPresenter : ContentControl
{
    private const string PartContent = "PART_Content";
    private const string PartPlaceholder = "PART_Placeholder";

    public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.Register(nameof(IsDragging),
        typeof(bool), typeof(EmbeddedWindowDragPresenter), new FrameworkPropertyMetadata(false, OnIsDraggingChanged));

    public static readonly DependencyProperty ApplicationNameProperty = DependencyProperty.Register(
        nameof(ApplicationName), typeof(ApplicationName), typeof(EmbeddedWindowDragPresenter),
        new FrameworkPropertyMetadata(ApplicationName.None));

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title),
        typeof(string), typeof(EmbeddedWindowDragPresenter), new FrameworkPropertyMetadata(string.Empty));

    private ContentPresenter _content;
    private FrameworkElement _placeholder;

    public EmbeddedWindowDragPresenter()
    {
        // The view of a tab moves to another window / branch (dragged out, merged, split): bind to the ancestors of
        // the new place each time - a RelativeSource FindAncestor binding is only resolved once
        Loaded += (_, _) => BindIsDragging();
        Unloaded += (_, _) => BindingOperations.ClearBinding(this, IsDraggingProperty);
    }

    /// <summary>
    ///     A window with tabs of the application is dragged (drag state of Dragablz, see <see cref="BindIsDragging" />):
    ///     the window of this tab is dragged (<see cref="TabablzControl.IsDraggingWindow" />) or the dragged window can
    ///     be dropped into the layout of this tab (<see cref="Layout.IsParticipatingInDrag" />).
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

    /// <summary>
    ///     Title of the tab, for the placeholder (set when the drag starts).
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _content = GetTemplateChild(PartContent) as ContentPresenter;
        _placeholder = GetTemplateChild(PartPlaceholder) as FrameworkElement;

        Update(IsDragging);
    }

    /// <summary>
    ///     Binds <see cref="IsDragging" /> to the <see cref="TabablzControl" /> and the <see cref="Layout" /> around
    ///     this presenter (called when it is loaded, also after the tab moved to another window or branch).
    /// </summary>
    private void BindIsDragging()
    {
        var binding = new MultiBinding { Converter = new BooleansOrConverter() };

        var tabablzControl = FindAncestor<TabablzControl>();
        if (tabablzControl != null)
            binding.Bindings.Add(new Binding(nameof(TabablzControl.IsDraggingWindow)) { Source = tabablzControl });

        var layout = FindAncestor<Layout>();
        if (layout != null)
            binding.Bindings.Add(new Binding(nameof(Layout.IsParticipatingInDrag)) { Source = layout });

        BindingOperations.SetBinding(this, IsDraggingProperty, binding);
    }

    private T FindAncestor<T>() where T : DependencyObject
    {
        DependencyObject element = this;

        while ((element = VisualTreeHelper.GetParent(element)) != null)
            if (element is T ancestor)
                return ancestor;

        return null;
    }

    private static void OnIsDraggingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EmbeddedWindowDragPresenter presenter)
            presenter.Update((bool)e.NewValue);
    }

    private void Update(bool isDragging)
    {
        // Template not applied (yet)
        if (_content == null || _placeholder == null)
            return;

        _content.Visibility = isDragging ? Visibility.Hidden : Visibility.Visible;

        if (isDragging)
            Title = FindTabItem()?.Header ?? string.Empty;

        // Only instead of a shown embedded window: e.g. a disconnected tab hides it and shows its own message
        _placeholder.Visibility = isDragging && Content is UIElement { Visibility: Visibility.Visible }
            ? Visibility.Visible
            : Visibility.Hidden;
    }

    /// <summary>
    ///     The tab of this embedded window: the tab of the surrounding TabablzControl whose view (e.g. PuTTYControl)
    ///     contains this presenter. Not the selected item of the TabablzControl - in the window that was just
    ///     created for a dragged-out tab, no tab is selected yet during the drag.
    /// </summary>
    private DragablzTabItem FindTabItem()
    {
        UserControl view = null;
        DependencyObject element = this;

        while (element != null)
        {
            element = VisualTreeHelper.GetParent(element);

            switch (element)
            {
                case UserControl userControl when view == null:
                    view = userControl;
                    break;
                case TabablzControl tabablzControl:
                    return tabablzControl.Items.OfType<DragablzTabItem>().FirstOrDefault(x => ReferenceEquals(x.View, view))
                           ?? tabablzControl.SelectedItem as DragablzTabItem;
            }
        }

        return null;
    }
}
