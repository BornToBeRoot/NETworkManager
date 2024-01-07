using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using NETworkManager.Models;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Documentation;

/// <summary>
///     This class is designed to interact with the documentation at https://borntoberoot.net/NETworkManager/.
/// </summary>
public static class DocumentationManager
{
    /// <summary>
    ///     Base path of the documentation.
    /// </summary>
    private const string DocumentationBaseUrl = @"https://borntoberoot.net/NETworkManager/";

    /// <summary>
    ///     List with all known documentation entries.
    /// </summary>
    private static IEnumerable<DocumentationInfo> List =>
    [
        new DocumentationInfo(DocumentationIdentifier.ApplicationDashboard,
            @"docs/application/dashboard"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationNetworkInterface,
            @"docs/application/network-interface"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationWiFi,
            @"docs/application/wifi"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationIPScanner,
            @"docs/application/ip-scanner"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationPortScanner,
            @"docs/application/port-scanner"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationPingMonitor,
            @"docs/application/ping-monitor"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationTraceroute,
            @"docs/application/traceroute"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationDnsLookup,
            @"docs/application/dns-lookup"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationRemoteDesktop,
            @"docs/application/remote-desktop"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationPowerShell,
            @"docs/application/powershell"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationPutty,
            @"docs/application/putty"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationAWSSessionManager,
            @"docs/application/aws-session-manager"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationTigerVNC,
            @"docs/application/tigervnc"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationWebConsole,
            @"docs/application/web-console"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationSnmp,
            @"docs/application/snmp"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationSntpLookup,
            @"docs/application/sntp-lookup"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationDiscoveryProtocol,
            @"docs/application/discovery-protocol"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationWakeOnLan,
            @"docs/application/wake-on-lan"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationWhois,
            @"docs/application/whois"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationIPGeolocation,
            @"docs/application/ip-geolocation"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationSubnetCalculator,
            @"docs/application/subnet-calculator"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationBitCalculator,
            @"docs/application/bit-calculator"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationLookup,
            @"docs/application/lookup"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationConnections,
            @"docs/application/connection"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationListeners,
            @"docs/application/listeners"),

        new DocumentationInfo(DocumentationIdentifier.ApplicationArpTable,
            @"docs/application/arp-table"),

        new DocumentationInfo(DocumentationIdentifier.SettingsGeneral,
            @"docs/settings/general"),

        new DocumentationInfo(DocumentationIdentifier.SettingsWindow,
            @"docs/settings/window"),

        new DocumentationInfo(DocumentationIdentifier.SettingsAppearance,
            @"docs/settings/appearance"),

        new DocumentationInfo(DocumentationIdentifier.SettingsLanguage,
            @"docs/settings/language"),

        new DocumentationInfo(DocumentationIdentifier.SettingsNetwork,
            @"docs/settings/network"),

        new DocumentationInfo(DocumentationIdentifier.SettingsStatus,
            @"docs/settings/status"),

        new DocumentationInfo(DocumentationIdentifier.SettingsHotKeys,
            @"docs/settings/hotkeys"),

        new DocumentationInfo(DocumentationIdentifier.SettingsAutostart,
            @"docs/settings/autostart"),

        new DocumentationInfo(DocumentationIdentifier.SettingsUpdate,
            @"docs/settings/update"),

        new DocumentationInfo(DocumentationIdentifier.SettingsProfiles,
            @"docs/settings/profiles"),

        new DocumentationInfo(DocumentationIdentifier.SettingsSettings,
            @"docs/settings/settings"),

        new DocumentationInfo(DocumentationIdentifier.Profiles,
            @"Documentation/profiles"),

        new DocumentationInfo(DocumentationIdentifier.CommandLineArguments,
            @"docs/commandline-arguments")
    ];

