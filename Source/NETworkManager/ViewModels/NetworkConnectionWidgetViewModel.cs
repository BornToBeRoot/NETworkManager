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

        var (localIPv4, isIPv4Routed) = localIPv4Task.Result;
        var (localIPv6, isIPv6Routed) = localIPv6Task.Result;

        var computerIPv4Result = NetworkConnectionCheck.GetLocalIPAddressResult(localIPv4, AddressFamily.InterNetwork, isIPv4Routed);
        var computerIPv6Result = NetworkConnectionCheck.GetLocalIPAddressResult(localIPv6, AddressFamily.InterNetworkV6, isIPv6Routed);

        ComputerIPv4.Complete(localIPv4?.ToString() ?? "-/-", computerIPv4Result);
        ComputerIPv6.Complete(localIPv6?.ToString() ?? "-/-", computerIPv6Result);

        // Without a routed IPv6 address, the IPv6 checks of the router and the internet are not available
        var isIPv6Available = computerIPv6Result.State == ConnectionState.OK;

        await Task.WhenAll(
            CheckConnectionComputerDnsAsync(localIPv4, isIPv6Available ? localIPv6 : null),
            CheckConnectionRouterAsync(localIPv4, isIPv6Available ? localIPv6 : null),
            CheckConnectionInternetAsync(isIPv6Available)
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
    /// <returns>Local IP address (<c>null</c> if none was found) and whether it was detected via the route.</returns>
    private static async Task<(IPAddress Address, bool IsRouted)> DetectLocalIPAddressAsync(string remoteIPAddress,
        AddressFamily addressFamily)
    {
        Log.Debug($"{nameof(DetectLocalIPAddressAsync)} - Detecting local {addressFamily} address...");

        IPAddress detected = null;

        if (IPAddress.TryParse(remoteIPAddress, out var remote) && remote.AddressFamily == addressFamily)
            detected = await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(remote);
        else
            Log.Warn($"{nameof(DetectLocalIPAddressAsync)} - Invalid public {addressFamily} address \"{remoteIPAddress}\" in the settings, skipping routing based detection.");

        var isRouted = detected != null;

        if (!isRouted)
        {
            Log.Debug($"{nameof(DetectLocalIPAddressAsync)} - Local {addressFamily} address detection via routing failed, trying network interfaces...");
            detected = await NetworkInterface.DetectLocalIPAddressFromNetworkInterfaceAsync(addressFamily);
        }

        Log.Debug(detected != null
            ? $"{nameof(DetectLocalIPAddressAsync)} - Local {addressFamily} address detected: {detected} (routed: {isRouted})"
            : $"{nameof(DetectLocalIPAddressAsync)} - Local {addressFamily} address not detected.");

        return (detected, isRouted);
    }

    /// <summary>
    /// Applies a detected gateway to a <see cref="ConnectionCheckItem"/>.
    /// </summary>
    private static void SetGatewayResult(ConnectionCheckItem item, IPAddress detected)
    {
        item.Complete(detected?.ToString() ?? "-/-", detected != null ? ConnectionState.OK : ConnectionState.Critical);
    }

    /// <summary>
    /// Marks an IPv6 check as not available (the computer has no routed IPv6 address).
    /// </summary>
    private static void SetIPv6NotAvailable(ConnectionCheckItem item)
    {
        item.Complete("-/-", new ConnectionCheckResult(ConnectionState.Info, ConnectionCheckReason.IPv6NotAvailable));
    }

    /// <summary>
    /// Resolves the DNS (PTR) name of the IPv4 address, and of the IPv6 address if needed (see
    /// <see cref="NetworkConnectionCheck.ShouldResolveIPv6" /> / <see cref="NetworkConnectionCheck.GetDNSResult" />).
    /// </summary>
    private static async Task ResolveDnsAsync(ConnectionCheckItem dnsItem, IPAddress ipv4Address, IPAddress ipv6Address,
        string context)
    {
        DNSClientResultString ipv4Result = null;
        DNSClientResultString ipv6Result = null;

        if (ipv4Address != null)
        {
            Log.Debug($"{context} > {nameof(ResolveDnsAsync)} - Resolving DNS based on IPv4...");
            ipv4Result = await DNSClient.GetInstance().ResolvePtrAsync(ipv4Address);
        }

        if (ipv6Address != null && NetworkConnectionCheck.ShouldResolveIPv6(ipv4Result))
        {
            Log.Debug($"{context} > {nameof(ResolveDnsAsync)} - Resolving DNS based on IPv6...");
            ipv6Result = await DNSClient.GetInstance().ResolvePtrAsync(ipv6Address);
        }

        var (result, hostname) = NetworkConnectionCheck.GetDNSResult(ipv4Result, ipv6Result);

        Log.Debug($"{context} > {nameof(ResolveDnsAsync)} - {result.State} ({result.Reason}): {hostname}. IPv4: {ipv4Result?.ErrorMessage ?? "n/a"} | IPv6: {ipv6Result?.ErrorMessage ?? "n/a"}");

        dnsItem.Complete(hostname ?? "-/-", result);
    }

    /// <summary>
    /// Resolves the computer's DNS (PTR) name.
    /// </summary>
    /// <param name="localIPv4">The local IPv4 address, or <c>null</c>.</param>
    /// <param name="localIPv6">The local IPv6 address, or <c>null</c> (also if IPv6 is not available).</param>
    private Task CheckConnectionComputerDnsAsync(IPAddress localIPv4, IPAddress localIPv6)
    {
        return Task.Run(async () =>
        {
            Log.Debug($"{nameof(CheckConnectionComputerDnsAsync)} - Checking computer DNS...");
            await ResolveDnsAsync(ComputerDNS, localIPv4, localIPv6, nameof(CheckConnectionComputerDnsAsync));
            Log.Debug($"{nameof(CheckConnectionComputerDnsAsync)} - Computer DNS check completed.");
        });
    }

    /// <summary>
    /// Checks the router connection asynchronously.
    /// </summary>
    /// <param name="localIPv4">The already-detected local IPv4 address, or <c>null</c>.</param>
    /// <param name="localIPv6">The already-detected local IPv6 address, or <c>null</c> if IPv6 is not available.</param>
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

            SetGatewayResult(RouterIPv4, routerIPv4);

            // Detect router IPv6 gateway
            IPAddress routerIPv6 = null;

            if (localIPv6 != null)
            {
                Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Detecting router IPv6 address...");

                routerIPv6 = await NetworkInterface.DetectGatewayFromLocalIPAddressAsync(localIPv6);

                SetGatewayResult(RouterIPv6, routerIPv6);
            }
            else
            {
                SetIPv6NotAvailable(RouterIPv6);
            }

            // Resolve router DNS
            await ResolveDnsAsync(RouterDNS, routerIPv4, routerIPv6, nameof(CheckConnectionRouterAsync));

            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Router connection check completed.");
        });
    }

    /// <summary>
    /// Checks the internet connection asynchronously.
    /// </summary>
    /// <param name="isIPv6Available">The computer has a routed IPv6 address (otherwise IPv6 is not checked).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task CheckConnectionInternetAsync(bool isIPv6Available)
    {
        return Task.Run(async () =>
        {
            // If public IP address check is disabled
            if (!SettingsManager.Current.Dashboard_CheckPublicIPAddress)
                return;

            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Checking internet connection...");

            IPAddress publicIPv4 = null;
            IPAddress publicIPv6 = null;

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
                    publicIPv4 = IPAddress.Parse(match.Value);
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

            // Detect public IPv6 and if it is reachable (only if the computer has a routed IPv6 address)
            if (!isIPv6Available)
            {
                SetIPv6NotAvailable(InternetIPv6);
            }
            else
            {
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
                        publicIPv6 = IPAddress.Parse(match.Value);
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
            }

            // Resolve internet DNS
            await ResolveDnsAsync(InternetDNS, publicIPv4, publicIPv6, nameof(CheckConnectionInternetAsync));

            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Internet connection check completed.");
        });
    }

    #endregion
}
