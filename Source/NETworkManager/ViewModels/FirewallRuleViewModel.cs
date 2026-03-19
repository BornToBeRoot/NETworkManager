using System.IO;

namespace NETworkManager.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Localization.Resources;
using Models.Firewall;
using NW = Models.Network;
using NETworkManager.Interfaces.ViewModels;
using Settings;
using Utilities;

/// <summary>
/// ViewModel for a firewall rule
/// </summary>
public class FirewallRuleViewModel : ViewModelBase, ICloneable, IFirewallRuleViewModel
{
    #region Variables

    /// <summary>
    /// Reflected access to converter.
    /// </summary>
    private static IValueConverter PortRangeToPortSpecificationConverter
    {
        get
        {
            if (field is not null) return field;
            var type = Type.GetType(
                "NETworkManager.Converters.PortRangeToPortSpecificationConverter, NETworkManager.Converters");

            if (type is null) return field;
            var ctor = Expression.New(type);
            var lambda = Expression.Lambda<Func<IValueConverter>>(ctor);
            field = lambda.Compile().Invoke();

            return field;
        }
    }

    /// <summary>
    /// Represents the underlying firewall rule associated with the configuration.
    /// </summary>
    [NotNull]
    private readonly FirewallRule _rule = new();

    public bool NameHasError
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool DescriptionHasError
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool ProgramHasError
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    private bool ProfilesHaveError
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => NameHasError || DescriptionHasError || ProgramHasError || ProfilesHaveError;

    /// <summary>
    /// Represents the name or identifier associated with an entity or object.
    /// </summary>
    public string Name
    {
        get => _rule.Name;
        set
        {
            if (value == _rule.Name)
                return;
            _rule.Name = value;
            ValidateName();
            OnPropertyChanged();
            OnRuleChangedEvent();
        }
    }

    /// <summary>
    /// Optionally describe the firewall rule with this property
    /// </summary>
    public string Description
    {
        get => _rule.Description;
        set
        {
            if (value == _rule.Description)
                return;
            _rule.Description = value;
            OnRuleChangedEvent();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Name of the currently loaded profile
    /// </summary>
    public string ProfileName
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            UpdateRuleName();
            OnPropertyChanged();
            OnPropertyChanged(nameof(MaxLengthName));
            OnPropertyChanged(nameof(UserDefinedName));
            OnRuleChangedEvent();
        }
    }

