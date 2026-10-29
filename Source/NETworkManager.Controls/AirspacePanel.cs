// Based on AirspaceFixer (https://github.com/chris84948/AirspaceFixer) and its fork AirspaceFixerDotNet
// (https://github.com/ellipticbit/AirspaceFixerDotNet), adapted for NETworkManager.
//
// MIT License
//
// Copyright (c) 2017 chris84948
// Copyright (c) 2024 EllipticBit, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NETworkManager.Controls;

/// <summary>
///     Hosts content with embedded windows (e.g. Remote Desktop, PuTTY, WebView2), which are always drawn on top of
///     WPF elements (airspace issue). While <see cref="FixAirspace" /> is <see langword="true" />, the content is
///     replaced by a screenshot of it, so that WPF elements like dialogs can be shown above it.
///     The template is defined in <c>Resources/Styles/AirspacePanelStyles.xaml</c> of the application.
/// </summary>
[TemplatePart(Name = PartAirspaceContent, Type = typeof(ContentControl))]
[TemplatePart(Name = PartAirspaceScreenshot, Type = typeof(Image))]
public class AirspacePanel : ContentControl
{
    private const string PartAirspaceContent = "PART_AirspaceContent";
    private const string PartAirspaceScreenshot = "PART_AirspaceScreenshot";

    /// <summary>
    ///     Identifies the <see cref="FixAirspace" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty FixAirspaceProperty = DependencyProperty.Register(nameof(FixAirspace),
        typeof(bool), typeof(AirspacePanel), new FrameworkPropertyMetadata(false, OnFixAirspaceChanged));

    private ContentControl _airspaceContent;
    private Image _airspaceScreenshot;

    /// <summary>
    ///     Replace the content with a screenshot of it (e.g. while a dialog is shown above the content).
    /// </summary>
    public bool FixAirspace
    {
        get => (bool)GetValue(FixAirspaceProperty);
        set => SetValue(FixAirspaceProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _airspaceContent = GetTemplateChild(PartAirspaceContent) as ContentControl;
        _airspaceScreenshot = GetTemplateChild(PartAirspaceScreenshot) as Image;
    }

    private static void OnFixAirspaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AirspacePanel panel)
            panel.UpdateAirspace((bool)e.NewValue);
    }

    private void UpdateAirspace(bool fix)
    {
        // Template not applied (yet)
        if (_airspaceContent == null || _airspaceScreenshot == null)
            return;

        if (fix)
        {
            // Content not visible or not shown in a window - nothing to capture, keep the content
            var screenshot = CreateScreenshotFromContent();

            if (screenshot == null)
                return;

            // Show the screenshot in the size of the content when it was taken (top left aligned in the template), so
            // it is not stretched / scaled if the window is resized while it is shown
            _airspaceScreenshot.Width = _airspaceContent.ActualWidth;
            _airspaceScreenshot.Height = _airspaceContent.ActualHeight;
            _airspaceScreenshot.Source = screenshot;
            _airspaceContent.Visibility = Visibility.Hidden;
            _airspaceScreenshot.Visibility = Visibility.Visible;
        }
        else
        {
            // Always show the content again, even if the size changed in the meantime
            _airspaceContent.Visibility = Visibility.Visible;
            _airspaceScreenshot.Visibility = Visibility.Hidden;
            _airspaceScreenshot.Source = null;
            _airspaceScreenshot.ClearValue(WidthProperty);
            _airspaceScreenshot.ClearValue(HeightProperty);
        }
    }

    private BitmapSource CreateScreenshotFromContent()
    {
        var presentationSource = PresentationSource.FromVisual(_airspaceContent);

        if (presentationSource?.CompositionTarget == null)
            return null;

        // The application is per monitor DPI aware (PerMonitorV2, see app.manifest): PointToScreen returns physical
        // pixels, but the size is in device independent pixels and has to be scaled with the DPI of the monitor.
        var transformToDevice = presentationSource.CompositionTarget.TransformToDevice;

        var upperLeftPoint = _airspaceContent.PointToScreen(new Point(0, 0));
        var width = (int)Math.Round(_airspaceContent.ActualWidth * transformToDevice.M11);
        var height = (int)Math.Round(_airspaceContent.ActualHeight * transformToDevice.M22);

        if (width <= 0 || height <= 0)
            return null;

        using var bitmap = new System.Drawing.Bitmap(width, height);

        using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen((int)Math.Round(upperLeftPoint.X), (int)Math.Round(upperLeftPoint.Y), 0, 0,
                new System.Drawing.Size(width, height));
        }

        return GetBitmapSource(bitmap);
    }

    private static BitmapSource GetBitmapSource(System.Drawing.Bitmap bitmap)
    {
        using var memoryStream = new MemoryStream();

        bitmap.Save(memoryStream, ImageFormat.Png);
        memoryStream.Position = 0;

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memoryStream;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        return bitmapImage;
    }
}
