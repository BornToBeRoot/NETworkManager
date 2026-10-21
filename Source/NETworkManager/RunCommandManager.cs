using DnsClient;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
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

    private static readonly IReadOnlyList<RunCommandDefinition> Commands = CreateCommands();

    /// <summary>
    ///     Gets the commands of the visible applications and the settings.
    /// </summary>
    /// <returns>Available commands.</returns>
    public static IReadOnlyList<RunCommandDefinition> GetCommands()
    {
        var visibleApplications = SettingsManager.Current.General_ApplicationList.Where(x => x.IsVisible)
            .Select(x => x.Name).ToHashSet();

        return [.. Commands.Where(x => x.Type == RunCommandType.Setting || visibleApplications.Contains(x.Application))];
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
                var profiles = GetProfiles(result.Command)
                    .Where(x => string.Equals(x.Name, result.ProfileName, StringComparison.OrdinalIgnoreCase))
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

        // Only open the application
        if (result.Target == null)
        {
            request = new ApplicationRequest(result.Command.Application);

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
            _ => null
        };

        var error = Validate(targetValidator, result.Target);

        if (error != null)
            return error;

        if (result.Command.Argument == RunCommandArgument.Ports && result.Argument != null)
            return Validate(new PortRangeValidator(), result.Argument);

        // RDAP: the query must match the type (e.g. "1.10" as ASN)
        if (result.Command.Application == ApplicationName.RDAP)
        {
            var parseError = RDAPQueryParser.TryParse(GetRDAPQueryType(result.Type), result.Target, out _);

            if (parseError != RDAPQueryParseError.None)
                return RDAPQueryValidator.GetErrorMessage(parseError);
        }

        return null;
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
                DNSServer = result.Argument,
                QueryType = result.Type != null
                    ? Enum.Parse<QueryType>(result.Type, true)
                    // Only IP addresses: reverse lookup
                    : result.Target.Split(';').All(x => IPAddress.TryParse(x, out _))
                        ? QueryType.PTR
                        : null
            },
            ApplicationName.PortScanner => new PortScannerRequest { Target = result.Target, Ports = result.Argument },
            ApplicationName.RDAP => new RDAPRequest { Target = result.Target, QueryType = GetRDAPQueryType(result.Type) },
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
        var commands = ApplicationManager.GetNames().Where(x => x != ApplicationName.None).Select(name => name switch
        {
            ApplicationName.IPScanner => Create(name, ["scan"], "<range>", true),
            ApplicationName.PortScanner => Create(name, ["portscan"], "<hosts> [ports]", true,
                RunCommandArgument.Ports),
            ApplicationName.PingMonitor => Create(name, ["ping"], "<hosts>", true),
            ApplicationName.Traceroute => Create(name, ["trace", "tracert"], "<host>", true),
            ApplicationName.DNSLookup => Create(name, ["dns", "nslookup"], "<hosts> [server] [type]", true,
                RunCommandArgument.DNSServer,
                [.. DNSLookup.QueryTypes.Select(x => x.ToString().ToLowerInvariant()).Order()]),
            ApplicationName.RemoteDesktop => Create(name, ["rdp", "mstsc"], "<host[:port]>", true),
            ApplicationName.PowerShell => Create(name, ["ps", "pwsh"], "<host>", true),
            ApplicationName.PuTTY => Create(name, ["ssh"], "<host>", true),
            ApplicationName.TigerVNC => Create(name, ["vnc"], "<host>", true),
            ApplicationName.WebConsole => Create(name, ["web"], "<url>", true),
            ApplicationName.SNMP => Create(name, [], "<host>", true),
            ApplicationName.RDAP => Create(name, [], "<query> [type]", true, types: [.. RDAPTypes.Keys]),
            ApplicationName.Whois => Create(name, [], "<domain>", true),
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
        bool supportsTarget, RunCommandArgument argument = RunCommandArgument.None, string[] types = null)
    {
        return new RunCommandDefinition
        {
            Type = RunCommandType.Application,
            Application = application,
            Command = application.ToString().ToLowerInvariant(),
            Aliases = aliases,
            SupportsTarget = supportsTarget,
            // Every application that accepts a target also supports profiles.
            SupportsProfile = supportsTarget,
            Argument = argument,
            Types = types ?? [],
            Syntax = supportsTarget ? $"{syntax} | @<profile>" : string.Empty
        };
    }
}
