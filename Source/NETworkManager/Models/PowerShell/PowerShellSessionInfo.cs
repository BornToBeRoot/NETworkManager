using static NETworkManager.Models.PowerShell.PowerShell;

namespace NETworkManager.Models.PowerShell
{
    public class PowerShellSessionInfo
    {
        public string ApplicationFilePath { get; set; }
        public bool EnableRemoteConsole { get; set; }
        public string Host { get; set; }
        public string AdditionalCommandLine { get; set; }
        public ExecutionPolicy ExecutionPolicy { get; set; }

        public PowerShellSessionInfo()
        {

        }
    }
}
