using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETworkManager.Models.Firewall;

/// <summary>
/// Represents a security rule used within a firewall to control network traffic based on
/// specific conditions such as IP addresses, ports, and protocols.
/// </summary>
public class FirewallRule
{
    #region Variables

    /// <summary>
    /// Internal unique identifier of the rule (i.e. the value of <c>$rule.Id</c> in PowerShell).
    /// Used to target the rule in Set-NetFirewallRule / Remove-NetFirewallRule calls.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Indicates whether the rule is managed by NETworkManager (i.e. its display name starts with
    /// the NETworkManager prefix). Rules not managed by NETworkManager (e.g. Windows built-in rules)
    /// may have properties that are not supported by NETworkManager and can therefore not be edited.
    /// </summary>
    public bool IsManaged { get; set; }

    /// <summary>
    /// Where the rule comes from (e.g. local store or Group Policy).
    /// </summary>
    public FirewallRuleSource Source { get; set; } = FirewallRuleSource.Local;

    /// <summary>
    /// Indicates whether the rule is managed by a policy (e.g. Group Policy or MDM) or created by Windows at runtime,
    /// i.e. it does not come from the local store. Such rules can't be changed locally (only copied).
    /// </summary>
    public bool IsManagedByPolicy => Source != FirewallRuleSource.Local;

    /// <summary>
    /// Sort order of the rule by where it comes from: managed by NETworkManager (0), not managed by NETworkManager (1),
    /// Group Policy (2), MDM (3) or created by Windows at runtime (4). Used to sort the rule list, so rules managed
    /// by a policy are not mixed up with rules that are only not managed by NETworkManager.
    /// </summary>
    public int SourceSortOrder => IsManaged ? 0 : IsManagedByPolicy ? 1 + (int)Source : 1;

    /// <summary>
    /// Human-readable display name of the firewall rule. Used for display purposes
    /// or as an identifier in various contexts.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Indicates whether the firewall rule is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Represents a text-based explanation or information associated with an object.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Represents the communication protocol to be used in the network configuration.
    /// </summary>
    public FirewallProtocol Protocol { get; set; } = FirewallProtocol.TCP;

    /// <summary>
    /// Defines the direction of traffic impacted by the rule or configuration.
    /// </summary>
    public FirewallRuleDirection Direction { get; set; } = FirewallRuleDirection.Inbound;

    /// <summary>
    /// Represents the entry point and core execution logic for an application.
    /// </summary>
    public FirewallRuleProgram Program { get; set; }

    /// <summary>
    /// Local IP addresses or address specifiers (e.g. "192.168.1.0/24", "LocalSubnet").
    /// An empty list means "Any".
    /// </summary>
    public List<string> LocalAddresses
    {
        get;
        set
        {
            if (value is null)
            {
                field = [];
                return;
            }
            field = value;
        }
    } = [];

    /// <summary>
    /// Remote IP addresses or address specifiers (e.g. "10.0.0.0/8", "Internet").
    /// An empty list means "Any".
    /// </summary>
    public List<string> RemoteAddresses
    {
        get;
        set
        {
            if (value is null)
            {
                field = [];
                return;
            }
            field = value;
        }
    } = [];

    /// <summary>
    /// Defines the local ports associated with the firewall rule.
    /// </summary>
    public List<FirewallPortSpecification> LocalPorts
    {
        get;
        set
        {
            if (value is null)
            {
                field = [];
                return;
            }
            field = value;
        }
    } = [];

    /// <summary>
    /// Defines the range of remote ports associated with the firewall rule.
    /// </summary>
    public List<FirewallPortSpecification> RemotePorts
    {
        get;
        set
        {
            if (value is null)
            {
                field = [];
                return;
            }
            field = value;
        }
    } = [];

    /// <summary>
    /// Network profiles in order Domain, Private, Public.
    /// </summary>
    public bool[] NetworkProfiles
    {
        get;
        set
        {
            if (value?.Length is not 3)
                return;
            field = value;
        }
    } = new bool[3];

