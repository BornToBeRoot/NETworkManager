using System.Collections.Generic;
using NETworkManager.Models.IPApi;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of an <see cref="IPGeolocationInfo" />.
    /// </summary>
    private static readonly ExportField<IPGeolocationInfo>[] IPGeolocationInfoExportFields =
    [
        new(nameof(IPGeolocationInfo.Status), x => x.Status),
        new(nameof(IPGeolocationInfo.Continent), x => x.Continent, true),
        new(nameof(IPGeolocationInfo.ContinentCode), x => x.ContinentCode),
        new(nameof(IPGeolocationInfo.Country), x => x.Country, true),
        new(nameof(IPGeolocationInfo.CountryCode), x => x.CountryCode),
        new(nameof(IPGeolocationInfo.Region), x => x.Region),
        new(nameof(IPGeolocationInfo.RegionName), x => x.RegionName, true),
        new(nameof(IPGeolocationInfo.City), x => x.City, true),
        new(nameof(IPGeolocationInfo.District), x => x.District, true),
        new(nameof(IPGeolocationInfo.Zip), x => x.Zip),
        new(nameof(IPGeolocationInfo.Lat), x => x.Lat),
        new(nameof(IPGeolocationInfo.Lon), x => x.Lon),
        new(nameof(IPGeolocationInfo.Timezone), x => x.Timezone),
        // Seconds (offset of the time zone to UTC)
        new(nameof(IPGeolocationInfo.Offset), x => x.Offset),
        new(nameof(IPGeolocationInfo.Currency), x => x.Currency),
        new(nameof(IPGeolocationInfo.Isp), x => x.Isp, true),
        new(nameof(IPGeolocationInfo.Org), x => x.Org, true),
        new(nameof(IPGeolocationInfo.As), x => x.As, true),
        new(nameof(IPGeolocationInfo.Asname), x => x.Asname, true),
        new(nameof(IPGeolocationInfo.Reverse), x => x.Reverse, true),
        new(nameof(IPGeolocationInfo.Mobile), x => x.Mobile),
        new(nameof(IPGeolocationInfo.Proxy), x => x.Proxy),
        new(nameof(IPGeolocationInfo.Hosting), x => x.Hosting),
        new(nameof(IPGeolocationInfo.Query), x => x.Query)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="IPGeolocationInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{IPGeolocationInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<IPGeolocationInfo> collection)
    {
        Export(filePath, fileType, collection, IPGeolocationInfoExportFields, ApplicationName.IPGeolocation);
    }
}
