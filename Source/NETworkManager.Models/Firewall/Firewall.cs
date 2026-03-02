using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private readonly ILog _logger = LogManager.GetLogger(typeof(Firewall));
    
    #endregion
    
    #region Methods

    /// <summary>
    /// Applies the firewall rules specified in the list to the system's firewall settings.
    /// </summary>
    /// <param name="rules">A list of <see cref="FirewallRule"/> objects representing the firewall rules to be applied.</param>
    /// <returns>Returns <c>true</c> if the rules were successfully applied; otherwise, <c>false</c>.</returns>
    private bool ApplyRules(List<FirewallRule> rules)
    {
        string command = GetClearAllRulesCommand();
        if (rules.Count is 0)
            return true;
        command += "; ";
        foreach (FirewallRule rule in rules)
        {
            string nextRule = string.Empty;
            try
            {
                nextRule += $"New-NetFirewallRule -DisplayName '{rule.Name}'";
                if (!string.IsNullOrEmpty(rule.Description))
                    nextRule += $" -Description '{rule.Description}'";
                nextRule += $" -Direction {Enum.GetName(rule.Direction)}";
                if (rule.LocalPorts.Count > 0
                    && rule.Protocol is FirewallProtocol.TCP or FirewallProtocol.UDP)
                {
                    nextRule += $" -LocalPort {FirewallRule.PortsToString(rule.LocalPorts, ',', false)}";
                }

                if (rule.RemotePorts.Count > 0
                    && rule.Protocol is FirewallProtocol.TCP or FirewallProtocol.UDP)
                    nextRule += $" -RemotePort {FirewallRule.PortsToString(rule.RemotePorts, ',', false)}";
                if (rule.Protocol is FirewallProtocol.Any)
                    nextRule += $" -Protocol Any";
                else
                    nextRule += $" -Protocol {(int)rule.Protocol}";
                if (!string.IsNullOrWhiteSpace(rule.Program?.Name))
                {
                    try
                    {
                        if (File.Exists(rule.Program.Name))
                            nextRule += $" -Program '{rule.Program.Name}'";
                        else
                            continue;
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (rule.InterfaceType is not FirewallInterfaceType.Any)
                    nextRule += $" -InterfaceType {Enum.GetName(rule.InterfaceType)}";
                if (!rule.NetworkProfiles.All(x => x))
                {
                    nextRule += $" -Profile ";
                    for (int i = 0; i < rule.NetworkProfiles.Length; i++)
                    {
                        if (rule.NetworkProfiles[i])
                            nextRule += $"{Enum.GetName(typeof(NetworkProfiles), i)},";
                    }
                    nextRule = nextRule[..^1];
                }
                nextRule += $" -Action {Enum.GetName(rule.Action)}; ";
                command += nextRule;
            }
            catch (ArgumentException)
            {
            }
        }

        command = command[..^2];
        try
        {
            _logger.Info($"[Firewall] Applying rules:{Environment.NewLine}{command}");
            PowerShellHelper.ExecuteCommand(command, true);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
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
        PowerShellHelper.ExecuteCommand($"{GetClearAllRulesCommand()}", true);
    }

    private static string GetClearAllRulesCommand()
    {
        return $"Remove-NetFirewallRule -DisplayName 'NwM_*'";
    }
    
    /// <summary>
    /// Applies firewall rules asynchronously by invoking the ApplyRules method in a separate task.
    /// </summary>
    /// <param name="rules">A list of firewall rules to apply.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating whether the rules were successfully applied.</returns>
    public async Task<bool> ApplyRulesAsync(List<FirewallRule> rules)
    {
        return await Task.Run(() => ApplyRules(rules));
    }
    #endregion
}