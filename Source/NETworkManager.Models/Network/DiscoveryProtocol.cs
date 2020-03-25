using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;
using System;
using System.Text;

namespace NETworkManager.Models.Network
{
    public partial class DiscoveryProtocol
    {
        private readonly string DiscoveryScript = string.Empty;

        public event EventHandler<DiscoveryProtocolPackageArgs> PackageReceived;

        protected virtual void OnPackageReceived(DiscoveryProtocolPackageArgs e)
        {
            PackageReceived?.Invoke(this, e);
        }

        public event EventHandler<DiscoveryProtocolErrorArgs> ErrorReceived;

        protected virtual void OnErrorReceived(DiscoveryProtocolErrorArgs e)
        {
            ErrorReceived?.Invoke(this, e);
        }

        public event EventHandler<DiscoveryProtocolWarningArgs> WarningReceived;

        protected virtual void OnWarningReceived(DiscoveryProtocolWarningArgs e)
        {
            WarningReceived?.Invoke(this, e);
        }

        public event EventHandler Complete;

        protected virtual void OnComplete()
        {
            Complete?.Invoke(this, EventArgs.Empty);
        }

        public DiscoveryProtocol()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NETworkManager.Models.Resources.DiscoveryProtocol.ps1"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    DiscoveryScript = reader.ReadToEnd();
                }
            }
        }

        #region Methods
        public void CaptureAsync(string netAdapter, int duration, Protocol protocol)
        {
            Task.Run(() =>
            {
                using (System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create())
                {
                    powerShell.AddScript(DiscoveryScript);
                    powerShell.AddScript($"Invoke-DiscoveryProtocolCapture -NetAdapter \"{netAdapter}\" -Duration {duration}" + (protocol != Protocol.LLDP_CDP ? $" -Type {protocol.ToString()}" : "") + "| Get-DiscoveryProtocolData");

                    Collection<PSObject> PSOutput = powerShell.Invoke();

                    if (powerShell.Streams.Error.Count > 0)
                    {
                        StringBuilder stringBuilder = new StringBuilder();

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
                        StringBuilder stringBuilder = new StringBuilder();

                        foreach (var warning in powerShell.Streams.Warning)
                        {
                            if (string.IsNullOrEmpty(stringBuilder.ToString()))
                                stringBuilder.Append(Environment.NewLine);

                            stringBuilder.Append(warning.Message);
                        }

                        OnWarningReceived(new DiscoveryProtocolWarningArgs(stringBuilder.ToString()));
                    }

                    foreach (PSObject outputItem in PSOutput)
                    {
                        if (outputItem != null)
                            OnPackageReceived(new DiscoveryProtocolPackageArgs(outputItem.Properties["Device"]?.Value.ToString(), outputItem.Properties["Port"]?.Value.ToString(), outputItem.Properties["Description"]?.Value.ToString(), outputItem.Properties["Model"]?.Value.ToString(), outputItem.Properties["VLAN"]?.Value.ToString(), outputItem.Properties["IPAddress"]?.Value.ToString(), outputItem.Properties["Type"]?.Value.ToString(), outputItem.Properties["TimeCreated"]?.Value.ToString()));
                    }
                }

                OnComplete();
            });
        }
        #endregion
    }

}