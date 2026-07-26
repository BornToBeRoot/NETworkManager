using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using NetworkInterface = NETworkManager.Models.Network.NetworkInterface;
using System.Net.Sockets;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the network connection widget.
/// </summary>
public class NetworkConnectionWidgetViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// The logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(NetworkConnectionWidgetViewModel));

    /// <summary>
    /// Shared, timeout-bound HTTP client used to detect the public IPv4/IPv6 address.
    /// </summary>
    private static readonly HttpClient PublicIPHttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    #region Computer

    /// <summary>
    /// Gets the computer IPv4 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem ComputerIPv4 { get; } = new();

    /// <summary>
    /// Gets the computer IPv6 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem ComputerIPv6 { get; } = new();

    /// <summary>
    /// Gets the computer DNS check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem ComputerDNS { get; } = new();

    #endregion

    #region Router

    /// <summary>
    /// Gets the router IPv4 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem RouterIPv4 { get; } = new();

    /// <summary>
    /// Gets the router IPv6 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem RouterIPv6 { get; } = new();

    /// <summary>
    /// Gets the router DNS check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem RouterDNS { get; } = new();

    #endregion

    #region Internet

    /// <summary>
    /// Gets the internet IPv4 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem InternetIPv4 { get; } = new();

    /// <summary>
    /// Gets the internet IPv6 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem InternetIPv6 { get; } = new();

    /// <summary>
    /// Gets the internet DNS check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem InternetDNS { get; } = new();

    #endregion

    /// <summary>
    /// Gets a value indicating whether checking the public IP address is enabled.
    /// </summary>
    public bool CheckPublicIPAddressEnabled => SettingsManager.Current.Dashboard_CheckPublicIPAddress;

    /// <summary>
    /// Gets or sets a value indicating whether a check is currently running.
    /// </summary>
    public bool IsChecking
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

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to check connections.
    /// </summary>
    public ICommand CheckCommand => new RelayCommand(_ => Check());

    #endregion

    #region Methods

    /// <summary>
    /// Checks the network connections.
    /// </summary>
    public void Check()
    {
        _ = CheckAsync();
    }

    /// <summary>
    /// The cancellation token source.
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// The check task.
    /// </summary>
    private Task _checkTask = Task.CompletedTask;

    /// <summary>
    /// Monotonically increasing id of the current check run. None of the underlying detection/DNS
    /// calls support real mid-flight cancellation (see <see cref="RunTask"/>), so a superseded run
    /// can still be executing in the background after a newer run has started. Every write to a
    /// <see cref="ConnectionCheckItem"/> is guarded by comparing against this field, so a superseded
    /// run's results are silently discarded instead of racing with the current run's writes.
    /// </summary>
    private long _generation;

    /// <summary>
    /// Checks the network connections asynchronously.
    /// </summary>
    private async Task CheckAsync()
    {
        Log.Info("Checking network connection...");

        // Bump the generation immediately so a still-running previous check stops writing
        // its results as soon as possible, even before it notices the cancellation below.
        var generation = Interlocked.Increment(ref _generation);

        // Cancel previous checks if running
        if (!_checkTask.IsCompleted)
        {
            Log.Info("Cancelling previous checks...");
            await _cancellationTokenSource.CancelAsync();

            try
            {
                await _checkTask;
            }
            catch (OperationCanceledException)
            {
                Log.Info("Task was cancelled from previous checks.");
            }
            finally
            {
                _cancellationTokenSource.Dispose();
            }
        }

        _cancellationTokenSource = new CancellationTokenSource();
        var wasCanceled = false;

        IsChecking = true;

        try
        {
            _checkTask = RunTask(_cancellationTokenSource.Token, generation);
            await _checkTask;
        }
        catch (OperationCanceledException)
        {
            wasCanceled = true;
            Log.Info("Task was cancelled from current checks.");
        }
        finally
        {
            IsChecking = false;
            _cancellationTokenSource.Dispose();

            if (!wasCanceled)
                Log.Info("Network connection check completed.");
        }
    }

    /// <summary>
    /// Runs the check tasks.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <param name="generation">The id of this check run, see <see cref="_generation"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunTask(CancellationToken ct, long generation)
    {
        ResetAllItems();

        // Detect the local IPv4/IPv6 address once and share it between the Computer and Router
        // checks below, instead of each of them redetecting it independently (both used to
        // trigger their own, potentially PowerShell-backed, network interface lookup).
        var localIPv4Task = DetectLocalIPv4Async();
        var localIPv6Task = DetectLocalIPv6Async();

        await Task.WhenAll(localIPv4Task, localIPv6Task);

        if (ct.IsCancellationRequested)
            ct.ThrowIfCancellationRequested();

        var localIPv4 = localIPv4Task.Result;
        var localIPv6 = localIPv6Task.Result;

        SetAddressResult(ComputerIPv4, localIPv4, generation);
        SetAddressResult(ComputerIPv6, localIPv6, generation);

        await Task.WhenAll(
            CheckConnectionComputerDnsAsync(ct, generation),
            CheckConnectionRouterAsync(localIPv4, localIPv6, ct, generation),
            CheckConnectionInternetAsync(ct, generation)
        );
    }

    /// <summary>
    /// Resets all connection check items to their initial "checking" state. The Internet items are
    /// only reset when the public IP address check is enabled, so they stay untouched (not stuck
    /// "checking") when it is disabled.
    /// </summary>
    private void ResetAllItems()
    {
        ComputerIPv4.Reset();
        ComputerIPv6.Reset();
        ComputerDNS.Reset();

        RouterIPv4.Reset();
        RouterIPv6.Reset();
        RouterDNS.Reset();

        if (!CheckPublicIPAddressEnabled)
            return;

        InternetIPv4.Reset();
        InternetIPv6.Reset();
        InternetDNS.Reset();
    }

    /// <summary>
    /// Detects the local IPv4 address, based on routing to the configured public IPv4 address, with
    /// a fallback to detection based on the network interfaces.
    /// </summary>
    private static async Task<IPAddress> DetectLocalIPv4Async()
    {
        Log.Debug($"{nameof(DetectLocalIPv4Async)} - Detecting local IPv4 address...");

        IPAddress remoteIPv4 = null;

        try
        {
            remoteIPv4 = IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPv4Address);
        }
        catch (Exception ex)
        {
            Log.Warn($"{nameof(DetectLocalIPv4Async)} - Invalid Dashboard_PublicIPv4Address setting, skipping routing based detection.", ex);
        }

        var detected = remoteIPv4 != null
            ? await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(remoteIPv4)
            : null;

        if (detected == null)
        {
            Log.Debug($"{nameof(DetectLocalIPv4Async)} - Local IPv4 address detection via routing failed, trying network interfaces...");
            detected = await NetworkInterface.DetectLocalIPAddressFromNetworkInterfaceAsync(AddressFamily.InterNetwork);
        }

        Log.Debug(detected != null
            ? $"{nameof(DetectLocalIPv4Async)} - Local IPv4 address detected: " + detected
            : $"{nameof(DetectLocalIPv4Async)} - Local IPv4 address not detected.");

        return detected;
    }

    /// <summary>
    /// Detects the local IPv6 address, based on routing to the configured public IPv6 address, with
    /// a fallback to detection based on the network interfaces.
    /// </summary>
    private static async Task<IPAddress> DetectLocalIPv6Async()
    {
        Log.Debug($"{nameof(DetectLocalIPv6Async)} - Detecting local IPv6 address...");

        IPAddress remoteIPv6 = null;

        try
        {
            remoteIPv6 = IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPv6Address);
        }
        catch (Exception ex)
        {
            Log.Warn($"{nameof(DetectLocalIPv6Async)} - Invalid Dashboard_PublicIPv6Address setting, skipping routing based detection.", ex);
        }

        var detected = remoteIPv6 != null
            ? await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(remoteIPv6)
            : null;

        if (detected == null)
        {
            Log.Debug($"{nameof(DetectLocalIPv6Async)} - Local IPv6 address detection via routing failed, trying network interfaces...");
            detected = await NetworkInterface.DetectLocalIPAddressFromNetworkInterfaceAsync(AddressFamily.InterNetworkV6);
        }

        Log.Debug(detected != null
            ? $"{nameof(DetectLocalIPv6Async)} - Local IPv6 address detected: " + detected
            : $"{nameof(DetectLocalIPv6Async)} - Local IPv6 address not detected.");

        return detected;
    }

    /// <summary>
    /// Applies a detected address to a <see cref="ConnectionCheckItem"/>, unless a newer check has
    /// started in the meantime.
    /// </summary>
    private void SetAddressResult(ConnectionCheckItem item, IPAddress detected, long generation)
    {
        if (generation != _generation)
            return;

        item.Complete(detected?.ToString() ?? "-/-", detected != null ? ConnectionState.OK : ConnectionState.Critical);
    }

    /// <summary>
    /// Resolves the DNS (PTR) name for an address item, trying IPv4 first and falling back to IPv6,
    /// unless a newer check has started in the meantime.
    /// </summary>
    /// <remarks>
    /// A PTR lookup that completes without a server error but simply has no record (e.g. most home
    /// routers and many ISPs don't configure reverse DNS) is reported as <see cref="ConnectionState.Info"/>
    /// rather than <see cref="ConnectionState.Critical"/> - it's not a sign of a broken DNS setup.
    /// </remarks>
    private async Task ResolveDnsAsync(ConnectionCheckItem dnsItem, ConnectionCheckItem addressIPv4, ConnectionCheckItem addressIPv6, long generation, string context)
    {
        DNSClientResultString ipv4Result = null;
        DNSClientResultString ipv6Result = null;

        if (addressIPv4.State == ConnectionState.OK)
        {
            Log.Debug($"{context} > {nameof(ResolveDnsAsync)} - Resolving DNS based on IPv4...");
            ipv4Result = await DNSClient.GetInstance().ResolvePtrAsync(IPAddress.Parse(addressIPv4.Value));
        }

        if (ipv4Result is not { HasError: false } && addressIPv6.State == ConnectionState.OK)
        {
            Log.Debug($"{context} > {nameof(ResolveDnsAsync)} - Resolving DNS based on IPv6...");
            ipv6Result = await DNSClient.GetInstance().ResolvePtrAsync(IPAddress.Parse(addressIPv6.Value));
        }

        if (generation != _generation)
            return;

        // Prefer a successful resolution from either family. If neither succeeded, a clean
        // "not found" from either family still wins over a hard failure from the other - e.g.
        // IPv4 has no PTR (NXDOMAIN) while IPv6 times out should show as "no record", not as a
        // broken DNS setup.
        var resolved = ipv4Result is { HasError: false } ? ipv4Result : ipv6Result is { HasError: false } ? ipv6Result : null;
        var notFound = ipv4Result is { IsNotFound: true } || ipv6Result is { IsNotFound: true };

        if (resolved != null)
        {
            Log.Debug($"{context} > {nameof(ResolveDnsAsync)} - DNS resolved: " + resolved.Value);
            dnsItem.Complete(resolved.Value, ConnectionState.OK);
        }
        else if (notFound)
        {
            Log.Debug($"{context} > {nameof(ResolveDnsAsync)} - DNS not resolved (no record found).");
            dnsItem.Complete("-/-", ConnectionState.Info);
        }
        else
        {
            Log.Debug($"{context} > {nameof(ResolveDnsAsync)} - DNS not resolved due to error. IPv4: {ipv4Result?.ErrorMessage ?? "n/a"} | IPv6: {ipv6Result?.ErrorMessage ?? "n/a"}");
            dnsItem.Complete("-/-", ConnectionState.Critical);
        }
    }

    /// <summary>
    /// Resolves the computer's DNS (PTR) name.
    /// </summary>
    private Task CheckConnectionComputerDnsAsync(CancellationToken ct, long generation)
    {
        return Task.Run(async () =>
        {
            Log.Debug($"{nameof(CheckConnectionComputerDnsAsync)} - Checking computer DNS...");
            await ResolveDnsAsync(ComputerDNS, ComputerIPv4, ComputerIPv6, generation, nameof(CheckConnectionComputerDnsAsync));
            Log.Debug($"{nameof(CheckConnectionComputerDnsAsync)} - Computer DNS check completed.");
        }, ct);
    }

    /// <summary>
    /// Checks the router connection asynchronously.
    /// </summary>
    /// <param name="localIPv4">The already-detected local IPv4 address, or <c>null</c>.</param>
    /// <param name="localIPv6">The already-detected local IPv6 address, or <c>null</c>.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <param name="generation">The id of this check run, see <see cref="_generation"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task CheckConnectionRouterAsync(IPAddress localIPv4, IPAddress localIPv6, CancellationToken ct, long generation)
    {
        return Task.Run(async () =>
        {
            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Checking router connection...");

            // Detect router IPv4 gateway
            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Detecting router IPv4 address...");

            var routerIPv4 = localIPv4 != null
                ? await NetworkInterface.DetectGatewayFromLocalIPAddressAsync(localIPv4)
                : null;

            SetAddressResult(RouterIPv4, routerIPv4, generation);

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // Detect router IPv6 gateway
            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Detecting router IPv6 address...");

            var routerIPv6 = localIPv6 != null
                ? await NetworkInterface.DetectGatewayFromLocalIPAddressAsync(localIPv6)
                : null;

            SetAddressResult(RouterIPv6, routerIPv6, generation);

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // Resolve router DNS
            await ResolveDnsAsync(RouterDNS, RouterIPv4, RouterIPv6, generation, nameof(CheckConnectionRouterAsync));

            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Router connection check completed.");
        }, ct);
    }

    /// <summary>
    /// Checks the internet connection asynchronously.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <param name="generation">The id of this check run, see <see cref="_generation"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task CheckConnectionInternetAsync(CancellationToken ct, long generation)
    {
        return Task.Run(async () =>
        {
            // If public IP address check is disabled
            if (!CheckPublicIPAddressEnabled)
                return;

            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Checking internet connection...");

            // Detect public IPv4 and if it is reachable
            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Detecting public IPv4 address...");

            var publicIPv4AddressAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv4AddressAPI
                ? SettingsManager.Current.Dashboard_CustomPublicIPv4AddressAPI
                : GlobalStaticConfiguration.Dashboard_PublicIPv4AddressAPI;

            try
            {
                Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Checking public IPv4 address from: " + publicIPv4AddressAPI);

                var httpResponse = await PublicIPHttpClient.GetAsync(publicIPv4AddressAPI, ct);
                var result = await httpResponse.Content.ReadAsStringAsync(ct);
                var match = RegexHelper.IPv4AddressExtractRegex().Match(result);

                if (generation != _generation)
                    return;

                if (match.Success)
                {
                    Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Public IPv4 address detected: " + match.Value);
                    InternetIPv4.Complete(match.Value, ConnectionState.OK);
                }
                else
                {
                    Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Public IPv4 address not detected due to invalid format.");
                    InternetIPv4.Complete("-/-", ConnectionState.Critical);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Public IPv4 address not detected due to exception: " + ex.Message);

                if (generation == _generation)
                    InternetIPv4.Complete("-/-", ConnectionState.Critical);
            }

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // Detect public IPv6 and if it is reachable
            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Detecting public IPv6 address...");

            var publicIPv6AddressAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv6AddressAPI
                ? SettingsManager.Current.Dashboard_CustomPublicIPv6AddressAPI
                : GlobalStaticConfiguration.Dashboard_PublicIPv6AddressAPI;

            try
            {
                Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Checking public IPv6 address from: " + publicIPv6AddressAPI);

                var httpResponse = await PublicIPHttpClient.GetAsync(publicIPv6AddressAPI, ct);
                var result = await httpResponse.Content.ReadAsStringAsync(ct);
                var match = Regex.Match(result, RegexHelper.IPv6AddressRegex);

                if (generation != _generation)
                    return;

                if (match.Success)
                {
                    Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Public IPv6 address detected: " + match.Value);
                    InternetIPv6.Complete(match.Value, ConnectionState.OK);
                }
                else
                {
                    Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Public IPv6 address not detected due to invalid format.");
                    InternetIPv6.Complete("-/-", ConnectionState.Critical);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Public IPv6 address not detected due to exception: " + ex.Message);

                if (generation == _generation)
                    InternetIPv6.Complete("-/-", ConnectionState.Critical);
            }

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // Resolve internet DNS
            await ResolveDnsAsync(InternetDNS, InternetIPv4, InternetIPv6, generation, nameof(CheckConnectionInternetAsync));

            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Internet connection check completed.");
        }, ct);
    }

    #endregion
}
