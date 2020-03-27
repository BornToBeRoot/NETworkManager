using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Models.PowerShell;

namespace NETworkManager.ViewModels
{
    public class PowerShellConnectViewModel : ViewModelBase
    {
        public ICommand ConnectCommand { get; }
        public ICommand CancelCommand { get; }

        private bool _enableRemoteConsole;
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

        private string _host;
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

        private string _additionalCommandLine;
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

        private List<PowerShell.ExecutionPolicy> _executionPolicies = new List<PowerShell.ExecutionPolicy>();
        public List<PowerShell.ExecutionPolicy> ExecutionPolicies
        {
            get => _executionPolicies;
            set
            {
                if (value == _executionPolicies)
                    return;

                _executionPolicies = value;
                OnPropertyChanged();
            }
        }

        private PowerShell.ExecutionPolicy _executionPolicy;
        public PowerShell.ExecutionPolicy ExecutionPolicy
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

        public PowerShellConnectViewModel(Action<PowerShellConnectViewModel> connectCommand, Action<PowerShellConnectViewModel> cancelHandler, string host = null)
        {
            ConnectCommand = new RelayCommand(p => connectCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            if (!string.IsNullOrEmpty(host))
            {
                Host = host;
                EnableRemoteConsole = true;
            }


            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PowerShell_HostHistory);

            LoadSettings();
        }

        private void LoadSettings()
        {
            AdditionalCommandLine = SettingsManager.Current.PowerShell_AdditionalCommandLine;
            
            LoadExecutionPolicies();
        }

        private void LoadExecutionPolicies()
        {
            ExecutionPolicies = System.Enum.GetValues(typeof(PowerShell.ExecutionPolicy)).Cast<PowerShell.ExecutionPolicy>().ToList();
            ExecutionPolicy = ExecutionPolicies.FirstOrDefault(x => x == SettingsManager.Current.PowerShell_ExecutionPolicy);
        }
    }
}
