using System;
using System.Collections.Generic;
using System.Diagnostics;
using SMA = System.Management.Automation;
using System.Threading.Tasks;
using log4net;

namespace NETworkManager.Models.Firewall;

/// <summary>
/// Represents a firewall configuration and management class that provides functionalities
/// for applying and managing firewall rules based on a specified profile.
/// </summary>
public class Firewall
{
    #region Variables

    /// <summary>
    /// The Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(Firewall));

    private const string RuleIdentifier = "NETworkManager_";
    
    #endregion

    #region Methods

    /// <summary>
    /// Reads all Windows Firewall rules whose display name starts with <see cref="RuleIdentifier"/>
    /// and maps them to <see cref="FirewallRule"/> objects.
    /// </summary>
    /// <returns>A task that resolves to the list of matching firewall rules.</returns>
    public static async Task<List<FirewallRule>> GetRulesAsync()
    {
        return await Task.Run(() =>
        {
            var rules = new List<FirewallRule>();

            using var ps = SMA.PowerShell.Create();

            ps.AddScript($@"
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
Import-Module NetSecurity -ErrorAction Stop
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
                        Id              = result.Properties["Id"]?.Value?.ToString() ?? string.Empty,
                        IsEnabled       = result.Properties["Enabled"]?.Value as bool? == true,
                        Name            = displayName.StartsWith(RuleIdentifier, StringComparison.Ordinal)
                                              ? displayName[RuleIdentifier.Length..]
                                              : displayName,
                        Description     = result.Properties["Description"]?.Value?.ToString() ?? string.Empty,
                        Direction       = ParseDirection(result.Properties["Direction"]?.Value?.ToString()),
                        Action          = ParseAction(result.Properties["Action"]?.Value?.ToString()),
                        Protocol        = ParseProtocol(result.Properties["Protocol"]?.Value?.ToString()),
                        LocalPorts      = ParsePorts(result.Properties["LocalPort"]?.Value?.ToString()),
                        RemotePorts     = ParsePorts(result.Properties["RemotePort"]?.Value?.ToString()),
                        LocalAddresses  = ParseAddresses(result.Properties["LocalAddress"]?.Value?.ToString()),
                        RemoteAddresses = ParseAddresses(result.Properties["RemoteAddress"]?.Value?.ToString()),
                        NetworkProfiles = ParseProfile(result.Properties["Profile"]?.Value?.ToString()),
                        InterfaceType   = ParseInterfaceType(result.Properties["InterfaceType"]?.Value?.ToString()),
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

    /// <summary>Parses a PowerShell direction string to <see cref="FirewallRuleDirection"/>.</summary>
    private static FirewallRuleDirection ParseDirection(string value)
    {
        return value switch
        {
            "Outbound" => FirewallRuleDirection.Outbound,
            _          => FirewallRuleDirection.Inbound,
        };
    }

    /// <summary>Parses a PowerShell action string to <see cref="FirewallRuleAction"/>.</summary>
    private static FirewallRuleAction ParseAction(string value)
    {
        return value switch
        {
            "Allow" => FirewallRuleAction.Allow,
            _       => FirewallRuleAction.Block,
        };
    }

    /// <summary>Parses a PowerShell protocol string to <see cref="FirewallProtocol"/>.</summary>
    private static FirewallProtocol ParseProtocol(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("Any", StringComparison.OrdinalIgnoreCase))
            return FirewallProtocol.Any;

        return value.ToUpperInvariant() switch
        {
            "TCP"     => FirewallProtocol.TCP,
            "UDP"     => FirewallProtocol.UDP,
            "ICMPV4"  => FirewallProtocol.ICMPv4,
            "ICMPV6"  => FirewallProtocol.ICMPv6,
            "GRE"     => FirewallProtocol.GRE,
            "L2TP"    => FirewallProtocol.L2TP,
            _ => int.TryParse(value, out var proto) ? (FirewallProtocol)proto : FirewallProtocol.Any,
        };
    }

    /// <summary>
    /// Parses a comma-separated port string (e.g. <c>"80,443,8080-8090"</c>) to a list of
    /// <see cref="FirewallPortSpecification"/> objects. Returns an empty list for <c>Any</c> or blank input.
    /// </summary>
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
    /// Parses a PowerShell profile string (e.g. <c>"Domain, Private"</c>) to a three-element boolean array
    /// in the order Domain, Private, Public.
    /// </summary>
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
                case "Domain":  profiles[0] = true; break;
                case "Private": profiles[1] = true; break;
                case "Public":  profiles[2] = true; break;
            }
        }

        return profiles;
    }

    /// <summary>
    /// Parses a comma-separated address string (e.g. <c>"192.168.1.0/24,LocalSubnet"</c>) to a list of
    /// address strings. Returns an empty list for <c>Any</c> or blank input.
    /// </summary>
    private static List<string> ParseAddresses(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("Any", StringComparison.OrdinalIgnoreCase))
            return [];

        return [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }

    /// <summary>Parses a PowerShell interface-type string to <see cref="FirewallInterfaceType"/>.</summary>
    private static FirewallInterfaceType ParseInterfaceType(string value)
    {
        return value switch
        {
            "Wired"        => FirewallInterfaceType.Wired,
            "Wireless"     => FirewallInterfaceType.Wireless,
            "RemoteAccess" => FirewallInterfaceType.RemoteAccess,
            _              => FirewallInterfaceType.Any,
        };
    }
    #endregion
}
