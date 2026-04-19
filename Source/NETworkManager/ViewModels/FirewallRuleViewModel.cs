using NETworkManager.Models.Firewall;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for adding or editing a firewall rule in the FirewallRule dialog.
/// </summary>
public class FirewallRuleViewModel : ViewModelBase
{
    /// <summary>
    /// Creates a new instance of <see cref="FirewallRuleViewModel"/> for adding or
    /// editing a firewall rule.
    /// </summary>
    /// <param name="okCommand">OK command to save the rule.</param>
    /// <param name="cancelHandler">Cancel command to discard changes.</param>
    /// <param name="entry">Existing rule to edit; <see langword="null"/> to add a new rule.</param>
    public FirewallRuleViewModel(Action<FirewallRuleViewModel> okCommand,
        Action<FirewallRuleViewModel> cancelHandler, FirewallRule entry = null)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        Entry = entry;

        if (entry == null)
        {
            IsEnabled = true;
            Direction = FirewallRuleDirection.Inbound;
            Action = FirewallRuleAction.Allow;
            Protocol = FirewallProtocol.Any;
            InterfaceType = FirewallInterfaceType.Any;
            NetworkProfileDomain = true;
            NetworkProfilePrivate = true;
            NetworkProfilePublic = true;
        }
        else
        {
            Name = entry.Name;
            IsEnabled = entry.IsEnabled;
            Description = entry.Description ?? string.Empty;
            Direction = entry.Direction;
            Action = entry.Action;
            Protocol = entry.Protocol;
            LocalPorts = FirewallRule.PortsToString(entry.LocalPorts);
            RemotePorts = FirewallRule.PortsToString(entry.RemotePorts);
            LocalAddresses = entry.LocalAddresses.Count > 0 ? string.Join("; ", entry.LocalAddresses) : string.Empty;
            RemoteAddresses = entry.RemoteAddresses.Count > 0 ? string.Join("; ", entry.RemoteAddresses) : string.Empty;
            Program = entry.Program?.Name ?? string.Empty;
            InterfaceType = entry.InterfaceType;
            NetworkProfileDomain = entry.NetworkProfiles.Length > 0 && entry.NetworkProfiles[0];
            NetworkProfilePrivate = entry.NetworkProfiles.Length > 1 && entry.NetworkProfiles[1];
            NetworkProfilePublic = entry.NetworkProfiles.Length > 2 && entry.NetworkProfiles[2];
        }
    }

    /// <summary>
    /// OK command to save the rule.
    /// </summary>
    public ICommand OKCommand { get; }

    /// <summary>
    /// Cancel command to discard changes.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// The original firewall rule being edited, or <see langword="null"/> when adding a new rule.
    /// </summary>
    public FirewallRule Entry { get; }

    /// <summary>
    /// Protocols available in the protocol drop-down.
    /// </summary>
    public IEnumerable<FirewallProtocol> Protocols { get; } =
    [
        FirewallProtocol.Any,
        FirewallProtocol.TCP,
        FirewallProtocol.UDP,
        FirewallProtocol.ICMPv4,
        FirewallProtocol.ICMPv6,
        FirewallProtocol.GRE,
        FirewallProtocol.L2TP
    ];

    /// <summary>
    /// Directions available in the direction drop-down.
    /// </summary>
    public IEnumerable<FirewallRuleDirection> Directions { get; } = Enum.GetValues<FirewallRuleDirection>();

    /// <summary>
    /// Actions available in the action drop-down.
    /// </summary>
    public IEnumerable<FirewallRuleAction> Actions { get; } = Enum.GetValues<FirewallRuleAction>();

    /// <summary>
    /// Interface types available in the interface type drop-down.
    /// </summary>
    public IEnumerable<FirewallInterfaceType> InterfaceTypes { get; } = Enum.GetValues<FirewallInterfaceType>();

    /// <summary>
    /// Human-readable display name of the rule (without the NETworkManager_ prefix).
    /// </summary>
    public string Name
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether the rule is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Optional description of the rule.
    /// </summary>
    public string Description
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Traffic direction (Inbound or Outbound).
    /// </summary>
    public FirewallRuleDirection Direction
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Rule action (Allow or Block).
    /// </summary>
    public FirewallRuleAction Action
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Network protocol. When changed away from TCP/UDP, local and remote port fields are cleared.
    /// </summary>
    public FirewallProtocol Protocol
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            if (value is not (FirewallProtocol.TCP or FirewallProtocol.UDP))
            {
                LocalPorts = string.Empty;
                RemotePorts = string.Empty;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(PortsVisible));
        }
    }

    /// <summary>
    /// <see langword="true"/> when the current protocol supports port filtering (TCP or UDP).
    /// </summary>
    public bool PortsVisible => Protocol is FirewallProtocol.TCP or FirewallProtocol.UDP;

    /// <summary>
    /// Semicolon-separated local port numbers or ranges (e.g. "80; 443; 8080-8090").
    /// Only relevant when <see cref="Protocol"/> is TCP or UDP.
    /// </summary>
    public string LocalPorts
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Semicolon-separated remote port numbers or ranges.
    /// Only relevant when <see cref="Protocol"/> is TCP or UDP.
    /// </summary>
    public string RemotePorts
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Semicolon-separated local addresses (IPs, CIDR subnets, or keywords such as LocalSubnet).
    /// Empty means "Any".
    /// </summary>
    public string LocalAddresses
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Semicolon-separated remote addresses.
    /// Empty means "Any".
    /// </summary>
    public string RemoteAddresses
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Full path to the executable this rule applies to. Empty means "Any program".
    /// </summary>
    public string Program
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Network interface type filter.
    /// </summary>
    public FirewallInterfaceType InterfaceType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Whether the rule applies to the Domain network profile.
    /// </summary>
    public bool NetworkProfileDomain
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Whether the rule applies to the Private network profile.
    /// </summary>
    public bool NetworkProfilePrivate
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Whether the rule applies to the Public network profile.
    /// </summary>
    public bool NetworkProfilePublic
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }
}