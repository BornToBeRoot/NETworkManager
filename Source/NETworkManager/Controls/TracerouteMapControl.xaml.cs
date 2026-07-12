using log4net;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.IPApi;
using NETworkManager.Models.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using NETworkManager.ViewModels;
using NETworkManager.Views;
using Path = System.Windows.Shapes.Path;

namespace NETworkManager.Controls;

/// <summary>
/// Control that visualizes traceroute hops with known geolocation on an abstract, offline world
/// map (no tile/network requests) with mouse-wheel zoom and drag-to-pan.
/// </summary>
/// <remarks>
/// Known limitations (flagged in PR #3520's review, deliberately left as-is for now):
/// - Arrows are drawn using the raw longitude delta between two points, so a route crossing the
///   antimeridian (e.g. Tokyo to the US west coast) is fitted/drawn the long way around the map
///   instead of wrapping across the edge.
/// - Markers/arrows are only reachable via mouse hover (no keyboard focus or automation names),
///   so their info panel isn't accessible via keyboard or screen reader.
/// </remarks>
public partial class TracerouteMapControl
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TracerouteMapControl));

    private const double MapWidth = 1000;
    private const double MapHeight = 500;

    // Country labels are only shown once zoomed in this far past the fit-to-view scale,
    // otherwise all ~240 names would overlap into an unreadable mess.
    private const double LabelVisibleScaleFactor = 1.5;

    private const double HopMarkerSize = 14;
    private const double HopMarkerHitTestSize = 32;
    private const double CityDotSize = 3;
    private const double CityLabelGapScreenPixels = 4;
    private const double HopLabelGapScreenPixels = 6;
    private const double ArrowStrokeThickness = 2;
    private const double ArrowHitTestStrokeThickness = 14;
    private const double ArrowHeadLength = 12;
    private const double CountryBorderStrokeThickness = 0.75;

    // How far arrow paths bow to one side (see DrawArrow), as a fraction of their own length,
    // floored/capped so both very short and very long segments still look reasonable. Kept
    // subtle on purpose - enough to tell two overlapping directions apart, not a half-loop.
    private const double ArrowCurveFraction = 0.08;
    private const double ArrowCurveMinOffset = 3;
    private const double ArrowCurveMaxOffset = 18;

    private const double ArrowDefaultOpacity = 0.55;

    private static readonly Lazy<List<CountryData>> Countries = new(LoadCountries);
    private static readonly Lazy<StreamGeometry> CountriesGeometry = new(BuildCountriesGeometry);
    private static readonly Lazy<List<(string Name, Point Position)>> CountryLabels = new(BuildCountryLabels);
    private static readonly Lazy<List<CityData>> Cities = new(LoadCities);

    // Hops/arrows and country labels follow the theme (accent / muted gray, via
    // SetResourceReference per element so they track theme switches a frozen brush couldn't).
    // Cities deliberately don't: they keep a fixed brand blue (the speedtest download series'
    // "#1ba1e2") so they stay a consistent "reference point" color that doesn't compete with the
    // accent-colored traceroute path in either theme.
    private static readonly Brush CityBrush = FreezeBrush(Color.FromRgb(0x1B, 0xA1, 0xE2));

    // Shared by every label/dot so a single assignment (in ApplyTransform) keeps their on-screen
    // size constant while the map itself is scaled by the parent Canvas' zoom transform. Elements
    // using this must be scaled around their own center (RenderTransformOrigin 0.5,0.5), since
    // that's the only anchor a single shared transform can honor for differently-positioned elements.
    private readonly ScaleTransform _inverseScaleTransform = new(1, 1);

    // Arrow lines/heads can't use the shared transform above: a line's own endpoints must stay
    // exactly on the hops they connect, so only their stroke thickness / arrowhead size may be
    // counter-scaled, each around its own fixed anchor (the arrow's tip) - tracked here so
    // ApplyTransform can update them every time the zoom level changes.
    private readonly List<ArrowVisual> _arrows = [];

    // Every label's Tag holds (Point Anchor, Size TextSize): the anchor it's positioned relative
    // to, and its measured size cached at build time. The size can NOT be read back later via
    // TextBlock.DesiredSize - WPF resets DesiredSize to (0,0) for a Collapsed element on every
    // Measure() call, including ones WPF's own layout system performs internally once the
    // element is added to the tree, silently invalidating a perfectly good earlier measurement.
    //
    // Tracked in separate lists (rather than distinguished via Tag type) so ApplyTransform and
    // DeclutterReferenceLabels don't need to re-scan/re-classify LabelsCanvas.Children every time.
    private readonly List<TextBlock> _countryLabels = [];
    private readonly List<TextBlock> _cityLabels = [];

    // Hop number labels reposition their gap to the marker on every zoom step, same idea as the
    // city labels, but tracked separately since they must stay visible regardless of zoom level.
    private readonly List<TextBlock> _hopNumberLabels = [];

    // Last drawn hop points, cached so a resize or "Reset" click can re-fit to the same hops.
    private List<Point> _hopPoints = [];

    // Scale value the label/arrow visuals were last recomputed for; panning alone never
    // invalidates them (see ApplyTransform), only an actual zoom change does.
    private double _lastScaleDependentUpdateScale = -1;

    private double _scale = 1;
    private double _minScale = 1;
    private double _maxScale = 1;
    private double _panX;
    private double _panY;

    private bool _isPanning;
    private Point _panStartMouse;
    private double _panStartX;
    private double _panStartY;

    public TracerouteMapControl()
    {
        InitializeComponent();

        Loaded += TracerouteMapControl_Loaded;
    }

    #region Dependency properties

    /// <summary>
    /// Gets or sets the traceroute hops (e.g. <see cref="TracerouteViewModel.Results" />) to plot on the map.
    /// </summary>
    public static readonly DependencyProperty HopsProperty = DependencyProperty.Register(
        nameof(Hops), typeof(IEnumerable), typeof(TracerouteMapControl),
        new PropertyMetadata(null, OnHopsChanged));

    public IEnumerable Hops
    {
        get => (IEnumerable)GetValue(HopsProperty);
        set => SetValue(HopsProperty, value);
    }

    private static void OnHopsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((TracerouteMapControl)d).OnHopsChanged(e.OldValue as INotifyCollectionChanged);
    }

    private void OnHopsChanged(INotifyCollectionChanged oldCollection)
    {
        if (oldCollection != null)
            oldCollection.CollectionChanged -= Hops_CollectionChanged;

        if (Hops is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += Hops_CollectionChanged;

        RedrawHops();
    }

    private void Hops_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RedrawHops();
    }

    /// <summary>
    /// Gets or sets whether the host (e.g. <see cref="TracerouteView" />) currently shows this
    /// control at full size, vs. collapsed down to just its top-left toggle button. Two-way
    /// bindable so the host's collapse row-height logic and this control's own chevron icon
    /// (see the DataTrigger on the toggle button in XAML) both stay in sync with a single value.
    /// </summary>
    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(TracerouteMapControl),
        new PropertyMetadata(true, OnIsExpandedChanged));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    private void ButtonToggleExpand_Click(object sender, RoutedEventArgs e)
    {
        IsExpanded = !IsExpanded;
    }

    // Set whenever RedrawHops is skipped because the control is collapsed (see RedrawHops) -
    // triggers a real redraw once expanded again, with the now-correct (no longer collapsed-strip)
    // size available.
    private bool _hopsRedrawPending;

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var view = (TracerouteMapControl)d;

        if (!(bool)e.NewValue || !view._hopsRedrawPending)
            return;

        // Deferred for the same reason as the initial RedrawHops call in TracerouteMapControl_Loaded:
        // MapRow.Height was just changed (by TracerouteView's ExpandMapRow, in response to this
        // same IsExpanded change reaching it through the ExpandMapView binding), but that resize
        // - and with it BorderHost.ActualWidth/Height - has not actually been applied by WPF's
        // layout system yet at this exact point. Redrawing right now would still see the old,
        // collapsed size and reproduce the very bug this deferral exists to avoid.
        view.Dispatcher.BeginInvoke(view.RedrawHops, DispatcherPriority.Loaded);
    }

    #endregion

    #region Load, draw

    private void TracerouteMapControl_Loaded(object sender, RoutedEventArgs e)
    {
        // The embedded world map is ~1.4 MB of JSON; parsing it and building the (frozen)
        // country geometry synchronously here would briefly hitch the UI thread the first time a
        // Traceroute tab is opened. Materialize the shared Lazy caches on a background thread
        // instead - they hold pure data plus a frozen Freezable, both safe to build off the UI
        // thread - then hop back to the UI thread to create the actual WPF label/city elements
        // and draw. Hops themselves need none of this data (they project directly), so a trace
        // arriving while this is still loading still renders fine on its own.
        Task.Run(() =>
        {
            try
            {
                // CountriesGeometry.Value also forces Countries.Value.
                _ = CountriesGeometry.Value;
                _ = CountryLabels.Value;
                _ = Cities.Value;
            }
            catch
            {
                // Swallowed so the task never faults (which would surface as an unobserved task
                // exception) - the continuation re-touches the same Lazy values and logs there.
            }
        }).ContinueWith(_ =>
        {
            try
            {
                CountriesPath.Data = CountriesGeometry.Value;
                BuildLabels();
                BuildCities();
            }
            catch (Exception ex)
            {
                Log.Error("Error while loading the embedded world map data", ex);
            }

            // Deferred: right after Loaded, ancestors (Expander/Grid/tab) can still be mid-layout,
            // so BorderHost.ActualWidth/Height may read as 0 even after an explicit UpdateLayout()
            // call. Waiting for the current layout pass to fully settle guarantees FitToHops/
            // FitToView can compute the real scale/pan on the very first run, instead of hop/city
            // labels sitting at their approximate build-time position until the user zooms.
            Dispatcher.BeginInvoke(RedrawHops, DispatcherPriority.Loaded);
        }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void BuildLabels()
    {
        LabelsCanvas.Children.Clear();
        _countryLabels.Clear();

        foreach (var (name, position) in CountryLabels.Value)
        {
            var label = new TextBlock
            {
                Text = name,
                FontSize = 12,
                IsHitTestVisible = false,
                RenderTransform = _inverseScaleTransform,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            label.SetResourceReference(TextBlock.ForegroundProperty, "MahApps.Brushes.Gray3");

            // Must run while still Visibility.Visible (the default) - see the comment on the
            // _countryLabels/_cityLabels fields for why this measurement has to be cached now.
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var size = label.DesiredSize;
            label.Tag = (position, size);

            // Hidden by default so a fully zoomed-out view never briefly shows the full,
            // unfiltered wall of ~240 country + 660 city names before the first zoom-driven
            // ApplyTransform call has a chance to declutter/hide them.
            label.Visibility = Visibility.Collapsed;

            Canvas.SetLeft(label, position.X - size.Width / 2);
            Canvas.SetTop(label, position.Y - size.Height / 2);

            LabelsCanvas.Children.Add(label);
            _countryLabels.Add(label);
        }
    }

    /// <summary>
    /// Draws a small dot for every major city (always visible) plus a name label next to it
    /// (added to <see cref="LabelsCanvas"/>, so it shares the country labels' zoom-based visibility).
    /// </summary>
    private void BuildCities()
    {
        CitiesCanvas.Children.Clear();
        _cityLabels.Clear();

        foreach (var city in Cities.Value)
        {
            var point = Project(city.Lat, city.Lon);

            var dot = new Ellipse
            {
                Width = CityDotSize,
                Height = CityDotSize,
                Fill = CityBrush,
                RenderTransform = _inverseScaleTransform,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            Canvas.SetLeft(dot, point.X - dot.Width / 2);
            Canvas.SetTop(dot, point.Y - dot.Height / 2);

            CitiesCanvas.Children.Add(dot);

            // Renders to the left of the dot (hop number labels render to the right - see
            // DrawHopMarker), so a hop located exactly in a labelled city doesn't collide with
            // its own number. Uses the same center-pivot scaling (RenderTransformOrigin 0.5,0.5)
            // as every other label/dot, just centered on an offset point instead of the dot itself.
            var label = new TextBlock
            {
                Text = city.N,
                FontSize = 11,
                Foreground = CityBrush,
                IsHitTestVisible = false,
                RenderTransform = _inverseScaleTransform,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            // Must run while still Visibility.Visible - see the comment on the
            // _countryLabels/_cityLabels fields for why this measurement has to be cached now.
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var size = label.DesiredSize;

            // Stashes the anchor point + measured size so ApplyTransform can keep the gap to the
            // dot a constant number of screen pixels instead of it growing while zooming in.
            label.Tag = (point, size);

            // Hidden by default - see the matching comment in BuildLabels.
            label.Visibility = Visibility.Collapsed;

            // Approximates the gap assuming scale=1 as a sane default before the first
            // ApplyTransform call recomputes it precisely for the actual current zoom.
            var halfWidth = size.Width / 2;
            Canvas.SetLeft(label, point.X - CityLabelGapScreenPixels - halfWidth - halfWidth);
            Canvas.SetTop(label, point.Y - size.Height / 2);

            LabelsCanvas.Children.Add(label);
            _cityLabels.Add(label);
        }
    }

    private void RedrawHops()
    {
        // While collapsed, BorderHost is just the thin strip behind the toggle button (see
        // TracerouteView's CollapseMapRow) - fitting/spreading against that tiny size would
        // compute a _scale wildly more zoomed-out than the real one, and SpreadOverlappingPoints'
        // screen-pixel-based offset (see its own doc comment) would then be converted to a huge
        // number of map units, badly mislocating markers that share a city (e.g. multiple
        // Frankfurt hops). Deferring the redraw until the control is actually expanded again -
        // see OnIsExpandedChanged - avoids ever computing against that bogus collapsed size.
        if (!IsExpanded)
        {
            _hopsRedrawPending = true;
            return;
        }

        _hopsRedrawPending = false;

        HopsCanvas.Children.Clear();
        _arrows.Clear();
        _hopNumberLabels.Clear();

        // Forces the FitToHops -> ApplyTransform call at the end of this method to fully
        // recompute positions even if it lands on the same scale as before - otherwise the
        // brand-new hop labels just created below would keep their scale=1 build-time
        // approximation whenever a new hop doesn't happen to change the required zoom.
        _lastScaleDependentUpdateScale = -1;

        var groups = new List<(List<TracerouteHopInfo> Hops, Point Point, IPGeolocationInfo Info)>();

        if (Hops != null)
        {
            foreach (var item in Hops)
            {
                if (item is not TracerouteHopInfo hop)
                    continue;

                var info = hop.IPGeolocationResult?.Info;

                // Skip hops without a resolved geolocation (e.g. private/local addresses).
                if (info == null || (info.Lat == 0 && info.Lon == 0))
                    continue;

                // Merge into the previous group if it's the same city/country and directly
                // follows it, so a run of hops within the same city gets a single marker
                // instead of several overlapping dots. "Directly follows" is checked against the
                // group's last actual hop number (not just group adjacency) - otherwise an
                // unresolved hop skipped by the check above (e.g. resolved hops 2 and 4 in the
                // same city with unresolved hop 3 between them) would silently merge into the
                // same marker and its "Hops 2-4" label would wrongly imply hop 3 belongs to it.
                if (groups.Count > 0 && groups[^1].Hops[^1].Hop == hop.Hop - 1 &&
                    IsSameLocation(groups[^1].Info, info))
                {
                    groups[^1].Hops.Add(hop);
                    continue;
                }

                groups.Add(([hop], Project(info.Lat, info.Lon), info));
            }
        }

        var rawPoints = groups.Select(g => g.Point).ToList();

        // Fits first (against the still-empty canvas - harmless) so the spread below can be
        // computed in actual *screen* pixels using the scale this route renders at. Doing it
        // the other way around (spread first, using whatever _scale happened to be left over
        // from the previous view) made the spacing wrong whenever this route's own fit scale
        // differs a lot from that leftover value - e.g. a route spanning several cities zooms
        // out much further than a tight single-city cluster, so a spread that looked fine for
        // one made the markers overlap again for the other.
        FitToHops(rawPoints);

        // Fans out repeat visits to the same city that weren't merged above (something was in
        // between - e.g. Frankfurt -> Cologne -> Frankfurt) so their markers don't overlap; see
        // SpreadOverlappingPoints. Grouped by city/country name (see BuildLocationKey for the
        // missing-city fallback). Converts the offset via _minScale (the most-zoomed-out scale
        // reachable, not this route's fit _scale) so the gap stays >= the intended screen pixels
        // at every zoom - baking in the current _scale would shrink it below a pixel once zoomed
        // back out.
        var displayPoints = SpreadOverlappingPoints(
            groups.Select(g => (g.Point, LocationKey: BuildLocationKey(g.Info, g.Point))).ToList(),
            1 / _minScale);

        _hopPoints = displayPoints;

        // Markers are drawn first (so their highlight callbacks exist for the arrows below to
        // wire up), but given a higher Panel.ZIndex so they still render on top of the arrows,
        // regardless of add order.
        var markerHighlighters = new List<Action<bool>>();
        var markerHitTargets = new List<UIElement>();

        for (var i = 0; i < groups.Count; i++)
        {
            var (highlighter, hitTarget) = DrawHopMarker(groups[i].Hops, groups[i].Info, displayPoints[i]);
            markerHighlighters.Add(highlighter);
            markerHitTargets.Add(hitTarget);
        }

        var arrowHighlighters = new List<Action<bool>>();

        for (var i = 0; i < groups.Count - 1; i++)
            arrowHighlighters.Add(DrawArrow(
                (groups[i].Hops, displayPoints[i], groups[i].Info),
                (groups[i + 1].Hops, displayPoints[i + 1], groups[i + 1].Info),
                markerHighlighters[i], markerHighlighters[i + 1]));

        // Wires each marker to also highlight its adjacent arrow(s) when hovered, now that both
        // markers and arrows (and their highlight callbacks) exist.
        for (var i = 0; i < groups.Count; i++)
        {
            var incoming = i > 0 ? arrowHighlighters[i - 1] : null;
            var outgoing = i < groups.Count - 1 ? arrowHighlighters[i] : null;

            if (incoming == null && outgoing == null)
                continue;

            var hitTarget = markerHitTargets[i];

            hitTarget.MouseEnter += (_, _) =>
            {
                incoming?.Invoke(true);
                outgoing?.Invoke(true);
            };

            hitTarget.MouseLeave += (_, _) =>
            {
                incoming?.Invoke(false);
                outgoing?.Invoke(false);
            };
        }
    }

    /// <summary>
    /// Nudges repeat occurrences of the same location straight down, stacked below one another,
    /// so e.g. a route that hairpins through the same city twice gets two distinguishable
    /// markers instead of one hiding the other. A predictable vertical stack reads more cleanly
    /// than a spiral once combined with the curved arrows in DrawArrow. Spacing is given in
    /// *screen* pixels (converted via inverseScale) rather than fixed map units, since a fixed
    /// map-unit offset looked fine for a tight single-city cluster but was nowhere near enough
    /// once a route spans several cities and the map zooms out much further to fit all of them.
    /// Grouped by a caller-supplied location key rather than the point itself, since different
    /// IPs in the same city commonly resolve to slightly different lat/lon.
    /// </summary>
    private static List<Point> SpreadOverlappingPoints(List<(Point Point, string LocationKey)> points, double inverseScale)
    {
        const double verticalSpacingScreenPixels = 26;
        var verticalSpacing = verticalSpacingScreenPixels * inverseScale;

        var occurrenceCounts = new Dictionary<string, int>();
        var result = new List<Point>(points.Count);

        foreach (var (point, locationKey) in points)
        {
            var occurrence = occurrenceCounts.GetValueOrDefault(locationKey);
            occurrenceCounts[locationKey] = occurrence + 1;

            result.Add(occurrence == 0 ? point : point with { Y = point.Y + verticalSpacing * occurrence });
        }

        return result;
    }

    /// <summary>
    /// Draws a hop marker and returns a callback to toggle its highlight (invoked both on its
    /// own hover and, from RedrawHops, when an adjacent arrow is hovered) plus its hit-test
    /// element (so RedrawHops can wire the reverse: hovering this marker highlighting its
    /// adjacent arrows too).
    /// </summary>
    private (Action<bool> SetHighlighted, UIElement HitTarget) DrawHopMarker(List<TracerouteHopInfo> hops, IPGeolocationInfo info, Point point)
    {
        var tooltip = BuildHopTooltip(hops, info);

        // Markers render above arrows regardless of add order - RedrawHops creates all markers
        // before the arrows, so their highlight callbacks exist for the arrows to wire up.
        const int markerZIndex = 1;

        // An invisible, much larger hit target behind the visible dot - the marker itself is
        // only ~9 screen pixels, too small to reliably hover.
        var hitTarget = new Ellipse
        {
            Width = HopMarkerHitTestSize,
            Height = HopMarkerHitTestSize,
            Fill = Brushes.Transparent,
            RenderTransform = _inverseScaleTransform,
            RenderTransformOrigin = new Point(0.5, 0.5)
        };

        Canvas.SetLeft(hitTarget, point.X - hitTarget.Width / 2);
        Canvas.SetTop(hitTarget, point.Y - hitTarget.Height / 2);
        Panel.SetZIndex(hitTarget, markerZIndex);

        HopsCanvas.Children.Add(hitTarget);

        // Purely visual - IsHitTestVisible=false so it never shadows hitTarget underneath it;
        // without this, hovering exactly on the (smaller) visible dot hit *this* element instead
        // (topmost at that point, since it's added after hitTarget), which has no handlers, so
        // nothing happened - only hovering the "donut" around it (hitTarget without ellipse on
        // top) worked.
        var ellipse = new Ellipse
        {
            Width = HopMarkerSize,
            Height = HopMarkerSize,
            IsHitTestVisible = false,
            RenderTransform = _inverseScaleTransform,
            RenderTransformOrigin = new Point(0.5, 0.5)
        };

        ellipse.SetResourceReference(Shape.FillProperty, "MahApps.Brushes.Accent");

        Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
        Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);
        Panel.SetZIndex(ellipse, markerZIndex);

        HopsCanvas.Children.Add(ellipse);

        // Renders to the right of the dot - see the matching comment on city labels in
        // BuildCities for why this uses a center-pivot offset instead of an edge anchor.
        var numberLabel = new TextBlock
        {
            Text = hops.Count == 1 ? $"{hops[0].Hop}" : $"{hops[0].Hop}–{hops[^1].Hop}",
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            IsHitTestVisible = false,
            RenderTransform = _inverseScaleTransform,
            RenderTransformOrigin = new Point(0.5, 0.5)
        };

        numberLabel.SetResourceReference(TextBlock.ForegroundProperty, "MahApps.Brushes.Gray3");

        numberLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var size = numberLabel.DesiredSize;
        numberLabel.Tag = (point, size);

        // RedrawHops fits the view to the route before calling this, so _scale is already
        // correct here - unlike BuildCities/BuildLabels (which run once at Loaded, before any
        // fit has happened), this can position exactly instead of approximating at scale=1.
        RepositionOffsetLabel(numberLabel, HopLabelGapScreenPixels, 1 / _scale, growLeft: false);
        Canvas.SetTop(numberLabel, point.Y - size.Height / 2);
        Panel.SetZIndex(numberLabel, markerZIndex);

        HopsCanvas.Children.Add(numberLabel);
        _hopNumberLabels.Add(numberLabel);

        // Purely visual - deliberately does NOT touch the info panel itself, since this is also
        // invoked from RedrawHops when an *adjacent arrow* is hovered (cross-highlighting this
        // marker), and that shouldn't replace whatever info the arrow itself is showing.
        void SetHighlighted(bool highlighted)
        {
            // Fetched live (rather than a frozen field) so the ring keeps matching the current
            // theme even if the user switches theme while this view is still alive.
            ellipse.Stroke = highlighted
                ? (Brush)Application.Current.FindResource("MahApps.Brushes.ThemeForeground")
                : null;
            ellipse.StrokeThickness = highlighted ? 2 : 0;
        }

        // Only this marker's own hover (not cross-highlighting) shows/hides its info.
        hitTarget.MouseEnter += (_, _) =>
        {
            SetHighlighted(true);
            ShowInfoPanel(tooltip);
        };

        hitTarget.MouseLeave += (_, _) =>
        {
            SetHighlighted(false);
            HideInfoPanel();
        };

        return (SetHighlighted, hitTarget);
    }

    private static bool IsSameLocation(IPGeolocationInfo a, IPGeolocationInfo b)
    {
        // A missing city (e.g. a rural hop the geolocation service could only place in a
        // country, not a specific city) must not compare equal to another missing city - two
        // such hops are otherwise treated as the "same" location purely because both are blank,
        // even when their actual coordinates are far apart. Fall back to comparing the resolved
        // coordinate directly instead.
        if (string.IsNullOrEmpty(a.City) || string.IsNullOrEmpty(b.City))
            return a.Lat.Equals(b.Lat) && a.Lon.Equals(b.Lon);

        return string.Equals(a.City, b.City, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.Country, b.Country, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds the key SpreadOverlappingPoints groups markers by. Same city/country logic as
    /// IsSameLocation - a missing city falls back to the projected point itself (rounded, so
    /// float noise doesn't split what's really the same point into different keys) rather than
    /// comparing two blank cities as equal, which would otherwise treat unrelated rural hops in
    /// the same country as repeat visits to one location and shift them away from their real
    /// coordinates.
    /// </summary>
    private static string BuildLocationKey(IPGeolocationInfo info, Point point)
    {
        return !string.IsNullOrEmpty(info.City) ?
            $"{info.City}␟{info.Country}".ToLowerInvariant() : 
            $"{info.Country}␟{Math.Round(point.X, 1)}␟{Math.Round(point.Y, 1)}".ToLowerInvariant();
    }

    private static string BuildHopTooltip(List<TracerouteHopInfo> hops, IPGeolocationInfo info)
    {
        var firstHop = hops[0];

        var hopLabel = hops.Count == 1
            ? $"{Strings.Hop} {firstHop.Hop}"
            : $"{Strings.Hops} {firstHop.Hop}–{hops[^1].Hop}";

        var lines = new List<string> { hopLabel, FormatLocation(info, includeRegion: true) };

        if (!string.IsNullOrEmpty(info.Isp))
            lines.Add(info.Isp);

        if (!string.IsNullOrEmpty(info.As))
            lines.Add(info.As);

        if (!string.IsNullOrEmpty(firstHop.Hostname))
            lines.Add(firstHop.Hostname);

        var ipLine = firstHop.IPAddress?.ToString() ?? "-";

        if (hops.Count > 1)
            ipLine += " " + string.Format(Strings.PlusXMore, hops.Count - 1);

        lines.Add(ipLine);

        var averageRoundTripTime = GetAverageRoundTripTime(hops);

        if (averageRoundTripTime.HasValue)
            lines.Add($"Ø {averageRoundTripTime.Value:F0} ms");

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Averages every valid ping across all hops in a group (e.g. "Hops 3-6"), since showing
    /// just the first hop's own times would be misleading for a merged marker. A "valid" ping
    /// is Success OR TtlExpired - matching Ping.TimeToString's own definition of a real reply,
    /// since intermediate router hops normally answer with TtlExpired, not Success (only the
    /// actual destination host does) - checking Success alone would only ever find a time for
    /// the very last hop.
    /// </summary>
    private static double? GetAverageRoundTripTime(List<TracerouteHopInfo> hops)
    {
        var times = new List<long>();

        foreach (var hop in hops)
        {
            if (IsValidPingStatus(hop.Status1))
                times.Add(hop.Time1);

            if (IsValidPingStatus(hop.Status2))
                times.Add(hop.Time2);

            if (IsValidPingStatus(hop.Status3))
                times.Add(hop.Time3);
        }

        return times.Count > 0 ? times.Average() : null;
    }

    private static bool IsValidPingStatus(IPStatus status)
    {
        return status is IPStatus.Success or IPStatus.TtlExpired;
    }

    private static string FormatLocation(IPGeolocationInfo info, bool includeRegion = false)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(info.City))
            parts.Add(info.City);

        if (includeRegion && !string.IsNullOrEmpty(info.RegionName))
            parts.Add(info.RegionName);

        if (!string.IsNullOrEmpty(info.Country))
            parts.Add(info.Country);

        return parts.Count > 0 ? string.Join(", ", parts) : "-";
    }

    /// <summary>
    /// Draws an arrow and returns a callback to toggle its highlight (invoked both on its own
    /// hover and, via RedrawHops, when either endpoint marker is hovered). Also wires the
    /// reverse: hovering this arrow highlights the two marker callbacks passed in.
    /// </summary>
    private Action<bool> DrawArrow((List<TracerouteHopInfo> Hops, Point Point, IPGeolocationInfo Info) from,
        (List<TracerouteHopInfo> Hops, Point Point, IPGeolocationInfo Info) to,
        Action<bool> highlightFromMarker, Action<bool> highlightToMarker)
    {
        var dx = to.Point.X - from.Point.X;
        var dy = to.Point.Y - from.Point.Y;
        var length = Math.Sqrt(dx * dx + dy * dy);

        // Skip hops that resolved to (nearly) the same location.
        if (length < 0.01)
            return static _ => { };

        var tooltip = $"{FormatLocation(from.Info, includeRegion: true)}\n↓\n{FormatLocation(to.Info, includeRegion: true)}";

        // Bows the path to one side, consistently relative to its own direction of travel, so a
        // segment and its reverse (e.g. Frankfurt -> Cologne -> Frankfurt) curve to opposite
        // sides instead of drawing exactly on top of each other. Also capped to a fraction of
        // this segment's own length - ArrowCurveMinOffset is tuned for normal inter-city hops,
        // but for two markers only a few map units apart (e.g. a hop resolving to a suburb right
        // next to the previous one) that fixed floor dwarfed the segment itself and bowed it into
        // a near-loop instead of a slight curve.
        var perpX = -dy / length;
        var perpY = dx / length;
        var curveOffset = Math.Min(
            Math.Clamp(length * ArrowCurveFraction, ArrowCurveMinOffset, ArrowCurveMaxOffset),
            length * 0.5);
        var controlPoint = new Point(
            (from.Point.X + to.Point.X) / 2 + perpX * curveOffset,
            (from.Point.Y + to.Point.Y) / 2 + perpY * curveOffset);

        // The tangent direction at each end (used to trim back to the marker's outer edge below)
        // depends only on the fixed endpoints/control point, never on scale, so it's computed
        // once here rather than in UpdateArrowGeometry.
        var startAngle = Math.Atan2(controlPoint.Y - from.Point.Y, controlPoint.X - from.Point.X);
        var endAngle = Math.Atan2(to.Point.Y - controlPoint.Y, to.Point.X - controlPoint.X);

        var geometry = new PathGeometry();
        var figure = new PathFigure { IsClosed = false };
        var segment = new QuadraticBezierSegment { Point1 = controlPoint };
        figure.Segments.Add(segment);
        geometry.Figures.Add(figure);

        // An invisible, much wider hit target along the same curve - the visible stroke is only
        // ~1-2 screen pixels, too thin to reliably hover. Shares the same (mutable) geometry
        // object with the visible path below, so both update together in UpdateArrowGeometry.
        var hitPath = new Path
        {
            Data = geometry,
            Stroke = Brushes.Transparent
        };

        HopsCanvas.Children.Add(hitPath);

        // The curve's endpoints must stay exactly on the hops it connects. Kept semi-transparent
        // by default (see hitPath's MouseEnter/Leave below) so a dense tangle of arrows doesn't
        // visually dominate the map; hovering highlights just that one. IsHitTestVisible=false so
        // it never shadows hitPath underneath it - see the matching comment on the ellipse in
        // DrawHopMarker for why that matters.
        var path = new Path
        {
            Data = geometry,
            StrokeDashArray = [3, 2.2],
            // Rounded dash caps/joins read softer/more modern than the default hard rectangular
            // ends on each dash segment.
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeDashCap = PenLineCap.Round,
            Opacity = ArrowDefaultOpacity,
            IsHitTestVisible = false
        };

        path.SetResourceReference(Shape.StrokeProperty, "MahApps.Brushes.Accent");

        HopsCanvas.Children.Add(path);

        var headTransform = new ScaleTransform();

        var arrowHead = new Polygon
        {
            RenderTransform = headTransform,
            Opacity = ArrowDefaultOpacity,
            IsHitTestVisible = false
        };

        arrowHead.SetResourceReference(Shape.FillProperty, "MahApps.Brushes.Accent");

        HopsCanvas.Children.Add(arrowHead);

        var arrow = new ArrowVisual
        {
            FromPoint = from.Point,
            ToPoint = to.Point,
            Length = length,
            ControlPoint = controlPoint,
            StartAngle = startAngle,
            EndAngle = endAngle,
            Figure = figure,
            Segment = segment,
            Path = path,
            HitPath = hitPath,
            ArrowHead = arrowHead,
            HeadTransform = headTransform
        };

        // Positions everything for the current scale right away - RedrawHops fits the view before
        // drawing, so _scale is already correct. ApplyTransform calls UpdateArrowGeometry again on
        // every zoom (see its doc) so the marker-edge trim keeps tracking the marker's constant
        // screen size instead of freezing at this route's auto-fit scale.
        UpdateArrowGeometry(arrow, 1 / _scale);

        _arrows.Add(arrow);

        // Purely visual - deliberately does NOT touch the info panel; see the matching comment
        // in DrawHopMarker (this is also invoked when this arrow's own endpoint markers are
        // hovered, cross-highlighting it, and that shouldn't replace the marker's own info).
        void SetHighlighted(bool highlighted)
        {
            var opacity = highlighted ? 1 : ArrowDefaultOpacity;
            path.Opacity = opacity;
            arrowHead.Opacity = opacity;
            highlightFromMarker(highlighted);
            highlightToMarker(highlighted);
        }

        // Only this arrow's own hover (not cross-highlighting) shows/hides its info.
        hitPath.MouseEnter += (_, _) =>
        {
            SetHighlighted(true);
            ShowInfoPanel(tooltip);
        };

        hitPath.MouseLeave += (_, _) =>
        {
            SetHighlighted(false);
            HideInfoPanel();
        };

        return SetHighlighted;
    }

    /// <summary>
    /// Re-trims an arrow's curve to the marker's outer edge and re-scales its stroke/arrowhead
    /// for the given inverseScale. Called once when the arrow is first drawn and again from
    /// ApplyTransform every time the user zooms, so the trim - a fixed number of *map* units at
    /// any given moment - keeps representing a constant number of *screen* pixels instead of
    /// staying frozen at whatever scale happened to be current when the arrow was created.
    /// </summary>
    private static void UpdateArrowGeometry(ArrowVisual arrow, double inverseScale)
    {
        // Capped to a fraction of this segment's own length: a route might also contain a hop far
        // away (e.g. another continent), which forces a much more zoomed-out auto-fit _scale for
        // the whole view - inverseScale (and so the trim, in map units) grows accordingly, and
        // for a short segment (e.g. two hops in the same city) an uncapped trim could eat most or
        // all of it, badly distorting - or even inverting - the curve.
        var markerRadius = Math.Min(HopMarkerSize / 2 * inverseScale, arrow.Length * 0.3);

        var startPoint = new Point(
            arrow.FromPoint.X + Math.Cos(arrow.StartAngle) * markerRadius,
            arrow.FromPoint.Y + Math.Sin(arrow.StartAngle) * markerRadius);
        var endPoint = new Point(
            arrow.ToPoint.X - Math.Cos(arrow.EndAngle) * markerRadius,
            arrow.ToPoint.Y - Math.Sin(arrow.EndAngle) * markerRadius);

        arrow.Figure.StartPoint = startPoint;
        arrow.Segment.Point2 = endPoint;

        const double arrowAngle = 25 * Math.PI / 180;

        var p1 = new Point(
            endPoint.X - ArrowHeadLength * Math.Cos(arrow.EndAngle - arrowAngle),
            endPoint.Y - ArrowHeadLength * Math.Sin(arrow.EndAngle - arrowAngle));
        var p2 = new Point(
            endPoint.X - ArrowHeadLength * Math.Cos(arrow.EndAngle + arrowAngle),
            endPoint.Y - ArrowHeadLength * Math.Sin(arrow.EndAngle + arrowAngle));

        arrow.ArrowHead.Points = [endPoint, p1, p2];

        // Scaled around the trimmed tip, so the arrowhead keeps a constant on-screen size while
        // staying pinned exactly to the marker edge it points at.
        arrow.HeadTransform.CenterX = endPoint.X;
        arrow.HeadTransform.CenterY = endPoint.Y;
        arrow.HeadTransform.ScaleX = inverseScale;
        arrow.HeadTransform.ScaleY = inverseScale;

        arrow.Path.StrokeThickness = ArrowStrokeThickness * inverseScale;
        arrow.HitPath.StrokeThickness = ArrowHitTestStrokeThickness * inverseScale;
    }

    /// <summary>
    /// Shows the fixed info panel (top-left) with the given text - see the XAML comment on
    /// InfoPanel for why this replaces a WPF ToolTip popup for hop markers/arrows.
    /// </summary>
    private void ShowInfoPanel(string text)
    {
        InfoPanelText.Text = text;
        InfoPanel.Visibility = Visibility.Visible;
    }

    private void HideInfoPanel()
    {
        InfoPanel.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Projects WGS84 latitude/longitude to the map's equirectangular canvas coordinates.
    /// </summary>
    private static Point Project(double lat, double lon)
    {
        var x = (lon + 180.0) / 360.0 * MapWidth;
        var y = (90.0 - lat) / 180.0 * MapHeight;

        return new Point(x, y);
    }

    // Latitude the empty (no-trace) view centers on vertically. ~40°N keeps populated land
    // (USA / Europe / Mediterranean) around the middle instead of the empty equator ocean.
    private const double DefaultViewCenterLatitude = 40;

    #endregion

    #region Zoom & pan

    private void UpdateScaleBounds()
    {
        _minScale = Math.Min(BorderHost.ActualWidth / MapWidth, BorderHost.ActualHeight / MapHeight);
        _maxScale = _minScale * 30;
    }

    private void FitToView()
    {
        // Forces any pending layout to complete so ActualWidth/Height are already correct the
        // very first time this runs (e.g. right after Loaded), instead of silently bailing out
        // on a still-zero size and leaving labels stuck at their un-transformed Canvas.Left=0.
        BorderHost.UpdateLayout();

        if (BorderHost.ActualWidth <= 0 || BorderHost.ActualHeight <= 0)
            return;

        UpdateScaleBounds();

        // Fill the view with the map ("cover", not "contain"): the map row is usually much wider
        // than tall, so scaling to fit the whole world would leave big empty side margins with a
        // small map floating in the middle. Scaling to the larger axis instead makes the map fill
        // BorderHost; any overflow on the other axis is clipped by BorderHost's own ClipToBounds.
        _scale = Math.Clamp(Math.Max(BorderHost.ActualWidth / MapWidth, BorderHost.ActualHeight / MapHeight), _minScale, _maxScale);

        // Center horizontally on the map's middle and vertically on a populated latitude (see
        // DefaultViewCenterLatitude) instead of the empty equator. ClampPan keeps either axis from
        // exposing blank canvas past the map's edge (and just centers an axis that already fits).
        var centerX = MapWidth / 2;
        var centerY = Project(DefaultViewCenterLatitude, 0).Y;

        _panX = ClampPan(BorderHost.ActualWidth / 2 - centerX * _scale, MapWidth * _scale,
            BorderHost.ActualWidth);
        _panY = ClampPan(BorderHost.ActualHeight / 2 - centerY * _scale, MapHeight * _scale,
            BorderHost.ActualHeight);

        ApplyTransform();
    }

    /// <summary>
    /// Keeps a pan offset from showing blank canvas past the map's real edge on one side while
    /// content still reaches the opposite viewport edge. Centers instead whenever the scaled map
    /// is already smaller than the viewport on this axis, since then no pan can make real content
    /// reach both edges anyway.
    /// </summary>
    private static double ClampPan(double pan, double scaledContentSize, double viewportSize)
    {
        if (scaledContentSize <= viewportSize)
            return (viewportSize - scaledContentSize) / 2;

        return Math.Clamp(pan, viewportSize - scaledContentSize, 0);
    }

    /// <summary>
    /// Zooms/pans so all given (already projected) points are visible, falling back to the
    /// whole world when there are none. Called after every hop redraw, on resize, and on Reset.
    /// </summary>
    private void FitToHops(IReadOnlyList<Point> points)
    {
        if (points.Count == 0)
        {
            FitToView();
            return;
        }

        // See FitToView: forces layout so a still-pending initial measure can't leave this
        // bailing out silently and labels stuck un-transformed.
        BorderHost.UpdateLayout();

        if (BorderHost.ActualWidth <= 0 || BorderHost.ActualHeight <= 0)
            return;

        UpdateScaleBounds();

        // Padding scales with the hops' own spread (~12.5% border on each side), so a route
        // across continents and a cluster of hops in one city both end up a similarly tight fit.
        // Floored for degenerate spans (e.g. a single hop), so it doesn't zoom in indefinitely.
        const double paddingFraction = 0.125;
        const double minPaddingMapUnits = 3;

        var rawMinX = points.Min(p => p.X);
        var rawMaxX = points.Max(p => p.X);
        var rawMinY = points.Min(p => p.Y);
        var rawMaxY = points.Max(p => p.Y);

        var paddingX = Math.Max((rawMaxX - rawMinX) * paddingFraction, minPaddingMapUnits);
        var paddingY = Math.Max((rawMaxY - rawMinY) * paddingFraction, minPaddingMapUnits);

        var minX = rawMinX - paddingX;
        var maxX = rawMaxX + paddingX;
        var minY = rawMinY - paddingY;
        var maxY = rawMaxY + paddingY;

        var width = Math.Max(maxX - minX, 1);
        var height = Math.Max(maxY - minY, 1);

        _scale = Math.Clamp(Math.Min(BorderHost.ActualWidth / width, BorderHost.ActualHeight / height),
            _minScale, _maxScale);

        var centerX = (minX + maxX) / 2;
        var centerY = (minY + maxY) / 2;

        _panX = BorderHost.ActualWidth / 2 - centerX * _scale;
        _panY = BorderHost.ActualHeight / 2 - centerY * _scale;

        ApplyTransform();
    }

    private void ApplyTransform()
    {
        ZoomTransform.ScaleX = _scale;
        ZoomTransform.ScaleY = _scale;
        PanTransform.X = _panX;
        PanTransform.Y = _panY;

        // Counter-scaling, arrow geometry and label repositioning only depend on the zoom level
        // (panning shifts every point by the same offset), so skip them while the user is just
        // dragging the map around.
        var scaleChanged = Math.Abs(_scale - _lastScaleDependentUpdateScale) >= 1e-9;

        if (scaleChanged)
        {
            _lastScaleDependentUpdateScale = _scale;

            // Counter-scale the labels/dots so their on-screen size stays constant.
            var inverseScale = 1 / _scale;
            _inverseScaleTransform.ScaleX = inverseScale;
            _inverseScaleTransform.ScaleY = inverseScale;

            // Country border StrokeThickness is otherwise in map units like everything else in
            // RootCanvas, so at high zoom (up to 30x) it would render many screen pixels thick.
            CountriesPath.StrokeThickness = CountryBorderStrokeThickness * inverseScale;

            foreach (var arrow in _arrows)
                UpdateArrowGeometry(arrow, inverseScale);

            // Hop number labels always stay visible regardless of zoom/declutter - they're the
            // actual traceroute data, not the reference layer.
            foreach (var label in _hopNumberLabels)
                RepositionOffsetLabel(label, HopLabelGapScreenPixels, inverseScale, growLeft: false);

            // City labels grow to the left instead (see BuildCities).
            foreach (var label in _cityLabels)
                RepositionOffsetLabel(label, CityLabelGapScreenPixels, inverseScale, growLeft: true);
        }

        // Unlike the block above, *which* reference labels are in view changes with panning too,
        // and DeclutterReferenceLabels culls off-screen ones - so it must also re-run on pan,
        // otherwise a label panned into view would stay hidden from when it was still off-screen
        // (leaving just the city dots). Cheap on pan: off-screen labels are dropped by a single
        // viewport check before the overlap pass. Below the visibility threshold nothing shows
        // regardless of pan, so there only a scale change needs to act (to collapse them once).
        var referenceLabelsVisible = _scale > _minScale * LabelVisibleScaleFactor;

        if (scaleChanged || referenceLabelsVisible)
            DeclutterReferenceLabels(referenceLabelsVisible);
    }

    /// <summary>
    /// Repositions a label center-pivoted (RenderTransformOrigin 0.5,0.5 - set in BuildCities/
    /// DrawHopMarker) on a point offset a constant number of screen pixels to the left or right
    /// of its anchor (Tag), so the gap doesn't grow while zooming in.
    /// </summary>
    private static void RepositionOffsetLabel(TextBlock label, double gapScreenPixels, double inverseScale, bool growLeft)
    {
        if (label.Tag is not (Point anchor, Size size))
            return;

        var halfWidth = size.Width / 2;
        var centerOffset = growLeft ? -(gapScreenPixels + halfWidth) : gapScreenPixels + halfWidth;
        Canvas.SetLeft(label, anchor.X + centerOffset * inverseScale - halfWidth);
    }

    /// <summary>
    /// Hides overlapping country/city labels instead of letting them render on top of each
    /// other. Cities take priority over countries, since they're the more specific reference
    /// near a hop. Deliberately does NOT consider hop markers/numbers here: a city label must
    /// stay visible even right next to a hop, since that's the one piece of on-map context
    /// (e.g. "Frankfurt") the user actually needs there - only text-on-text overlap is fixed.
    /// </summary>
    private void DeclutterReferenceLabels(bool referenceLabelsVisible)
    {
        if (!referenceLabelsVisible)
        {
            foreach (var label in _cityLabels)
                label.Visibility = Visibility.Collapsed;

            foreach (var label in _countryLabels)
                label.Visibility = Visibility.Collapsed;

            return;
        }

        var placedRects = new List<Rect>();

        // Only labels actually inside the viewport can matter: culling the (often large)
        // off-screen majority up front keeps this pass roughly O(visible^2) instead of O(all^2)
        // over all ~900 country + city labels on every zoom step - and an off-screen label is
        // hidden anyway, so this changes nothing visible.
        var viewport = new Rect(0, 0, BorderHost.ActualWidth, BorderHost.ActualHeight);

        foreach (var label in _cityLabels.Concat(_countryLabels))
        {
            var rect = GetLabelScreenRect(label);

            if (!viewport.IntersectsWith(rect))
            {
                label.Visibility = Visibility.Collapsed;
                continue;
            }

            if (placedRects.Any(r => r.IntersectsWith(rect)))
            {
                label.Visibility = Visibility.Collapsed;
                continue;
            }

            label.Visibility = Visibility.Visible;
            placedRects.Add(rect);
        }
    }

    /// <summary>
    /// Computes the actual on-screen rect of a label. Every label is center-pivoted
    /// (RenderTransformOrigin 0.5,0.5) with a constant on-screen size, so its rendered center is
    /// simply its Canvas.Left/Top center run through the current zoom/pan, and the rect spans
    /// its cached (zoom-independent) size around that center.
    /// </summary>
    private Rect GetLabelScreenRect(TextBlock label)
    {
        if (label.Tag is not (Point, Size size))
            return Rect.Empty;

        var centerX = (Canvas.GetLeft(label) + size.Width / 2) * _scale + _panX;
        var centerY = (Canvas.GetTop(label) + size.Height / 2) * _scale + _panY;

        return new Rect(centerX - size.Width / 2, centerY - size.Height / 2, size.Width, size.Height);
    }

    private void BorderHost_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Full redraw rather than just re-fitting the cached _hopPoints: the spread offsets baked
        // into those points (see SpreadOverlappingPoints, converted via _minScale) depend on the
        // now-changed viewport size, so re-deriving them keeps markers that share a city (hairpin
        // routes) correctly spaced at the new size. Cheap given how few hops a route has.
        RedrawHops();
    }

    private void BorderHost_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Zooming a strip too thin to show anything meaningful doesn't make sense - see
        // BorderHost_MouseLeftButtonDown for the matching "click to expand" behavior instead.
        if (!IsExpanded)
            return;

        var position = e.GetPosition(BorderHost);
        var oldScale = _scale;
        var factor = e.Delta > 0 ? 1.15 : 1 / 1.15;
        var newScale = Math.Clamp(oldScale * factor, _minScale, _maxScale);

        if (Math.Abs(newScale - oldScale) < 0.0001)
            return;

        // Keep the point under the cursor fixed while zooming.
        _panX = position.X - (position.X - _panX) * (newScale / oldScale);
        _panY = position.Y - (position.Y - _panY) * (newScale / oldScale);
        _scale = newScale;

        // Clamp so the cursor-anchored pan above can't leave blank canvas past the map's edge
        // inside the viewport (same reasoning as FitToView), e.g. when zooming near an edge.
        _panX = ClampPan(_panX, MapWidth * _scale, BorderHost.ActualWidth);
        _panY = ClampPan(_panY, MapHeight * _scale, BorderHost.ActualHeight);

        ApplyTransform();

        e.Handled = true;
    }

    private void BorderHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // While collapsed, BorderHost is just the thin strip behind the toggle button - too
        // small to usefully pan/zoom, so a click there expands the map instead (matching the
        // toggle button) rather than starting a drag that can't show anything meaningful.
        if (!IsExpanded)
        {
            IsExpanded = true;
            e.Handled = true;
            return;
        }

        _isPanning = true;
        _panStartMouse = e.GetPosition(BorderHost);
        _panStartX = _panX;
        _panStartY = _panY;

        BorderHost.CaptureMouse();
    }

    private void BorderHost_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning || !IsExpanded)
            return;

        var current = e.GetPosition(BorderHost);

        _panX = _panStartX + (current.X - _panStartMouse.X);
        _panY = _panStartY + (current.Y - _panStartMouse.Y);

        // Clamp so the map can't be dragged off-screen into blank canvas (same reasoning as
        // FitToView / the mouse-wheel zoom); Reset re-centers on the route if needed.
        _panX = ClampPan(_panX, MapWidth * _scale, BorderHost.ActualWidth);
        _panY = ClampPan(_panY, MapHeight * _scale, BorderHost.ActualHeight);

        ApplyTransform();
    }

    private void BorderHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isPanning = false;
        BorderHost.ReleaseMouseCapture();
    }

    // Mouse capture can be lost without a matching MouseLeftButtonUp (e.g. Alt+Tab while
    // dragging) - without this, _isPanning would stay true and the next mouse move over
    // BorderHost would keep panning the map even though the button is no longer held.
    private void BorderHost_LostMouseCapture(object sender, MouseEventArgs e)
    {
        _isPanning = false;
    }

    private void ButtonResetView_Click(object sender, RoutedEventArgs e)
    {
        FitToHops(_hopPoints);
    }

    #endregion

    #region Country data

    private static Brush FreezeBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();

        return brush;
    }

    private static StreamGeometry BuildCountriesGeometry()
    {
        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            foreach (var country in Countries.Value)
            {
                if (country.R == null)
                    continue;

                foreach (var ring in country.R)
                {
                    if (ring.Count < 2)
                        continue;

                    var points = ring.Select(coordinate => Project(coordinate[1], coordinate[0])).ToList();

                    context.BeginFigure(points[0], true, true);
                    context.PolyLineTo(points.Skip(1).ToList(), true, false);
                }
            }
        }

        geometry.Freeze();

        return geometry;
    }

    /// <summary>
    /// Picks a label anchor per country: the bounding-box center of its largest ring (by bounding-
    /// box area), so a mainland-plus-islands country gets labelled on the mainland. Bounding box
    /// rather than a true polygon centroid (e.g. shoelace) because the simplified, coordinate-
    /// rounded ring data can self-intersect, and a shoelace centroid is only guaranteed inside a
    /// simple polygon - for a self-intersecting ring it can land outside the landmass entirely.
    /// </summary>
    private static List<(string Name, Point Position)> BuildCountryLabels()
    {
        var labels = new List<(string, Point)>();

        foreach (var country in Countries.Value)
        {
            if (string.IsNullOrEmpty(country.N) || country.R == null)
                continue;

            var bestBoundsArea = -1.0;
            var bestCenter = default(Point);

            foreach (var ring in country.R)
            {
                if (ring.Count < 3)
                    continue;

                var points = ring.Select(coordinate => Project(coordinate[1], coordinate[0])).ToList();

                var minX = points.Min(p => p.X);
                var maxX = points.Max(p => p.X);
                var minY = points.Min(p => p.Y);
                var maxY = points.Max(p => p.Y);

                var boundsArea = (maxX - minX) * (maxY - minY);

                if (boundsArea <= bestBoundsArea)
                    continue;

                bestBoundsArea = boundsArea;
                bestCenter = new Point((minX + maxX) / 2, (minY + maxY) / 2);
            }

            if (bestBoundsArea >= 0)
                labels.Add((country.N, bestCenter));
        }

        return labels;
    }

    private static List<CountryData> LoadCountries()
    {
        const string resourceName = "NETworkManager.Resources.Maps.world-map.json";

        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName) ??
                            throw new FileNotFoundException($"Embedded resource \"{resourceName}\" not found.");
        using var reader = new StreamReader(stream);

        return System.Text.Json.JsonSerializer.Deserialize<List<CountryData>>(reader.ReadToEnd()) ?? [];
    }

    /// <summary>
    /// Minimal representation of a simplified country outline (name + outer ring coordinates as [lon, lat] pairs).
    /// </summary>
    private sealed class CountryData
    {
        [JsonPropertyName("n")]
        public string N { get; set; }

        [JsonPropertyName("r")]
        public List<List<List<double>>> R { get; set; }
    }

    private static List<CityData> LoadCities()
    {
        const string resourceName = "NETworkManager.Resources.Maps.world-cities.json";

        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName) ??
                            throw new FileNotFoundException($"Embedded resource \"{resourceName}\" not found.");
        using var reader = new StreamReader(stream);

        return System.Text.Json.JsonSerializer.Deserialize<List<CityData>>(reader.ReadToEnd()) ?? [];
    }

    /// <summary>
    /// Minimal representation of a major city (national capital or population above the script's threshold).
    /// </summary>
    private sealed class CityData
    {
        [JsonPropertyName("n")]
        public string N { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }
    }

    #endregion

    /// <summary>
    /// The scale-independent geometry (endpoints, control point, tangent angles) plus the visual
    /// elements for one hop-to-hop arrow. UpdateArrowGeometry uses this to re-trim the curve to
    /// the marker's outer edge and re-scale its stroke/arrowhead every time the zoom level
    /// changes, instead of freezing the trim at whatever scale was current when it was drawn.
    /// </summary>
    private sealed class ArrowVisual
    {
        public Point FromPoint { get; init; }
        public Point ToPoint { get; init; }
        public double Length { get; init; }
        public Point ControlPoint { get; init; }
        public double StartAngle { get; init; }
        public double EndAngle { get; init; }
        public PathFigure Figure { get; init; }
        public QuadraticBezierSegment Segment { get; init; }
        public Path Path { get; init; }
        public Path HitPath { get; init; }
        public Polygon ArrowHead { get; init; }
        public ScaleTransform HeadTransform { get; init; }
    }
}
