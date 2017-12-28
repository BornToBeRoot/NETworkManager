using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Dialogs
{
    public class RemoteDesktopSessionConnectViewModel : ViewModelBase
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

        private string _hostname;
        public string Hostname
        {
            get { return _hostname; }
            set
            {
                if (value == _hostname)
                    return;

                _hostname = value;
                OnPropertyChanged();
            }
        }

        private bool _useCredential;
        public bool UseCredential
        {
            get { return _useCredential; }
            set
            {
                if (value == _useCredential)
                    return;

                _useCredential = value;
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

        public RemoteDesktopSessionConnectViewModel(Action<RemoteDesktopSessionConnectViewModel> connectCommand, Action<RemoteDesktopSessionConnectViewModel> cancelHandler)
        {
            _connectCommand = new RelayCommand(p => connectCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
        }
    }
}
