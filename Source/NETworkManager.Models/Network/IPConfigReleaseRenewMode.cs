namespace NETworkManager.Models.Network;

/// <summary>
/// Release or renew modes to control ipconfig.exe.
/// </summary>
public enum IPConfigReleaseRenewMode
{
    /// <summary>
    /// Release and renew IPv4.
    /// </summary>
    ReleaseRenew,
    
    /// <summary>
    /// Release and renew IPv6.
    /// </summary>
    ReleaseRenew6,

    /// <summary>
    /// Release IPv4.
    /// </summary>
    Release,

    /// <summary>
    /// Release IPv6.
    /// </summary>
    Release6,

    /// <summary>
    /// Renew IPv4.
    /// </summary>
    Renew,

    /// <summary>
    /// Renew IPv6.
    /// </summary>
    Renew6        
}
