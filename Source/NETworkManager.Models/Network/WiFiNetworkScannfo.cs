using System;
using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class contains information about a WiFi network scan.
/// </summary>
public class WiFiNetworkScanInfo
{
    #region Variables
    /// <summary>
    /// If of the network adapter who performed the scan.
    /// </summary>
    public Guid NetworkAdapterId { get; set; }

    /// <summary>
    /// List of available WiFi networks on the network adapter.
    /// </summary>
    public List<WiFiNetworkInfo> WiFiNetworkInfos  { get; init; }

    /// <summary>
    /// Timestamp when the scan was performed.
    /// </summary>
    public DateTime Timestamp { get; init; }
    #endregion

    /// <summary>
    /// Create an instance of <see cref="WiFiNetworkScanInfo"/>.
    /// </summary>
    public WiFiNetworkScanInfo()
    {
        
    }
}
