using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

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
    private readonly ILog Log = LogManager.GetLogger(typeof(Firewall));

    #endregion

    #region Methods

    /// <summary>
    /// Applies the firewall rules specified in the list to the system's firewall settings.
    /// </summary>
    /// <param name="rules">A list of <see cref="FirewallRule"/> objects representing the firewall rules to be applied.</param>
    /// <returns>Returns <c>true</c> if the rules were successfully applied; otherwise, <c>false</c>.</returns>
    private bool ApplyRules(List<FirewallRule> rules)
    {
        // If there are no rules to apply, return true as there is nothing to do.
        if (rules.Count is 0)
            return true;

        // Start by clearing all existing rules for the current profile to ensure a clean state.
        var sb = new StringBuilder(GetClearAllRulesCommand());
        sb.Append("; ");

        foreach (var rule in rules)
        {
            try
            {
                var ruleSb = new StringBuilder($"New-NetFirewallRule -DisplayName '{SanitizeStringArguments(rule.Name)}'");

                if (!string.IsNullOrEmpty(rule.Description))
                    ruleSb.Append($" -Description '{SanitizeStringArguments(rule.Description)}'");

                ruleSb.Append($" -Direction {Enum.GetName(rule.Direction)}");

                if (rule.LocalPorts.Count > 0 && rule.Protocol is FirewallProtocol.TCP or FirewallProtocol.UDP)
                    ruleSb.Append($" -LocalPort {FirewallRule.PortsToString(rule.LocalPorts, ',', false)}");

                if (rule.RemotePorts.Count > 0 && rule.Protocol is FirewallProtocol.TCP or FirewallProtocol.UDP)
                    ruleSb.Append($" -RemotePort {FirewallRule.PortsToString(rule.RemotePorts, ',', false)}");

                ruleSb.Append(rule.Protocol is FirewallProtocol.Any
                    ? " -Protocol Any"
                    : $" -Protocol {(int)rule.Protocol}");

                if (!string.IsNullOrWhiteSpace(rule.Program?.Name))
                {
                    if (File.Exists(rule.Program.Name))
                        ruleSb.Append($" -Program '{SanitizeStringArguments(rule.Program.Name)}'");
                    else
                    {
                        Log.Warn($"Program path '{rule.Program.Name}' in rule '{rule.Name}' does not exist. Skipping rule.");
                        continue;
                    }
                }

                if (rule.InterfaceType is not FirewallInterfaceType.Any)
                    ruleSb.Append($" -InterfaceType {Enum.GetName(rule.InterfaceType)}");

                // If not all network profiles are enabled, specify the ones that are.
                if (!rule.NetworkProfiles.All(x => x))
                {
                    var profiles = Enumerable.Range(0, rule.NetworkProfiles.Length)
                        .Where(i => rule.NetworkProfiles[i])
                        .Select(i => Enum.GetName(typeof(NetworkProfiles), i));

                    ruleSb.Append($" -Profile {string.Join(',', profiles)}");
                }

                ruleSb.Append($" -Action {Enum.GetName(rule.Action)}; ");

                sb.Append(ruleSb);
            }
            catch (ArgumentException ex)
            {
                Log.Warn($"Failed to build firewall rule '{rule.Name}': {ex.Message}");
            }
        }

        // Remove the trailing "; " from the last command.
        sb.Length -= 2;

        var command = sb.ToString();

        Log.Debug($"Applying rules:{Environment.NewLine}{command}");

        try
        {
            PowerShellHelper.ExecuteCommand(command, true);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Removes all existing firewall rules associated with the specified profile.
    /// </summary>
    /// <remarks>
    /// This method clears all configured firewall rules for the current profile.
    /// It is useful for resetting the rules to a clean state. Errors or exceptions
    /// during the operation are logged using the configured logging mechanism.
    /// </remarks>
    public static void ClearAllRules()
    {
        PowerShellHelper.ExecuteCommand(GetClearAllRulesCommand(), true);
    }

    /// <summary>
    /// Generates a command string that removes all Windows Firewall rules with a display name starting with 'NwM_'.
    /// </summary>
    /// <returns>A command string that can be executed in PowerShell to remove the specified firewall rules.</returns>
    private static string GetClearAllRulesCommand()
    {
        return "Remove-NetFirewallRule -DisplayName 'NwM_*'";
    }

    /// <summary>
    /// Sanitizes string arguments by replacing single quotes with double single quotes to prevent issues in PowerShell command execution.
    /// </summary>
    /// <param name="value">The input string to be sanitized.</param>
    /// <returns>A sanitized string with single quotes escaped.</returns>
    private static string SanitizeStringArguments(string value)
    {
        return value.Replace("'", "''");
    }

    /// <summary>
    /// Applies firewall rules asynchronously by invoking the ApplyRules method in a separate task.
    /// </summary>
    /// <param name="rules">A list of firewall rules to apply.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating whether the rules were successfully applied.</returns>
    public async Task ApplyRulesAsync(List<FirewallRule> rules)
    {
        await Task.Run(() => ApplyRules(rules));
    }
    #endregion
}
