using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace NETworkManager.Models.Network
{
    public class DiscoveryProtocol
    {
        private string DiscoveryScript = string.Empty;

        public DiscoveryProtocol()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NETworkManager.Scripts.DiscoveryProtocol.ps1"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    DiscoveryScript = reader.ReadToEnd();
                }
            }
        }

        #region Methods
        public Task<DiscoveryProtocolInfo> GetDiscoveryProtocolAsync(string netAdapter, int duration, Protocol protocol)
        {
            return Task.Run(() => GetDiscoveryProtocol(netAdapter, duration, protocol));
        }

        public DiscoveryProtocolInfo GetDiscoveryProtocol(string netAdapter, int duration, Protocol protocol)
        {
            using (System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create())
            {
                powerShell.AddScript(DiscoveryScript);
                powerShell.AddScript($"Invoke-DiscoveryProtocolCapture -NetAdapter \"{netAdapter}\" -Duration {duration}" + (protocol != Protocol.LLDP_CDP ? $" -Type {protocol.ToString()}" : "") + "| Get-DiscoveryProtocolData");

                Collection<PSObject> PSOutput = powerShell.Invoke();

                if (powerShell.Streams.Error.Count > 0)
                {
                    foreach (var error in powerShell.Streams.Error)
                        Debug.WriteLine("Error: " + error.Exception.Message);
                }
                if (powerShell.Streams.Warning.Count > 0)
                {
                    foreach (var warning in powerShell.Streams.Warning)
                        Debug.WriteLine("Warning: " + warning.Message);
                }

                foreach (PSObject outputItem in PSOutput)
                {
                    if (outputItem != null)
                    {
                        return new DiscoveryProtocolInfo()
                        {
                            Device = outputItem.Properties["Device"]?.Value.ToString(),
                            Port = outputItem.Properties["Port"]?.Value.ToString(),
                            Description = outputItem.Properties["Description"]?.Value.ToString(),
                            Model = outputItem.Properties["Model"]?.Value.ToString(),
                            VLAN = outputItem.Properties["VLAN"]?.Value.ToString(),
                            IPAddress = outputItem.Properties["IPAddress"]?.Value.ToString(),
                            Protocol = outputItem.Properties["Type"]?.Value.ToString(),
                            Time = outputItem.Properties["TimeCreated"]?.Value.ToString(),
                        };
                    }
                }

                return null;
            }
        }
        #endregion

        #region Enum
        public enum Protocol
        {
            LLDP_CDP,
            LLDP,
            CDP
        }

        #endregion
    }
}
