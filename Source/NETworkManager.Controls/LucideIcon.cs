using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace NETworkManager.Controls;

/// <summary>
///     Lucide icon (https://lucide.dev): the lines of the icon drawn with a pen (round caps and joins) in the
///     <see cref="Foreground" />, scaled from 24 x 24 units to the size of the element (default 24 x 24). Icons are
///     created by Scripts/Create-LucideIcons.ps1 (<see cref="LucideIconKind" />).
///
///     The whole 24 x 24 box is part of the drawing (transparent), so a <see cref="VisualBrush" /> with the icon (e.g. as
///     opacity mask of a rectangle, see <see cref="LucideExtension" />) shows every icon in the same size and position.
///     Icons that only cover a small part of the box (plus, X, chevrons, ...) use a smaller box (see SmallIcons).
/// </summary>
public class LucideIcon : FrameworkElement
{
    /// <summary>
    ///     Size of the icons in units (view box).
    /// </summary>
    private const double Size = 24;

    /// <summary>
    ///     Default thickness of the lines in units (Lucide: 2). Thinner lines look calmer, especially at 16 px.
    /// </summary>
    public const double DefaultStrokeThickness = 1.75;

    /// <summary>
    ///     Box of the icons that only cover a small part of the 24 units (plus, minus, X, chevrons, arrows: 12 - 14
    ///     units, most icons about 20): drawn in a smaller box (1.25 times larger), so they look as large as the other
    ///     icons and as before with the MahApps icons (stretched to their outline).
    /// </summary>
    private const double SmallIconBox = 19.2;

    private static readonly HashSet<LucideIconKind> SmallIcons =
    [
        LucideIconKind.Plus, LucideIconKind.Minus, LucideIconKind.X, LucideIconKind.ChevronDown,
        LucideIconKind.ChevronUp, LucideIconKind.ChevronLeft, LucideIconKind.ChevronRight,
        LucideIconKind.ChevronsRight, LucideIconKind.ArrowDown, LucideIconKind.ArrowLeft, LucideIconKind.ArrowUp
    ];

    private static readonly ConcurrentDictionary<LucideIconKind, (Geometry Stroke, Geometry Fill)> Geometries = new();

    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind),
        typeof(LucideIconKind), typeof(LucideIcon),
        new FrameworkPropertyMetadata(LucideIconKind.None, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner(typeof(LucideIcon),
            new FrameworkPropertyMetadata(Brushes.Black,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness), typeof(double), typeof(LucideIcon),
        new FrameworkPropertyMetadata(DefaultStrokeThickness, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    ///     Icon to show.
    /// </summary>
    public LucideIconKind Kind
    {
        get => (LucideIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>
    ///     Color of the icon (inherited like the foreground of text).
    /// </summary>
    public Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>
    ///     Thickness of the lines in units of the 24 x 24 icon (scaled with the icon).
    /// </summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    ///     24 x 24 if the size is not set or not limited (e.g. in a <see cref="VisualBrush" />), otherwise the available
    ///     size (square).
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = Math.Min(availableSize.Width, availableSize.Height);

        if (double.IsInfinity(size))
            size = double.IsInfinity(availableSize.Width) ? availableSize.Height : availableSize.Width;

        if (double.IsInfinity(size))
            size = Size;

        return new Size(size, size);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var size = Math.Min(ActualWidth, ActualHeight);

        if (size <= 0)
            return;

        // Box in units (centered in the 24 units), see SmallIcons
        var box = SmallIcons.Contains(Kind) ? SmallIconBox : Size;
        var inset = (Size - box) / 2;
        var scale = size / box;

        drawingContext.PushTransform(new MatrixTransform(scale, 0, 0, scale,
            (ActualWidth - size) / 2 - inset * scale, (ActualHeight - size) / 2 - inset * scale));

        // The whole box, so the bounds are the same for all icons (VisualBrush)
        drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(inset, inset, box, box));

        var (stroke, fill) = GetGeometries(Kind);

        if (fill != null)
            drawingContext.DrawGeometry(Foreground, null, fill);

        if (stroke != null)
        {
            var pen = new Pen(Foreground, StrokeThickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                DashCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };

            // Freezing the pen freezes its brush too: only if the brush is frozen already (a shared brush must not be
            // frozen as a side effect, an animated / bound brush can't be frozen)
            if (Foreground is not { IsFrozen: false })
                pen.Freeze();

            drawingContext.DrawGeometry(null, pen, stroke);
        }

        drawingContext.Pop();
    }

    /// <summary>
    ///     Geometries of the icon (parsed once, frozen).
    /// </summary>
    private static (Geometry Stroke, Geometry Fill) GetGeometries(LucideIconKind kind)
    {
        if (kind == LucideIconKind.None || !LucideIconData.Icons.TryGetValue(kind, out var data))
            return (null, null);

        return Geometries.GetOrAdd(kind, _ => (Parse(data.Stroke), Parse(data.Fill)));
    }

    private static Geometry Parse(string data)
    {
        if (data == null)
            return null;

        var geometry = Geometry.Parse(data);
        geometry.Freeze();

        return geometry;
    }
}

/// <summary>
///     Creates a <see cref="LucideIcon" /> in XAML, e.g. as visual of a <see cref="VisualBrush" />:
///     <c>Visual="{icons:Lucide Kind=Router}"</c>.
/// </summary>
[MarkupExtensionReturnType(typeof(LucideIcon))]
public class LucideExtension : MarkupExtension
{
    public LucideExtension()
    {
    }

    public LucideExtension(LucideIconKind kind)
    {
        Kind = kind;
    }

    [ConstructorArgument("kind")] public LucideIconKind Kind { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new LucideIcon { Kind = Kind };
    }
}
