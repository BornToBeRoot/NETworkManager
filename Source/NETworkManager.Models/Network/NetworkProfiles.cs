namespace NETworkManager.Models.Network;

/// <summary>
/// Defines the network profile detected by Windows.
/// </summary>
public enum NetworkProfiles
{
    /// <summary>
    /// Network profile is not configured.
    /// </summary>
    NotConfigured = -1,

    /// <summary>
    /// Network has an Active Directory (AD) controller and you are authenticated.
    /// </summary>
    Domain,

    /// <summary>
    /// Network is private. Firewall will allow most connections.
    /// </summary>
    Private,

    /// <summary>
    /// Network is public. Firewall will block most connections.
    /// </summary>
    Public
}