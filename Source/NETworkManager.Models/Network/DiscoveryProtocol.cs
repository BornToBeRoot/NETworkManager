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
            using (var powerShell = System.Management.Automation.PowerShell.Create())
            {
                powerShell.AddScript("Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process");
                powerShell.AddScript("Import-Module NetAdapter");
                powerShell.AddScript(_psDiscoveryProtocolModule);
                powerShell.AddScript($"Invoke-DiscoveryProtocolCapture -Duration {duration}" +
                                     (protocol != DiscoveryProtocol.LldpCdp
                                         ? $" -Type {protocol.ToString().ToUpper()}"
                                         : "") + " -Force | Get-DiscoveryProtocolData");

                var psOutputs = powerShell.Invoke();

                if (powerShell.Streams.Error.Count > 0)
                {
                    StringBuilder stringBuilder = new();

                    foreach (var error in powerShell.Streams.Error)
                    {
                        if (string.IsNullOrEmpty(stringBuilder.ToString()))
                            stringBuilder.Append(Environment.NewLine);

                        stringBuilder.Append(error.Exception.Message);
                    }

                    OnErrorReceived(new DiscoveryProtocolErrorArgs(stringBuilder.ToString()));
                }

                if (powerShell.Streams.Warning.Count > 0)
                {
                    StringBuilder stringBuilder = new();

                    foreach (var warning in powerShell.Streams.Warning)
                    {
                        if (string.IsNullOrEmpty(stringBuilder.ToString()))
                            stringBuilder.Append(Environment.NewLine);

                        stringBuilder.Append(warning.Message);
                    }

                    OnWarningReceived(new DiscoveryProtocolWarningArgs(stringBuilder.ToString()));
                }

                foreach (var outputItem in psOutputs)
                {
                    if (outputItem == null)
                        continue;

                    List<string> ipAddresses = new();

                    if (outputItem.Properties["IPAddress"] != null)
                        ipAddresses.AddRange(outputItem.Properties["IPAddress"].Value as List<string>);

                    List<string> managements = new();

                    if (outputItem.Properties["Management"] != null)
                        managements.AddRange(outputItem.Properties["Management"].Value as List<string>);

                    var packageInfo = new DiscoveryProtocolPackageInfo
                    {
                        Device = outputItem.Properties["Device"]?.Value.ToString(),
                        DeviceDescription = outputItem.Properties["SystemDescription"]?.Value.ToString(),
                        Port = outputItem.Properties["Port"]?.Value.ToString(),
                        PortDescription = outputItem.Properties["PortDescription"]?.Value.ToString(),
                        Model = outputItem.Properties["Model"]?.Value.ToString(),
                        IPAddress = string.Join("; ", ipAddresses),
                        VLAN = outputItem.Properties["VLAN"]?.Value.ToString(),
                        Protocol = outputItem.Properties["Type"]?.Value.ToString(),
                        TimeToLive = outputItem.Properties["TimeToLive"]?.Value.ToString(),
                        Management = string.Join("; ", managements),
                        ChassisId = outputItem.Properties["ChassisId"]?.Value.ToString(),
                        LocalConnection = outputItem.Properties["Connection"]?.Value.ToString(),
                        LocalInterface = outputItem.Properties["Interface"]?.Value.ToString()
                    };

                    OnPackageReceived(new DiscoveryProtocolPackageArgs(packageInfo));
                }
            }

            OnComplete();
        });
    }

    #endregion
}