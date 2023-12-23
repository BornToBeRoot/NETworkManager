using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Event arguments when a discovery protocol package is returned.
/// </summary>
public class DiscoveryProtocolPackageArgs : EventArgs
{
    /// <summary>
    ///     Creates a new instance of <see cref="DiscoveryProtocolPackageArgs" /> with the given
    ///     <see cref="DiscoveryProtocolPackageInfo" />.
    /// </summary>
    /// <param name="packageInfo">Discovery protocol package.</param>
    public DiscoveryProtocolPackageArgs(DiscoveryProtocolPackageInfo packageInfo)
    {
        PackageInfo = packageInfo;
    }

    /// <summary>
    ///     Discovery protocol package.
    /// </summary>
    public DiscoveryProtocolPackageInfo PackageInfo { get; set; }
}