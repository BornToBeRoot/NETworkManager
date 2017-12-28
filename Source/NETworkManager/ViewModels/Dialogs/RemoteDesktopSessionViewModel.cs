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

        private int? _credentialID = null;
        public int? CredentialID
        {
            get { return _credentialID; }
            set
            {
                if (value == _credentialID)
                    return;

                _credentialID = value;

                if (!_isLoading)
                    HasSessionInfoChanged();

                OnPropertyChanged();
            }
        }

        ICollectionView _credentials;
        public ICollectionView Credentials
        {
            get { return _credentials; }
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

        private bool _showUnlockCredentialsHint;
        public bool ShowUnlockCredentialsHint
        {
            get { return _showUnlockCredentialsHint; }
            set
            {
                if (value == _showUnlockCredentialsHint)
                    return;

                _showUnlockCredentialsHint = value;
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
            CredentialID = _sessionInfo.CredentialID;

            // Get the group, if not --> get the first group (ascending), fallback --> default group 
            Group = string.IsNullOrEmpty(_sessionInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : Application.Current.Resources["String_Default"] as string) : _sessionInfo.Group;
            Tags = _sessionInfo.Tags;

            if (CredentialManager.Loaded)
                _credentials = CollectionViewSource.GetDefaultView(CredentialManager.CredentialInfoList);
            else
                ShowUnlockCredentialsHint = true;

            // Fake the entry for the user (if credentials are not loaded or credential was deleted) --> example [12]
            if (CredentialID != null && (!CredentialManager.Loaded || CredentialManager.GetCredentialByID((int)CredentialID) == null))
                _credentials = CollectionViewSource.GetDefaultView(new List<CredentialInfo>() { new CredentialInfo((int)CredentialID) });

            _groups = CollectionViewSource.GetDefaultView(groups);
            _groups.SortDescriptions.Add(new SortDescription());

            _isLoading = false;
        }

        private void HasSessionInfoChanged()
        {
            SessionInfoChanged = (_sessionInfo.Name != Name) || (_sessionInfo.Hostname != Hostname) || (_sessionInfo.CredentialID != CredentialID) || (_sessionInfo.Group != Group) || (_sessionInfo.Tags != Tags);
        }

        #region ICommand & Actions
        public ICommand UnselectCredentialCommand
        {
            get { return new RelayCommand(p => UnselectCredentialAction()); }
        }

        private void UnselectCredentialAction()
        {
            CredentialID = null;
        }
        #endregion
    }
}