    public FirewallInterfaceType InterfaceType { get; set; } = FirewallInterfaceType.Any;

    /// <summary>
    /// Local port keywords (e.g. "RPC", "RPCEPMap", "IPHTTPSIn") used by Windows built-in rules.
    /// Not supported by NETworkManager (only shown).
    /// </summary>
    public List<string> LocalPortKeywords { get; set; } = [];

    /// <summary>
    /// Remote port keywords (e.g. "IPHTTPSOut") used by Windows built-in rules.
    /// Not supported by NETworkManager (only shown).
    /// </summary>
    public List<string> RemotePortKeywords { get; set; } = [];

    /// <summary>
    /// Raw interface type if the rule uses a combination of interface types (e.g. "Wired, Wireless"),
    /// which is not supported by NETworkManager; otherwise, <c>null</c>. <see cref="InterfaceType"/> is
    /// <see cref="FirewallInterfaceType.Any"/> in this case.
    /// </summary>
    public string InterfaceTypeRaw { get; set; }

    /// <summary>
    /// Properties that are not supported (and not shown) by NETworkManager, but restrict the traffic the rule
    /// applies to (e.g. ICMP types or a service, set in the Windows Firewall management console). They are kept
    /// when the rule is edited, but not when it is copied.
    /// </summary>
    public FirewallRuleHiddenProperties HiddenProperties { get; set; }

    /// <summary>
    /// Indicates whether the rule has <see cref="HiddenProperties"/>. A copy of the rule would apply to more traffic.
    /// </summary>
    public bool HasHiddenProperties => HiddenProperties != FirewallRuleHiddenProperties.None;

    /// <summary>
    /// Protocols supported by NETworkManager (e.g. available in the add / edit rule dialog).
    /// Other protocols (e.g. IPv6 = 41 or IGMP = 2) are only shown.
    /// </summary>
    public static IReadOnlyList<FirewallProtocol> SupportedProtocols { get; } =
    [
        FirewallProtocol.Any,
        FirewallProtocol.TCP,
        FirewallProtocol.UDP,
        FirewallProtocol.ICMPv4,
        FirewallProtocol.ICMPv6,
        FirewallProtocol.GRE,
        FirewallProtocol.L2TP
    ];

    /// <summary>
    /// Program keyword used by Windows built-in rules for the system process (e.g. File and Printer Sharing).
    /// Not supported by NETworkManager, because the program must be a path to an existing file.
    /// </summary>
    private const string SystemProgramKeyword = "System";

