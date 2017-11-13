using NETworkManager.Models.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Dialogs
{
    public class RemoteDesktopSessionViewModel : ViewModelBase
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

        private RemoteDesktopSessionInfo _sessionInfo;

        private bool _sessionInfoChanged;
        public bool SessionInfoChanged
        {
            get { return _sessionInfoChanged; }
            set
            {
                if (value == _sessionInfoChanged)
                    return;

                _sessionInfoChanged = value;
                OnPropertyChanged();
            }
        }

        public RemoteDesktopSessionViewModel(Action<RemoteDesktopSessionViewModel> saveCommand, Action<RemoteDesktopSessionViewModel> cancelHandler, List<string> groups, RemoteDesktopSessionInfo sessionInfo = null)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _sessionInfo = sessionInfo ?? new RemoteDesktopSessionInfo();

            Name = _sessionInfo.Name;
            Hostname = _sessionInfo.Hostname;

            // Get the group, if not --> get the first group (ascending), fallback --> default group 
            Group = string.IsNullOrEmpty(_sessionInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : Application.Current.Resources["String_Default"] as string) : _sessionInfo.Group;
            Tags = _sessionInfo.Tags;

            _groups = CollectionViewSource.GetDefaultView(groups);
            _groups.SortDescriptions.Add(new SortDescription());

            _isLoading = false;
        }

        private void HasSessionInfoChanged()
        {
            SessionInfoChanged = (_sessionInfo.Name != Name) || (_sessionInfo.Hostname != Hostname) || (_sessionInfo.Group != Group) || (_sessionInfo.Tags != Tags);
        }
    }
}