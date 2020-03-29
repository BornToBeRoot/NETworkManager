namespace NETworkManager.Models.PowerShell
{
    public partial class PowerShell
    {
        public enum ExecutionPolicy
        {
            Restricted,
            AllSigned,
            RemoteSigned,
            Unrestricted,
            Bypass
        }
    }
}