    private string _userDefineName;
    /// <summary>
    /// Name override for the firewall DisplayName
    /// </summary>
    public string UserDefinedName
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            if (value?.Length <= MaxLengthName)
                _userDefineName = value;
            ValidateName();
            OnPropertyChanged();
            UpdateRuleName();
            OnRuleChangedEvent();
        }
    }

    /// <summary>
    /// Max length of the firewall DisplayName
    /// </summary>
    public int MaxLengthName =>
        9999 - "NwM__".Length - ProfileName?.Length ??
        9999 - "NwM__".Length - "Default".Length;

    /// <summary>
    /// Default name shown in the field watermark and used, if no <see cref="UserDefinedName"/> is provided.
    /// </summary>
    public string DefaultName
    {
        get;
        private set
        {
            if (field == value)
                return;
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Firewall protocol to apply the rule to.
    /// </summary>
    public FirewallProtocol Protocol
    {
        get => _rule.Protocol;
        set
        {
            if (value == _rule.Protocol)
                return;
            _rule.Protocol = value;
            UpdateRuleName();
            OnPropertyChanged();
            OnPropertyChanged(nameof(PortsEnabled));
            OnRuleChangedEvent();
        }
    }

    /// <summary>
    /// Specifies the direction of traffic flow for the rule.
    /// </summary>
    public FirewallRuleDirection Direction
    {
        get => _rule.Direction;
        set
        {
            if (value == _rule.Direction)
                return;
            _rule.Direction = value;
            UpdateRuleName();
            OnPropertyChanged();
            OnRuleChangedEvent();
        }
    }

    /// <summary>
    /// Program for which the rule applies.
    /// </summary>
    public FirewallRuleProgram Program
    {
        get => _rule.Program;
        set
        {
            if (value == _rule.Program)
                return;
            _rule.Program = value;
            ValidateProgramPath();
            UpdateRuleName();
            OnPropertyChanged();
            OnRuleChangedEvent();
        }
    }

    /// <summary>
    /// Binding field for the port input fields to be activated.
    /// </summary>
    public bool PortsEnabled => Protocol is FirewallProtocol.TCP or FirewallProtocol.UDP;

    /// <summary>
    /// Specifies the local ports that the rule applies to.
    /// </summary>
    public List<FirewallPortSpecification> LocalPorts
    {
        get => _rule.LocalPorts;
        set
        {
            _rule.LocalPorts = value;
            UpdateRuleName();
            OnPropertyChanged();
            OnRuleChangedEvent();
        }
    }

    /// <summary>
    /// Index of the <see cref="LocalPorts" /> history combobox.
    /// </summary>
    public int LocalPortsIndex
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            OnPropertyChanged();
        }
    } = -1;

    /// <summary>
    /// Specifies the remote ports that the rule applies to.
    /// </summary>
    public List<FirewallPortSpecification> RemotePorts
    {
        get => _rule.RemotePorts;
        set
        {
            _rule.RemotePorts = value;
            UpdateRuleName();
            OnPropertyChanged();
            OnRuleChangedEvent();
        }
    }

    /// <summary>
    /// Index of the <see cref="RemotePorts" /> history combobox.
    /// </summary>
    public int RemotePortsIndex
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            OnPropertyChanged();
        }
    } = -1;

    /// <summary>
    /// View for <see cref="LocalPorts" /> history combobox.
    /// </summary>
    public ICollectionView LocalPortsHistoryView
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
    /// View for <see cref="RemotePorts" /> history combobox.
    /// </summary>
    public ICollectionView RemotePortsHistoryView
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
    /// Local port history.
    /// </summary>
    public static ObservableCollection<string> LocalPortsHistory
    {
        get;
        set;
    } = [];

    /// <summary>
    /// Remote port history.
    /// </summary>
    public static ObservableCollection<string> RemotePortsHistory
    {
        get;
        set;
    } = [];

    /// <summary>
    /// View for the combination of <see cref="LocalPorts" /> and <see cref="RemotePorts" /> history.
    /// </summary>
    public static ObservableCollection<string> CombinedPortsHistory
    {
        get;
    } = [];

    private string _lastLocalPortValue = string.Empty;
    private string _lastRemotePortValue = string.Empty;
    private readonly bool _isInit;

    /// <summary>
    /// Watermark for the port input fields.
    /// </summary>
    public string PortWatermark
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
    /// Checkbox for the domain network profile.
    /// </summary>
    public bool NetworkProfileDomain
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            if (_isInit)
                return;
            // Temporarily apply the change to a copy to check validity
            var newProfiles = _rule.NetworkProfiles.ToArray();
            newProfiles[(int)NW.NetworkProfiles.Domain] = value;

            if (newProfiles.Any(x => x))
            {
                NetworkProfiles[(int)NW.NetworkProfiles.Domain] = value;
                NetworkProfiles[(int)NW.NetworkProfiles.Private] = NetworkProfilePrivate;
                NetworkProfiles[(int)NW.NetworkProfiles.Public] = NetworkProfilePublic;
                ProfilesHaveError = false;
                UpdateRuleName();
                OnRuleChangedEvent();
            }
            else
            {
                ProfilesHaveError = true;
            }
            OnPropertyChanged();
            OnPropertyChanged(nameof(NetworkProfilePrivate));
            OnPropertyChanged(nameof(NetworkProfilePublic));
            OnPropertyChanged(nameof(NetworkProfiles));
        }
    }

    /// <summary>
    /// Checkbox for the private network profile.
    /// </summary>
    public bool NetworkProfilePrivate
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            if (_isInit)
                return;
            var newProfiles = _rule.NetworkProfiles.ToArray();
            newProfiles[(int)NW.NetworkProfiles.Private] = value;

            if (newProfiles.Any(x => x))
            {
                NetworkProfiles[(int)NW.NetworkProfiles.Domain] = NetworkProfileDomain;
                NetworkProfiles[(int)NW.NetworkProfiles.Private] = value;
                NetworkProfiles[(int)NW.NetworkProfiles.Public] = NetworkProfilePublic;
                ProfilesHaveError = false;
                UpdateRuleName();
                OnRuleChangedEvent();
            }
            else
            {
                ProfilesHaveError = true;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(NetworkProfileDomain));
            OnPropertyChanged(nameof(NetworkProfilePublic));
            OnPropertyChanged(nameof(NetworkProfiles));
        }
    }

    /// <summary>
    /// Checkbox for the public network profile.
    /// </summary>
    public bool NetworkProfilePublic
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            if (_isInit)
                return;
            var newProfiles = _rule.NetworkProfiles.ToArray();
            newProfiles[(int)NW.NetworkProfiles.Public] = value;

            if (newProfiles.Any(x => x))
            {
                NetworkProfiles[(int)NW.NetworkProfiles.Domain] = NetworkProfileDomain;
                NetworkProfiles[(int)NW.NetworkProfiles.Private] = NetworkProfilePrivate;
                NetworkProfiles[(int)NW.NetworkProfiles.Public] = value;
                ProfilesHaveError = false;
                UpdateRuleName();
                OnRuleChangedEvent();
            }
            else
            {
                ProfilesHaveError = true;
            }
            OnPropertyChanged();
            OnPropertyChanged(nameof(NetworkProfileDomain));
            OnPropertyChanged(nameof(NetworkProfilePrivate));
            OnPropertyChanged(nameof(NetworkProfiles));
        }
    }

    /// <summary>
    /// Combination of all checkboxes for network profiles.
    /// </summary>
    public bool[] NetworkProfiles
    {
        get => _rule.NetworkProfiles;
        init
        {
            if (value == _rule.NetworkProfiles)
                return;
            _isInit = true;
            _rule.NetworkProfiles = value;
            NetworkProfileDomain = value[(int)NW.NetworkProfiles.Domain];
            NetworkProfilePrivate = value[(int)NW.NetworkProfiles.Private];
            NetworkProfilePublic = value[(int)NW.NetworkProfiles.Public];
            _isInit = false;
            OnPropertyChanged();
            OnPropertyChanged(nameof(NetworkProfileDomain));
            OnPropertyChanged(nameof(NetworkProfilePrivate));
            OnPropertyChanged(nameof(NetworkProfilePublic));
            OnRuleChangedEvent();
        }
    }

    /// <summary>
    /// Interface type filter for the firewall rule.
    /// </summary>
    public FirewallInterfaceType InterfaceType
    {
        get => _rule.InterfaceType;
        set
        {
            if (value == _rule.InterfaceType)
                return;
            _rule.InterfaceType = value;
            OnPropertyChanged();
            UpdateRuleName();
            OnRuleChangedEvent();
        }
    }

    /// <summary>
    /// Action to execute when the rule is applied.
    /// </summary>
    public FirewallRuleAction Action
    {
        get => _rule.Action;
        set
        {
            if (value == _rule.Action)
                return;
            _rule.Action = value;
            UpdateRuleName();
            OnPropertyChanged();
            OnRuleChangedEvent();
        }
    }

    public int MaxLengthHistory
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
    #endregion

    #region Constructor

    /// <summary>
    /// Represents a view model for a firewall rule that provides details
    /// about the rule's configuration and state for user interface bindings.
    /// </summary>
    public FirewallRuleViewModel()
    {
        NetworkProfiles = [true, true, true];
        ProfileName = "Default";
        PortWatermark = StaticStrings.ExamplePortScanRange;
        
        // Set the collection views for port histories
            LocalPortsHistoryView = CollectionViewSource.
                GetDefaultView(LocalPortsHistory);
            RemotePortsHistoryView = CollectionViewSource.
                GetDefaultView(RemotePortsHistory);
    }

    /// <summary>
    /// Construct a rule from a Firewall rule and a profile name. 
    /// </summary>
    /// <param name="rule">The rule to get data from.</param>
    /// <param name="profileName">The profile name to use.</param>
    public FirewallRuleViewModel(FirewallRule rule, string profileName = null) : this()
    {
        Direction = rule.Direction;
        Protocol = rule.Protocol;
        LocalPorts = rule.LocalPorts;
        RemotePorts = rule.RemotePorts;
        /*
        if (SettingsManager.Current.Firewall_CombinePortHistory)
        {
            char separator = SettingsManager.Current.Firewall_UseWindowsPortSyntax ? ',' : ';';
            LocalPortsIndex = CombinedPortsHistory.IndexOf(FirewallRule.PortsToString(LocalPorts, separator));
            RemotePortsIndex = CombinedPortsHistory.IndexOf(FirewallRule.PortsToString(RemotePorts, separator));
        }
        */
        Program = rule.Program;
        Description = rule.Description;
        Action = rule.Action;
        InterfaceType = rule.InterfaceType;
        NetworkProfiles = rule.NetworkProfiles;
        ProfileName = profileName;
        UpdateRuleName();
        string ruleName = rule.Name.Substring("NwM_".Length);
        ruleName = ruleName.Substring(0, ruleName.LastIndexOf('_'));
        if (DefaultName != ruleName)
            UserDefinedName = ruleName;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the firewall rule's name based on the current configuration.
    /// </summary>
    private void UpdateRuleName()
    {
        StringBuilder resultBuilder = new();
        if (!string.IsNullOrWhiteSpace(_userDefineName))
        {
            resultBuilder.Append(_userDefineName);
        }
        else
        {
            string nextToken;
            var direction = Direction switch
            {
                FirewallRuleDirection.Inbound => "in",
                FirewallRuleDirection.Outbound => "out",
                _ => null
            };
            if (direction is not null)
                resultBuilder.Append(direction);
            resultBuilder.Append($"_{Protocol}");
            if (Protocol is FirewallProtocol.TCP or FirewallProtocol.UDP)
            {
                if (LocalPorts?.Count is 0 && RemotePorts?.Count is 0)
                {
                    resultBuilder.Append("_any");
                }
                else
                {
                    char separator = ';';
                    if (LocalPorts?.Count > 0)
                    {
                        nextToken = $"_loc:{FirewallRule.PortsToString(LocalPorts, separator, false)}";
                        if (nextToken.Length > 20)
                            nextToken = $"{nextToken[..20]}...";
                        resultBuilder.Append(nextToken);
                    }

                    if (RemotePorts?.Count > 0)
                    {
                        nextToken = $"_rem:{FirewallRule.PortsToString(RemotePorts, separator, false)}";
                        if (nextToken.Length > 20)
                            nextToken = $"{nextToken[..20]}...";
                        resultBuilder.Append(nextToken);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Program?.Executable?.Name))
            {
                nextToken = $"_{Program.Executable.Name}";
                if (nextToken.Length > 30)
                    nextToken = $"{nextToken[..30]}...";
                resultBuilder.Append(nextToken);
            }

            if (NetworkProfiles.Any(x => x))
            {
                resultBuilder.Append('_');
                if (NetworkProfiles.All(x => x))
                {
                    resultBuilder.Append("all");
                }
                else
                {
                    if (NetworkProfiles[(int)NW.NetworkProfiles.Domain])
                        resultBuilder.Append("dom,");
                    if (NetworkProfiles[(int)NW.NetworkProfiles.Private])
                        resultBuilder.Append("prv,");
                    if (NetworkProfiles[(int)NW.NetworkProfiles.Public])
                        resultBuilder.Append("pub");
                    if (resultBuilder[^1] == ',')
                        resultBuilder.Remove(resultBuilder.Length - 1, 1);
                }
            }
            string type = InterfaceType switch
            {
                FirewallInterfaceType.RemoteAccess => "vpn",
                FirewallInterfaceType.Wired => "wire",
                FirewallInterfaceType.Wireless => "wifi",
                _ => null
            };
            if (type is not null)
                resultBuilder.Append($"_if:{type}");
            string action = Action switch
            {
                FirewallRuleAction.Allow => "acc",
                FirewallRuleAction.Block => "blk",
                _ => null
            };
            if (action is not null)
                resultBuilder.Append($"_{action}");
        }

        string defaultName = resultBuilder.ToString();
        if (defaultName.Length > MaxLengthName)
            defaultName = $"{defaultName[..(MaxLengthName - 3)]}...";
        Name = $"NwM_{defaultName}_{ProfileName ?? "Default"}";
        DefaultName = defaultName;
    }

    /// <summary>
    /// Sets the error state while the view is not in the VisualTree, which can happen
    /// in the ProfileChildWindow until the tab has been opened.
    /// </summary>
    private void ValidateName()
    {
        if (string.IsNullOrEmpty(UserDefinedName) || UserDefinedName.Length <= MaxLengthName)
        {
            NameHasError = false;
            return;
        }

        NameHasError = true;
    }

    private void ValidateProgramPath()
    {
        if (Program?.Executable?.FullName is not { } strValue)
        {
            ProgramHasError = false;
            return;
        }
        ProgramHasError = !File.Exists(strValue);
    }

    /// Converts the current instance of the FirewallRuleViewModel to a FirewallRule object.
    /// <returns>
    /// A <see cref="FirewallRule"/> object representing the current <see cref="FirewallRuleViewModel"/> instance.
    /// </returns>
    public FirewallRule ToRule(bool toLoadOrSave = false)
    {
        ValidateProgramPath();
        ValidateName();
        return HasError && !toLoadOrSave ? null : _rule;
    }

    /// <summary>
    /// Retrieves the localized translation for a given enumeration value.
    /// </summary>
    /// <param name="enumType">The enumeration type to translate.</param>
    /// <returns>The localized string corresponding to the provided enumeration value.</returns>
    public static string[] GetEnumTranslation(Type enumType)
    {
        if (!enumType.IsEnum)
            return null;

        var enumStrings = Enum.GetNames(enumType);
        var transStrings = new string[enumStrings.Length];
        for (int i = 0; i < enumStrings.Length; i++)
            transStrings[i] = Strings.ResourceManager.GetString(enumStrings[i], Strings.Culture) ?? enumStrings[i];

        return transStrings;
    }

    /// <summary>
    /// Store the current port values to spare them from deletion on collection clearing.
    /// </summary>
    public void StorePortValues()
    {
        // Store original port values
        var converter = PortRangeToPortSpecificationConverter;
        _lastLocalPortValue = converter.ConvertBack(LocalPorts, typeof(string), null, null) as string;
        _lastRemotePortValue = converter.ConvertBack(RemotePorts, typeof(string), null, null) as string;
    }

    /// <summary>
    /// Restore the port values after the history has been modified.
    /// </summary>
    public void RestorePortValues()
    {
        var converter = PortRangeToPortSpecificationConverter;
        // Restore the original field values
        if (!string.IsNullOrWhiteSpace(_lastLocalPortValue))
        {
            // Find appropriate index
            int tmpLocalIndex = LocalPortsHistory.IndexOf(_lastLocalPortValue);
            // Restore field value 
            if (tmpLocalIndex != -1
                && converter.Convert(_lastLocalPortValue, typeof(List<FirewallPortSpecification>),
                    null, null) is List<FirewallPortSpecification> convertedPorts)
                LocalPorts = convertedPorts;
            LocalPortsIndex = tmpLocalIndex;


        }
        // Reset stored value
        _lastLocalPortValue = string.Empty;

        // Same for remote ports
        if (!string.IsNullOrWhiteSpace(_lastRemotePortValue))
        {
            int tmpRemoteIndex = RemotePortsHistory.IndexOf(_lastRemotePortValue);
            if (tmpRemoteIndex != -1
                && converter.Convert(_lastRemotePortValue, typeof(List<FirewallPortSpecification>),
                    null, null) is List<FirewallPortSpecification> convertedPorts)
                RemotePorts = convertedPorts;
            RemotePortsIndex = tmpRemoteIndex;
        }
        // Reset stored value
        _lastRemotePortValue = string.Empty;
    }

    /// <summary>
    /// Add ports to history.
    /// </summary>
    /// <param name="ports">Port list to add.</param>
    /// <param name="firewallPortType">Type of port history to add to.</param>
    public void AddPortsToHistory(string ports, FirewallPortLocation firewallPortType)
    {
        OnAddingPortsToHistoryEvent();
        ObservableCollection<string> portHistory;
        switch (firewallPortType)
        {
            case FirewallPortLocation.LocalPorts:
                portHistory = LocalPortsHistory;
                break;
            case FirewallPortLocation.RemotePorts:
                portHistory = RemotePortsHistory;
                break;
            default:
                return;
        }

        // Create the new list
        var list = ListHelper.Modify(portHistory.ToList(), ports,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        portHistory.Clear();

        // Raise property changed again after the collection has been cleared
        switch (firewallPortType)
        {
            case FirewallPortLocation.LocalPorts:
                OnPropertyChanged(nameof(LocalPortsHistoryView));
                break;
            case FirewallPortLocation.RemotePorts:
                OnPropertyChanged(nameof(RemotePortsHistoryView));
                break;
        }

        // Fill with the new items
        list.ForEach(x => portHistory.Add(x));

        // Update history config
        /*
        switch (firewallPortType)
        {
            case FirewallPortLocation.LocalPorts:
                LocalPortsHistory = portHistory;
                SettingsManager.Current.Firewall_LocalPortsHistoryConfig = list;
                FirewallSettingsViewModel.Instance.LocalPortsHaveItems = true;
                break;
            case FirewallPortLocation.RemotePorts:
                RemotePortsHistory = portHistory;
                SettingsManager.Current.Firewall_RemotePortsHistoryConfig = list;
                FirewallSettingsViewModel.Instance.RemotePortsHaveItems = true;
                break;
        }
        */

        // Update the combined history if configured
        
        OnAddedPortsToHistoryEvent();
    }

    /// <summary>
    /// Update or create the combined port history.
    /// </summary>
    public static void UpdateCombinedPortsHistory()
    {
        // This will as a side effect reset all unchanged combobox fields in all rules, because its source is empty
        // StorePorts() and RestorePorts() are required to circumvent this.
        CombinedPortsHistory.Clear();

        // Refill the combined history alternating between local and remote fields when possible
        int count = 0;
        int indexLocal = 0;
        int indexRemote = 0;
        bool swap = false;
        var localPorts = LocalPortsHistory;
        var remotePorts = RemotePortsHistory;
        if (localPorts is null | remotePorts is null)
            return;
        while (count < SettingsManager.Current.General_HistoryListEntries)
        {
            if (indexLocal >= localPorts.Count
                && indexRemote >= remotePorts.Count)
                break;
            if (indexLocal < localPorts.Count && (!swap || indexRemote >= remotePorts.Count))
            {
                // Avoid duplicates
                if (CombinedPortsHistory.Contains(localPorts[indexLocal++]))
                    continue;
                CombinedPortsHistory.Add(localPorts[indexLocal - 1]);
                swap = true;
                count++;
                continue;
            }
            if (indexRemote < remotePorts.Count)
            {
                if (CombinedPortsHistory.Contains(remotePorts[indexRemote++]))
                    continue;
                CombinedPortsHistory.Add(remotePorts[indexRemote - 1]);
                count++;
                swap = false;
            }
        }
    }

    /// <summary>
    /// Command for selecting a program.
    /// </summary>
    public ICommand SelectProgramCommand => new RelayCommand(_ => SelectProgramAction());

    /// <summary>
    /// Select the program using a file dialog.
    /// </summary>
    private void SelectProgramAction()
    {
        var openFileDialog = new OpenFileDialog();

        var fileExtension = "exe";

        openFileDialog.Filter = $@"{Strings.Program} | *.{fileExtension}";

        if (openFileDialog.ShowDialog() == DialogResult.OK)
            Program = new FirewallRuleProgram(openFileDialog.FileName);
    }

    /// <summary>
    /// Clone this instance.
    /// </summary>
    /// <returns>Cloned instance.</returns>
    public object Clone()
    {
        var clone = new FirewallRuleViewModel
        {
            Name = new string(Name ?? string.Empty),
            Program = Program?.Clone() as FirewallRuleProgram,
            NetworkProfileDomain = NetworkProfileDomain,
            NetworkProfilePrivate = NetworkProfilePrivate,
            NetworkProfilePublic = NetworkProfilePublic,
            DefaultName = new string(DefaultName ?? string.Empty),
            Action = Action,
            InterfaceType = InterfaceType,
            Protocol = Protocol,
            Direction = Direction,
            LocalPorts = LocalPorts?.ToList(),
            RemotePorts = RemotePorts?.ToList(),
            NetworkProfiles = NetworkProfiles.ToArray(),
            PortWatermark = new string(PortWatermark ?? string.Empty),
            Description = new string(Description ?? string.Empty),
            UserDefinedName = new string(UserDefinedName ?? string.Empty),
            LocalPortsIndex = LocalPortsIndex,
            RemotePortsIndex = RemotePortsIndex,
        };
        UpdateCombinedPortsHistory();
        return clone;
    }
    #endregion

    #region Events

    /// <summary>
    /// Event when ports are added to history.
    /// </summary>
    public event EventHandler OnAddingPortsToHistory;

    /// <summary>
    /// Fire <see cref="OnAddingPortsToHistory"/> event.
    /// </summary>
    private void OnAddingPortsToHistoryEvent()
    {
        OnAddingPortsToHistory?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event after ports have been added to history.
    /// </summary>
    public event EventHandler OnAddedPortsToHistory;

    /// <summary>
    /// Fire <see cref="OnAddedPortsToHistory"/> event.
    /// </summary>
    private void OnAddedPortsToHistoryEvent()
    {
        OnAddedPortsToHistory?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event when the rule configuration has changed.
    /// </summary>
    public event EventHandler OnRuleChanged;

    /// <summary>
    /// Fire <see cref="OnRuleChanged"/> event.
    /// </summary>
    private void OnRuleChangedEvent()
    {
        OnRuleChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion
}