    /// <summary>
    ///     Command to open a documentation page based on <see cref="DocumentationIdentifier" />.
    /// </summary>
    public static ICommand OpenDocumentationCommand => new RelayCommand(OpenDocumentationAction);

    /// <summary>
    ///     Method to create the documentation url from <see cref="DocumentationIdentifier" />.
    /// </summary>
    /// <param name="documentationIdentifier">
    ///     <see cref="DocumentationIdentifier" /> of the documentation page you want to
    ///     open.
    /// </param>
    /// <returns>URL of the documentation page.</returns>
    private static string CreateUrl(DocumentationIdentifier documentationIdentifier)
    {
        var info = List.FirstOrDefault(x => x.Identifier == documentationIdentifier);

        var url = DocumentationBaseUrl;

        if (info != null)
            url += info.Path;

        return url;
    }

    /// <summary>
    ///     Method for opening a documentation page with the default web browser based on the
    ///     <see cref="DocumentationIdentifier" /> .
    /// </summary>
    /// <param name="documentationIdentifier">
    ///     <see cref="DocumentationIdentifier" /> of the documentation page you want to
    ///     open.
    /// </param>
    public static void OpenDocumentation(DocumentationIdentifier documentationIdentifier)
    {
        ExternalProcessStarter.OpenUrl(CreateUrl(documentationIdentifier));
    }

    /// <summary>
    ///     Method to open a documentation page based on <see cref="DocumentationIdentifier" />.
    /// </summary>
    /// <param name="documentationIdentifier"></param>
    private static void OpenDocumentationAction(object documentationIdentifier)
    {
        if (documentationIdentifier != null)
            OpenDocumentation((DocumentationIdentifier)documentationIdentifier);
    }

    /// <summary>
    ///     Method to get the <see cref="DocumentationIdentifier" /> from an <see cref="ApplicationName" />.
    /// </summary>
    /// <param name="name">
    ///     <see cref="ApplicationName" /> from which you want to get the <see cref="DocumentationIdentifier" />
    ///     .
    /// </param>
    /// <returns><see cref="DocumentationIdentifier" /> of the application.</returns>
    public static DocumentationIdentifier GetIdentifierByApplicationName(ApplicationName name)
    {
        return name switch
        {
            ApplicationName.Dashboard => DocumentationIdentifier.ApplicationDashboard,
            ApplicationName.NetworkInterface => DocumentationIdentifier.ApplicationNetworkInterface,
            ApplicationName.WiFi => DocumentationIdentifier.ApplicationWiFi,
            ApplicationName.IPScanner => DocumentationIdentifier.ApplicationIPScanner,
            ApplicationName.PortScanner => DocumentationIdentifier.ApplicationPortScanner,
            ApplicationName.PingMonitor => DocumentationIdentifier.ApplicationPingMonitor,
            ApplicationName.Traceroute => DocumentationIdentifier.ApplicationTraceroute,
            ApplicationName.DNSLookup => DocumentationIdentifier.ApplicationDnsLookup,
            ApplicationName.RemoteDesktop => DocumentationIdentifier.ApplicationRemoteDesktop,
            ApplicationName.PowerShell => DocumentationIdentifier.ApplicationPowerShell,
            ApplicationName.PuTTY => DocumentationIdentifier.ApplicationPutty,
            ApplicationName.AWSSessionManager => DocumentationIdentifier.ApplicationAWSSessionManager,
            ApplicationName.TigerVNC => DocumentationIdentifier.ApplicationTigerVNC,
            ApplicationName.WebConsole => DocumentationIdentifier.ApplicationWebConsole,
            ApplicationName.SNMP => DocumentationIdentifier.ApplicationSnmp,
            ApplicationName.SNTPLookup => DocumentationIdentifier.ApplicationSntpLookup,
            ApplicationName.DiscoveryProtocol => DocumentationIdentifier.ApplicationDiscoveryProtocol,
            ApplicationName.WakeOnLAN => DocumentationIdentifier.ApplicationWakeOnLan,
            ApplicationName.Whois => DocumentationIdentifier.ApplicationWhois,
            ApplicationName.IPGeolocation => DocumentationIdentifier.ApplicationIPGeolocation,
            ApplicationName.SubnetCalculator => DocumentationIdentifier.ApplicationSubnetCalculator,
            ApplicationName.BitCalculator => DocumentationIdentifier.ApplicationBitCalculator,
            ApplicationName.Lookup => DocumentationIdentifier.ApplicationLookup,
            ApplicationName.Connections => DocumentationIdentifier.ApplicationConnections,
            ApplicationName.Listeners => DocumentationIdentifier.ApplicationListeners,
            ApplicationName.ARPTable => DocumentationIdentifier.ApplicationArpTable,
            ApplicationName.None => DocumentationIdentifier.Default,
            _ => DocumentationIdentifier.Default
        };
    }

