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
    /// Shared PowerShell runspace, see <see cref="SharedRunspace"/>.
    /// </summary>
    private static Runspace _sharedRunspace;

    /// <summary>
    /// Returns the shared runspace. It is created and configured on first access, which is the initial load
    /// of the rules when the Firewall view is opened for the first time. If the runspace could not be created
    /// (e.g. the <c>NetSecurity</c> module failed to load) or is broken, it is created again on the next access,
    /// so a temporary error doesn't break the Firewall until the application is restarted.
    /// Must only be accessed while <see cref="RunspaceLock"/> is held.
    /// </summary>
    private static Runspace SharedRunspace
    {
        get
        {
            if (_sharedRunspace?.RunspaceStateInfo.State == RunspaceState.Opened)
                return _sharedRunspace;

            _sharedRunspace?.Dispose();
            _sharedRunspace = null;

            var runspace = RunspaceFactory.CreateRunspace();

            try
            {
                runspace.Open();

                using var ps = SMA.PowerShell.Create();
                ps.Runspace = runspace;
                ps.AddScript(@"
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
Import-Module NetSecurity -ErrorAction Stop").Invoke();
            }
            catch
            {
                runspace.Dispose();
                throw;
            }

            _sharedRunspace = runspace;

            return runspace;
        }
    }

    #endregion

    /// <summary>
    /// PowerShell expression that maps a rule (<c>$rule</c>) and its filters (<c>$portFilter</c>,
    /// <c>$addressFilter</c>, <c>$appFilter</c>, <c>$ifTypeFilter</c>, <c>$svcFilter</c>, <c>$ifFilter</c>, <c>$secFilter</c>)
    /// to the object parsed by <see cref="GetRulesAsync"/>.
    /// Shared by the query for rules managed by NETworkManager and the query for all rules.
    /// </summary>
    private const string RuleProjectionScript = @"[PSCustomObject]@{
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
        Program       = [string]$appFilter.Program
        SourceType    = [string]$rule.PolicyStoreSourceType
        IcmpType      = ($portFilter.IcmpType -join ',')
        Service       = [string]$svcFilter.Service
        Package       = [string]$appFilter.Package
        InterfaceAlias = ($ifFilter.InterfaceAlias -join ',')
        LocalUser      = [string]$secFilter.LocalUser
        RemoteUser     = [string]$secFilter.RemoteUser
        RemoteMachine  = [string]$secFilter.RemoteMachine
        Authentication = [string]$secFilter.Authentication
        Encryption     = [string]$secFilter.Encryption
    }";

    #region Methods

    /// <summary>
    /// Number of rules up to which the filters (ports, addresses, ...) are queried per rule. For more rules, the
    /// filters of all rules in the policy store are queried once and matched by <c>InstanceID</c>, which is faster
    /// than one query per rule and filter type, but has a fixed cost (all rules on the system, often 500+).
    /// </summary>
    private const int BulkFilterQueryThreshold = 10;

    /// <summary>
    /// Builds the PowerShell script that queries the rules and their filters and maps them with
    /// <see cref="RuleProjectionScript"/>.
    /// </summary>
    /// <param name="onlyManagedRules">
    /// <see langword="true"/> to query only rules managed by NETworkManager (local store); <see langword="false"/> to
    /// query all active rules.
    /// </param>
    private static string BuildGetRulesScript(bool onlyManagedRules)
    {
        var rulesQuery = onlyManagedRules
            ? $"Get-NetFirewallRule -DisplayName '{RuleIdentifier}*'"
            : "Get-NetFirewallRule -PolicyStore ActiveStore";

        // Get-NetFirewallRule uses the PersistentStore (local rules) by default
        var policyStore = onlyManagedRules ? "PersistentStore" : "ActiveStore";

        // All rules are always many rules, so the filters are always queried at once
        var threshold = onlyManagedRules ? BulkFilterQueryThreshold : 0;

        return $@"
$rules = @({rulesQuery})
$bulk = $rules.Count -gt {threshold}
if ($bulk) {{
    $pf = @{{}}; Get-NetFirewallPortFilter -PolicyStore {policyStore} | ForEach-Object {{ $pf[$_.InstanceID] = $_ }}
    $af = @{{}}; Get-NetFirewallAddressFilter -PolicyStore {policyStore} | ForEach-Object {{ $af[$_.InstanceID] = $_ }}
    $app = @{{}}; Get-NetFirewallApplicationFilter -PolicyStore {policyStore} | ForEach-Object {{ $app[$_.InstanceID] = $_ }}
    $if = @{{}}; Get-NetFirewallInterfaceTypeFilter -PolicyStore {policyStore} | ForEach-Object {{ $if[$_.InstanceID] = $_ }}
    $sf = @{{}}; Get-NetFirewallServiceFilter -PolicyStore {policyStore} | ForEach-Object {{ $sf[$_.InstanceID] = $_ }}
    $inf = @{{}}; Get-NetFirewallInterfaceFilter -PolicyStore {policyStore} | ForEach-Object {{ $inf[$_.InstanceID] = $_ }}
    $sec = @{{}}; Get-NetFirewallSecurityFilter -PolicyStore {policyStore} | ForEach-Object {{ $sec[$_.InstanceID] = $_ }}
    # The filters are matched by InstanceID. If an ID occurs more than once (e.g. a local and a Group Policy rule
    # with the same ID), the filters of these rules are queried per rule, so they are not mixed up.
    $count = @{{}}; foreach ($rule in $rules) {{ $count[$rule.InstanceID]++ }}
}}
$list = [System.Collections.Generic.List[PSCustomObject]]::new($rules.Count)
foreach ($rule in $rules) {{
    $id = $rule.InstanceID
    if (-not $bulk -or $count[$id] -gt 1) {{
        $portFilter    = $rule | Get-NetFirewallPortFilter
        $addressFilter = $rule | Get-NetFirewallAddressFilter
        $appFilter     = $rule | Get-NetFirewallApplicationFilter
        $ifTypeFilter  = $rule | Get-NetFirewallInterfaceTypeFilter
        $svcFilter     = $rule | Get-NetFirewallServiceFilter
        $ifFilter      = $rule | Get-NetFirewallInterfaceFilter
        $secFilter     = $rule | Get-NetFirewallSecurityFilter
    }} else {{
        $portFilter    = $pf[$id]
        $addressFilter = $af[$id]
        $appFilter     = $app[$id]
        $ifTypeFilter  = $if[$id]
        $svcFilter     = $sf[$id]
        $ifFilter      = $inf[$id]
        $secFilter     = $sec[$id]
    }}
    $list.Add({RuleProjectionScript})
}}
$list";
    }

    /// <summary>
    /// Retrieves Windows Firewall rules and maps each one to a <see cref="FirewallRule"/> object.
    /// When <paramref name="onlyManagedRules"/> is <see langword="true"/>, only rules from the local store whose display name
    /// starts with <see cref="RuleIdentifier"/> are returned; otherwise all active rules on the system are returned
    /// (including rules from Group Policy, MDM or created by Windows at runtime, see <see cref="FirewallRule.Source"/>).
    /// If the query fails completely, an exception is thrown. If only parts of it fail (e.g. a filter could not be
    /// queried or a single rule could not be parsed), the loaded rules are returned together with the errors, so the
    /// user can be told that the list may be incomplete.
    /// </summary>
    /// <param name="onlyManagedRules">
    /// <see langword="true"/> to query only rules managed by NETworkManager; <see langword="false"/> to query all system firewall rules.
    /// </param>
    /// <returns>
    /// The <see cref="FirewallRule"/> objects representing the matching rules, and the errors that occurred
    /// (empty if the rules were loaded without errors).
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when PowerShell reports errors and no rule was returned (e.g. the firewall service is not running).
    /// </exception>
    public static async Task<(List<FirewallRule> Rules, List<string> Errors)> GetRulesAsync(bool onlyManagedRules = true)
    {
        await RunspaceLock.WaitAsync();
        try
        {
            return await Task.Run(() =>
            {
                var rules = new List<FirewallRule>();

                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript(BuildGetRulesScript(onlyManagedRules));

                var results = ps.Invoke();

                // No rule found (e.g. no rules managed by NETworkManager yet) is not an error. Only for the rule query:
                // a filter that is not found (e.g. the rule was deleted while loading) would show the rule as unrestricted.
                var errors = ps.Streams.Error
                    .Where(error => !(error.FullyQualifiedErrorId?.StartsWith("CmdletizationQuery_NotFound", StringComparison.Ordinal) == true &&
                                      error.InvocationInfo?.MyCommand?.Name == "Get-NetFirewallRule"))
                    .Select(error => error.ToString())
                    .ToList();

                foreach (var error in errors)
                    Log.Warn($"PowerShell error: {error}");

                if (errors.Count > 0 && results.Count == 0)
                    throw new Exception(string.Join("; ", errors));

                foreach (var result in results)
                {
                    try
                    {
                        var displayName = result.Properties["DisplayName"]?.Value?.ToString() ?? string.Empty;
                        var source = ParseSource(result.Properties["SourceType"]?.Value?.ToString());

                        // Rules from a policy can't be changed locally, even if they have the NETworkManager prefix
                        var isManaged = source == FirewallRuleSource.Local &&
                                        displayName.StartsWith(RuleIdentifier, StringComparison.Ordinal);

                        var rule = new FirewallRule
                        {
                            Id = result.Properties["Id"]?.Value?.ToString() ?? string.Empty,
                            IsEnabled = result.Properties["Enabled"]?.Value as bool? == true,
                            IsManaged = isManaged,
                            Source = source,
                            Name = isManaged ? displayName[RuleIdentifier.Length..] : displayName,
                            Description = result.Properties["Description"]?.Value?.ToString() ?? string.Empty,
                            Direction = ParseDirection(result.Properties["Direction"]?.Value?.ToString()),
                            Action = ParseAction(result.Properties["Action"]?.Value?.ToString()),
                            Protocol = ParseProtocol(result.Properties["Protocol"]?.Value?.ToString()),
                            LocalPorts = ParsePorts(result.Properties["LocalPort"]?.Value?.ToString(), out var localPortKeywords),
                            LocalPortKeywords = localPortKeywords,
                            RemotePorts = ParsePorts(result.Properties["RemotePort"]?.Value?.ToString(), out var remotePortKeywords),
                            RemotePortKeywords = remotePortKeywords,
                            LocalAddresses = ParseAddresses(result.Properties["LocalAddress"]?.Value?.ToString()),
                            RemoteAddresses = ParseAddresses(result.Properties["RemoteAddress"]?.Value?.ToString()),
                            NetworkProfiles = ParseProfile(result.Properties["Profile"]?.Value?.ToString()),
                            InterfaceType = ParseInterfaceType(result.Properties["InterfaceType"]?.Value?.ToString(), out var interfaceTypeRaw),
                            InterfaceTypeRaw = interfaceTypeRaw,
                            HiddenProperties = ParseHiddenProperties(result),
                        };

                        // Use ToString() (not "as string"), because the value can be wrapped in a PSObject
                        var program = result.Properties["Program"]?.Value?.ToString();

                        if (!string.IsNullOrWhiteSpace(program) && !program.Equals("Any", StringComparison.OrdinalIgnoreCase))
                            rule.Program = new FirewallRuleProgram(program);

                        rules.Add(rule);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"Failed to parse firewall rule: {ex.Message}");

                        errors.Add(ex.Message);
                    }
                }

                return (rules, errors);
            });
        }
        finally
        {
            RunspaceLock.Release();
        }
    }

    /// <summary>
    /// Enables or disables the given <paramref name="rules"/> by running
    /// <c>Enable-NetFirewallRule</c> or <c>Disable-NetFirewallRule</c> against the rules'
    /// internal <see cref="FirewallRule.Id"/>. The cmdlet is run for all rules (also for rules that already
    /// seem to be in the target state, since the state may have changed outside the application).
    /// Nothing is changed if one of the rules no longer exists.
    /// </summary>
    /// <param name="rules">
    /// The firewall rules to modify.
    /// </param>
    /// <param name="enabled">
    /// <see langword="true"/> to enable the rules; <see langword="false"/> to disable them.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown when a rule no longer exists or the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static Task SetRulesEnabledAsync(IEnumerable<FirewallRule> rules, bool enabled)
    {
        return InvokeRuleCommandAsync(enabled ? "Enable-NetFirewallRule" : "Disable-NetFirewallRule",
            rules.Select(rule => rule.Id).ToList());
    }

    /// <summary>
    /// Permanently removes the given <paramref name="rules"/> by running
    /// <c>Remove-NetFirewallRule</c> against the rules' internal <see cref="FirewallRule.Id"/>.
    /// Nothing is deleted if one of the rules no longer exists.
    /// </summary>
    /// <param name="rules">
    /// The firewall rules to delete.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown when a rule no longer exists or the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static Task DeleteRulesAsync(IEnumerable<FirewallRule> rules)
    {
        return InvokeRuleCommandAsync("Remove-NetFirewallRule", rules.Select(rule => rule.Id).ToList());
    }

    /// <summary>
    /// Runs the given firewall cmdlet (e.g. <c>Enable-NetFirewallRule</c>) for all rule IDs in a
    /// single PowerShell call. It is checked first that all rules still exist, otherwise nothing is changed.
    /// This is not a transaction: if the cmdlet fails for a single rule (e.g. access denied), the other
    /// rules may already be changed.
    /// </summary>
    /// <param name="command">
    /// The cmdlet to run. Must accept the rules via the pipeline.
    /// </param>
    /// <param name="ids">
    /// The internal IDs of the rules. Nothing is done if the list is empty.
    /// </param>
    /// <exception cref="FirewallRuleNotFoundException">
    /// Thrown when a rule no longer exists.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    private static async Task InvokeRuleCommandAsync(string command, IReadOnlyCollection<string> ids)
    {
        if (ids.Count == 0)
            return;

        await RunspaceLock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript($@"
{BuildGetRulesByIdScript(ids)}
$rules | {command}");
                var results = ps.Invoke();

                ThrowIfRuleNotFound(results);

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
    /// Builds a PowerShell script that gets the rules with the given IDs and stores them in <c>$rules</c>.
    /// If one of the rules no longer exists, <see cref="RuleNotFoundMarker"/> is returned and the script is stopped,
    /// so nothing is changed in this case (see <see cref="ThrowIfRuleNotFound"/>).
    /// The IDs of rules not created by NETworkManager can contain wildcard characters (e.g. <c>"[x64]"</c> in
    /// rules created by Windows for a program). Since <c>-Name</c> supports wildcards (including the backtick as
    /// escape character), these rules are found by comparing the names instead, otherwise a different or no rule could be changed.
    /// </summary>
    /// <param name="ids">
    /// The internal IDs of the rules.
    /// </param>
    private static string BuildGetRulesByIdScript(IReadOnlyCollection<string> ids) => $@"
$ids = {ToPsArray(ids)}
if ($ids | Where-Object {{ $_ -match '[\[\]\*\?`]' }}) {{
    $rules = @(Get-NetFirewallRule | Where-Object {{ $ids -contains $_.Name }})
}} else {{
    $rules = @(Get-NetFirewallRule -Name $ids -ErrorAction SilentlyContinue)
}}
if ($rules.Count -ne $ids.Count) {{
    return '{RuleNotFoundMarker}'
}}";

    /// <summary>
    /// Value returned by the script of <see cref="BuildGetRulesByIdScript"/> if a rule no longer exists.
    /// </summary>
    private const string RuleNotFoundMarker = "NETworkManager_FirewallRuleNotFound";

    /// <summary>
    /// Throws a <see cref="FirewallRuleNotFoundException"/> if the script of <see cref="BuildGetRulesByIdScript"/>
    /// returned <see cref="RuleNotFoundMarker"/>.
    /// </summary>
    /// <param name="results">The results of the PowerShell invocation.</param>
    private static void ThrowIfRuleNotFound(IEnumerable<SMA.PSObject> results)
    {
        if (results.Any(result => result?.BaseObject is string value && value == RuleNotFoundMarker))
            throw new FirewallRuleNotFoundException();
    }

    /// <summary>
    /// Updates the existing rule <paramref name="original"/> with the properties of <paramref name="updated"/>
    /// using <c>Set-NetFirewallRule</c>. Only changed properties are set, so properties that are not
    /// supported by NETworkManager (e.g. rule group, service or ICMP types) and the internal ID are kept.
    /// Nothing is done if no property was changed.
    /// </summary>
    /// <param name="original">
    /// The rule as it was loaded (used to identify the rule and to detect the changes).
    /// </param>
    /// <param name="updated">
    /// The rule with the new properties.
    /// </param>
    /// <exception cref="FirewallRuleNotFoundException">
    /// Thrown when the rule no longer exists.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static async Task UpdateRuleAsync(FirewallRule original, FirewallRule updated)
    {
        var paramsScript = BuildUpdateParamsScript(original, updated);

        if (paramsScript == null)
            return;

        await RunspaceLock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript($@"
{BuildGetRulesByIdScript([original.Id])}
{paramsScript}
$rules | Set-NetFirewallRule @params");
                var results = ps.Invoke();

                ThrowIfRuleNotFound(results);

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
    /// Builds the PowerShell hashtable <c>$params</c> for <c>Set-NetFirewallRule</c> with all properties
    /// that differ between <paramref name="original"/> and <paramref name="updated"/>.
    /// </summary>
    /// <param name="original">The rule as it was loaded.</param>
    /// <param name="updated">The rule with the new properties.</param>
    /// <returns>The script, or <see langword="null"/> if no property was changed.</returns>
    private static string BuildUpdateParamsScript(FirewallRule original, FirewallRule updated)
    {
        var sb = new StringBuilder();

        void Add(string key, string value) => sb.AppendLine($"$params['{key}'] = {value}");

        if (!string.Equals(original.Name, updated.Name, StringComparison.Ordinal))
        {
            var prefix = original.IsManaged ? RuleIdentifier : string.Empty;
            Add("NewDisplayName", $"'{prefix}{PowerShellHelper.EscapeSingleQuotes(updated.Name)}'");
        }

        if (original.IsEnabled != updated.IsEnabled)
            Add("Enabled", $"'{(updated.IsEnabled ? "True" : "False")}'");

        if (!string.Equals(original.Description ?? string.Empty, updated.Description ?? string.Empty, StringComparison.Ordinal))
            Add("Description", $"'{PowerShellHelper.EscapeSingleQuotes(updated.Description ?? string.Empty)}'");

        if (original.Direction != updated.Direction)
            Add("Direction", $"'{updated.Direction}'");

        if (original.Action != updated.Action)
            Add("Action", $"'{updated.Action}'");

        var protocolChanged = original.Protocol != updated.Protocol;

        if (protocolChanged)
        {
            Add("Protocol", $"'{GetProtocolString(updated.Protocol)}'");

            // ICMP types (e.g. set in the Windows Firewall management console) are only valid for the ICMP protocol
            // they were set for, so they are reset if the protocol is changed (otherwise the change could be rejected)
            Add("IcmpType", "'Any'");
        }

        // Ports are only allowed for TCP / UDP, so they are reset to "Any" if the protocol is changed to another one
        var portsAllowed = updated.Protocol is FirewallProtocol.TCP or FirewallProtocol.UDP;

        if (protocolChanged || PortsChanged(original.LocalPorts, updated.LocalPorts))
            Add("LocalPort", portsAllowed && updated.LocalPorts.Count > 0
                ? ToPsArray(updated.LocalPorts.Select(p => p.ToString()))
                : "'Any'");

        if (protocolChanged || PortsChanged(original.RemotePorts, updated.RemotePorts))
            Add("RemotePort", portsAllowed && updated.RemotePorts.Count > 0
                ? ToPsArray(updated.RemotePorts.Select(p => p.ToString()))
                : "'Any'");

        if (AddressesChanged(original.LocalAddresses, updated.LocalAddresses))
            Add("LocalAddress", updated.LocalAddresses.Count > 0 ? ToPsArray(updated.LocalAddresses) : "'Any'");

        if (AddressesChanged(original.RemoteAddresses, updated.RemoteAddresses))
            Add("RemoteAddress", updated.RemoteAddresses.Count > 0 ? ToPsArray(updated.RemoteAddresses) : "'Any'");

        if (!string.Equals(original.Program?.Name, updated.Program?.Name, StringComparison.OrdinalIgnoreCase))
            Add("Program", updated.Program != null && !string.IsNullOrWhiteSpace(updated.Program.Name)
                ? $"'{PowerShellHelper.EscapeSingleQuotes(updated.Program.Name)}'"
                : "'Any'");

        if (original.InterfaceType != updated.InterfaceType)
            Add("InterfaceType", $"'{GetInterfaceTypeString(updated.InterfaceType)}'");

        if (!original.NetworkProfiles.SequenceEqual(updated.NetworkProfiles))
            Add("Profile", $"'{GetProfileString(updated.NetworkProfiles)}'");

        if (sb.Length == 0)
            return null;

        return "$params = @{}" + Environment.NewLine + sb;
    }

    /// <summary>
    /// Indicates whether the ports differ (in the given order).
    /// </summary>
    private static bool PortsChanged(List<FirewallPortSpecification> original, List<FirewallPortSpecification> updated) =>
        FirewallRule.PortsToString(original, ',', false) != FirewallRule.PortsToString(updated, ',', false);

    /// <summary>
    /// Indicates whether the addresses differ (in the given order, case-insensitive).
    /// </summary>
    private static bool AddressesChanged(List<string> original, List<string> updated) =>
        !string.Equals(string.Join(",", original), string.Join(",", updated), StringComparison.OrdinalIgnoreCase);

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
        // Only TCP, UDP, ICMPv4 and ICMPv6 are documented as names, all other protocols are passed by number
        FirewallProtocol.GRE => "47",
        FirewallProtocol.L2TP => "115",
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
    /// Port keywords (e.g. <c>"RPC"</c>, <c>"RPCEPMap"</c>, <c>"IPHTTPSIn"</c>), which are used by
    /// Windows built-in rules but are not supported by NETworkManager, are returned in <paramref name="keywords"/>.
    /// </summary>
    /// <param name="value">
    /// The raw comma-separated port string returned by PowerShell.
    /// </param>
    /// <param name="keywords">
    /// The port keywords found in <paramref name="value"/>.
    /// </param>
    private static List<FirewallPortSpecification> ParsePorts(string value, out List<string> keywords)
    {
        var list = new List<FirewallPortSpecification>();
        keywords = [];

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
            else
            {
                keywords.Add(token);
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
    /// Determines the <see cref="FirewallRuleHiddenProperties"/> of a rule. A restriction is set if its value
    /// is not empty and not <c>"Any"</c> (authentication / encryption: not <c>"NotRequired"</c>).
    /// </summary>
    /// <param name="result">The rule object returned by PowerShell (see <see cref="RuleProjectionScript"/>).</param>
    private static FirewallRuleHiddenProperties ParseHiddenProperties(SMA.PSObject result)
    {
        string Get(string name) => result.Properties[name]?.Value?.ToString();

        static bool IsSet(string value) =>
            !string.IsNullOrWhiteSpace(value) && !value.Equals("Any", StringComparison.OrdinalIgnoreCase);

        static bool IsRequired(string value) =>
            !string.IsNullOrWhiteSpace(value) && !value.Equals("NotRequired", StringComparison.OrdinalIgnoreCase);

        var properties = FirewallRuleHiddenProperties.None;

        if (IsSet(Get("IcmpType")))
            properties |= FirewallRuleHiddenProperties.IcmpTypes;

        if (IsSet(Get("Service")))
            properties |= FirewallRuleHiddenProperties.Service;

        if (IsSet(Get("Package")))
            properties |= FirewallRuleHiddenProperties.Package;

        if (IsSet(Get("InterfaceAlias")))
            properties |= FirewallRuleHiddenProperties.InterfaceAlias;

        if (IsSet(Get("LocalUser")) || IsSet(Get("RemoteUser")) || IsSet(Get("RemoteMachine")) ||
            IsRequired(Get("Authentication")) || IsRequired(Get("Encryption")))
            properties |= FirewallRuleHiddenProperties.Security;

        return properties;
    }

    /// <summary>
    /// Parses a PowerShell policy store source type string (e.g. <c>"Local"</c>, <c>"GroupPolicy"</c>) to a
    /// <see cref="FirewallRuleSource"/> value. Defaults to <see cref="FirewallRuleSource.Local"/> if empty
    /// and to <see cref="FirewallRuleSource.Other"/> for other sources (e.g. <c>"Dynamic"</c>, <c>"Hardcoded"</c>).
    /// </summary>
    /// <param name="value">
    /// The raw string value returned by PowerShell.
    /// </param>
    private static FirewallRuleSource ParseSource(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return FirewallRuleSource.Local;

        return value switch
        {
            "Local" => FirewallRuleSource.Local,
            "GroupPolicy" => FirewallRuleSource.GroupPolicy,
            "MDM" => FirewallRuleSource.MDM,
            _ => FirewallRuleSource.Other,
        };
    }

    /// <summary>
    /// Parses a PowerShell interface-type string (e.g. <c>"Wired"</c>) to a
    /// <see cref="FirewallInterfaceType"/> value. Defaults to <see cref="FirewallInterfaceType.Any"/>.
    /// A combination of interface types (e.g. <c>"Wired, Wireless"</c>) is not supported by NETworkManager;
    /// in this case <see cref="FirewallInterfaceType.Any"/> is returned and the raw value is returned in
    /// <paramref name="raw"/>. A combination of all interface types is the same as <c>"Any"</c>.
    /// </summary>
    /// <param name="value">
    /// The raw string value returned by PowerShell.
    /// </param>
    /// <param name="raw">
    /// The raw value if it is an unsupported combination of interface types; otherwise, <see langword="null"/>.
    /// </param>
    private static FirewallInterfaceType ParseInterfaceType(string value, out string raw)
    {
        raw = null;

        if (value != null && value.Contains(','))
        {
            var types = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (!(types.Contains("Wired") && types.Contains("Wireless") && types.Contains("RemoteAccess")))
                raw = value;

            return FirewallInterfaceType.Any;
        }

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