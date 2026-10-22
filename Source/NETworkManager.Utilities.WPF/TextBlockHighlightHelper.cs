using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NETworkManager.Utilities.WPF;

/// <summary>
/// Attached-property helper to show a text in a <see cref="TextBlock"/> with the first occurrence of a search text
/// highlighted (case-insensitive), e.g. the typed text in a suggestion list.
/// Use <see cref="TextProperty"/> instead of <see cref="TextBlock.Text"/>.
/// </summary>
public static class TextBlockHighlightHelper
{
    /// <summary>
    /// Resource key of the brush of the highlighted text.
    /// </summary>
    private const string HighlightBrushKey = "MahApps.Brushes.Accent";

    /// <summary>
    /// Text to show.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
        "Text", typeof(string), typeof(TextBlockHighlightHelper), new PropertyMetadata(null, OnChanged));

    /// <summary>
    /// Text to highlight. Nothing is highlighted if it is empty or not found.
    /// </summary>
    public static readonly DependencyProperty HighlightProperty = DependencyProperty.RegisterAttached(
        "Highlight", typeof(string), typeof(TextBlockHighlightHelper), new PropertyMetadata(null, OnChanged));

    public static string GetText(DependencyObject element)
    {
        return (string)element.GetValue(TextProperty);
    }

    public static void SetText(DependencyObject element, string value)
    {
        element.SetValue(TextProperty, value);
    }

    public static string GetHighlight(DependencyObject element)
    {
        return (string)element.GetValue(HighlightProperty);
    }

    public static void SetHighlight(DependencyObject element, string value)
    {
        element.SetValue(HighlightProperty, value);
    }

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock textBlock)
            return;

        var text = GetText(textBlock) ?? string.Empty;
        var highlight = GetHighlight(textBlock);

        textBlock.Inlines.Clear();

        var index = string.IsNullOrEmpty(highlight)
            ? -1
            : text.IndexOf(highlight, StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            textBlock.Inlines.Add(new Run(text));
            return;
        }

        var match = new Run(text.Substring(index, highlight.Length)) { FontWeight = FontWeights.SemiBold };
        match.SetResourceReference(TextElement.ForegroundProperty, HighlightBrushKey);

        textBlock.Inlines.Add(new Run(text[..index]));
        textBlock.Inlines.Add(match);
        textBlock.Inlines.Add(new Run(text[(index + highlight.Length)..]));
    }
}
