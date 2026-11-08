using System.Collections.Generic;
using NETworkManager.Models.Network;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="BitCaluclatorInfo" />.
    /// </summary>
    private static readonly ExportField<BitCaluclatorInfo>[] BitCaluclatorInfoExportFields =
    [
        new(nameof(BitCaluclatorInfo.Bits), x => x.Bits),
        new(nameof(BitCaluclatorInfo.Bytes), x => x.Bytes),
        new(nameof(BitCaluclatorInfo.Kilobits), x => x.Kilobits),
        new(nameof(BitCaluclatorInfo.Kilobytes), x => x.Kilobytes),
        new(nameof(BitCaluclatorInfo.Megabits), x => x.Megabits),
        new(nameof(BitCaluclatorInfo.Megabytes), x => x.Megabytes),
        new(nameof(BitCaluclatorInfo.Gigabits), x => x.Gigabits),
        new(nameof(BitCaluclatorInfo.Gigabytes), x => x.Gigabytes),
        new(nameof(BitCaluclatorInfo.Terabits), x => x.Terabits),
        new(nameof(BitCaluclatorInfo.Terabytes), x => x.Terabytes),
        new(nameof(BitCaluclatorInfo.Petabits), x => x.Petabits),
        new(nameof(BitCaluclatorInfo.Petabytes), x => x.Petabytes)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="BitCaluclatorInfo" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{BitCaluclatorInfo}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<BitCaluclatorInfo> collection)
    {
        Export(filePath, fileType, collection, BitCaluclatorInfoExportFields, ApplicationName.BitCalculator);
    }
}
