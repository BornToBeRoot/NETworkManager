namespace NETworkManager.Models.RemoteDesktop
{
    /// <summary>
    /// Specifies the RD Gateway server usage method.
    /// Docs: https://learn.microsoft.com/en-us/windows/win32/termserv/imsrdpclienttransportsettings-gatewayusagemethod
    /// </summary>
    public enum GatewayUsageMethod : uint
    {
        /// <summary>
        /// Do not use an RD Gateway server. In the Remote Desktop Connection (RDC) client UI, 
        /// the Bypass RD Gateway server for local addresses check box is cleared.
        /// </summary>
        NoneDirect = 0,

        /// <summary>
        /// Always use an RD Gateway server. In the RDC client UI, the Bypass RD Gateway server 
        /// for local addresses check box is cleared.
        /// </summary>
        Direct = 1,

        /// <summary>
        /// Use an RD Gateway server if a direct connection cannot be made to the RD Session 
        /// Host server. In the RDC client UI, the Bypass RD Gateway server for local addresses 
        /// check box is selected.
        /// </summary>
        Detect = 2,

        /// <summary>
        /// Use the default RD Gateway server settings.
        /// </summary>
        Default = 3,

        /// <summary>
        /// Do not use an RD Gateway server. In the RDC client UI, the Bypass RD Gateway server 
        /// for local addresses check box is selected.
        /// </summary>
        NoneDetect = 4
    }
}
