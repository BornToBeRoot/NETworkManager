using System;
using System.Collections.Generic;
using NETworkManager.Models.IPApi;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="TracerouteHopInfo" />. The times (ms) are empty without a reply, the IP
    ///     geolocation values without a geolocation.
    /// </summary>
    private static readonly ExportField<TracerouteHopInfo>[] TracerouteHopInfoExportFields =
    [
        new(nameof(TracerouteHopInfo.Hop), x => x.Hop),
        new(nameof(TracerouteHopInfo.Status1), x => x.Status1),
        new(nameof(TracerouteHopInfo.Time1), x => HasPingReply(x.Status1) ? x.Time1 : null),
        new(nameof(TracerouteHopInfo.Status2), x => x.Status2),
        new(nameof(TracerouteHopInfo.Time2), x => HasPingReply(x.Status2) ? x.Time2 : null),
        new(nameof(TracerouteHopInfo.Status3), x => x.Status3),
        new(nameof(TracerouteHopInfo.Time3), x => HasPingReply(x.Status3) ? x.Time3 : null),
        new(nameof(TracerouteHopInfo.IPAddress), x => x.IPAddress),
        new(nameof(TracerouteHopInfo.Hostname), x => x.Hostname, true),
        new(nameof(IPGeolocationInfo.Continent), IfGeolocation(x => x.Continent), true),
        new(nameof(IPGeolocationInfo.Country), IfGeolocation(x => x.Country), true),
        new(nameof(IPGeolocationInfo.Region), IfGeolocation(x => x.Region)),
        new(nameof(IPGeolocationInfo.RegionName), IfGeolocation(x => x.RegionName), true),
        new(nameof(IPGeolocationInfo.City), IfGeolocation(x => x.City), true),
        new(nameof(IPGeolocationInfo.District), IfGeolocation(x => x.District), true),
        new(nameof(IPGeolocationInfo.Isp), IfGeolocation(x => x.Isp), true),
        new(nameof(IPGeolocationInfo.Org), IfGeolocation(x => x.Org), true),
        new(nameof(IPGeolocationInfo.As), IfGeolocation(x => x.As), true),
        new(nameof(IPGeolocationInfo.Asname), IfGeolocation(x => x.Asname), true),
        new(nameof(IPGeolocationInfo.Hosting), IfGeolocation(x => x.Hosting)),
        new(nameof(IPGeolocationInfo.Proxy), IfGeolocation(x => x.Proxy)),
        new(nameof(IPGeolocationInfo.Mobile), IfGeolocation(x => x.Mobile))
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="TracerouteHopInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{TracerouteHopInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType,
        IReadOnlyList<TracerouteHopInfo> collection)
    {
        Export(filePath, fileType, collection, TracerouteHopInfoExportFields, ApplicationName.Traceroute);
    }

    /// <summary>
    ///     Gets a value of the IP geolocation of a hop, null without a geolocation.
    /// </summary>
    /// <param name="value">Value of the IP geolocation.</param>
    private static Func<TracerouteHopInfo, object> IfGeolocation(Func<IPGeolocationInfo, object> value)
    {
        return x => x.IPGeolocationResult?.Info is { } info ? value(info) : null;
    }
}
