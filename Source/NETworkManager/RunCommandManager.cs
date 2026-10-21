using DnsClient;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;
using NETworkManager.Models.RDAP;
using NETworkManager.Models.RunCommand;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Validators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;

namespace NETworkManager;

/// <summary>
///     Commands of the run command dialog: definitions, profiles, validation and the request to run.
/// </summary>
public static class RunCommandManager
{
    /// <summary>
    ///     Type keywords of RDAP and their query type.
    /// </summary>
    private static readonly Dictionary<string, RDAPQueryType> RDAPTypes = new()
    {
        ["domain"] = RDAPQueryType.Domain,
        ["nameserver"] = RDAPQueryType.Nameserver,
        ["tld"] = RDAPQueryType.TLD,
        ["ip"] = RDAPQueryType.IPAddress,
        ["asn"] = RDAPQueryType.ASN,
        ["entity"] = RDAPQueryType.Entity
    };

    /// <summary>
    ///     Type keywords of the Lookup and their lookup.
    /// </summary>
    private static readonly Dictionary<string, LookupType> LookupTypes = new()
    {
        ["oui"] = LookupType.OUI,
        ["port"] = LookupType.Port
    };

    /// <summary>
    ///     Gets the commands of the visible applications and the settings.
    /// </summary>
    /// <returns>Available commands.</returns>
    public static IReadOnlyList<RunCommandDefinition> GetCommands()
    {
        var visibleApplications = SettingsManager.Current.General_ApplicationList.Where(x => x.IsVisible)
            .Select(x => x.Name).ToHashSet();

        // Created on each call, since the syntax is translated.
        return [.. CreateCommands().Where(x => x.Type == RunCommandType.Setting || visibleApplications.Contains(x.Application))];
    }

    /// <summary>
    ///     Gets the names of the DNS servers configured in the DNS lookup (e.g. "Cloudflare").
    /// </summary>
    /// <returns>Names of the DNS servers.</returns>
    public static IEnumerable<string> GetDNSServers()
    {
        return SettingsManager.Current.DNSLookup_DNSServers.Select(x => x.Name);
    }

    /// <summary>
    ///     Gets the translated name of the application (or settings) of a command.
    /// </summary>
    /// <param name="command">Command.</param>
    /// <returns>Translated name.</returns>
    public static string GetTranslatedName(RunCommandDefinition command)
    {
        return command.Type == RunCommandType.Setting
            ? Strings.Settings
            : ResourceTranslator.Translate(ResourceIdentifier.ApplicationName, command.Application);
    }

    /// <summary>
    ///     Gets the profiles that are enabled for the application of a command.
    /// </summary>
    /// <param name="command">Command.</param>
    /// <returns>Profiles, ordered by group and name.</returns>
    public static IEnumerable<ProfileInfo> GetProfiles(RunCommandDefinition command)
    {
        if (!command.SupportsProfile || ProfileManager.LoadedProfileFileData == null)
            return [];

        return ProfileManager.LoadedProfileFileData.Groups.SelectMany(x => x.Profiles)
            .Where(x => x.IsEnabled(command.Application))
            .OrderBy(x => x.Group).ThenBy(x => x.Name);
    }

