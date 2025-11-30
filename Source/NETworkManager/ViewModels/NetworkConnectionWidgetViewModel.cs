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

namespace NETworkManager.ViewModels;

public class NetworkConnectionWidgetViewModel : ViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(NetworkConnectionWidgetViewModel));


    #region Computer

    private bool _isComputerIPv4Checking;

    public bool IsComputerIPv4Checking
    {
        get => _isComputerIPv4Checking;
        set
        {
            if (value == _isComputerIPv4Checking)
                return;

            _isComputerIPv4Checking = value;
            OnPropertyChanged();
        }
    }

    private string _computerIPv4;

    public string ComputerIPv4
    {
        get => _computerIPv4;
        set
        {
            if (value == _computerIPv4)
                return;

            _computerIPv4 = value;
            OnPropertyChanged();
        }
    }

    private ConnectionState _computerIPv4State = ConnectionState.None;

    public ConnectionState ComputerIPv4State
    {
        get => _computerIPv4State;
        private set
        {
            if (value == _computerIPv4State)
                return;

            _computerIPv4State = value;
            OnPropertyChanged();
        }
    }

    private bool _isComputerIPv6Checking;

    public bool IsComputerIPv6Checking
    {
        get => _isComputerIPv6Checking;
        set
        {
            if (value == _isComputerIPv6Checking)
                return;

            _isComputerIPv6Checking = value;
            OnPropertyChanged();
        }
    }

    private string _computerIPv6;

    public string ComputerIPv6
    {
        get => _computerIPv6;
        set
        {
            if (value == _computerIPv6)
                return;

            _computerIPv6 = value;
            OnPropertyChanged();
        }
    }

    private ConnectionState _computerIPv6State = ConnectionState.None;

    public ConnectionState ComputerIPv6State
    {
        get => _computerIPv6State;
        private set
        {
            if (value == _computerIPv6State)
                return;

            _computerIPv6State = value;
            OnPropertyChanged();
        }
    }

    private bool _isComputerDNSChecking;

    public bool IsComputerDNSChecking
    {
        get => _isComputerDNSChecking;
        set
        {
            if (value == _isComputerDNSChecking)
                return;

            _isComputerDNSChecking = value;
            OnPropertyChanged();
        }
    }

    private string _computerDNS;

    public string ComputerDNS
    {
        get => _computerDNS;
        set
        {
            if (value == _computerDNS)
                return;

            _computerDNS = value;
            OnPropertyChanged();
        }
    }

    private ConnectionState _computerDNSState = ConnectionState.None;

    public ConnectionState ComputerDNSState
    {
        get => _computerDNSState;
        private set
        {
            if (value == _computerDNSState)
                return;

            _computerDNSState = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Router

    private bool _isRouterIPv4Checking;

    public bool IsRouterIPv4Checking
    {
        get => _isRouterIPv4Checking;
        set
        {
            if (value == _isRouterIPv4Checking)
                return;

            _isRouterIPv4Checking = value;
            OnPropertyChanged();
        }
    }

    private string _routerIPv4;

    public string RouterIPv4
    {
        get => _routerIPv4;
        set
        {
            if (value == _routerIPv4)
                return;

            _routerIPv4 = value;
            OnPropertyChanged();
        }
    }

    private ConnectionState _routerIPv4State = ConnectionState.None;

    public ConnectionState RouterIPv4State
    {
        get => _routerIPv4State;
        private set
        {
            if (value == _routerIPv4State)
                return;

            _routerIPv4State = value;
            OnPropertyChanged();
        }
    }

    private bool _isRouterIPv6Checking;

    public bool IsRouterIPv6Checking
    {
        get => _isRouterIPv6Checking;
        set
        {
            if (value == _isRouterIPv6Checking)
                return;

            _isRouterIPv6Checking = value;
            OnPropertyChanged();
        }
    }

    private string _routerIPv6;

    public string RouterIPv6
    {
        get => _routerIPv6;
        set
        {
            if (value == _routerIPv6)
                return;

            _routerIPv6 = value;
            OnPropertyChanged();
        }
    }

    private ConnectionState _routerIPv6State = ConnectionState.None;

    public ConnectionState RouterIPv6State
    {
        get => _routerIPv6State;
        private set
        {
            if (value == _routerIPv6State)
                return;

            _routerIPv6State = value;
            OnPropertyChanged();
        }
    }

    private bool _isRouterDNSChecking;

    public bool IsRouterDNSChecking
    {
        get => _isRouterDNSChecking;
        set
        {
            if (value == _isRouterDNSChecking)
                return;

            _isRouterDNSChecking = value;
            OnPropertyChanged();
        }
    }

    private string _routerDNS;

    public string RouterDNS
    {
        get => _routerDNS;
        set
        {
            if (value == _routerDNS)
                return;

            _routerDNS = value;
            OnPropertyChanged();
        }
    }

    private ConnectionState _routerDNSState = ConnectionState.None;

    public ConnectionState RouterDNSState
    {
        get => _routerDNSState;
        private set
        {
            if (value == _routerDNSState)
                return;

            _routerDNSState = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Internet

    private bool _isInternetIPv4Checking;

    public bool IsInternetIPv4Checking
    {
        get => _isInternetIPv4Checking;
        set
        {
            if (value == _isInternetIPv4Checking)
                return;

            _isInternetIPv4Checking = value;
            OnPropertyChanged();
        }
    }

    private string _internetIPv4;

    public string InternetIPv4
    {
        get => _internetIPv4;
        set
        {
            if (value == _internetIPv4)
                return;

            _internetIPv4 = value;
            OnPropertyChanged();
        }
    }

    private ConnectionState _internetIPv4State = ConnectionState.None;

    public ConnectionState InternetIPv4State
    {
        get => _internetIPv4State;
        private set
        {
            if (value == _internetIPv4State)
                return;

            _internetIPv4State = value;
            OnPropertyChanged();
        }
    }

    private bool _isInternetIPv6Checking;

    public bool IsInternetIPv6Checking
    {
        get => _isInternetIPv6Checking;
        set
        {
            if (value == _isInternetIPv6Checking)
                return;

            _isInternetIPv6Checking = value;
            OnPropertyChanged();
        }
    }

    private string _internetIPv6;

    public string InternetIPv6
    {
        get => _internetIPv6;
        set
        {
            if (value == _internetIPv6)
                return;

            _internetIPv6 = value;
            OnPropertyChanged();
        }
    }

    private ConnectionState _internetIPv6State = ConnectionState.None;

    public ConnectionState InternetIPv6State
    {
        get => _internetIPv6State;
        private set
        {
            if (value == _internetIPv6State)
                return;

            _internetIPv6State = value;
            OnPropertyChanged();
        }
    }

    private bool _isInternetDNSChecking;

    public bool IsInternetDNSChecking
    {
        get => _isInternetDNSChecking;
        set
        {
            if (value == _isInternetDNSChecking)
                return;

            _isInternetDNSChecking = value;
            OnPropertyChanged();
        }
    }

    private string _internetDNS;

    public string InternetDNS
    {
        get => _internetDNS;
        set
        {
            if (value == _internetDNS)
                return;

            _internetDNS = value;
            OnPropertyChanged();
        }
    }

    private ConnectionState _internetDNSState = ConnectionState.None;

    public ConnectionState InternetDNSState
    {
        get => _internetDNSState;
        private set
        {
            if (value == _internetDNSState)
                return;

            _internetDNSState = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public bool CheckPublicIPAddressEnabled => SettingsManager.Current.Dashboard_CheckPublicIPAddress;

    #endregion

    #region Constructor, load settings

    public NetworkConnectionWidgetViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
    }

    #endregion

    #region ICommands & Actions

    public ICommand CheckViaHotkeyCommand => new RelayCommand(_ => CheckViaHotkeyAction());

    private void CheckViaHotkeyAction()
    {
        Check();
    }

    #endregion

    #region Methods

    public void Check()
    {
        CheckAsync().ConfigureAwait(false);
    }

    private CancellationTokenSource _cancellationTokenSource;
    private Task _checkTask = Task.CompletedTask;

    private async Task CheckAsync()
    {
        Log.Info("Checking network connection...");

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

        try
        {
            _checkTask = RunTask(_cancellationTokenSource.Token);
            await _checkTask;
        }
        catch (OperationCanceledException)
        {
            wasCanceled = true;
            Log.Info("Task was cancelled from current checks.");
        }
        finally
        {
            if (!wasCanceled)
                Log.Info("Network connection check completed.");
        }
    }

    private async Task RunTask(CancellationToken ct)
    {
        await Task.WhenAll(
            CheckConnectionComputerAsync(ct),
            CheckConnectionRouterAsync(ct),
            CheckConnectionInternetAsync(ct)
        );
    }

    private Task CheckConnectionComputerAsync(CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            Log.Debug("CheckConnectionComputerAsync - Checking local connection...");

            // Init variables
            IsComputerIPv4Checking = true;
            ComputerIPv4 = "";
            ComputerIPv4State = ConnectionState.None;

            IsComputerIPv6Checking = true;
            ComputerIPv6 = "";
            ComputerIPv6State = ConnectionState.None;

            IsComputerDNSChecking = true;
            ComputerDNS = "";
            ComputerDNSState = ConnectionState.None;

            // Detect local IPv4 address
            Log.Debug("CheckConnectionComputerAsync - Detecting local IPv4 address...");

            var detectedLocalIPv4Address =
                await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(
                    IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPv4Address));

            if (detectedLocalIPv4Address != null)
            {
                Log.Debug("CheckConnectionComputerAsync - Local IPv4 address detected: " + detectedLocalIPv4Address);

                ComputerIPv4 = detectedLocalIPv4Address.ToString();
                ComputerIPv4State = string.IsNullOrEmpty(ComputerIPv4) ? ConnectionState.Critical : ConnectionState.OK;
            }
            else
            {
                Log.Debug("CheckConnectionComputerAsync - Local IPv4 address not detected.");

                ComputerIPv4 = "-/-";
                ComputerIPv4State = ConnectionState.Critical;
            }

            IsComputerIPv4Checking = false;

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // Detect local IPv6 address
            Log.Debug("CheckConnectionComputerAsync - Detecting local IPv6 address...");

            var detectedLocalIPv6Address =
                await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(
                    IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPv6Address));

            if (detectedLocalIPv6Address != null)
            {
                Log.Debug("CheckConnectionComputerAsync - Local IPv6 address detected: " + detectedLocalIPv6Address);

                ComputerIPv6 = detectedLocalIPv6Address.ToString();
                ComputerIPv6State = string.IsNullOrEmpty(ComputerIPv6) ? ConnectionState.Critical : ConnectionState.OK;
            }
            else
            {
                Log.Debug("CheckConnectionComputerAsync - Local IPv6 address not detected.");

                ComputerIPv6 = "-/-";
                ComputerIPv6State = ConnectionState.Critical;
            }

            IsComputerIPv6Checking = false;

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // Try to resolve local DNS based on IPv4
            if (ComputerIPv4State == ConnectionState.OK)
            {
                Log.Debug("CheckConnectionComputerAsync - Resolving local DNS based on IPv4...");

                var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(IPAddress.Parse(ComputerIPv4));

                if (!dnsResult.HasError)
                {
                    Log.Debug("CheckConnectionComputerAsync - Local DNS based on IPv4 resolved: " + dnsResult.Value);

                    ComputerDNS = dnsResult.Value;
                    ComputerDNSState = ConnectionState.OK;
                }
                else
                {
                    Log.Debug("CheckConnectionComputerAsync - Local DNS based on IPv4 not resolved.");
                }
            }
            else
            {
                Log.Debug("CheckConnectionComputerAsync - Local DNS based on IPv4 not resolved due to invalid IPv4 address.");
            }

            // Try to resolve local DNS based on IPv6 if IPv4 failed
            if (string.IsNullOrEmpty(ComputerDNS) && ComputerIPv6State == ConnectionState.OK)
            {
                Log.Debug("CheckConnectionComputerAsync - Resolving local DNS based on IPv6...");

                var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(IPAddress.Parse(ComputerIPv6));

                if (!dnsResult.HasError)
                {
                    Log.Debug("CheckConnectionComputerAsync - Local DNS based on IPv6 resolved: " + dnsResult.Value);

                    ComputerDNS = dnsResult.Value;
                    ComputerDNSState = ConnectionState.OK;
                }
                else
                {
                    Log.Debug("CheckConnectionComputerAsync - Local DNS based on IPv6 not resolved.");
                }
            }
            else
            {
                Log.Debug("CheckConnectionComputerAsync - Local DNS based on IPv6 not resolved due to IPv4 DNS resolved or invalid IPv6 address");
            }

            if (string.IsNullOrEmpty(ComputerDNS))
            {
                ComputerDNS = "-/-";
                ComputerDNSState = ConnectionState.Critical;
            }

            IsComputerDNSChecking = false;

            Log.Debug("CheckConnectionComputerAsync - Local connection check completed.");
        }, ct);
    }

    private Task CheckConnectionRouterAsync(CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            Log.Debug("CheckConnectionRouterAsync - Checking router connection...");

            // Init variables
            IsRouterIPv4Checking = true;
            RouterIPv4 = "";
            RouterIPv4State = ConnectionState.None;
            IsRouterIPv6Checking = true;
            RouterIPv6 = "";
            RouterIPv6State = ConnectionState.None;
            IsRouterDNSChecking = true;
            RouterDNS = "";
            RouterDNSState = ConnectionState.None;

            // Detect router IPv4 and if it is reachable
            Log.Debug("CheckConnectionRouterAsync - Detecting computer and router IPv4 address...");

            var detectedLocalIPv4Address =
                await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(
                    IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPv4Address));

            if (detectedLocalIPv4Address != null)
            {
                Log.Debug("CheckConnectionRouterAsync - Computer IPv4 address detected: " + detectedLocalIPv4Address);

                var detectedRouterIPv4 =
                    await NetworkInterface.DetectGatewayBasedOnLocalIPAddressAsync(
                        detectedLocalIPv4Address);

                if (detectedRouterIPv4 != null)
                {
                    Log.Debug("CheckConnectionRouterAsync - Router IPv4 address detected: " + detectedRouterIPv4);

                    RouterIPv4 = detectedRouterIPv4.ToString();
                    RouterIPv4State = string.IsNullOrEmpty(RouterIPv4) ? ConnectionState.Critical : ConnectionState.OK;
                }
                else
                {
                    Log.Debug("CheckConnectionRouterAsync - Router IPv4 address not detected.");

                    RouterIPv4 = "-/-";
                    RouterIPv4State = ConnectionState.Critical;
                }
            }
            else
            {
                Log.Debug("CheckConnectionRouterAsync - Computer IPv4 address not detected.");

                RouterIPv4 = "-/-";
                RouterIPv4State = ConnectionState.Critical;
            }

            IsRouterIPv4Checking = false;

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // Detect router IPv6 and if it is reachable
            Log.Debug("CheckConnectionRouterAsync - Detecting computer and router IPv6 address...");

            var detectedComputerIPv6 =
                await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(
                    IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPv6Address));

            if (detectedComputerIPv6 != null)
            {
                Log.Debug("CheckConnectionRouterAsync - Computer IPv6 address detected: " + detectedComputerIPv6);

                var detectedRouterIPv6 =
                    await NetworkInterface.DetectGatewayBasedOnLocalIPAddressAsync(detectedComputerIPv6);

                if (detectedRouterIPv6 != null)
                {
                    Log.Debug("CheckConnectionRouterAsync - Router IPv6 address detected: " + detectedRouterIPv6);

                    RouterIPv6 = detectedRouterIPv6.ToString();
                    RouterIPv6State = string.IsNullOrEmpty(RouterIPv6) ? ConnectionState.Critical : ConnectionState.OK;
                }
                else
                {
                    Log.Debug("CheckConnectionRouterAsync - Router IPv6 address not detected.");

                    RouterIPv6 = "-/-";
                    RouterIPv6State = ConnectionState.Critical;
                }
            }
            else
            {
                Log.Debug("CheckConnectionRouterAsync - Computer IPv6 address not detected.");

                RouterIPv6 = "-/-";
                RouterIPv6State = ConnectionState.Critical;
            }

            IsRouterIPv6Checking = false;

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // Try to resolve router DNS based on IPv4
            if (RouterIPv4State == ConnectionState.OK)
            {
                Log.Debug("CheckConnectionRouterAsync - Resolving router DNS based on IPv4...");

                var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(IPAddress.Parse(RouterIPv4));

                if (!dnsResult.HasError)
                {
                    Log.Debug("CheckConnectionRouterAsync - Router DNS based on IPv4 resolved: " + dnsResult.Value);

                    RouterDNS = dnsResult.Value;
                    RouterDNSState = ConnectionState.OK;
                }
                else
                {
                    Log.Debug("CheckConnectionRouterAsync - Router DNS based on IPv4 not resolved.");
                }
            }
            else
            {
                Log.Debug("CheckConnectionRouterAsync - Router DNS based on IPv4 not resolved due to invalid IPv4 address.");
            }

            // Try to resolve router DNS based on IPv6 if IPv4 failed
            if (string.IsNullOrEmpty(RouterDNS) && RouterIPv6State == ConnectionState.OK)
            {
                Log.Debug("CheckConnectionRouterAsync - Resolving router DNS based on IPv6...");

                var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(IPAddress.Parse(RouterIPv6));

                if (!dnsResult.HasError)
                {
                    Log.Debug("CheckConnectionRouterAsync - Router DNS based on IPv6 resolved: " + dnsResult.Value);

                    RouterDNS = dnsResult.Value;
                    RouterDNSState = ConnectionState.OK;
                }
                else
                {
                    Log.Debug("CheckConnectionRouterAsync - Router DNS based on IPv6 not resolved.");
                }
            }
            else
            {
                Log.Debug("CheckConnectionRouterAsync - Router DNS based on IPv6 not resolved due to IPv4 DNS resolved or invalid IPv6 address.");
            }

            if (string.IsNullOrEmpty(RouterDNS))
            {
                RouterDNS = "-/-";
                RouterDNSState = ConnectionState.Critical;
            }

            IsRouterDNSChecking = false;

            Log.Debug("CheckConnectionRouterAsync - Router connection check completed.");
        }, ct);
    }

    private Task CheckConnectionInternetAsync(CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            // If public IP address check is disabled
            if (!CheckPublicIPAddressEnabled)
                return;

            Log.Debug("CheckConnectionInternetAsync - Checking internet connection...");

            // Init variables
            IsInternetIPv4Checking = true;
            InternetIPv4 = "";
            InternetIPv4State = ConnectionState.None;
            IsInternetIPv6Checking = true;
            InternetIPv6 = "";
            InternetIPv6State = ConnectionState.None;
            IsInternetDNSChecking = true;
            InternetDNS = "";
            InternetDNSState = ConnectionState.None;

            // Detect public IPv4 and if it is reachable
            Log.Debug("Detecting public IPv4 address...");

            var publicIPv4AddressAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv4AddressAPI
                ? SettingsManager.Current.Dashboard_CustomPublicIPv4AddressAPI
                : GlobalStaticConfiguration.Dashboard_PublicIPv4AddressAPI;

            try
            {
                Log.Debug("CheckConnectionInternetAsync - Checking public IPv4 address from: " + publicIPv4AddressAPI);

                HttpClient httpClient = new();
                var httpResponse = await httpClient.GetAsync(publicIPv4AddressAPI, ct);

                var result = await httpResponse.Content.ReadAsStringAsync(ct);

                var match = Regex.Match(result, RegexHelper.IPv4AddressExtractRegex);

                if (match.Success)
                {
                    Log.Debug("CheckConnectionInternetAsync - Public IPv4 address detected: " + match.Value);

                    InternetIPv4 = match.Value;
                    InternetIPv4State = ConnectionState.OK;
                }
                else
                {
                    Log.Debug("CheckConnectionInternetAsync - Public IPv4 address not detected due to invalid format.");

                    InternetIPv4 = "-/-";
                    InternetIPv4State = ConnectionState.Critical;
                }
            }
            catch
            {
                Log.Debug("CheckConnectionInternetAsync - Public IPv4 address not detected due to exception.");

                InternetIPv4 = "-/-";
                InternetIPv4State = ConnectionState.Critical;
            }

            IsInternetIPv4Checking = false;

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // Detect public IPv6 and if it is reachable
            Log.Debug("CheckConnectionInternetAsync - Detecting public IPv6 address...");

            var publicIPv6AddressAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv6AddressAPI
                ? SettingsManager.Current.Dashboard_CustomPublicIPv6AddressAPI
                : GlobalStaticConfiguration.Dashboard_PublicIPv6AddressAPI;

            try
            {
                Log.Debug("CheckConnectionInternetAsync - Checking public IPv6 address from: " + publicIPv6AddressAPI);

                HttpClient httpClient = new();
                var httpResponse = await httpClient.GetAsync(publicIPv6AddressAPI, ct);

                var result = await httpResponse.Content.ReadAsStringAsync(ct);

                var match = Regex.Match(result, RegexHelper.IPv6AddressRegex);

                if (match.Success)
                {
                    Log.Debug("CheckConnectionInternetAsync - Public IPv6 address detected: " + match.Value);

                    InternetIPv6 = match.Value;
                    InternetIPv6State = ConnectionState.OK;
                }
                else
                {
                    Log.Debug("CheckConnectionInternetAsync - Public IPv6 address not detected due to invalid format.");

                    InternetIPv6 = "-/-";
                    InternetIPv6State = ConnectionState.Critical;
                }
            }
            catch
            {
                Log.Debug("CheckConnectionInternetAsync - Public IPv6 address not detected due to exception.");

                InternetIPv6 = "-/-";
                InternetIPv6State = ConnectionState.Critical;
            }

            IsInternetIPv6Checking = false;

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            // Try to resolve public DNS based on IPv4
            if (InternetIPv4State == ConnectionState.OK)
            {
                Log.Debug("CheckConnectionInternetAsync - Resolving public DNS based on IPv4...");

                var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(IPAddress.Parse(InternetIPv4));

                if (!dnsResult.HasError)
                {
                    Log.Debug("CheckConnectionInternetAsync - Public DNS based on IPv4 resolved: " + dnsResult.Value);

                    InternetDNS = dnsResult.Value;
                    InternetDNSState = ConnectionState.OK;
                }
                else
                {
                    Log.Debug("CheckConnectionInternetAsync - Public DNS based on IPv4 not resolved.");
                }
            }
            else
            {
                Log.Debug("CheckConnectionInternetAsync - Public DNS based on IPv4 not resolved due to invalid IPv4 address.");
            }

            // Try to resolve public DNS based on IPv6 if IPv4 failed
            if (string.IsNullOrEmpty(InternetDNS) && InternetIPv6State == ConnectionState.OK)
            {
                Log.Debug("CheckConnectionInternetAsync - Resolving public DNS based on IPv6...");

                var dnsResult = await DNSClient.GetInstance().ResolvePtrAsync(IPAddress.Parse(InternetIPv6));

                if (!dnsResult.HasError)
                {
                    Log.Debug("CheckConnectionInternetAsync - Public DNS based on IPv6 resolved: " + dnsResult.Value);

                    InternetDNS = dnsResult.Value;
                    InternetDNSState = ConnectionState.OK;
                }
                else
                {
                    Log.Debug("CheckConnectionInternetAsync - Public DNS based on IPv6 not resolved.");
                }
            }
            else
            {
                Log.Debug("CheckConnectionInternetAsync - Public DNS based on IPv6 not resolved due to IPv4 DNS resolved or invalid IPv6 address.");
            }

            if (string.IsNullOrEmpty(InternetDNS))
            {
                InternetDNS = "-/-";
                InternetDNSState = ConnectionState.Critical;
            }

            IsInternetDNSChecking = false;

            Log.Debug("CheckConnectionInternetAsync - Internet connection check completed.");
        }, ct);
    }
    #endregion
}
