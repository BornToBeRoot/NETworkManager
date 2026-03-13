using NETworkManager.Models.Firewall;

namespace NETworkManager.Interfaces.ViewModels;

/// <summary>
/// Interface to allow converters and validators access to the firewall rule view model,
/// in this case validating with the other network profile checkboxes.
/// </summary>
public interface IFirewallRuleViewModel
{
    public bool NetworkProfileDomain { get; }

    public bool NetworkProfilePrivate { get; }

    public bool NetworkProfilePublic { get; }

    public List<FirewallPortSpecification>? LocalPorts { get; }

    public List<FirewallPortSpecification>? RemotePorts { get; }

    public FirewallRuleProgram? Program { get; }

    public int MaxLengthName { get; }

    public string? UserDefinedName { get; }

    public bool NameHasError { get; set; }

    public bool HasError { get; }
}