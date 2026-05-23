using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using log4net;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Cloudflare;
using NETworkManager.Utilities;
using SkiaSharp;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the Cloudflare speed test widget. Exposes live values for
/// the metric tiles plus per-sample history for the download/upload sparkline
/// charts (LiveCharts2).
/// </summary>
public class SpeedTestWidgetViewModel : ViewModelBase
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(SpeedTestWidgetViewModel));

    private CancellationTokenSource _cts;

    public bool IsRunning
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

    /// <summary>
    /// True until the user accepts the privacy disclaimer for the current
    /// VM lifetime. Not persisted — the disclaimer is shown on every app
    /// start by design.
    /// </summary>
    public bool ShowDisclaimer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public SpeedTestResult Result
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasResult));
        }
    }

    public bool HasResult => Result != null;

    public string StatusMessage
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

    public double? CurrentDownloadMbps
    {
        get;
        private set
        {
            if (Nullable.Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public double? CurrentUploadMbps
    {
        get;
        private set
        {
            if (Nullable.Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public double? CurrentLatencyMs
    {
        get;
        private set
        {
            if (Nullable.Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public double? CurrentJitterMs
    {
        get;
        private set
        {
            if (Nullable.Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Download samples (Mbps), one per completed HTTP request.</summary>
    private ObservableCollection<double> DownloadSamples { get; } = [];

    /// <summary>Upload samples (Mbps), one per completed HTTP request.</summary>
    private ObservableCollection<double> UploadSamples { get; } = [];

    /// <summary>LiveCharts2 series for the download sparkline.</summary>
    public ISeries[] DownloadSeries { get; }

    /// <summary>LiveCharts2 series for the upload sparkline.</summary>
    public ISeries[] UploadSeries { get; }

    /// <summary>Hidden X-axes for the download sparkline (anchored at 0 so the first sample sits at the left edge).</summary>
    public ICartesianAxis[] DownloadXAxes { get; } = [new Axis { IsVisible = false, MinLimit = 0, MinStep = 1 }];

    /// <summary>Hidden Y-axes for the download sparkline.</summary>
    public ICartesianAxis[] DownloadYAxes { get; } = [new Axis { IsVisible = false, MinLimit = 0 }];

    /// <summary>Hidden X-axes for the upload sparkline.</summary>
    public ICartesianAxis[] UploadXAxes { get; } = [new Axis { IsVisible = false, MinLimit = 0, MinStep = 1 }];

    /// <summary>Hidden Y-axes for the upload sparkline.</summary>
    public ICartesianAxis[] UploadYAxes { get; } = [new Axis { IsVisible = false, MinLimit = 0 }];

    private ICommand _runCommand;
    public ICommand RunCommand => _runCommand ??= new RelayCommand(_ => RunAction());

    private ICommand _acceptDisclaimerCommand;
    public ICommand AcceptDisclaimerCommand => _acceptDisclaimerCommand ??= new RelayCommand(_ => AcceptDisclaimerAction());

    public SpeedTestWidgetViewModel()
    {
        var downloadColor = SKColor.Parse("#1ba1e2");
        DownloadSeries =
        [
            new LineSeries<double>
            {
                Values = DownloadSamples,
                GeometrySize = 3,
                LineSmoothness = 0.3,
                DataPadding = new LvcPoint(0, 0),
                Stroke = new SolidColorPaint(downloadColor) { StrokeThickness = 1.5f },
                Fill = new SolidColorPaint(downloadColor.WithAlpha(0x33)),
                GeometryStroke = new SolidColorPaint(downloadColor) { StrokeThickness = 1.5f },
                GeometryFill = new SolidColorPaint(downloadColor),
                YToolTipLabelFormatter = point => $"{point.Model:F1} Mbps"
            }
        ];

        var uploadColor = SKColor.Parse("#7fba00");
        UploadSeries =
        [
            new LineSeries<double>
            {
                Values = UploadSamples,
                GeometrySize = 3,
                LineSmoothness = 0.3,
                DataPadding = new LvcPoint(0, 0),
                Stroke = new SolidColorPaint(uploadColor) { StrokeThickness = 1.5f },
                Fill = new SolidColorPaint(uploadColor.WithAlpha(0x33)),
                GeometryStroke = new SolidColorPaint(uploadColor) { StrokeThickness = 1.5f },
                GeometryFill = new SolidColorPaint(uploadColor),
                YToolTipLabelFormatter = point => $"{point.Model:F1} Mbps"
            }
        ];
    }

    private void RunAction()
    {
        if (IsRunning)
        {
            _cts?.Cancel();
            return;
        }

        if (ShowDisclaimer)
            return;

        RunAsync().ConfigureAwait(false);
    }

    private void AcceptDisclaimerAction()
    {
        ShowDisclaimer = false;
        RunAsync().ConfigureAwait(false);
    }

    private async Task RunAsync()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        Result = null;
        CurrentDownloadMbps = null;
        CurrentUploadMbps = null;
        CurrentLatencyMs = null;
        CurrentJitterMs = null;
        DownloadSamples.Clear();
        UploadSamples.Clear();
        StatusMessage = Strings.FetchingMetadataDots;

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        using var service = new SpeedTestService();
        var progress = new Progress<SpeedTestProgress>(p =>
        {
            StatusMessage = p.Phase switch
            {
                SpeedTestPhase.FetchingMetadata => Strings.FetchingMetadataDots,
                SpeedTestPhase.MeasuringLatency => Strings.MeasuringLatencyDots,
                SpeedTestPhase.MeasuringDownload => Strings.MeasuringDownloadSpeedDots,
                SpeedTestPhase.MeasuringUpload => Strings.MeasuringUploadSpeedDots,
                _ => string.Empty
            };

            if (p.DownloadMbps.HasValue)
                CurrentDownloadMbps = p.DownloadMbps;
            if (p.UploadMbps.HasValue)
                CurrentUploadMbps = p.UploadMbps;
            if (p.LatencyMs.HasValue)
                CurrentLatencyMs = p.LatencyMs;
            if (p.JitterMs.HasValue)
                CurrentJitterMs = p.JitterMs;

            if (p.NewDownloadSampleMbps.HasValue)
            {
                if (DownloadSamples.Count == 0)
                    DownloadSamples.Add(p.NewDownloadSampleMbps.Value);
                DownloadSamples.Add(p.NewDownloadSampleMbps.Value);
            }
            if (p.NewUploadSampleMbps.HasValue)
            {
                if (UploadSamples.Count == 0)
                    UploadSamples.Add(p.NewUploadSampleMbps.Value);
                UploadSamples.Add(p.NewUploadSampleMbps.Value);
            }

            if (p.Meta != null)
            {
                // Populate Result with metadata-only fields so the UI can
                // show ISP / location / server while measurements are still running.
                Result = new SpeedTestResult
                {
                    Isp = p.Meta.AsOrganization,
                    ClientCity = p.Meta.City,
                    ClientCountry = p.Meta.Country,
                    ServerCity = p.Meta.Colo?.City,
                    ServerCountry = p.Meta.Colo?.Cca2,
                    ServerIata = p.Meta.Colo?.Iata
                };
            }
        });

        try
        {
            var result = await service.RunAsync(progress, _cts.Token);
            Result = result;
            CurrentDownloadMbps = result.DownloadMbps;
            CurrentUploadMbps = result.UploadMbps;
            CurrentLatencyMs = result.LatencyMs;
            CurrentJitterMs = result.JitterMs;
        }
        catch (OperationCanceledException)
        {
            // Partial results remain visible; everything resets at the start of the next run.
        }
        catch (Exception ex)
        {
            Log.Error("Speed test failed.", ex);
            Result = new SpeedTestResult
            {
                HasError = true,
                ErrorMessage = ex.Message
            };
        }
        finally
        {
            IsRunning = false;
        }
    }
}
