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
    public class ProfileViewModel : ViewModelBase
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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

                OnPropertyChanged();
            }
        }

        private TracerouteProfileInfo _profileInfo;

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

        public ProfileViewModel(Action<ProfileViewModel> saveCommand, Action<ProfileViewModel> cancelHandler, List<string> groups, TracerouteProfileInfo profileInfo = null)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _profileInfo = profileInfo ?? new TracerouteProfileInfo();

            Name = _profileInfo.Name;
            Host = _profileInfo.Host;

            // Get the group, if not --> get the first group (ascending), fallback --> default group 
            Group = string.IsNullOrEmpty(_profileInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : LocalizationManager.GetStringByKey("String_Default")) : _profileInfo.Group;
            Tags = _profileInfo.Tags;

            _groups = CollectionViewSource.GetDefaultView(groups);
            _groups.SortDescriptions.Add(new SortDescription());

            _isLoading = false;
        }

        private void HasSessionInfoChanged()
        {
            ProfileInfoChanged = (_profileInfo.Name != Name) || (_profileInfo.Host != Host) || (_profileInfo.Group != Group) || (_profileInfo.Tags != Tags);
        }
    }
}