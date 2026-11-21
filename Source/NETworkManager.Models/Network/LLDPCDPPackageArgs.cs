using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Event arguments when a discovery protocol package is returned.
/// </summary>
public class LLDPCDPPackageArgs : EventArgs
{
    /// <summary>
    ///     Creates a new instance of <see cref="LLDPCDPPackageArgs" /> with the given
    ///     <see cref="LLDPCDPPackageInfo" />.
    /// </summary>
    /// <param name="packageInfo">Discovery protocol package.</param>
    public LLDPCDPPackageArgs(LLDPCDPPackageInfo packageInfo)
    {
        PackageInfo = packageInfo;
    }

    /// <summary>
    ///     Discovery protocol package.
    /// </summary>
    public LLDPCDPPackageInfo PackageInfo { get; set; }
}