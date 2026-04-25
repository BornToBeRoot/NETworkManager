using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class ServerConnectionInfoProfileViewModel : ViewModelBase
{
    private readonly ServerConnectionInfo _defaultValues;

    public ServerConnectionInfoProfileViewModel(Action<ServerConnectionInfoProfileViewModel> saveCommand,
        Action<ServerConnectionInfoProfileViewModel> cancelHandler,
        (List<string> UsedNames, bool IsEdited, bool allowOnlyIPAddress) options, ServerConnectionInfo defaultValues,
        ServerConnectionInfoProfile info = null)
    {
        SaveCommand = new RelayCommand(_ => saveCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        UsedNames = options.UsedNames;
        AllowOnlyIPAddress = options.allowOnlyIPAddress;

        _defaultValues = defaultValues;

        info ??= new ServerConnectionInfoProfile();

        // Remove the current profile name from the list
        if (options.IsEdited)
            UsedNames.Remove(info.Name);

        Name = info.Name;
        Servers = new ObservableCollection<ServerConnectionInfo>(info.Servers);

        ServerWatermark = _defaultValues.Server;
        PortWatermark = _defaultValues.Port.ToString();
        Port = _defaultValues.Port;
    }

    public ICommand AddServerCommand => new RelayCommand(_ => AddServerAction());

    public ICommand DeleteServerCommand => new RelayCommand(_ => DeleteServerAction());

    private void AddServerAction()
    {
        Servers.Add(new ServerConnectionInfo(Server, Port, _defaultValues.TransportProtocol));

        Server = string.Empty;
    }

    private void DeleteServerAction()
    {
        // Cast to list to avoid exception: Collection was modified; enumeration operation may not execute.
        // This will create a copy of the list and allow us to remove items from the original list (SelectedServers
        // is modified when Servers changes).
        foreach (var item in SelectedServers.Cast<ServerConnectionInfo>().ToList())
            Servers.Remove(item);
    }

    #region Commands

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    #endregion

    #region Variables

    #region Helper

    // ReSharper disable once MemberCanBePrivate.Global
    // Used by the view in a BindingProxy
    public List<string> UsedNames
    {
        get;
        init
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool AllowOnlyIPAddress
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

    public string Name
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ServerConnectionInfo> Servers
    {
        get;
        private init
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public IList SelectedServers
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();

    public string ServerWatermark { get; private set; }

    public string Server
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PortWatermark { get; private set; }

    public int Port
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion
}