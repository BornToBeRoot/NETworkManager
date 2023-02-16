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
    public class ServerInfoProfileViewModel : ViewModelBase
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

        private ServerInfoProfile _currentProfile;
        public ServerInfoProfile CurrentProfile
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

        private List<ServerInfo> _servers;
        public List<ServerInfo> Servers
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

        public ServerInfoProfileViewModel(Action<ServerInfoProfileViewModel> saveCommand, Action<ServerInfoProfileViewModel> cancelHandler, (List<string> UsedNames, bool IsEdited) options, ServerInfoProfile info = null)
        {
            _isLoading = true;

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            UsedNames = options.UsedNames;
            IsEdited = options.IsEdited;
            CurrentProfile = info ?? new ServerInfoProfile();

            // Remove the current profile name from the list
            if (IsEdited)
                UsedNames.Remove(CurrentProfile.Name);

            Name = _currentProfile.Name;
            Servers = _currentProfile.Servers;

            _isLoading = false;
        }
    }
}
