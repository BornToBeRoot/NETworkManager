using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.Linq;

namespace NETworkManager.ViewModels;

public class ServerConnectionInfoProfileViewModel : ViewModelBase
{
    private readonly bool _isLoading;

    private ServerConnectionInfo _defaultValues;

    #region Commands
    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }
    #endregion

    #region Variables

    #region Helper
    private List<string> _usedNames;
    public List<string> UsedNames
    {
        get => _usedNames;
        set
        {
            if (value == _usedNames)
                return;

            _usedNames = value;
            OnPropertyChanged();
        }
    }

    private bool _allowOnlyIPAddress;
    public bool AllowOnlyIPAddress
    {
        get => _allowOnlyIPAddress;
        set
        {
            if (value == _allowOnlyIPAddress)
                return;

            _allowOnlyIPAddress = value;
            OnPropertyChanged();
        }
    }

    private bool _isEdited;
    public bool IsEdited
    {
        get => _isEdited;
        set
        {
            if (value == _isEdited)
                return;

            _isEdited = value;
            OnPropertyChanged();
        }
    }

    private ServerConnectionInfoProfile _currentProfile;
    public ServerConnectionInfoProfile CurrentProfile
    {
        get => _currentProfile;
        set
        {
            if (value == _currentProfile)
                return;

            _currentProfile = value;
            OnPropertyChanged();
        }
    }
    #endregion

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;

            _name = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<ServerConnectionInfo> _servers;
    public ObservableCollection<ServerConnectionInfo> Servers
    {
        get => _servers;
        set
        {
            if (value == _servers)
                return;

            _servers = value;
            OnPropertyChanged();
        }
    }

    private IList _selectedServers = new ArrayList();
    public IList SelectedServers
    {
        get => _selectedServers;
        set
        {
            if (Equals(value, _selectedServers))
                return;

            _selectedServers = value;
            OnPropertyChanged();
        }
    }

    public string ServerWatermark { get; private set; }

    private string _server;
    public string Server
    {
        get => _server;
        set
        {
            if (_server == value)
                return;

            _server = value;
            OnPropertyChanged();
        }
    }

    public string PortWatermark { get; private set; }

    private int _port;
    public int Port
    {
        get => _port;
        set
        {
            if (_port == value)
                return;

            _port = value;
            OnPropertyChanged();
        }
    }
    #endregion

    public ServerConnectionInfoProfileViewModel(Action<ServerConnectionInfoProfileViewModel> saveCommand, Action<ServerConnectionInfoProfileViewModel> cancelHandler, (List<string> UsedNames, bool IsEdited, bool allowOnlyIPAddress) options, ServerConnectionInfo defaultValues, ServerConnectionInfoProfile info = null)
    {
        _isLoading = true;

        SaveCommand = new RelayCommand(p => saveCommand(this));
        CancelCommand = new RelayCommand(p => cancelHandler(this));

        UsedNames = options.UsedNames;
        AllowOnlyIPAddress = options.allowOnlyIPAddress;
        IsEdited = options.IsEdited;

        _defaultValues = defaultValues;

        CurrentProfile = info ?? new ServerConnectionInfoProfile();

        // Remove the current profile name from the list
        if (IsEdited)
            UsedNames.Remove(CurrentProfile.Name);

        Name = _currentProfile.Name;
        Servers = new ObservableCollection<ServerConnectionInfo>(_currentProfile.Servers);

        ServerWatermark = _defaultValues.Server;
        PortWatermark = _defaultValues.Port.ToString();
        Port = _defaultValues.Port;

        _isLoading = false;
    }

    public ICommand AddServerCommand => new RelayCommand(p => AddServerAction());

    private void AddServerAction()
    {
        Servers.Add(new ServerConnectionInfo(Server, Port, _defaultValues.TransportProtocol));
        
        Server = string.Empty;
    }

    public ICommand DeleteServerCommand => new RelayCommand(p => DeleteServerAction());

    private void DeleteServerAction()
    {
        // Cast to list to avoid exception: Collection was modified; enumeration operation may not execute.
        // This will create a copy of the list and allow us to remove items from the original list (SelectedServers
        // is modified when Servers changes).
        foreach (var item in SelectedServers.Cast<ServerConnectionInfo>().ToList())
            Servers.Remove(item);
    }
}
