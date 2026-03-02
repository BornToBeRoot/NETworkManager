using System.Collections.Generic;
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
    /// Represents the name associated with the object.
    /// </summary>
    /// <remarks>
    /// This property is used to identify the object with a descriptive, human-readable name.
    /// The value of this property is typically a string and can be used for display purposes
    /// or as an identifier in various contexts.
    /// </remarks>
    public string Name { get; set; }

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
    
    #region Methods

    /// <summary>
    /// Converts a collection of port numbers to a single, comma-separated string representation.
    /// </summary>
    /// <param name="ports">A collection of integers representing port numbers.</param>
    /// <param name="separator">Separator character to use</param>
    /// <param name="spacing">Separate entries with a space.</param>
    /// <returns>A separated string containing all the port numbers from the input collection.</returns>
    public static string PortsToString(List<FirewallPortSpecification> ports, char separator = ';',
        bool spacing = true)
    {
        if (ports.Count is 0)
            return string.Empty;
        string whitespace = spacing ? "  " : string.Empty;
        var builder = new StringBuilder();
        foreach (var port in ports)
            builder.Append($"{port}{separator}{whitespace}");
        int offset = spacing ? 2 : 1;
        return builder.ToString()[..^offset];
    }
    #endregion
}