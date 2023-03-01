using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Linq;
using System.Diagnostics;

namespace NETworkManager.ViewModels
{
    public class ServerConnectionInfoProfileViewModel : ViewModelBase
    {
        private readonly bool _isLoading;

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

        private List<ServerConnectionInfo> _servers;
        public List<ServerConnectionInfo> Servers
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
        #endregion

        public ServerConnectionInfoProfileViewModel(Action<ServerConnectionInfoProfileViewModel> saveCommand, Action<ServerConnectionInfoProfileViewModel> cancelHandler, (List<string> UsedNames, bool IsEdited, bool allowOnlyIPAddress) options, ServerConnectionInfoProfile info = null)
        {
            _isLoading = true;

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            UsedNames = options.UsedNames;
            AllowOnlyIPAddress = options.allowOnlyIPAddress;
            IsEdited = options.IsEdited;
            CurrentProfile = info ?? new ServerConnectionInfoProfile();

            // Remove the current profile name from the list
            if (IsEdited)
                UsedNames.Remove(CurrentProfile.Name);

            Name = _currentProfile.Name;
            Servers = _currentProfile.Servers;

            _isLoading = false;
        }
    }
}
