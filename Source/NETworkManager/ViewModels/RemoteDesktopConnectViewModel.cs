using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Security;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class RemoteDesktopConnectViewModel : ViewModelBase
    {
        public ICommand ConnectCommand { get; }

        public ICommand CancelCommand { get; }

        private bool _connectAs;
        public bool ConnectAs
        {
            get => _connectAs;
            set
            {
                if (value == _connectAs)
                    return;

                _connectAs = value;
                OnPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (value == _name)
                    return;

                _name = value;
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

        private bool _useCredentials;
        public bool UseCredentials
        {
            get => _useCredentials;
            set
            {
                if (value == _useCredentials)
                    return;

                _useCredentials = value;
                OnPropertyChanged();
            }
        }

        private bool _customCredentials = true;
        public bool CustomCredentials
        {
            get => _customCredentials;
            set
            {
                if (value == _customCredentials)
                    return;

                _customCredentials = value;
                OnPropertyChanged();
            }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (value == _username)
                    return;

                _username = value;
                OnPropertyChanged();
            }
        }

        private SecureString _password = new SecureString();
        public SecureString Password
        {
            get => _password;
            set
            {
                if (value == _password)
                    return;

                _password = value;
                OnPropertyChanged();
            }
        }

        private Guid _credentialID;
        public Guid CredentialID
        {
            get => _credentialID;
            set
            {
                if (value == _credentialID)
                    return;

                _credentialID = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView Credentials { get; }

        private bool _credentialsLocked;
        public bool CredentialsLocked
        {
            get => _credentialsLocked;
            set
            {
                if (value == _credentialsLocked)
                    return;

                _credentialsLocked = value;
                OnPropertyChanged();
            }
        }

        public RemoteDesktopConnectViewModel(Action<RemoteDesktopConnectViewModel> connectCommand, Action<RemoteDesktopConnectViewModel> cancelHandler, bool connectAs = false)
        {
            ConnectCommand = new RelayCommand(p => connectCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            ConnectAs = connectAs;

            if (!ConnectAs)
                HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.RemoteDesktop_HostHistory);
        }

        #region ICommand & Actions
        public ICommand UnselectCredentialCommand => new RelayCommand(p => UnselectCredentialAction());

        private void UnselectCredentialAction()
        {
            CredentialID = Guid.Empty;
        }
        #endregion
    }
}
