using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Models.PowerShell;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class PowerShellConnectViewModel : ViewModelBase
{
    public PowerShellConnectViewModel(Action<PowerShellConnectViewModel> connectCommand,
        Action<PowerShellConnectViewModel> cancelHandler, string host = null)
    {
        ConnectCommand = new RelayCommand(_ => connectCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        if (!string.IsNullOrEmpty(host))
        {
            Host = host;
            EnableRemoteConsole = true;
        }

        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PowerShell_HostHistory);

        LoadSettings();
    }

    public ICommand ConnectCommand { get; }
    public ICommand CancelCommand { get; }

    public bool EnableRemoteConsole
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

    public string Host
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

    public ICollectionView HostHistoryView { get; }

    public string Command
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

    public string AdditionalCommandLine
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

    public List<ExecutionPolicy> ExecutionPolicies
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ExecutionPolicy ExecutionPolicy
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

    private void LoadSettings()
    {
        Command = SettingsManager.Current.PowerShell_Command;
        AdditionalCommandLine = SettingsManager.Current.PowerShell_AdditionalCommandLine;

        ExecutionPolicies = Enum.GetValues(typeof(ExecutionPolicy)).Cast<ExecutionPolicy>().ToList();
        ExecutionPolicy =
            ExecutionPolicies.FirstOrDefault(x => x == SettingsManager.Current.PowerShell_ExecutionPolicy);
    }
}