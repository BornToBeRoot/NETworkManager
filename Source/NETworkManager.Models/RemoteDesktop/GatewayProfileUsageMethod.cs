namespace NETworkManager.Models.RemoteDesktop;

/// <summary>
///     The RD Gateway profile usage method.
/// </summary>
public enum GatewayProfileUsageMethod : uint
{
    /// <summary>
    ///     Use the default profile mode, as specified by the administrator.
    /// </summary>
    Default = 0,

    /// <summary>
    ///     Use explicit settings, as specified by the user.
    /// </summary>
    Explicit = 1
}