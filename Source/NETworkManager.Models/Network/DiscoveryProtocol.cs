using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class to capture network discovery protocol packages.
/// </summary>
public class DiscoveryProtocolCapture
{
    /// <summary>
    ///     Holds the PowerShell script which is loaded when the class is initialized.
    /// </summary>
    private readonly string _psDiscoveryProtocolModule;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DiscoveryProtocol" /> class.
    /// </summary>
    public DiscoveryProtocolCapture()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("NETworkManager.Models.Resources.PSDiscoveryProtocol.psm1");

        using StreamReader reader =
            new(stream ?? throw new InvalidOperationException("Could not load PSDiscoveryProtocol.psm1"));

        _psDiscoveryProtocolModule = reader.ReadToEnd();
    }

    /// <summary>
    ///     Is triggered when a network package with a discovery protocol is received.
    /// </summary>
    public event EventHandler<DiscoveryProtocolPackageArgs> PackageReceived;

    /// <summary>
    ///     Triggers the <see cref="PackageReceived" /> event.
    /// </summary>
    /// <param name="e">Passes <see cref="DiscoveryProtocolPackageArgs" /> to the event.</param>
    protected virtual void OnPackageReceived(DiscoveryProtocolPackageArgs e)
    {
        PackageReceived?.Invoke(this, e);
    }

    /// <summary>
    ///     Is triggered when an error occurs during the capturing.
    /// </summary>
    public event EventHandler<DiscoveryProtocolErrorArgs> ErrorReceived;


    /// <summary>
    ///     Triggers the <see cref="ErrorReceived" /> event.
    /// </summary>
    /// <param name="e">Passes <see cref="DiscoveryProtocolErrorArgs" /> to the event.</param>
    protected virtual void OnErrorReceived(DiscoveryProtocolErrorArgs e)
    {
        ErrorReceived?.Invoke(this, e);
    }

    /// <summary>
    ///     Is triggered when a warning occurs during the capturing.
    /// </summary>
    public event EventHandler<DiscoveryProtocolWarningArgs> WarningReceived;

    /// <summary>
    ///     Triggers the <see cref="WarningReceived" /> event.
    /// </summary>
    /// <param name="e">Passes <see cref="DiscoveryProtocolWarningArgs" /> to the event.</param>
    protected virtual void OnWarningReceived(DiscoveryProtocolWarningArgs e)
    {
        WarningReceived?.Invoke(this, e);
    }

    /// <summary>
    ///     Is triggered when the capturing is completed.
    /// </summary>
    public event EventHandler Complete;

    /// <summary>
    ///     Triggers the <see cref="Complete" /> event.
    /// </summary>
    protected virtual void OnComplete()
    {
        Complete?.Invoke(this, EventArgs.Empty);
    }

    #region Methods

    /// <summary>
    ///     Captures the network packets on the network adapter asynchronously for a certain period of time and filters the
    ///     packets according to the protocol.
    /// </summary>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="protocol"><see cref="DiscoveryProtocol" /> to filter on.</param>
    public void CaptureAsync(int duration, DiscoveryProtocol protocol)
    {
        Task.Run(() =>
        {
            using var ps = System.Management.Automation.PowerShell.Create();
            
            var typeParam = protocol != DiscoveryProtocol.LldpCdp ? $" -Type {protocol.ToString().ToUpper()}" : "";

            ps.AddScript($@"
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
Import-Module NetAdapter -ErrorAction Stop
{_psDiscoveryProtocolModule}
Invoke-DiscoveryProtocolCapture -Duration {duration}{typeParam} -Force | Get-DiscoveryProtocolData");

            var results = ps.Invoke();

            if (ps.Streams.Error.Count > 0)
            {
                StringBuilder stringBuilder = new();

                foreach (var error in ps.Streams.Error)
                {
                    if (string.IsNullOrEmpty(stringBuilder.ToString()))
                        stringBuilder.Append(Environment.NewLine);

                    stringBuilder.Append(error.Exception.Message);
                }

                OnErrorReceived(new DiscoveryProtocolErrorArgs(stringBuilder.ToString()));
            }

            if (ps.Streams.Warning.Count > 0)
            {
                StringBuilder stringBuilder = new();

                foreach (var warning in ps.Streams.Warning)
                {
                    if (string.IsNullOrEmpty(stringBuilder.ToString()))
                        stringBuilder.Append(Environment.NewLine);

                    stringBuilder.Append(warning.Message);
                }

                OnWarningReceived(new DiscoveryProtocolWarningArgs(stringBuilder.ToString()));
            }

            foreach (var result in results)
            {
                if (result == null)
                    continue;

                List<string> ipAddresses = [];

                if (result.Properties["IPAddress"] != null)
                    ipAddresses.AddRange(result.Properties["IPAddress"].Value as List<string>);

                List<string> managements = [];

                if (result.Properties["Management"] != null)
                    managements.AddRange(result.Properties["Management"].Value as List<string>);

                var packageInfo = new DiscoveryProtocolPackageInfo
                {
                    Device = result.Properties["Device"]?.Value.ToString(),
                    DeviceDescription = result.Properties["SystemDescription"]?.Value.ToString(),
                    Port = result.Properties["Port"]?.Value.ToString(),
                    PortDescription = result.Properties["PortDescription"]?.Value.ToString(),
                    Model = result.Properties["Model"]?.Value.ToString(),
                    IPAddress = string.Join("; ", ipAddresses),
                    VLAN = result.Properties["VLAN"]?.Value.ToString(),
                    Protocol = result.Properties["Type"]?.Value.ToString(),
                    TimeToLive = result.Properties["TimeToLive"]?.Value.ToString(),
                    Management = string.Join("; ", managements),
                    ChassisId = result.Properties["ChassisId"]?.Value.ToString(),
                    LocalConnection = result.Properties["Connection"]?.Value.ToString(),
                    LocalInterface = result.Properties["Interface"]?.Value.ToString()
                };

                OnPackageReceived(new DiscoveryProtocolPackageArgs(packageInfo));
            }

            OnComplete();
        });
    }

    #endregion
}