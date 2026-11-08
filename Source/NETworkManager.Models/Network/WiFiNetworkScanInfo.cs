using System;
using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains information about a WiFi network scan.
/// </summary>
public class WiFiNetworkScanInfo
{
    #region Variables

    /// <summary>
    ///     List of available WiFi networks on the network adapter.
    /// </summary>
    public List<WiFiNetworkInfo> WiFiNetworkInfos { get; init; }

    /// <summary>
    ///     Timestamp when the scan was performed.
    /// </summary>
    public DateTime Timestamp { get; init; }

    #endregion
}