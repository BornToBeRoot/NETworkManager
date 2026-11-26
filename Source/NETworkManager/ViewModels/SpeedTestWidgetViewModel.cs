using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using log4net;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Cloudflare;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the Cloudflare speed test widget. Exposes live values for
/// the metric tiles plus per-sample history for the download/upload sparkline
/// charts (LiveCharts2).
/// </summary>
public class SpeedTestWidgetViewModel : ViewModelBase
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(SpeedTestWidgetViewModel));

    private readonly SpeedTestService _service = new();
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

    /// <summary>
    /// Time of the last completed test, <c>null</c> before the first one.
    /// </summary>
    public DateTime? LastTestTime
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

    /// <summary>
    /// Phase of the running test (e.g. "Measuring download speed...").
    /// </summary>
    public string PhaseMessage
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

    /// <summary>
    /// Message after the test, e.g. canceled by the user or an error.
    /// </summary>
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

    public StatusLevel StatusMessageLevel
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

    public bool IsStatusMessageDisplayed
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

    /// <summary>
    /// X axis of the download sparkline (sample index, hidden). From 0 to at least 1, so the first sample is at the left
    /// edge instead of in the middle of the chart.
    /// </summary>
    private readonly Axis _downloadXAxis = new() { IsVisible = false, MinLimit = 0, MaxLimit = 1, MinStep = 1 };

    /// <summary>X axis of the upload sparkline, see <see cref="_downloadXAxis" />.</summary>
    private readonly Axis _uploadXAxis = new() { IsVisible = false, MinLimit = 0, MaxLimit = 1, MinStep = 1 };

    /// <summary>Hidden X-axes for the download sparkline.</summary>
    public ICartesianAxis[] DownloadXAxes { get; }

    /// <summary>Hidden Y-axes for the download sparkline.</summary>
    public ICartesianAxis[] DownloadYAxes { get; } = [new Axis { IsVisible = false, MinLimit = 0 }];

    /// <summary>Hidden X-axes for the upload sparkline.</summary>
    public ICartesianAxis[] UploadXAxes { get; }

    /// <summary>Hidden Y-axes for the upload sparkline.</summary>
    public ICartesianAxis[] UploadYAxes { get; } = [new Axis { IsVisible = false, MinLimit = 0 }];

    public ICommand RunCommand => field ??= new RelayCommand(_ => RunAction());

    public ICommand AcceptDisclaimerCommand => field ??= new RelayCommand(_ => AcceptDisclaimerAction());

    public SpeedTestWidgetViewModel()
    {
        DownloadXAxes = [_downloadXAxis];
        UploadXAxes = [_uploadXAxis];

        var downloadColor = ChartColors.Primary;
        DownloadSeries =
        [
            new LineSeries<double>
            {
                Name = Strings.Download,
                Values = DownloadSamples,
                GeometrySize = 1.5f,
                LineSmoothness = 0.3,
                DataPadding = new LvcPoint(0, 0),
                Stroke = ChartColors.LineStroke(downloadColor),
                Fill = ChartColors.AreaFill(downloadColor)
            }
        ];

        var uploadColor = ChartColors.Secondary;
        UploadSeries =
        [
            new LineSeries<double>
            {
                Name = Strings.Upload,
                Values = UploadSamples,
                GeometrySize = 1.5f,
                LineSmoothness = 0.3,
                DataPadding = new LvcPoint(0, 0),
                Stroke = ChartColors.LineStroke(uploadColor),
                Fill = ChartColors.AreaFill(uploadColor)
            }
        ];

        // Colors of the sparklines for the new theme (the paints don't follow the theme)
        AppearanceManager.ThemeResourcesChanged += (_, _) => ApplyChartColors();
    }

    /// <summary>
    /// Sets the colors of the download / upload sparklines for the current theme.
    /// </summary>
    private void ApplyChartColors()
    {
        if (DownloadSeries[0] is LineSeries<double> download)
        {
            download.Stroke = ChartColors.LineStroke(ChartColors.Primary);
            download.Fill = ChartColors.AreaFill(ChartColors.Primary);
        }

        if (UploadSeries[0] is LineSeries<double> upload)
        {
            upload.Stroke = ChartColors.LineStroke(ChartColors.Secondary);
            upload.Fill = ChartColors.AreaFill(ChartColors.Secondary);
        }
    }

    private void RunAction()
    {
        if (IsRunning)
        {
            Cancel();
            return;
        }

        if (ShowDisclaimer)
            return;

        _ = RunAsync();
    }

    /// <summary>
    /// Cancels a running speed test, the values measured until then stay visible.
    /// </summary>
    public void Cancel()
    {
        if (IsRunning)
            _cts?.Cancel();
    }

    /// <summary>
    /// Shows the sample result of <see cref="ScreenshotData" /> instead of running a real speed test.
    /// </summary>
    private void SetScreenshotData()
    {
        var result = ScreenshotData.CreateSpeedTestResult();

        foreach (var value in ScreenshotData.CreateSpeedTestDownloadSamples())
            AddSample(DownloadSamples, _downloadXAxis, value);

        foreach (var value in ScreenshotData.CreateSpeedTestUploadSamples())
            AddSample(UploadSamples, _uploadXAxis, value);

        Result = result;
        LastTestTime = DateTime.Now;
        CurrentDownloadMbps = result.DownloadMbps;
        CurrentUploadMbps = result.UploadMbps;
        CurrentLatencyMs = result.LatencyMs;
        CurrentJitterMs = result.JitterMs;
    }

    private void ShowStatusMessage(string message, StatusLevel level)
    {
        StatusMessage = message;
        StatusMessageLevel = level;
        IsStatusMessageDisplayed = true;
    }

    /// <summary>
    /// Localized message for an error of the speed test (the exception text is logged).
    /// </summary>
    private static string GetErrorMessage(Exception ex)
    {
        var message = ex switch
        {
            TimeoutException => string.Format(Strings.XXXDidNotRespondWithinXXXSecondsMessage, SpeedTestService.Host,
                SpeedTestService.RequestTimeout.TotalSeconds),
            HttpRequestException { StatusCode: not null } httpEx => string.Format(Strings.CouldNotConnectToXXXReasonXXX,
                SpeedTestService.Host, $"{Strings.HTTPStatusCode} {(int)httpEx.StatusCode}"),
            _ => string.Format(Strings.CouldNotConnectToXXXMessage, SpeedTestService.Host)
        };

        return message + Environment.NewLine + Strings.CheckNetworkConnectionTryAgainMessage;
    }

    /// <summary>
    /// Adds a sample (one completed request) to a sparkline and extends its X axis to the samples.
    /// </summary>
    private static void AddSample(ObservableCollection<double> samples, Axis xAxis, double value)
    {
        samples.Add(value);
        xAxis.MaxLimit = Math.Max(1, samples.Count - 1);
    }

    private void ClearSamples()
    {
        DownloadSamples.Clear();
        UploadSamples.Clear();
        _downloadXAxis.MaxLimit = 1;
        _uploadXAxis.MaxLimit = 1;
    }

    private void AcceptDisclaimerAction()
    {
        ShowDisclaimer = false;
        _ = RunAsync();
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
        ClearSamples();
        IsStatusMessageDisplayed = false;
        PhaseMessage = Strings.FetchingMetadataDots;

        if (ScreenshotData.IsEnabled)
        {
            SetScreenshotData();
            IsRunning = false;
            return;
        }

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var progress = new Progress<SpeedTestProgress>(p =>
        {
            PhaseMessage = p.Phase switch
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
                AddSample(DownloadSamples, _downloadXAxis, p.NewDownloadSampleMbps.Value);

            if (p.NewUploadSampleMbps.HasValue)
                AddSample(UploadSamples, _uploadXAxis, p.NewUploadSampleMbps.Value);

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
            var result = await _service.RunAsync(progress, _cts.Token);
            Result = result;
            LastTestTime = DateTime.Now;
            CurrentDownloadMbps = result.DownloadMbps;
            CurrentUploadMbps = result.UploadMbps;
            CurrentLatencyMs = result.LatencyMs;
            CurrentJitterMs = result.JitterMs;

            if (result.FailedRequests > 0)
                ShowStatusMessage(string.Format(Strings.XRequestsFailedAndWereSkippedMessage, result.FailedRequests),
                    StatusLevel.Warning);
        }
        catch (OperationCanceledException)
        {
            // Partial results remain visible; everything resets at the start of the next run.
            ShowStatusMessage(Strings.CanceledByUserMessage, StatusLevel.None);
        }
        catch (Exception ex)
        {
            // Partial results remain visible, like after a cancel
            Log.Error("Speed test failed.", ex);
            ShowStatusMessage(GetErrorMessage(ex), StatusLevel.Error);
        }
        finally
        {
            IsRunning = false;
        }
    }
}
