using System.Collections.Generic;
using NETworkManager.Models.Firewall;

namespace NETworkManager.Models.Export;

public static partial class ExportManager
{
    /// <summary>
    ///     Exported fields of a <see cref="FirewallRule" />. Ports, addresses, network profiles, interface type and
    ///     program as shown (e.g. <c>80; 443</c>, empty if not restricted).
    /// </summary>
    private static readonly ExportField<FirewallRule>[] FirewallRuleExportFields =
    [
        new(nameof(FirewallRule.Name), x => x.Name, true),
        new(nameof(FirewallRule.IsEnabled), x => x.IsEnabled),
        new(nameof(FirewallRule.IsManaged), x => x.IsManaged),
        new(nameof(FirewallRule.Source), x => x.Source),
        new(nameof(FirewallRule.Direction), x => x.Direction),
        new(nameof(FirewallRule.Action), x => x.Action),
        new(nameof(FirewallRule.Protocol), x => x.Protocol),
        new(nameof(FirewallRule.LocalPorts), x => x.LocalPortsDisplay, true),
        new(nameof(FirewallRule.RemotePorts), x => x.RemotePortsDisplay, true),
        new(nameof(FirewallRule.LocalAddresses), x => x.LocalAddressesDisplay, true),
        new(nameof(FirewallRule.RemoteAddresses), x => x.RemoteAddressesDisplay, true),
        new(nameof(FirewallRule.NetworkProfiles), x => x.NetworkProfilesDisplay, true),
        new(nameof(FirewallRule.InterfaceType), x => x.InterfaceTypeDisplay, true),
        new(nameof(FirewallRule.Program), x => x.ProgramDisplay, true),
        new(nameof(FirewallRule.Description), x => x.Description, true)
    ];

    /// <summary>
    ///     Method to export objects from type <see cref="FirewallRule" /> to a file.
    /// </summary>
    /// <param name="filePath">Path to the export file.</param>
    /// <param name="fileType">Allowed <see cref="ExportFileType" /> are CSV, XML or JSON.</param>
    /// <param name="collection">Objects as <see cref="IReadOnlyList{FirewallRule}" /> to export.</param>
    public static void Export(string filePath, ExportFileType fileType, IReadOnlyList<FirewallRule> collection)
    {
        Export(filePath, fileType, collection, FirewallRuleExportFields, ApplicationName.Firewall);
    }
}
