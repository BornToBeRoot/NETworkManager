using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class PortScannerProfileViewModel : ViewModelBase
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

        private string _hostname;
        public string Hostname
        {
            get { return _hostname; }
            set
            {
                if (value == _hostname)
                    return;

                _hostname = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _ports;
        public string Ports
        {
            get { return _ports; }
            set
            {
                if (value == _ports)
                    return;

                _ports = value;

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
               
        private PortScannerProfileInfo _profileInfo;

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

        public PortScannerProfileViewModel(Action<PortScannerProfileViewModel> saveCommand, Action<PortScannerProfileViewModel> cancelHandler, List<string> groups, PortScannerProfileInfo profileInfo = null)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _profileInfo = profileInfo ?? new PortScannerProfileInfo();

            Name = _profileInfo.Name;
            Hostname = _profileInfo.Hostname;
            Ports = _profileInfo.Ports;

            // Get the group, if not --> get the first group (ascending), fallback --> default group 
            Group = string.IsNullOrEmpty(_profileInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : Application.Current.Resources["String_Default"] as string) : _profileInfo.Group;

            _groups = CollectionViewSource.GetDefaultView(groups);
            _groups.SortDescriptions.Add(new SortDescription());

            _isLoading = false;
        }

        private void HasProfileInfoChanged()
        {
            ProfileInfoChanged = (_profileInfo.Name != Name) || (_profileInfo.Hostname != Hostname) || (_profileInfo.Ports != Ports) || (_profileInfo.Group != Group);
        }
    }
}