    /// <summary>
    ///     Method to get the <see cref="DocumentationIdentifier" /> from an <see cref="SettingsName" />.
    /// </summary>
    /// <param name="name"><see cref="SettingsName" /> from which you want to get the <see cref="DocumentationIdentifier" />.</param>
    /// <returns><see cref="DocumentationIdentifier" /> of the application or settings page.</returns>
    public static DocumentationIdentifier GetIdentifierBySettingsName(SettingsName name)
    {
        return name switch
        {
            SettingsName.General => DocumentationIdentifier.SettingsGeneral,
            SettingsName.Window => DocumentationIdentifier.SettingsWindow,
            SettingsName.Appearance => DocumentationIdentifier.SettingsAppearance,
            SettingsName.Language => DocumentationIdentifier.SettingsLanguage,
            SettingsName.Network => DocumentationIdentifier.SettingsNetwork,
            SettingsName.Status => DocumentationIdentifier.SettingsStatus,
            SettingsName.HotKeys => DocumentationIdentifier.SettingsHotKeys,
            SettingsName.Autostart => DocumentationIdentifier.SettingsAutostart,
            SettingsName.Update => DocumentationIdentifier.SettingsUpdate,
            SettingsName.Profiles => DocumentationIdentifier.SettingsProfiles,
            SettingsName.Settings => DocumentationIdentifier.SettingsSettings,
            SettingsName.Dashboard => GetIdentifierByApplicationName(ApplicationName.Dashboard),
            SettingsName.IPScanner => GetIdentifierByApplicationName(ApplicationName.IPScanner),
            SettingsName.PortScanner => GetIdentifierByApplicationName(ApplicationName.PortScanner),
            SettingsName.PingMonitor => GetIdentifierByApplicationName(ApplicationName.PingMonitor),
            SettingsName.Traceroute => GetIdentifierByApplicationName(ApplicationName.Traceroute),
            SettingsName.DNSLookup => GetIdentifierByApplicationName(ApplicationName.DNSLookup),
            SettingsName.RemoteDesktop => GetIdentifierByApplicationName(ApplicationName.RemoteDesktop),
            SettingsName.PowerShell => GetIdentifierByApplicationName(ApplicationName.PowerShell),
            SettingsName.PuTTY => GetIdentifierByApplicationName(ApplicationName.PuTTY),
            SettingsName.AWSSessionManager => GetIdentifierByApplicationName(ApplicationName.AWSSessionManager),
            SettingsName.TigerVNC => GetIdentifierByApplicationName(ApplicationName.TigerVNC),
            SettingsName.SNMP => GetIdentifierByApplicationName(ApplicationName.SNMP),
            SettingsName.SNTPLookup => GetIdentifierByApplicationName(ApplicationName.SNTPLookup),
            SettingsName.WakeOnLAN => GetIdentifierByApplicationName(ApplicationName.WakeOnLAN),
            SettingsName.BitCalculator => GetIdentifierByApplicationName(ApplicationName.BitCalculator),
            _ => DocumentationIdentifier.Default
        };
    }
}