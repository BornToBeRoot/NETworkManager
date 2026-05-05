using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using SMA = System.Management.Automation;
using log4net;

namespace NETworkManager.Models.Firewall;

/// <summary>
/// Provides static methods to read and modify Windows Firewall rules via PowerShell.
/// All operations share a single <see cref="Runspace"/> that is lazily initialized on
/// first use with the required execution policy and the <c>NetSecurity</c> module imported,
/// reducing per-call overhead. A <see cref="SemaphoreSlim"/> serializes access so the
/// runspace is never used concurrently.
/// </summary>
public class Firewall
{
    #region Variables

    /// <summary>
    /// The logger for this class.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(Firewall));

    /// <summary>
    /// Prefix applied to the <c>DisplayName</c> of every rule managed by NETworkManager.
    /// Used to scope <c>Get-NetFirewallRule</c> queries to only our own rules.
    /// </summary>
    private const string RuleIdentifier = "NETworkManager_";

    /// <summary>
    /// Ensures that only one PowerShell pipeline runs on <see cref="SharedRunspace"/> at a time.
    /// </summary>
    private static readonly SemaphoreSlim RunspaceLock = new(1, 1);

    /// <summary>
    /// Lazily initialized PowerShell runspace. Created and configured on first access so that
    /// simply navigating to the Firewall view does not start a PowerShell process unless a
    /// modifying operation is actually performed.
    /// </summary>
    private static readonly Lazy<Runspace> _sharedRunspace = new(() =>
    {
        var runspace = RunspaceFactory.CreateRunspace();
        runspace.Open();

        using var ps = SMA.PowerShell.Create();
        ps.Runspace = runspace;
        ps.AddScript(@"
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
Import-Module NetSecurity -ErrorAction Stop").Invoke();

        return runspace;
    });

    /// <summary>Returns the shared runspace, initializing it on first access.</summary>
    private static Runspace SharedRunspace => _sharedRunspace.Value;

    #endregion

    #region Methods

    /// <summary>
    /// Retrieves all Windows Firewall rules whose display name starts with <see cref="RuleIdentifier"/>
    /// and maps each one to a <see cref="FirewallRule"/> object.
    /// PowerShell errors during the query are logged as warnings; errors for individual
    /// rules are caught so a single malformed rule does not abort the entire load.
    /// </summary>
    /// <returns>
    /// A list of <see cref="FirewallRule"/> objects representing the matching rules.
    /// </returns>
    public static async Task<List<FirewallRule>> GetRulesAsync()
    {
        await RunspaceLock.WaitAsync();
        try
        {
            return await Task.Run(() =>
            {
                var rules = new List<FirewallRule>();

                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript($@"
Get-NetFirewallRule -DisplayName '{RuleIdentifier}*' | ForEach-Object {{
    $rule              = $_
    $portFilter        = $rule | Get-NetFirewallPortFilter
    $addressFilter     = $rule | Get-NetFirewallAddressFilter
    $appFilter         = $rule | Get-NetFirewallApplicationFilter
    $ifTypeFilter      = $rule | Get-NetFirewallInterfaceTypeFilter
    [PSCustomObject]@{{
        Id            = $rule.ID
        DisplayName   = $rule.DisplayName
        Enabled       = ($rule.Enabled -eq 'True')
        Description   = $rule.Description
        Direction     = [string]$rule.Direction
        Action        = [string]$rule.Action
        Protocol      = $portFilter.Protocol
        LocalPort     = ($portFilter.LocalPort   -join ',')
        RemotePort    = ($portFilter.RemotePort  -join ',')
        LocalAddress  = ($addressFilter.LocalAddress  -join ',')
        RemoteAddress = ($addressFilter.RemoteAddress -join ',')
        Profile       = [string]$rule.Profile
        InterfaceType = [string]$ifTypeFilter.InterfaceType
        Program       = $appFilter.Program
    }}
}}");

                var results = ps.Invoke();

                if (ps.Streams.Error.Count > 0)
                {
                    foreach (var error in ps.Streams.Error)
                        Log.Warn($"PowerShell error: {error}");
                }

                foreach (var result in results)
                {
                    try
                    {
                        var displayName = result.Properties["DisplayName"]?.Value?.ToString() ?? string.Empty;

                        var rule = new FirewallRule
                        {
                            Id = result.Properties["Id"]?.Value?.ToString() ?? string.Empty,
                            IsEnabled = result.Properties["Enabled"]?.Value as bool? == true,
                            Name = displayName.StartsWith(RuleIdentifier, StringComparison.Ordinal)
                                                  ? displayName[RuleIdentifier.Length..]
                                                  : displayName,
                            Description = result.Properties["Description"]?.Value?.ToString() ?? string.Empty,
                            Direction = ParseDirection(result.Properties["Direction"]?.Value?.ToString()),
                            Action = ParseAction(result.Properties["Action"]?.Value?.ToString()),
                            Protocol = ParseProtocol(result.Properties["Protocol"]?.Value?.ToString()),
                            LocalPorts = ParsePorts(result.Properties["LocalPort"]?.Value?.ToString()),
                            RemotePorts = ParsePorts(result.Properties["RemotePort"]?.Value?.ToString()),
                            LocalAddresses = ParseAddresses(result.Properties["LocalAddress"]?.Value?.ToString()),
                            RemoteAddresses = ParseAddresses(result.Properties["RemoteAddress"]?.Value?.ToString()),
                            NetworkProfiles = ParseProfile(result.Properties["Profile"]?.Value?.ToString()),
                            InterfaceType = ParseInterfaceType(result.Properties["InterfaceType"]?.Value?.ToString()),
                        };

                        var program = result.Properties["Program"]?.Value as string;

                        if (!string.IsNullOrWhiteSpace(program) && !program.Equals("Any", StringComparison.OrdinalIgnoreCase))
                            rule.Program = new FirewallRuleProgram(program);

                        rules.Add(rule);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"Failed to parse firewall rule: {ex.Message}");
                    }
                }

                return rules;
            });
        }
        finally
        {
            RunspaceLock.Release();
        }
    }

    /// <summary>
    /// Enables or disables the given <paramref name="rule"/> by running
    /// <c>Enable-NetFirewallRule</c> or <c>Disable-NetFirewallRule</c> against
    /// the rule's internal <see cref="FirewallRule.Id"/>.
    /// </summary>
    /// <param name="rule">
    /// The firewall rule to modify.
    /// </param>
    /// <param name="enabled">
    /// <see langword="true"/> to enable the rule; <see langword="false"/> to disable it.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static async Task SetRuleEnabledAsync(FirewallRule rule, bool enabled)
    {
        await RunspaceLock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript($@"{(enabled ? "Enable" : "Disable")}-NetFirewallRule -Name '{rule.Id}'");
                ps.Invoke();

                if (ps.Streams.Error.Count > 0)
                    throw new Exception(string.Join("; ", ps.Streams.Error));
            });
        }
        finally
        {
            RunspaceLock.Release();
        }
    }

    /// <summary>
    /// Permanently removes the given <paramref name="rule"/> by running
    /// <c>Remove-NetFirewallRule</c> against the rule's internal <see cref="FirewallRule.Id"/>.
    /// </summary>
    /// <param name="rule">
    /// The firewall rule to delete.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static async Task DeleteRuleAsync(FirewallRule rule)
    {
        await RunspaceLock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript($@"Remove-NetFirewallRule -Name '{rule.Id}'");
                ps.Invoke();

                if (ps.Streams.Error.Count > 0)
                    throw new Exception(string.Join("; ", ps.Streams.Error));
            });
        }
        finally
        {
            RunspaceLock.Release();
        }
    }

    /// <summary>
    /// Creates a new Windows Firewall rule with the properties specified in <paramref name="rule"/>.
    /// The rule's <see cref="FirewallRule.Name"/> is prefixed with <see cref="RuleIdentifier"/> so
    /// it is picked up by <see cref="GetRulesAsync"/> on the next refresh.
    /// </summary>
    /// <param name="rule">
    /// The firewall rule to create.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static async Task AddRuleAsync(FirewallRule rule)
    {
        await RunspaceLock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript(BuildAddScript(rule));
                ps.Invoke();

                if (ps.Streams.Error.Count > 0)
                    throw new Exception(string.Join("; ", ps.Streams.Error));
            });
        }
        finally
        {
            RunspaceLock.Release();
        }
    }

    /// <summary>
    /// Builds the PowerShell script that calls <c>New-NetFirewallRule</c> with all
    /// properties from <paramref name="rule"/>.
    /// </summary>
    /// <param name="rule">
    /// The firewall rule whose properties are used to build the script.
    /// </param>
    private static string BuildAddScript(FirewallRule rule)
    {
        var sb = new StringBuilder();
        sb.AppendLine("$params = @{");
        sb.AppendLine($"    DisplayName   = '{RuleIdentifier}{PowerShellHelper.EscapeSingleQuotes(rule.Name)}'");
        sb.AppendLine($"    Enabled       = '{(rule.IsEnabled ? "True" : "False")}'");
        sb.AppendLine($"    Direction     = '{rule.Direction}'");
        sb.AppendLine($"    Action        = '{rule.Action}'");
        sb.AppendLine($"    Protocol      = '{GetProtocolString(rule.Protocol)}'");
        sb.AppendLine($"    InterfaceType = '{GetInterfaceTypeString(rule.InterfaceType)}'");
        sb.AppendLine($"    Profile       = '{GetProfileString(rule.NetworkProfiles)}'");
        sb.AppendLine("}");

        if (!string.IsNullOrWhiteSpace(rule.Description))
            sb.AppendLine($"$params['Description'] = '{PowerShellHelper.EscapeSingleQuotes(rule.Description)}'");

        if (rule.Protocol is FirewallProtocol.TCP or FirewallProtocol.UDP)
        {
            if (rule.LocalPorts.Count > 0)
                sb.AppendLine($"$params['LocalPort']  = {ToPsArray(rule.LocalPorts.Select(p => p.ToString()))}");

            if (rule.RemotePorts.Count > 0)
                sb.AppendLine($"$params['RemotePort'] = {ToPsArray(rule.RemotePorts.Select(p => p.ToString()))}");
        }

        if (rule.LocalAddresses.Count > 0)
            sb.AppendLine($"$params['LocalAddress']  = {ToPsArray(rule.LocalAddresses)}");

        if (rule.RemoteAddresses.Count > 0)
            sb.AppendLine($"$params['RemoteAddress'] = {ToPsArray(rule.RemoteAddresses)}");

        if (rule.Program != null && !string.IsNullOrWhiteSpace(rule.Program.Name))
            sb.AppendLine($"$params['Program'] = '{PowerShellHelper.EscapeSingleQuotes(rule.Program.Name)}'");

        sb.AppendLine("New-NetFirewallRule @params");

        return sb.ToString();
    }

    /// <summary>
    /// Builds a PowerShell array literal (e.g. <c>@('80','443','8080-8090')</c>) from the given values.
    /// New-NetFirewallRule parameters such as -LocalPort and -LocalAddress are typed as
    /// <c>String[]</c>; passing a single comma-joined string would be interpreted as one
    /// element and rejected, so we emit a real array.
    /// </summary>
    /// <param name="values">The values to embed into the array literal.</param>
    private static string ToPsArray(IEnumerable<string> values) =>
        $"@({string.Join(",", values.Select(v => $"'{PowerShellHelper.EscapeSingleQuotes(v)}'"))})";

    /// <summary>
    /// Maps a <see cref="FirewallProtocol"/> value to the string accepted by
    /// <c>New-NetFirewallRule -Protocol</c>.
    /// </summary>
    /// <param name="protocol">The protocol to convert.</param>
    private static string GetProtocolString(FirewallProtocol protocol) => protocol switch
    {
        FirewallProtocol.Any => "Any",
        FirewallProtocol.TCP => "TCP",
        FirewallProtocol.UDP => "UDP",
        FirewallProtocol.ICMPv4 => "ICMPv4",
        FirewallProtocol.ICMPv6 => "ICMPv6",
        FirewallProtocol.GRE => "GRE",
        FirewallProtocol.L2TP => "L2TP",
        _ => ((int)protocol).ToString()
    };

    /// <summary>
    /// Maps a <see cref="FirewallInterfaceType"/> value to the string accepted by
    /// <c>New-NetFirewallRule -InterfaceType</c>.
    /// </summary>
    /// <param name="interfaceType">The interface type to convert.</param>
    private static string GetInterfaceTypeString(FirewallInterfaceType interfaceType) => interfaceType switch
    {
        FirewallInterfaceType.Wired => "Wired",
        FirewallInterfaceType.Wireless => "Wireless",
        FirewallInterfaceType.RemoteAccess => "RemoteAccess",
        _ => "Any"
    };

    /// <summary>
    /// Converts the three-element network-profile boolean array (Domain, Private, Public)
    /// to the comma-separated profile string accepted by <c>New-NetFirewallRule -Profile</c>.
    /// All-false or all-true both map to <c>"Any"</c>.
    /// </summary>
    /// <param name="profiles">Three-element boolean array (Domain=0, Private=1, Public=2).</param>
    private static string GetProfileString(bool[] profiles)
    {
        if (profiles == null || profiles.Length < 3 || profiles.All(p => p) || profiles.All(p => !p))
            return "Any";

        var parts = new List<string>(3);
        if (profiles[0]) parts.Add("Domain");
        if (profiles[1]) parts.Add("Private");
        if (profiles[2]) parts.Add("Public");

        return parts.Count == 0 ? "Any" : string.Join(",", parts);
    }

    /// <summary>
    /// Parses a PowerShell direction string (e.g. <c>"Outbound"</c>) to a
    /// <see cref="FirewallRuleDirection"/> value. Defaults to <see cref="FirewallRuleDirection.Inbound"/>.
    /// </summary>
    /// <param name="value">
    /// The raw string value returned by PowerShell.
    /// </param>
    private static FirewallRuleDirection ParseDirection(string value)
    {
        return value switch
        {
            "Outbound" => FirewallRuleDirection.Outbound,
            _ => FirewallRuleDirection.Inbound,
        };
    }

    /// <summary>
    /// Parses a PowerShell action string (e.g. <c>"Allow"</c>) to a
    /// <see cref="FirewallRuleAction"/> value. Defaults to <see cref="FirewallRuleAction.Block"/>.
    /// </summary>
    /// <param name="value">
    /// The raw string value returned by PowerShell.
    /// </param>
    private static FirewallRuleAction ParseAction(string value)
    {
        return value switch
        {
            "Allow" => FirewallRuleAction.Allow,
            _ => FirewallRuleAction.Block,
        };
    }

    /// <summary>
    /// Parses a PowerShell protocol string (e.g. <c>"TCP"</c>, <c>"Any"</c>) to a
    /// <see cref="FirewallProtocol"/> value. Numeric protocol numbers are also accepted.
    /// Defaults to <see cref="FirewallProtocol.Any"/> for unrecognized values.
    /// </summary>
    /// <param name="value">
    /// The raw string value returned by PowerShell.
    /// </param>
    private static FirewallProtocol ParseProtocol(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("Any", StringComparison.OrdinalIgnoreCase))
            return FirewallProtocol.Any;

        return value.ToUpperInvariant() switch
        {
            "TCP" => FirewallProtocol.TCP,
            "UDP" => FirewallProtocol.UDP,
            "ICMPV4" => FirewallProtocol.ICMPv4,
            "ICMPV6" => FirewallProtocol.ICMPv6,
            "GRE" => FirewallProtocol.GRE,
            "L2TP" => FirewallProtocol.L2TP,
            _ => int.TryParse(value, out var proto) ? (FirewallProtocol)proto : FirewallProtocol.Any,
        };
    }

    /// <summary>
    /// Parses a comma-separated port string (e.g. <c>"80,443,8080-8090"</c>) to a list of
    /// <see cref="FirewallPortSpecification"/> objects.
    /// Returns an empty list when the value is blank or <c>"Any"</c>.
    /// </summary>
    /// <param name="value">
    /// The raw comma-separated port string returned by PowerShell.
    /// </param>
    private static List<FirewallPortSpecification> ParsePorts(string value)
    {
        var list = new List<FirewallPortSpecification>();

        if (string.IsNullOrWhiteSpace(value) || value.Equals("Any", StringComparison.OrdinalIgnoreCase))
            return list;

        foreach (var token in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var dashIndex = token.IndexOf('-');

            if (dashIndex > 0 &&
                int.TryParse(token[..dashIndex], out var start) &&
                int.TryParse(token[(dashIndex + 1)..], out var end))
            {
                list.Add(new FirewallPortSpecification(start, end));
            }
            else if (int.TryParse(token, out var port))
            {
                list.Add(new FirewallPortSpecification(port));
            }
        }

        return list;
    }

    /// <summary>
    /// Parses a PowerShell profile string (e.g. <c>"Domain, Private"</c>) to a
    /// three-element boolean array in the order Domain, Private, Public.
    /// <c>"Any"</c> and <c>"All"</c> set all three entries to <see langword="true"/>.
    /// </summary>
    /// <param name="value">
    /// The raw profile string returned by PowerShell.
    /// </param>
    private static bool[] ParseProfile(string value)
    {
        var profiles = new bool[3];

        if (string.IsNullOrWhiteSpace(value))
            return profiles;

        if (value.Equals("Any", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            profiles[0] = profiles[1] = profiles[2] = true;
            return profiles;
        }

        foreach (var token in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            switch (token)
            {
                case "Domain": profiles[0] = true; break;
                case "Private": profiles[1] = true; break;
                case "Public": profiles[2] = true; break;
            }
        }

        return profiles;
    }

    /// <summary>
    /// Parses a comma-separated address string (e.g. <c>"192.168.1.0/24,LocalSubnet"</c>) to a
    /// list of address strings. PowerShell returns IPv4 subnets in subnet-mask notation
    /// (e.g. <c>10.8.0.0/255.255.0.0</c>); these are normalized back to CIDR. IPv6 subnets
    /// are already returned in CIDR notation by PowerShell and are passed through unchanged.
    /// Returns an empty list when the value is blank or <c>"Any"</c>.
    /// </summary>
    /// <param name="value">
    /// The raw comma-separated address string returned by PowerShell.
    /// </param>
    private static List<string> ParseAddresses(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("Any", StringComparison.OrdinalIgnoreCase))
            return [];

        var addresses = new List<string>();

        foreach (var token in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var slashIndex = token.IndexOf('/');

            if (slashIndex > 0)
            {
                var maskPart = token[(slashIndex + 1)..];

                if (RegexHelper.SubnetmaskRegex().IsMatch(maskPart) && IPAddress.TryParse(maskPart, out var mask))
                {
                    addresses.Add($"{token[..slashIndex]}/{Subnetmask.ConvertSubnetmaskToCidr(mask)}");
                    continue;
                }
            }

            addresses.Add(token);
        }

        return addresses;
    }

    /// <summary>
    /// Parses a PowerShell interface-type string (e.g. <c>"Wired"</c>) to a
    /// <see cref="FirewallInterfaceType"/> value. Defaults to <see cref="FirewallInterfaceType.Any"/>.
    /// </summary>
    /// <param name="value">
    /// The raw string value returned by PowerShell.
    /// </param>
    private static FirewallInterfaceType ParseInterfaceType(string value)
    {
        return value switch
        {
            "Wired" => FirewallInterfaceType.Wired,
            "Wireless" => FirewallInterfaceType.Wireless,
            "RemoteAccess" => FirewallInterfaceType.RemoteAccess,
            _ => FirewallInterfaceType.Any,
        };
    }

    #endregion
}