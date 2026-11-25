using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using NetworkInterface = NETworkManager.Models.Network.NetworkInterface;
using System.Net.Sockets;
using System.Text;
using NETworkManager.Localization.Resources;

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

    /// <summary>
    /// Data sent with the ping of the gateway and the public IP address.
    /// </summary>
    private static readonly byte[] PingBuffer = new byte[GlobalStaticConfiguration.Dashboard_PingBuffer];

    #region Computer

    /// <summary>
    /// Gets the computer IPv4 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem ComputerIPv4 { get; } = new(ConnectionCheckItemKind.ComputerIPAddress);

    /// <summary>
    /// Gets the computer IPv6 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem ComputerIPv6 { get; } = new(ConnectionCheckItemKind.ComputerIPAddress);

    /// <summary>
    /// Gets the computer DNS check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem ComputerDNS { get; } = new(ConnectionCheckItemKind.DNSName);

    #endregion

    #region Router

    /// <summary>
    /// Gets the router IPv4 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem RouterIPv4 { get; } = new(ConnectionCheckItemKind.RouterIPAddress);

    /// <summary>
    /// Gets the router IPv6 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem RouterIPv6 { get; } = new(ConnectionCheckItemKind.RouterIPAddress);

    /// <summary>
    /// Gets the router DNS check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem RouterDNS { get; } = new(ConnectionCheckItemKind.DNSName);

    #endregion

    #region Internet

    /// <summary>
    /// Gets the internet IPv4 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem InternetIPv4 { get; } = new(ConnectionCheckItemKind.PublicIPAddress);

    /// <summary>
    /// Gets the internet IPv6 check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem InternetIPv6 { get; } = new(ConnectionCheckItemKind.PublicIPAddress);

    /// <summary>
    /// Gets the internet DNS check (is checking / value / state).
    /// </summary>
    public ConnectionCheckItem InternetDNS { get; } = new(ConnectionCheckItemKind.DNSName);

    #endregion

    /// <summary>
    /// Gets the time of the last check, <c>null</c> before the first one.
    /// </summary>
    public DateTime? LastCheckTime
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

    /// <summary>
    /// Gets the command to copy all values (with the reasons of the states), e.g. for a support request.
    /// </summary>
    public ICommand CopyAllCommand => new RelayCommand(_ => CopyAllAction(), _ => !IsChecking && LastCheckTime != null);

    private void CopyAllAction()
    {
        var text = new StringBuilder();

        foreach (var (title, ipv4, ipv6, dns) in new[]
                 {
                     (Strings.Computer, ComputerIPv4, ComputerIPv6, ComputerDNS),
                     (Strings.GatewayRouter, RouterIPv4, RouterIPv6, RouterDNS),
                     (Strings.Internet, InternetIPv4, InternetIPv6, InternetDNS)
                 })
        {
            if (text.Length > 0)
                text.AppendLine();

            text.AppendLine(title);

            foreach (var (label, item) in new[] { (Strings.IPv4, ipv4), (Strings.IPv6, ipv6), (Strings.DNS, dns) })
                text.AppendLine(item.Message == null
                    ? $"{label}: {item.Value}"
                    : $"{label}: {item.Value} ({item.Message})");
        }

        ClipboardHelper.SetClipboard(text.ToString().TrimEnd());
    }

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

                LastCheckTime = DateTime.Now;

                Log.Info("Network connection check completed.");
            } while (_recheckPending);
        }
        catch (Exception ex)
        {
            Log.Error("Network connection check failed.", ex);

            // Don't leave the remaining rows loading until the next check
            foreach (var item in GetAllItems().Where(x => x.IsChecking))
                item.Complete("-/-", ConnectionState.Critical);
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

        // Detect the local IPv4/IPv6 address once and share it between the Computer, Router and Internet checks below
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
            CheckConnectionRouterAsync(localIPv4, isIPv4Routed, isIPv6Available ? localIPv6 : null),
            CheckConnectionInternetAsync(isIPv6Available)
        );
    }

    /// <summary>
    /// Resets all connection check items to their initial "checking" state.
    /// </summary>
    private void ResetAllItems()
    {
        foreach (var item in GetAllItems())
            item.Reset();
    }

    /// <summary>
    /// Gets all connection check items.
    /// </summary>
    private ConnectionCheckItem[] GetAllItems()
    {
        return
        [
            ComputerIPv4, ComputerIPv6, ComputerDNS, RouterIPv4, RouterIPv6, RouterDNS, InternetIPv4, InternetIPv6,
            InternetDNS
        ];
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
    /// Detects the gateway of the local IP address and pings it, see <see cref="NetworkConnectionCheck.GetGatewayResult" />.
    /// </summary>
    /// <param name="item">Item of the router check (IPv4 or IPv6).</param>
    /// <param name="localIPAddress">Local IP address, or <c>null</c>.</param>
    /// <param name="isRouted">The local IP address was detected via the route to the public IP address.</param>
    /// <returns>The gateway, or <c>null</c> if none was found.</returns>
    private static async Task<IPAddress> CheckGatewayAsync(ConnectionCheckItem item, IPAddress localIPAddress,
        bool isRouted)
    {
        var gateway = localIPAddress != null
            ? await NetworkInterface.DetectGatewayFromLocalIPAddressAsync(localIPAddress)
            : null;

        var isReachable = false;

        if (gateway != null)
        {
            var pingInfo = await PingProbe.PingAsync(gateway, GlobalStaticConfiguration.Dashboard_PingTimeout,
                GlobalStaticConfiguration.Dashboard_PingAttempts, PingBuffer, CancellationToken.None);

            isReachable = pingInfo.Status == IPStatus.Success;

            Log.Debug($"{nameof(CheckGatewayAsync)} - Ping {gateway}: {pingInfo.Status}");
        }

        item.Complete(gateway?.ToString() ?? "-/-",
            NetworkConnectionCheck.GetGatewayResult(gateway, isRouted, isReachable));

        return gateway;
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
    /// Checks the router connection asynchronously: detects the IPv4 / IPv6 gateway, pings both at the same time, then
    /// resolves the DNS name.
    /// </summary>
    /// <param name="localIPv4">The already-detected local IPv4 address, or <c>null</c>.</param>
    /// <param name="isIPv4Routed">The local IPv4 address was detected via the route to the public IP address.</param>
    /// <param name="localIPv6">The already-detected local IPv6 address, or <c>null</c> if IPv6 is not available.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task CheckConnectionRouterAsync(IPAddress localIPv4, bool isIPv4Routed, IPAddress localIPv6)
    {
        return Task.Run(async () =>
        {
            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Checking router connection...");

            var routerIPv4Task = CheckGatewayAsync(RouterIPv4, localIPv4, isIPv4Routed);
            var routerIPv6Task = Task.FromResult<IPAddress>(null);

            if (localIPv6 != null)
                routerIPv6Task = CheckGatewayAsync(RouterIPv6, localIPv6, true);
            else
                SetIPv6NotAvailable(RouterIPv6);

            await Task.WhenAll(routerIPv4Task, routerIPv6Task);

            // Resolve router DNS
            await ResolveDnsAsync(RouterDNS, routerIPv4Task.Result, routerIPv6Task.Result,
                nameof(CheckConnectionRouterAsync));

            Log.Debug($"{nameof(CheckConnectionRouterAsync)} - Router connection check completed.");
        });
    }

    /// <summary>
    /// Requests the public IP address from the API (setting), see <see cref="PublicIPAddressCheck" />.
    /// </summary>
    /// <param name="addressFamily">IPv4 or IPv6.</param>
    /// <returns>Result of the request.</returns>
    private static async Task<PublicIPAddressCheckResult> RequestPublicIPAddressAsync(AddressFamily addressFamily)
    {
        var api = addressFamily == AddressFamily.InterNetwork
            ? SettingsManager.Current.Dashboard_UseCustomPublicIPv4AddressAPI
                ? SettingsManager.Current.Dashboard_CustomPublicIPv4AddressAPI
                : GlobalStaticConfiguration.Dashboard_PublicIPv4AddressAPI
            : SettingsManager.Current.Dashboard_UseCustomPublicIPv6AddressAPI
                ? SettingsManager.Current.Dashboard_CustomPublicIPv6AddressAPI
                : GlobalStaticConfiguration.Dashboard_PublicIPv6AddressAPI;

        try
        {
            Log.Debug($"{nameof(RequestPublicIPAddressAsync)} - Checking public {addressFamily} address from: {api}");

            using var httpResponse = await PublicIPHttpClient.GetAsync(api);
            var content = await httpResponse.Content.ReadAsStringAsync();

            var result = PublicIPAddressCheck.ParseResponse(httpResponse.IsSuccessStatusCode, content, addressFamily);

            Log.Debug($"{nameof(RequestPublicIPAddressAsync)} - HTTP {(int)httpResponse.StatusCode}: {result.Status} {result.IPAddress}");

            return result;
        }
        catch (Exception ex)
        {
            var result = PublicIPAddressCheck.FromException(ex);

            Log.Debug($"{nameof(RequestPublicIPAddressAsync)} - {result.Status}: {ex.Message}");

            return result;
        }
    }

    /// <summary>
    /// Pings the public IP address (setting) to check if the internet is reachable.
    /// </summary>
    /// <param name="ipAddress">Public IPv4 or IPv6 address (setting).</param>
    /// <param name="addressFamily">IPv4 or IPv6.</param>
    /// <returns>Result of the ping, or <c>null</c> if the address in the settings is invalid.</returns>
    private static async Task<PingInfo> PingPublicIPAddressAsync(string ipAddress, AddressFamily addressFamily)
    {
        if (!IPAddress.TryParse(ipAddress, out var target) || target.AddressFamily != addressFamily)
        {
            Log.Warn($"{nameof(PingPublicIPAddressAsync)} - Invalid public {addressFamily} address \"{ipAddress}\" in the settings, skipping the ping.");
            return null;
        }

        var pingInfo = await PingProbe.PingAsync(target, GlobalStaticConfiguration.Dashboard_PingTimeout,
            GlobalStaticConfiguration.Dashboard_PingAttempts, PingBuffer, CancellationToken.None);

        Log.Debug($"{nameof(PingPublicIPAddressAsync)} - Ping {target}: {pingInfo.Status}");

        return pingInfo;
    }

    /// <summary>
    /// Checks the internet connection of an address family: ping of the public IP address (setting) and the public IP
    /// address API (if enabled) at the same time, see <see cref="NetworkConnectionCheck.GetInternetResult" />.
    /// </summary>
    /// <param name="item">Item of the internet check (IPv4 or IPv6).</param>
    /// <param name="addressFamily">IPv4 or IPv6.</param>
    /// <param name="connectivityLevelTask">Internet connectivity reported by Windows (read once per check).</param>
    /// <returns>The IP address returned by the API (also if private), or <c>null</c>.</returns>
    private static async Task<IPAddress> CheckInternetAsync(ConnectionCheckItem item, AddressFamily addressFamily,
        Task<InternetConnectivityLevel> connectivityLevelTask)
    {
        var pingTask = PingPublicIPAddressAsync(addressFamily == AddressFamily.InterNetwork
            ? SettingsManager.Current.Dashboard_PublicIPv4Address
            : SettingsManager.Current.Dashboard_PublicIPv6Address, addressFamily);

        var publicIPAddressTask = SettingsManager.Current.Dashboard_CheckPublicIPAddress
            ? RequestPublicIPAddressAsync(addressFamily)
            : Task.FromResult<PublicIPAddressCheckResult>(null);

        await Task.WhenAll(pingTask, publicIPAddressTask, connectivityLevelTask);

        var pingInfo = pingTask.Result;
        var publicIPAddress = publicIPAddressTask.Result;
        var isPingSuccessful = pingInfo is { Status: IPStatus.Success };

        // Public IP address (API), otherwise the pinged address with the round-trip time
        var value = publicIPAddress?.IPAddress?.ToString() ??
                    (isPingSuccessful ? $"{pingInfo.IPAddress} ({pingInfo.Time} ms)" : "-/-");

        item.Complete(value, NetworkConnectionCheck.GetInternetResult(addressFamily, isPingSuccessful,
            publicIPAddress, connectivityLevelTask.Result), publicIPAddress?.IPAddress?.ToString() ??
                                                            (isPingSuccessful ? pingInfo.IPAddress.ToString() : null));

        return publicIPAddress?.IPAddress;
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
            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Checking internet connection...");

            var connectivityLevelTask = Task.Run(InternetConnectivity.GetLevel);

            var publicIPv4Task = CheckInternetAsync(InternetIPv4, AddressFamily.InterNetwork, connectivityLevelTask);
            var publicIPv6Task = Task.FromResult<IPAddress>(null);

            // Only if the computer has a routed IPv6 address
            if (isIPv6Available)
                publicIPv6Task = CheckInternetAsync(InternetIPv6, AddressFamily.InterNetworkV6, connectivityLevelTask);
            else
                SetIPv6NotAvailable(InternetIPv6);

            await Task.WhenAll(publicIPv4Task, publicIPv6Task);

            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Windows reports: {connectivityLevelTask.Result}");

            // Resolve internet DNS (of the public IP address)
            if (SettingsManager.Current.Dashboard_CheckPublicIPAddress)
                await ResolveDnsAsync(InternetDNS, publicIPv4Task.Result, publicIPv6Task.Result,
                    nameof(CheckConnectionInternetAsync));
            else
                InternetDNS.Complete("-/-", new ConnectionCheckResult(ConnectionState.Info,
                    ConnectionCheckReason.PublicIPAddressCheckDisabled));

            Log.Debug($"{nameof(CheckConnectionInternetAsync)} - Internet connection check completed.");
        });
    }

    #endregion
}
