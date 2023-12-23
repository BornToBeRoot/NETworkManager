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
    private string _additionalCommandLine;

    private string _command;

    private bool _enableRemoteConsole;

    private List<ExecutionPolicy> _executionPolicies = new();

    private ExecutionPolicy _executionPolicy;

    private string _host;

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
        get => _enableRemoteConsole;
        set
        {
            if (value == _enableRemoteConsole)
                return;

            _enableRemoteConsole = value;
            OnPropertyChanged();
        }
    }

    public string Host
    {
        get => _host;
        set
        {
            if (value == _host)
                return;

            _host = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView HostHistoryView { get; }

    public string Command
    {
        get => _command;
        set
        {
            if (value == _command)
                return;

            _command = value;
            OnPropertyChanged();
        }
    }

    public string AdditionalCommandLine
    {
        get => _additionalCommandLine;
        set
        {
            if (value == _additionalCommandLine)
                return;

            _additionalCommandLine = value;
            OnPropertyChanged();
        }
    }

    public List<ExecutionPolicy> ExecutionPolicies
    {
        get => _executionPolicies;
        private set
        {
            if (value == _executionPolicies)
                return;

            _executionPolicies = value;
            OnPropertyChanged();
        }
    }

    public ExecutionPolicy ExecutionPolicy
    {
        get => _executionPolicy;
        set
        {
            if (value == _executionPolicy)
                return;

            _executionPolicy = value;
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