    /// <summary>
    ///     Validates the input and creates the request to run.
    /// </summary>
    /// <param name="result">Parsed input.</param>
    /// <param name="profile">
    ///     Profile selected in the suggestions. If null, the profile is looked up by its name ("@name"), which must be
    ///     unique.
    /// </param>
    /// <param name="request">Request to open the application, or null for the settings or on error.</param>
    /// <returns>Error message, or null if the input is valid.</returns>
    public static string TryCreateRequest(RunCommandParseResult result, ProfileInfo profile,
        out ApplicationRequest request)
    {
        request = null;

        var error = GetParseErrorMessage(result);

        if (error != null)
            return error;

        if (result.Command.Type == RunCommandType.Setting)
            return null;

        // Profile ("@name" or selected in the suggestions)
        if (result.ProfileName != null)
        {
            if (profile == null)
            {
                // The name is compared like it is parsed (without surrounding whitespace and quotes).
                var profiles = GetProfiles(result.Command)
                    .Where(x => string.Equals(x.Name.Trim().Trim('"').Trim(), result.ProfileName,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();

                switch (profiles.Count)
                {
                    case 0:
                        return string.Format(Strings.ProfileXNotFound, result.ProfileName);
                    case > 1:
                        return string.Format(Strings.SeveralProfilesAreNamedXSelectOne, result.ProfileName);
                }

                profile = profiles[0];
            }

            request = new ApplicationRequest(result.Command.Application) { Profile = profile };

            return null;
        }

        // Only open the application (Lookup: show the lookup of the type)
        if (result.Target == null)
        {
            request = result.Type != null ? CreateRequest(result) : new ApplicationRequest(result.Command.Application);

            return null;
        }

        error = ValidateTarget(result);

        if (error != null)
            return error;

        request = CreateRequest(result);

        return null;
    }

    /// <summary>
    ///     Gets the message for a parse error.
    /// </summary>
    private static string GetParseErrorMessage(RunCommandParseResult result)
    {
        return result.Error switch
        {
            RunCommandParseError.None => null,
            RunCommandParseError.Empty => Strings.EnterACommand,
            RunCommandParseError.UnknownCommand => string.Format(Strings.UnknownCommandX, result.ErrorValue),
            RunCommandParseError.NoArguments => string.Format(Strings.CommandXDoesNotAcceptArguments, result.ErrorValue),
            RunCommandParseError.NoProfiles => string.Format(Strings.CommandXDoesNotSupportProfiles, result.ErrorValue),
            RunCommandParseError.MissingProfileName => Strings.EnterTheNameOfAProfileAfterAt,
            RunCommandParseError.UnknownArgument => string.Format(Strings.UnknownArgumentX, result.ErrorValue),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Error, null)
        };
    }

    /// <summary>
    ///     Validates the target and the second argument with the validators of the application's input fields.
    /// </summary>
    private static string ValidateTarget(RunCommandParseResult result)
    {
        ValidationRule targetValidator = result.Command.Application switch
        {
            ApplicationName.IPScanner or ApplicationName.PortScanner or ApplicationName.PingMonitor =>
                new MultipleHostsRangeValidator(),
            ApplicationName.Traceroute or ApplicationName.PowerShell or ApplicationName.PuTTY
                or ApplicationName.TigerVNC => new IPAddressOrHostnameValidator(),
            ApplicationName.RemoteDesktop => new RemoteDesktopHostnameAndPortValidator(),
            ApplicationName.WebConsole => new HttpAndHttpsUriValidator(),
            ApplicationName.Whois => new DomainValidator(),
            ApplicationName.IPGeolocation => new IPAddressOrHostnameValidator(),
            ApplicationName.WakeOnLAN => new MACAddressValidator(),
            ApplicationName.SubnetCalculator => new IPv4IPv6SubnetValidator(),
            ApplicationName.Lookup => new NotEndWithSemicolonValidator(),
            _ => null
        };

        var error = Validate(targetValidator, result.Target);

        if (error != null)
            return error;

        if (result.Argument != null)
        {
            ValidationRule argumentValidator = result.Command.Argument switch
            {
                RunCommandArgument.Ports => new PortRangeValidator(),
                RunCommandArgument.Broadcast => new IPv4AddressValidator(),
                _ => null
            };

            error = result.Command.Argument switch
            {
                RunCommandArgument.DNSServer => ValidateDNSServer(result.Argument),
                RunCommandArgument.Subnet => ValidateSubnetArgument(result.Target, result.Argument),
                _ => Validate(argumentValidator, result.Argument)
            };

            if (error != null)
                return error;
        }

        // RDAP: the query must match the type (e.g. "1.10" as ASN)
        if (result.Command.Application == ApplicationName.RDAP)
        {
            var parseError = RDAPQueryParser.TryParse(GetRDAPQueryType(result.Type), result.Target, out _);

            if (parseError != RDAPQueryParseError.None)
                return RDAPQueryValidator.GetErrorMessage(parseError);
        }

        return null;
    }

    /// <summary>
    ///     Validates the DNS server argument: the name of a configured DNS server, or custom DNS server(s) in the format
    ///     of the DNS lookup (IP address or host with a domain, optional port). A single word that is neither (e.g. a
    ///     mistyped type like "aaa") is an unknown argument instead of a DNS server.
    /// </summary>
    private static string ValidateDNSServer(string dnsServer)
    {
        if (GetDNSServers().Any(x => string.Equals(x, dnsServer, StringComparison.OrdinalIgnoreCase)))
            return null;

        foreach (var server in dnsServer.Split(';',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!ServerConnectionInfo.TryParse(server, out var serverInfo, 53, TransportProtocol.Udp) ||
                !IPAddress.TryParse(serverInfo.Server, out _) && !serverInfo.Server.Contains('.'))
                return string.Format(Strings.UnknownArgumentX, server);
        }

        return null;
    }

    /// <summary>
    ///     Validates the second argument of the Subnet Calculator: a new subnet mask or CIDR (Subnetting), which must be
    ///     between the CIDR of the subnet and the maximum of the address family, or a second subnet (Wide Subnet, IPv4
    ///     only like in the application).
    /// </summary>
    private static string ValidateSubnetArgument(string subnet, string argument)
    {
        if (!IsSubnetmaskOrCidr(argument))
            return Validate(new IPv4SubnetValidator(), subnet) ?? Validate(new IPv4SubnetValidator(), argument);

        var ipNetwork = IPNetwork2.Parse(subnet.Trim());
        var maxCidr = ipNetwork.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;

        var newCidr = Subnetmask.ConvertSubnetmaskOrCidrToCidr(argument);

        return newCidr >= ipNetwork.Cidr && newCidr <= maxCidr
            ? null
            : string.Format(Strings.EnterSubnetmaskOrCIDRBetweenXAndY, ipNetwork.Cidr, maxCidr);
    }

    private static bool IsSubnetmaskOrCidr(string value)
    {
        return Validate(new IPv4IPv6SubnetmaskOrCIDRValidator(), value) == null;
    }

    private static string Validate(ValidationRule validator, string value)
    {
        if (validator == null)
            return null;

        var validationResult = validator.Validate(value, CultureInfo.CurrentCulture);

        return validationResult.IsValid ? null : validationResult.ErrorContent as string;
    }

    /// <summary>
    ///     Creates the request with the options of the application.
    /// </summary>
    private static ApplicationRequest CreateRequest(RunCommandParseResult result)
    {
        return result.Command.Application switch
        {
            ApplicationName.DNSLookup => new DNSLookupRequest
            {
                Target = result.Target,
                // A configured DNS server is matched by its name (case-insensitive), otherwise it's a custom server.
                DNSServer = GetDNSServers().FirstOrDefault(x =>
                    string.Equals(x, result.Argument, StringComparison.OrdinalIgnoreCase)) ?? result.Argument,
                // Without a type, the DNS lookup uses PTR for IP addresses and its selected type for hosts.
                QueryType = result.Type != null ? Enum.Parse<QueryType>(result.Type, true) : null
            },
            ApplicationName.PortScanner => new PortScannerRequest { Target = result.Target, Ports = result.Argument },
            ApplicationName.WakeOnLAN => new WakeOnLANRequest { Target = result.Target, Broadcast = result.Argument },
            ApplicationName.RDAP => new RDAPRequest { Target = result.Target, QueryType = GetRDAPQueryType(result.Type) },
            // The argument is a new subnet mask (Subnetting) or a second subnet (Wide Subnet).
            ApplicationName.SubnetCalculator => result.Argument != null && IsSubnetmaskOrCidr(result.Argument)
                ? new SubnetCalculatorRequest { Target = result.Target, NewSubnetmask = result.Argument }
                : new SubnetCalculatorRequest { Target = result.Target, Subnet2 = result.Argument },
            ApplicationName.Lookup => new LookupRequest { Target = result.Target, Type = LookupTypes[result.Type] },
            _ => new ApplicationRequest(result.Command.Application) { Target = result.Target }
        };
    }

    private static RDAPQueryType GetRDAPQueryType(string type)
    {
        return type != null ? RDAPTypes[type] : RDAPQueryType.Auto;
    }

    /// <summary>
    ///     Creates the commands of all applications and the settings. Commands and aliases are lowercase English keywords
    ///     (not translated); the applications are shown with their translated name.
    /// </summary>
    private static List<RunCommandDefinition> CreateCommands()
    {
        // Syntax of the arguments, e.g. "<Hosts> [Ports]"
        static string Required(string name) => $"<{name}>";
        static string Optional(string name) => $"[{name}]";

        var commands = ApplicationManager.GetNames().Where(x => x != ApplicationName.None).Select(name => name switch
        {
            ApplicationName.IPScanner => Create(name, ["scan"], Required(Strings.IPRange), true),
            ApplicationName.PortScanner => Create(name, ["portscan"],
                $"{Required(Strings.Hosts)} {Optional(Strings.Ports)}", true, RunCommandArgument.Ports),
            ApplicationName.PingMonitor => Create(name, ["ping"], Required(Strings.Hosts), true),
            ApplicationName.Traceroute => Create(name, ["trace", "tracert"], Required(Strings.Host), true),
            ApplicationName.DNSLookup => Create(name, ["dns", "nslookup"],
                $"{Required(Strings.Hosts)} {Optional(Strings.DNSServer)} {Optional(Strings.Type)}", true,
                RunCommandArgument.DNSServer,
                [.. DNSLookup.QueryTypes.Select(x => x.ToString().ToLowerInvariant()).Order()]),
            ApplicationName.RemoteDesktop => Create(name, ["rdp", "mstsc"],
                $"{Required(Strings.Host)}[:{Required(Strings.Port)}]", true),
            ApplicationName.PowerShell => Create(name, ["ps", "pwsh"], Required(Strings.Host), true),
            ApplicationName.PuTTY => Create(name, ["ssh"], Required(Strings.Host), true),
            ApplicationName.TigerVNC => Create(name, ["vnc"], Required(Strings.Host), true),
            ApplicationName.WebConsole => Create(name, ["web"], Required(Strings.URL), true),
            ApplicationName.SNMP => Create(name, [], Required(Strings.Host), true),
            ApplicationName.RDAP => Create(name, [], $"{Required(Strings.Query)} {Optional(Strings.Type)}", true,
                types: [.. RDAPTypes.Keys]),
            ApplicationName.Whois => Create(name, [], Required(Strings.Domain), true),
            ApplicationName.IPGeolocation => Create(name, ["geo"], Required(Strings.Host), true),
            ApplicationName.WakeOnLAN => Create(name, ["wol"],
                $"{Required(Strings.MACAddress)} {Optional(Strings.Broadcast)}", true, RunCommandArgument.Broadcast),
            ApplicationName.SubnetCalculator => Create(name, ["subnet", "ipcalc"],
                $"{Required(Strings.Subnet)} {Optional($"{Strings.NewSubnetMask} | {Strings.Subnet2}")}", true,
                RunCommandArgument.Subnet, supportsProfile: false),
            ApplicationName.Lookup => Create(name, [],
                $"{Required(string.Join("|", LookupTypes.Keys))} {Required(Strings.Query)}", true,
                types: [.. LookupTypes.Keys], supportsProfile: false, typeBeforeTarget: true),
            _ => Create(name, [], string.Empty, false)
        }).ToList();

        commands.Add(new RunCommandDefinition
        {
            Type = RunCommandType.Setting,
            Application = ApplicationName.None,
            Command = "settings"
        });

        return commands;
    }

    private static RunCommandDefinition Create(ApplicationName application, string[] aliases, string syntax,
        bool supportsTarget, RunCommandArgument argument = RunCommandArgument.None, string[] types = null,
        bool supportsProfile = true, bool typeBeforeTarget = false)
    {
        // Applications that accept a target support profiles, unless they have none (e.g. Subnet Calculator).
        supportsProfile &= supportsTarget;

        return new RunCommandDefinition
        {
            Type = RunCommandType.Application,
            Application = application,
            Command = application.ToString().ToLowerInvariant(),
            Aliases = aliases,
            SupportsTarget = supportsTarget,
            SupportsProfile = supportsProfile,
            Argument = argument,
            Types = types ?? [],
            TypeBeforeTarget = typeBeforeTarget,
            Syntax = supportsProfile ? $"{syntax} | @<{Strings.Profile}>" : syntax
        };
    }
}
