using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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
    public ICommand CheckCommand => new RelayCommand(_ => Check(), _ => !IsChecking);

    #endregion

    #region Methods

    /// <summary>
    /// Checks the network connections. If a check is already running, it is repeated once it has finished.
    /// </summary>
    public void Check()
    {
        _ = CheckAsync();
    }

    /// <summary>
    /// A check was requested while another one was running (e.g. a network change during a check). The running check
    /// is repeated once it has finished, so the result shows the latest state. Only used on the UI thread.
    /// </summary>
    private bool _recheckPending;

    /// <summary>
    /// Checks the network connections asynchronously.
    /// </summary>
    private async Task CheckAsync()
    {
        if (IsChecking)
        {
            _recheckPending = true;
            return;
        }

        IsChecking = true;

        try
        {
            do
            {
                _recheckPending = false;

                Log.Info("Checking network connection...");

                // Takes at least the refresh interval, so the reload animation is visible and the check can't be
                // spammed - the checks run at the same time
                await TaskHelper.WithMinimumDurationAsync(RunTask, GlobalStaticConfiguration.ApplicationUIRefreshInterval);

                Log.Info("Network connection check completed.");
            } while (_recheckPending);
        }
        catch (Exception ex)
        {
            Log.Error("Network connection check failed.", ex);
        }
        finally
        {
            IsChecking = false;
        }
    }

    /// <summary>
    /// Runs the check tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunTask()
    {
        ResetAllItems();

        if (ScreenshotData.IsEnabled)
        {
            SetScreenshotData();
            return;
        }

        // Detect the local IPv4/IPv6 address once and share it between the Computer and Router
        // checks below, instead of each of them redetecting it independently (both used to
        // trigger their own, potentially PowerShell-backed, network interface lookup).
        var localIPv4Task = DetectLocalIPAddressAsync(SettingsManager.Current.Dashboard_PublicIPv4Address,
            AddressFamily.InterNetwork);
        var localIPv6Task = DetectLocalIPAddressAsync(SettingsManager.Current.Dashboard_PublicIPv6Address,
            AddressFamily.InterNetworkV6);

        await Task.WhenAll(localIPv4Task, localIPv6Task);

        var localIPv4 = localIPv4Task.Result;
        var localIPv6 = localIPv6Task.Result;

        SetAddressResult(ComputerIPv4, localIPv4);
        SetAddressResult(ComputerIPv6, localIPv6);

        await Task.WhenAll(
            CheckConnectionComputerDnsAsync(),
            CheckConnectionRouterAsync(localIPv4, localIPv6),
            CheckConnectionInternetAsync()
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

        if (!SettingsManager.Current.Dashboard_CheckPublicIPAddress)
            return;

        InternetIPv4.Reset();
        InternetIPv6.Reset();
        InternetDNS.Reset();
    }

    /// <summary>
    /// Sets the sample data of <see cref="ScreenshotData" /> instead of the real (personal) addresses.
    /// </summary>
    private void SetScreenshotData()
    {
        ComputerIPv4.Complete(ScreenshotData.ComputerIPv4, ConnectionState.OK);
        ComputerIPv6.Complete(ScreenshotData.ComputerIPv6, ConnectionState.OK);
        ComputerDNS.Complete(ScreenshotData.ComputerDNS, ConnectionState.OK);

        RouterIPv4.Complete(ScreenshotData.RouterIPv4, ConnectionState.OK);
        RouterIPv6.Complete(ScreenshotData.RouterIPv6, ConnectionState.OK);
        RouterDNS.Complete(ScreenshotData.RouterDNS, ConnectionState.OK);

        if (!SettingsManager.Current.Dashboard_CheckPublicIPAddress)
            return;

        InternetIPv4.Complete(ScreenshotData.InternetIPv4, ConnectionState.OK);
        InternetIPv6.Complete(ScreenshotData.InternetIPv6, ConnectionState.OK);
        InternetDNS.Complete(ScreenshotData.InternetDNS, ConnectionState.OK);
    }

    /// <summary>
    /// Detects the local IP address, based on routing to the configured public IP address (setting), with a fallback
    /// to detection based on the network interfaces.
    /// </summary>
    /// <param name="remoteIPAddress">Public IPv4 or IPv6 address (setting) used to find the route.</param>
    /// <param name="addressFamily">IPv4 or IPv6.</param>
    /// <returns>Local IP address, or <c>null</c> if none was found.</returns>
    private static async Task<IPAddress> DetectLocalIPAddressAsync(string remoteIPAddress, AddressFamily addressFamily)
    {
        Log.Debug($"{nameof(DetectLocalIPAddressAsync)} - Detecting local {addressFamily} address...");

        IPAddress detected = null;

        if (IPAddress.TryParse(remoteIPAddress, out var remote) && remote.AddressFamily == addressFamily)
            detected = await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(remote);
        else
            Log.Warn($"{nameof(DetectLocalIPAddressAsync)} - Invalid public {addressFamily} address \"{remoteIPAddress}\" in the settings, skipping routing based detection.");

        if (detected == null)
        {
            Log.Debug($"{nameof(DetectLocalIPAddressAsync)} - Local {addressFamily} address detection via routing failed, trying network interfaces...");
            detected = await NetworkInterface.DetectLocalIPAddressFromNetworkInterfaceAsync(addressFamily);
        }

        Log.Debug(detected != null
            ? $"{nameof(DetectLocalIPAddressAsync)} - Local {addressFamily} address detected: {detected}"
            : $"{nameof(DetectLocalIPAddressAsync)} - Local {addressFamily} address not detected.");

        return detected;
    }

    /// <summary>
    /// Applies a detected address to a <see cref="ConnectionCheckItem"/>.
    /// </summary>
    private static void SetAddressResult(ConnectionCheckItem item, IPAddress detected)
    {
        item.Complete(detected?.ToString() ?? "-/-", detected != null ? ConnectionState.OK : ConnectionState.Critical);
    }

    /// <summary>
    /// Resolves the DNS (PTR) name for an address item, trying IPv4 first and falling back to IPv6.
    /// </summary>
    /// <remarks>
    /// A PTR lookup that completes without a server error but simply has no record (e.g. most home
    /// routers and many ISPs don't configure reverse DNS) is reported as <see cref="ConnectionState.Info"/>
    /// rather than <see cref="ConnectionState.Critical"/> - it's not a sign of a broken DNS setup.
    /// </remarks>
    private async Task ResolveDnsAsync(ConnectionCheckItem dnsItem, ConnectionCheckItem addressIPv4, ConnectionCheckItem addressIPv6, string context)
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
    private Task CheckConnectionComputerDnsAsync()
    {
        return Task.Run(async () =>
        {
            Log.Debug($"{nameof(CheckConnectionComputerDnsAsync)} - Checking computer DNS...");
            await ResolveDnsAsync(ComputerDNS, ComputerIPv4, ComputerIPv6, nameof(CheckConnectionComputerDnsAsync));
            Log.Debug($"{nameof(CheckConnectionComputerDnsAsync)} - Computer DNS check completed.");
        });
    }

    /// <summary>
    /// Checks the router connection asynchronously.
    /// </summary>
    /// <param name="localIPv4">The already-detected local IPv4 address, or <c>null</c>.</param>
    /// <param name="localIPv6">The already-detected local IPv6 address, or <c>null</c>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task CheckConnectionRouterAsync(IPAddress localIPv4, IPAddress localIPv6)
    {
        return Task.Run(async () =>
        {
            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Checking router connection...");

            // Detect router IPv4 gateway
            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Detecting router IPv4 address...");

            var routerIPv4 = localIPv4 != null
                ? await NetworkInterface.DetectGatewayFromLocalIPAddressAsync(localIPv4)
                : null;

            SetAddressResult(RouterIPv4, routerIPv4);

            // Detect router IPv6 gateway
            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Detecting router IPv6 address...");

            var routerIPv6 = localIPv6 != null
                ? await NetworkInterface.DetectGatewayFromLocalIPAddressAsync(localIPv6)
                : null;

            SetAddressResult(RouterIPv6, routerIPv6);

            // Resolve router DNS
            await ResolveDnsAsync(RouterDNS, RouterIPv4, RouterIPv6, nameof(CheckConnectionRouterAsync));

            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Router connection check completed.");
        });
    }

    /// <summary>
    /// Checks the internet connection asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task CheckConnectionInternetAsync()
    {
        return Task.Run(async () =>
        {
            // If public IP address check is disabled
            if (!SettingsManager.Current.Dashboard_CheckPublicIPAddress)
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

                var httpResponse = await PublicIPHttpClient.GetAsync(publicIPv4AddressAPI);
                var result = await httpResponse.Content.ReadAsStringAsync();
                var match = RegexHelper.IPv4AddressExtractRegex().Match(result);

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
            catch (Exception ex)
            {
                Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Public IPv4 address not detected due to exception: " + ex.Message);

                InternetIPv4.Complete("-/-", ConnectionState.Critical);
            }

            // Detect public IPv6 and if it is reachable
            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Detecting public IPv6 address...");

            var publicIPv6AddressAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv6AddressAPI
                ? SettingsManager.Current.Dashboard_CustomPublicIPv6AddressAPI
                : GlobalStaticConfiguration.Dashboard_PublicIPv6AddressAPI;

            try
            {
                Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Checking public IPv6 address from: " + publicIPv6AddressAPI);

                var httpResponse = await PublicIPHttpClient.GetAsync(publicIPv6AddressAPI);
                var result = await httpResponse.Content.ReadAsStringAsync();
                var match = Regex.Match(result, RegexHelper.IPv6AddressRegex);

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
            catch (Exception ex)
            {
                Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Public IPv6 address not detected due to exception: " + ex.Message);

                InternetIPv6.Complete("-/-", ConnectionState.Critical);
            }

            // Resolve internet DNS
            await ResolveDnsAsync(InternetDNS, InternetIPv4, InternetIPv6, nameof(CheckConnectionInternetAsync));

            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Internet connection check completed.");
        });
    }

    #endregion
}
