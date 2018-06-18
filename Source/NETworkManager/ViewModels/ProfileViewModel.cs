using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private ProfileInfo _profileInfo;

        #region General
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                    return;

                _name = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                if (value == _host)
                    return;

                _host = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _group;
        public string Group
        {
            get { return _group; }
            set
            {
                if (value == _group)
                    return;

                _group = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        ICollectionView _groups;
        public ICollectionView Groups
        {
            get { return _groups; }
        }

        private string _tags;
        public string Tags
        {
            get { return _tags; }
            set
            {
                if (value == _tags)
                    return;

                _tags = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _displayNote;
        public bool DisplayNote
        {
            get { return _displayNote; }
            set
            {
                if (value == _displayNote)
                    return;

                _displayNote = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Network Interface
        private bool _networkInterface_Enabled;
        public bool NetworkInterface_Enabled
        {
            get { return _networkInterface_Enabled; }
            set
            {
                if (value == _networkInterface_Enabled)
                    return;

                _networkInterface_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion

        #region IP Scanner
        private bool _ipScanner_Enabled;
        public bool IPScanner_Enabled
        {
            get { return _ipScanner_Enabled; }
            set
            {
                if (value == _ipScanner_Enabled)
                    return;

                _ipScanner_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _ipScanner_InheritHost = true;
        public bool IPScanner_InheritHost
        {
            get { return _ipScanner_InheritHost; }
            set
            {
                if (value == _ipScanner_InheritHost)
                    return;

                _ipScanner_InheritHost = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _ipScanner_IPRange;
        public string IPScanner_IPRange
        {
            get { return _ipScanner_IPRange; }
            set
            {
                if (value == _ipScanner_IPRange)
                    return;

                _ipScanner_IPRange = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion

        #region Port Scanner
        private bool _portScanner_Enabled;
        public bool PortScanner_Enabled
        {
            get { return _portScanner_Enabled; }
            set
            {
                if (value == _portScanner_Enabled)
                    return;

                _portScanner_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _portScanner_InheritHost = true;
        public bool PortScanner_InheritHost
        {
            get { return _portScanner_InheritHost; }
            set
            {
                if (value == _portScanner_InheritHost)
                    return;

                _portScanner_InheritHost = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _portScanner_Host;
        public string PortScanner_Host
        {
            get { return _portScanner_Host; }
            set
            {
                if (value == _portScanner_Host)
                    return;

                _portScanner_Host = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _portScanner_Ports;
        public string PortScanner_Ports
        {
            get { return _portScanner_Ports; }
            set
            {
                if (value == _portScanner_Ports)
                    return;

                _portScanner_Ports = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion

        #endregion
        public ProfileViewModel(Action<ProfileViewModel> saveCommand, Action<ProfileViewModel> cancelHandler, List<string> groups, ProfileInfo profileInfo = null)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _profileInfo = profileInfo ?? new ProfileInfo();

            Name = _profileInfo.Name;
            Host = _profileInfo.Host;
            Group = string.IsNullOrEmpty(_profileInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : LocalizationManager.GetStringByKey("String_Default")) : _profileInfo.Group;
            Tags = _profileInfo.Tags;

            _groups = CollectionViewSource.GetDefaultView(groups);
            _groups.SortDescriptions.Add(new SortDescription());

            IPScanner_Enabled = _profileInfo.IPScanner_Enabled;
            IPScanner_InheritHost = _profileInfo.IPScanner_InheritHost;
            IPScanner_IPRange = _profileInfo.IPScanner_IPRange;

            PortScanner_Enabled = _profileInfo.PortScanner_Enabled;
            PortScanner_InheritHost = _profileInfo.PortScanner_InheritHost;
            PortScanner_Host = _profileInfo.PortScanner_Host;
            PortScanner_Ports = _profileInfo.PortScanner_Ports;

            Validate();

            _isLoading = false;
        }

        private void Validate()
        {
            // Note
            DisplayNote = (NetworkInterface_Enabled || IPScanner_Enabled || PortScanner_Enabled);
        }

        #region ICommands & Actions
        private readonly ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get { return _saveCommand; }
        }

        private readonly ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }
        #endregion
    }
}