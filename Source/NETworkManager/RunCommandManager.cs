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
    ///     Applications that are suggested for a target typed without a command, in the order they are suggested (if
    ///     they were not used with this kind of target before).
    /// </summary>
    private static readonly Dictionary<RunCommandTargetKind, ApplicationName[]> TargetApplications = new()
    {
        // The Port Scanner is not suggested, since it needs ports (or a port profile).
        [RunCommandTargetKind.Host] =
        [
            ApplicationName.PingMonitor, ApplicationName.Traceroute, ApplicationName.DNSLookup,
            ApplicationName.RemoteDesktop, ApplicationName.PuTTY, ApplicationName.PowerShell, ApplicationName.TigerVNC,
            ApplicationName.IPGeolocation, ApplicationName.Whois, ApplicationName.RDAP, ApplicationName.SNMP,
            ApplicationName.IPScanner
        ],
        // Without a domain (e.g. in a company network): no tools for public hosts (Whois, RDAP, IP Geolocation)
        [RunCommandTargetKind.Hostname] =
        [
            ApplicationName.PingMonitor, ApplicationName.Traceroute, ApplicationName.DNSLookup,
            ApplicationName.RemoteDesktop, ApplicationName.PuTTY, ApplicationName.PowerShell, ApplicationName.TigerVNC,
            ApplicationName.SNMP, ApplicationName.IPScanner
        ],
        [RunCommandTargetKind.Hosts] = [ApplicationName.PingMonitor, ApplicationName.DNSLookup, ApplicationName.IPScanner],
        [RunCommandTargetKind.IPRange] = [ApplicationName.IPScanner, ApplicationName.PingMonitor],
        [RunCommandTargetKind.Subnet] =
        [
            ApplicationName.IPScanner, ApplicationName.SubnetCalculator, ApplicationName.PingMonitor,
            ApplicationName.RDAP
        ],
        [RunCommandTargetKind.MACAddress] = [ApplicationName.WakeOnLAN, ApplicationName.Lookup],
        [RunCommandTargetKind.OUI] = [ApplicationName.Lookup],
        [RunCommandTargetKind.Ports] = [ApplicationName.Lookup],
        [RunCommandTargetKind.ASN] = [ApplicationName.RDAP],
        [RunCommandTargetKind.URL] = [ApplicationName.WebConsole]
    };

    /// <summary>
    ///     Remote tools, which open the connect dialog for a target (instead of connecting directly).
    /// </summary>
    private static readonly HashSet<ApplicationName> ConnectDialogApplications =
    [
        ApplicationName.RemoteDesktop, ApplicationName.PowerShell, ApplicationName.PuTTY, ApplicationName.TigerVNC,
        ApplicationName.WebConsole
    ];

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
    ///     Gets the port profiles of the Port Scanner (e.g. "Webserver"), which can be used instead of ports.
    /// </summary>
    /// <returns>Port profiles.</returns>
    public static IEnumerable<PortProfileInfo> GetPortProfiles()
    {
        return SettingsManager.Current.PortScanner_PortProfiles;
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
    ///     Gets the commands that accept a target typed without a command (e.g. "ping 10.0.0.1", "trace 10.0.0.1" for
    ///     "10.0.0.1"). Only valid commands are returned (validated like the input).
    /// </summary>
    /// <param name="target">Target (e.g. "10.0.0.1").</param>
    /// <param name="commands">Available commands.</param>
    /// <param name="kind">Kind of the target if it is already known, otherwise it is detected.</param>
    /// <returns>Commands with the input to run, in the default order of the kind of the target.</returns>
    public static IEnumerable<(RunCommandDefinition Command, string Text)> GetCommandsForTarget(string target,
        IReadOnlyList<RunCommandDefinition> commands, RunCommandTargetKind? kind = null)
    {
        kind ??= RunCommandTarget.GetKind(target);

        if (!TargetApplications.TryGetValue(kind.Value, out var applications))
            yield break;

        foreach (var application in applications)
        {
            var command = commands.FirstOrDefault(x => x.Application == application);

            // Hidden application
            if (command == null)
                continue;

            // The shortest name (e.g. "ping" instead of "pingmonitor")
            var name = command.Aliases.FirstOrDefault() ?? command.Command;

            var text = application == ApplicationName.Lookup
                ? $"{name} {(kind == RunCommandTargetKind.Ports ? "port" : "oui")} {target}"
                : $"{name} {target}";

            if (TryCreateRequest(RunCommandParser.Parse(text, commands), null, out _) == null)
                yield return (command, text);
        }
    }

    /// <summary>
    ///     Checks if the request opens a dialog, so the run command dialog must be closed: the settings or the connect
    ///     dialog of a remote tool. Messages of the applications (e.g. an error or a confirmation) close the run command
    ///     dialog when they are shown (<see cref="DialogHelper.DialogOpening" />).
    /// </summary>
    /// <param name="request">Request, or null for the settings.</param>
    /// <returns>True if the request opens a dialog.</returns>
    public static bool OpensDialog(ApplicationRequest request)
    {
        return request == null || request.Profile == null && !string.IsNullOrEmpty(request.Target) &&
            ConnectDialogApplications.Contains(request.Application);
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

        // A profile selected in the suggestions is run with its settings, also if its name can't be typed (e.g. it
        // starts with a quote, which is parsed as a quoted name).
        if (profile != null && result.Command is { Type: RunCommandType.Application })
        {
            request = new ApplicationRequest(result.Command.Application) { Profile = profile };

            return null;
        }

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
                // The profiles are not loaded until the (encrypted) profile file is unlocked.
                if (ConfigurationManager.Current.ProfileManagerShowUnlock)
                    return Strings.UnlockTheProfileFileMessage;

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

        // The Port Scanner needs ports (or a port profile), a new tab has none.
        if (result.Command.Argument == RunCommandArgument.Ports && result.Argument == null)
            return Strings.EnterValidPortOrPortRange;

        if (result.Argument != null)
        {
            ValidationRule argumentValidator = result.Command.Argument switch
            {
                RunCommandArgument.Ports => new PortsOrPortProfilesValidator(),
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
    ///     Validates the DNS server argument: names of configured DNS servers (case-insensitive) and / or custom DNS
    ///     servers in the format of the DNS lookup (IP address or host with a domain, optional port), separated by ";"
    ///     (e.g. "google;cloudflare;9.9.9.9"). A single word that is neither (e.g. a mistyped type like "aaa") is an
    ///     unknown argument instead of a DNS server.
    /// </summary>
    private static string ValidateDNSServer(string dnsServer)
    {
        var invalidEntry = DNSServer.GetInvalidEntries(dnsServer, SettingsManager.Current.DNSLookup_DNSServers,
            hostnameNeedsDomain: true).FirstOrDefault();

        return invalidEntry == null ? null : string.Format(Strings.UnknownArgumentX, invalidEntry);
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

        var (minCidr, maxCidr) = Subnetmask.GetSubnettingCidrRange(IPNetwork2.Parse(subnet.Trim()));
        var newCidr = Subnetmask.ConvertSubnetmaskOrCidrToCidr(argument);

        return newCidr >= minCidr && newCidr <= maxCidr
            ? null
            : string.Format(Strings.EnterSubnetmaskOrCIDRBetweenXAndY, minCidr, maxCidr);
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
            // Ports and / or port profiles (e.g. "webserver;8080"), resolved by the Port Scanner (shown as entered).
            ApplicationName.PortScanner => new PortScannerRequest
            {
                Target = result.Target,
                Ports = result.Argument
            },
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
        // Parts of the syntax, e.g. "<Hosts> [Ports]"
        static RunCommandSyntaxPart Target(string name) =>
            RunCommandSyntaxPart.Required(RunCommandSyntaxPartKind.Target, name);

        static RunCommandSyntaxPart Argument(string name) =>
            RunCommandSyntaxPart.Optional(RunCommandSyntaxPartKind.Argument, name);

        var type = RunCommandSyntaxPart.Optional(RunCommandSyntaxPartKind.Type, Strings.Type);

        var commands = ApplicationManager.GetNames().Where(x => x != ApplicationName.None).Select(name => name switch
        {
            ApplicationName.IPScanner => Create(name, ["ipscan"], [Target(Strings.IPRange)]),
            ApplicationName.PortScanner => Create(name, ["portscan"],
            [
                Target(Strings.Hosts),
                RunCommandSyntaxPart.Required(RunCommandSyntaxPartKind.Argument,
                    $"{Strings.Ports} | {Strings.PortProfile}")
            ], RunCommandArgument.Ports),
            ApplicationName.PingMonitor => Create(name, ["ping"], [Target(Strings.Hosts)]),
            ApplicationName.Traceroute => Create(name, ["trace", "tracert"], [Target(Strings.Host)]),
            ApplicationName.DNSLookup => Create(name, ["dns", "nslookup"],
                [Target(Strings.Hosts), Argument(Strings.DNSServer), type], RunCommandArgument.DNSServer,
                [.. DNSLookup.QueryTypes.Select(x => x.ToString().ToLowerInvariant()).Order()]),
            ApplicationName.RemoteDesktop => Create(name, ["rdp", "mstsc"],
            [
                new RunCommandSyntaxPart(RunCommandSyntaxPartKind.Target, Strings.Host,
                    $"<{Strings.Host}>[:<{Strings.Port}>]")
            ]),
            ApplicationName.PowerShell => Create(name, ["ps", "pwsh"], [Target(Strings.Host)]),
            ApplicationName.PuTTY => Create(name, ["ssh"], [Target(Strings.Host)]),
            ApplicationName.TigerVNC => Create(name, ["vnc"], [Target(Strings.Host)]),
            ApplicationName.WebConsole => Create(name, ["web"], [Target(Strings.URL)]),
            ApplicationName.SNMP => Create(name, [], [Target(Strings.Host)]),
            ApplicationName.RDAP => Create(name, [], [Target(Strings.Query), type], types: [.. RDAPTypes.Keys]),
            ApplicationName.Whois => Create(name, [], [Target(Strings.Domain)]),
            ApplicationName.IPGeolocation => Create(name, ["geo"], [Target(Strings.Host)]),
            ApplicationName.WakeOnLAN => Create(name, ["wol"], [Target(Strings.MACAddress), Argument(Strings.Broadcast)],
                RunCommandArgument.Broadcast),
            ApplicationName.SubnetCalculator => Create(name, ["subnet", "ipcalc"],
                [Target(Strings.Subnet), Argument($"{Strings.NewSubnetMask} | {Strings.Subnet2}")],
                RunCommandArgument.Subnet, supportsProfile: false),
            ApplicationName.Lookup => Create(name, [],
            [
                new RunCommandSyntaxPart(RunCommandSyntaxPartKind.Type, Strings.Type,
                    $"<{string.Join("|", LookupTypes.Keys)}>"),
                Target(Strings.Query)
            ], types: [.. LookupTypes.Keys], supportsProfile: false, typeBeforeTarget: true),
            _ => Create(name, [], null)
        }).ToList();

        commands.Add(new RunCommandDefinition
        {
            Type = RunCommandType.Setting,
            Application = ApplicationName.None,
            Command = "settings"
        });

        return commands;
    }

    /// <summary>
    ///     Creates the command of an application.
    /// </summary>
    /// <param name="application">Application.</param>
    /// <param name="aliases">Short forms of the command.</param>
    /// <param name="syntaxParts">Syntax of the arguments, or null if the command accepts no target.</param>
    /// <param name="argument">Second argument after the target.</param>
    /// <param name="types">Type keywords.</param>
    /// <param name="supportsProfile">The application has profiles (only with a target).</param>
    /// <param name="typeBeforeTarget">The type keyword comes before the target.</param>
    private static RunCommandDefinition Create(ApplicationName application, string[] aliases,
        RunCommandSyntaxPart[] syntaxParts, RunCommandArgument argument = RunCommandArgument.None,
        string[] types = null, bool supportsProfile = true, bool typeBeforeTarget = false)
    {
        var supportsTarget = syntaxParts != null;

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
            SyntaxParts = supportsProfile
                ? [.. syntaxParts, RunCommandSyntaxPart.Profile(Strings.Profile)]
                : syntaxParts ?? []
        };
    }
}
