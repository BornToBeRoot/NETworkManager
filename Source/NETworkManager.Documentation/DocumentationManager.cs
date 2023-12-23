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
    private static List<DocumentationInfo> List => new()
    {
        new DocumentationInfo(DocumentationIdentifier.ApplicationDashboard, @"Documentation/Application/Dashboard"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationNetworkInterface,
            @"Documentation/Application/NetworkInterface"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationWiFi, @"Documentation/Application/WiFi"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationIPScanner, @"Documentation/Application/IPScanner"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationPortScanner, @"Documentation/Application/PortScanner"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationPingMonitor, @"Documentation/Application/PingMonitor"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationTraceroute, @"Documentation/Application/Traceroute"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationDnsLookup, @"Documentation/Application/DNSLookup"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationRemoteDesktop,
            @"Documentation/Application/RemoteDesktop"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationPowerShell, @"Documentation/Application/PowerShell"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationPutty, @"Documentation/Application/PuTTY"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationAWSSessionManager,
            @"Documentation/Application/AWSSessionManager"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationTigerVNC, @"Documentation/Application/TigerVNC"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationWebConsole, @"Documentation/Application/WebConsole"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationSnmp, @"Documentation/Application/SNMP"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationSntpLookup, @"Documentation/Application/SNTPLookup"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationDiscoveryProtocol,
            @"Documentation/Application/DiscoveryProtocol"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationWakeOnLan, @"Documentation/Application/WakeOnLAN"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationWhois, @"Documentation/Application/Whois"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationIPGeolocation,
            @"Documentation/Application/IPGeolocation"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationSubnetCalculator,
            @"Documentation/Application/SubnetCalculator"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationBitCalculator,
            @"Documentation/Application/BitCalculator"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationLookup, @"Documentation/Application/Lookup"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationConnections, @"Documentation/Application/Connection"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationListeners, @"Documentation/Application/Listeners"),
        new DocumentationInfo(DocumentationIdentifier.ApplicationArpTable, @"Documentation/Application/ARPTable"),
        new DocumentationInfo(DocumentationIdentifier.SettingsGeneral, @"Documentation/Settings/General"),
        new DocumentationInfo(DocumentationIdentifier.SettingsWindow, @"Documentation/Settings/Window"),
        new DocumentationInfo(DocumentationIdentifier.SettingsAppearance, @"Documentation/Settings/Appearance"),
        new DocumentationInfo(DocumentationIdentifier.SettingsLanguage, @"Documentation/Settings/Language"),
        new DocumentationInfo(DocumentationIdentifier.SettingsNetwork, @"Documentation/Settings/Network"),
        new DocumentationInfo(DocumentationIdentifier.SettingsStatus, @"Documentation/Settings/Status"),
        new DocumentationInfo(DocumentationIdentifier.SettingsHotKeys, @"Documentation/Settings/HotKeys"),
        new DocumentationInfo(DocumentationIdentifier.SettingsAutostart, @"Documentation/Settings/Autostart"),
        new DocumentationInfo(DocumentationIdentifier.SettingsUpdate, @"Documentation/Settings/Update"),
        new DocumentationInfo(DocumentationIdentifier.SettingsProfiles, @"Documentation/Settings/Profiles"),
        new DocumentationInfo(DocumentationIdentifier.SettingsSettings, @"Documentation/Settings/Settings"),
        new DocumentationInfo(DocumentationIdentifier.Profiles, @"Documentation/Profiles"),
        new DocumentationInfo(DocumentationIdentifier.CommandLineArguments, @"Documentation/CommandLineArguments")
    };

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