using NETworkManager.Models.Settings;
using NETworkManager.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Dialogs
{
    public class NetworkInterfaceProfileViewModel : ViewModelBase
    {
        private bool _isLoading = true;

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
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private bool _enableDynamicIPAddress = true;
        public bool EnableDynamicIPAddress
        {
            get { return _enableDynamicIPAddress; }
            set
            {
                if (value == _enableDynamicIPAddress)
                    return;

                _enableDynamicIPAddress = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private bool _enableStaticIPAddress;
        public bool EnableStaticIPAddress
        {
            get { return _enableStaticIPAddress; }
            set
            {
                if (value == _enableStaticIPAddress)
                    return;

                EnableStaticDNS = true;

                _enableStaticIPAddress = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _ipAddress;
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                if (value == _ipAddress)
                    return;

                _ipAddress = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _subnetmaskOrCidr;
        public string SubnetmaskOrCidr
        {
            get { return _subnetmaskOrCidr; }
            set
            {
                if (value == _subnetmaskOrCidr)
                    return;

                _subnetmaskOrCidr = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _gateway;
        public string Gateway
        {
            get { return _gateway; }
            set
            {
                if (value == _gateway)
                    return;

                _gateway = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private bool _enableDynamicDNS = true;
        public bool EnableDynamicDNS
        {
            get { return _enableDynamicDNS; }
            set
            {
                if (value == _enableDynamicDNS)
                    return;

                _enableDynamicDNS = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private bool _enableStaticDNS;
        public bool EnableStaticDNS
        {
            get { return _enableStaticDNS; }
            set
            {
                if (value == _enableStaticDNS)
                    return;

                _enableStaticDNS = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _primaryDNSServer;
        public string PrimaryDNSServer
        {
            get { return _primaryDNSServer; }
            set
            {
                if (value == _primaryDNSServer)
                    return;

                _primaryDNSServer = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _secondaryDNSServer;
        public string SecondaryDNSServer
        {
            get { return _secondaryDNSServer; }
            set
            {
                if (value == _secondaryDNSServer)
                    return;

                _secondaryDNSServer = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

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
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        ICollectionView _groups;
        public ICollectionView Groups
        {
            get { return _groups; }
        }

        private NetworkInterfaceProfileInfo _profileInfo;

        private bool _profileInfoChanged;
        public bool ProfileInfoChanged
        {
            get { return _profileInfoChanged; }
            set
            {
                if (value == _profileInfoChanged)
                    return;

                _profileInfoChanged = value;
                OnPropertyChanged();
            }
        }

        public NetworkInterfaceProfileViewModel(Action<NetworkInterfaceProfileViewModel> saveCommand, Action<NetworkInterfaceProfileViewModel> cancelHandler, List<string> groups, NetworkInterfaceProfileInfo profileInfo = null)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _profileInfo = profileInfo ?? new NetworkInterfaceProfileInfo();

            Name = _profileInfo.Name;
            EnableStaticIPAddress = _profileInfo.EnableStaticIPAddress;
            IPAddress = profileInfo.IPAddress;
            SubnetmaskOrCidr = _profileInfo.Subnetmask;
            Gateway = _profileInfo.Gateway;
            EnableStaticDNS = _profileInfo.EnableStaticDNS;
            PrimaryDNSServer = _profileInfo.PrimaryDNSServer;
            SecondaryDNSServer = _profileInfo.SecondaryDNSServer;

            // Get the group, if not --> get the first group (ascending), fallback --> default group 
            Group = string.IsNullOrEmpty(_profileInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : Application.Current.Resources["String_Default"] as string) : _profileInfo.Group;
            
            _groups = CollectionViewSource.GetDefaultView(groups);
            _groups.SortDescriptions.Add(new SortDescription());

            _isLoading = false;
        }

        private void HasProfileInfoChanged()
        {
            ProfileInfoChanged = (_profileInfo.Name != Name) || (_profileInfo.EnableStaticIPAddress != EnableStaticIPAddress) || (_profileInfo.IPAddress != IPAddress) || (_profileInfo.Subnetmask != SubnetmaskOrCidr) || (_profileInfo.Gateway != Gateway) || (_profileInfo.EnableStaticDNS != EnableStaticDNS) || (_profileInfo.PrimaryDNSServer != PrimaryDNSServer) || (_profileInfo.SecondaryDNSServer != SecondaryDNSServer) || (_profileInfo.Group != Group);
        }
    }
}