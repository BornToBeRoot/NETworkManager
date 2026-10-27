using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows.Data;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
/// Formats the number of open ports of a Port Scanner group (one host) with the format string bound as the fourth
/// value (e.g. <c>Strings.XOpen</c> = "{0} Open", so the word order can be translated).
/// </summary>
/// <remarks>
/// Bound as a <see cref="MultiBinding"/> with the <see cref="CollectionViewGroup"/> as the first value, its
/// <see cref="CollectionViewGroup.ItemCount"/> as the second and the open ports per IP address (counted by the view
/// model when a row is added) as the third. The second value isn't used directly, it only forces re-evaluation when a
/// port is added to (or removed from) the group while the scan is running. The rows of the group are not counted
/// here: that would take rows x rows for a large group (e.g. 1-65535 with closed ports shown).
/// </remarks>
public sealed class PortScannerGroupOpenPortsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3 || values[0] is not CollectionViewGroup group ||
            values[2] is not IReadOnlyDictionary<IPAddress, int> openPortsPerHost)
            return string.Empty;

        // One host per group
        var open = group.Items.FirstOrDefault() is PortScannerPortInfo info
            ? openPortsPerHost.GetValueOrDefault(info.IPAddress)
            : 0;

        var format = values.Length > 3 ? values[3] as string ?? "{0}" : "{0}";

        return string.Format(culture, format, open);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