    /// <summary>
    /// Indicates whether the rule uses values that are not supported by NETworkManager
    /// (port keywords, a combination of interface types, a protocol that is not in <see cref="SupportedProtocols"/>
    /// or the program keyword <c>System</c>). Such rules can't be edited or copied, because these values
    /// would be lost and the new rule could apply to more traffic.
    /// </summary>
    public bool HasUnsupportedValues => LocalPortKeywords.Count > 0 ||
                                        RemotePortKeywords.Count > 0 ||
                                        InterfaceTypeRaw != null ||
                                        !SupportedProtocols.Contains(Protocol) ||
                                        string.Equals(Program?.Name, SystemProgramKeyword, System.StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Represents the operation to be performed or executed.
    /// </summary>
    public FirewallRuleAction Action { get; set; } = FirewallRuleAction.Block;

    #endregion

    #region Constructors

    /// <summary>
    /// Represents a rule within the firewall configuration.
    /// Used to control network traffic based on specified criteria, such as
    /// ports, protocols, the interface type, network profiles, and the used programs.  
    /// </summary>
    public FirewallRule()
    {

    }
    #endregion

    #region Display properties

    /// <summary>Program path, or <c>null</c> when no program restriction is set.</summary>
    public string ProgramDisplay => Program?.ToString();

    /// <summary>
    /// File name of the program (e.g. "putty.exe" for "%ProgramFiles%\PuTTY\putty.exe", or the keyword "System"),
    /// or <c>null</c> when no program restriction is set.
    /// </summary>
    public string ProgramFileName
    {
        get
        {
            var name = Program?.Name;

            if (string.IsNullOrEmpty(name))
                return null;

            return name[(name.LastIndexOfAny(['\\', '/']) + 1)..];
        }
    }

    /// <summary>Local addresses as a human-readable string (e.g. "192.168.1.0/24; LocalSubnet"). Returns <c>null</c> when unrestricted.</summary>
    public string LocalAddressesDisplay => LocalAddresses.Count == 0 ? null : string.Join("; ", LocalAddresses);

    /// <summary>Local ports as a human-readable string (e.g. "80; 443; 8080-8090"). Returns <c>null</c> when unrestricted.</summary>
    public string LocalPortsDisplay => PortsToDisplayString(LocalPorts, LocalPortKeywords);

    /// <summary>Remote addresses as a human-readable string (e.g. "10.0.0.0/8"). Returns <c>null</c> when unrestricted.</summary>
    public string RemoteAddressesDisplay => RemoteAddresses.Count == 0 ? null : string.Join("; ", RemoteAddresses);

    /// <summary>Remote ports as a human-readable string (e.g. "80; 443"). Returns <c>null</c> when unrestricted.</summary>
    public string RemotePortsDisplay => PortsToDisplayString(RemotePorts, RemotePortKeywords);

    /// <summary>
    /// Interface type as string (e.g. "Wired"). For a combination of interface types, which is not supported by
    /// NETworkManager, the raw value (e.g. "Wired, Wireless") is returned. Not localized (e.g. used for export).
    /// </summary>
    public string InterfaceTypeDisplay => InterfaceTypeRaw ?? InterfaceType.ToString();

    /// <summary>
    /// Network profiles (Domain / Private / Public) as a comma-separated string.
    /// Returns "Any" when all three are set.
    /// </summary>
    public string NetworkProfilesDisplay
    {
        get
        {
            if (NetworkProfiles.Length == 3 && NetworkProfiles.All(x => x))
                return "Any";

            var names = new List<string>(3);
            if (NetworkProfiles.Length > 0 && NetworkProfiles[0]) names.Add("Domain");
            if (NetworkProfiles.Length > 1 && NetworkProfiles[1]) names.Add("Private");
            if (NetworkProfiles.Length > 2 && NetworkProfiles[2]) names.Add("Public");

            return names.Count == 0 ? "Any" : string.Join(", ", names);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Converts a collection of port numbers to a single, comma-separated string representation.
    /// </summary>
    /// <param name="ports">A collection of integers representing port numbers.</param>
    /// <param name="separator">Separator character to use</param>
    /// <param name="spacing">Separate entries with a space.</param>
    /// <returns>A separated string containing all the port numbers from the input collection.</returns>
    public static string PortsToString(List<FirewallPortSpecification> ports, char separator = ';', bool spacing = true)
    {
        if (ports.Count is 0)
            return string.Empty;

        var delimiter = spacing ? $"{separator} " : separator.ToString();

        return string.Join(delimiter, ports.Select(port => port.ToString()));
    }

    /// <summary>
    /// Converts ports and port keywords (e.g. "RPC") to a human-readable string (e.g. "135; RPC").
    /// </summary>
    /// <param name="ports">The ports.</param>
    /// <param name="keywords">The port keywords.</param>
    /// <returns>The ports as string, or <c>null</c> when unrestricted.</returns>
    private static string PortsToDisplayString(List<FirewallPortSpecification> ports, List<string> keywords)
    {
        if (ports.Count == 0 && keywords.Count == 0)
            return null;

        return string.Join("; ", ports.Select(port => port.ToString()).Concat(keywords));
    }
    #endregion
}