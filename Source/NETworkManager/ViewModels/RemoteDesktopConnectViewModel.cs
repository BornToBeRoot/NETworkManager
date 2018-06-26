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
        private readonly ICommand _connectCommand;
        public ICommand ConnectCommand
        {
            get { return _connectCommand; }
        }

        private readonly ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        private bool _connectAs;
        public bool ConnectAs
        {
            get { return _connectAs; }
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
            get { return _name; }
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
            get { return _host; }
            set
            {
                if (value == _host)
                    return;

                _host = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _hostHistoryView;
        public ICollectionView HostHistoryView
        {
            get { return _hostHistoryView; }
        }

        private bool _useCredentials;
        public bool UseCredentials
        {
            get { return _useCredentials; }
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
            get { return _customCredentials; }
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
            get { return _username; }
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
            get { return _password; }
            set
            {
                if (value == _password)
                    return;

                _password = value;
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
                OnPropertyChanged();
            }
        }

        ICollectionView _credentials;
        public ICollectionView Credentials
        {
            get { return _credentials; }
        }

        private bool _credentialsLocked;
        public bool CredentialsLocked
        {
            get { return _credentialsLocked; }
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
            _connectCommand = new RelayCommand(p => connectCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            ConnectAs = connectAs;

            if (!ConnectAs)
                _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.RemoteDesktop_HostHistory);

            if (CredentialManager.Loaded)
                _credentials = new CollectionViewSource { Source = CredentialManager.CredentialInfoList }.View;
            else
                CredentialsLocked = true;
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
