using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Network
{
    public class RemoteDesktopSessionViewModel : ViewModelBase
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

        private string _domain;
        public string Domain
        {
            get { return _domain; }
            set
            {
                if (value == _domain)
                    return;

                _domain = value;
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

        private string _password;
        public string Password
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

        public RemoteDesktopSessionViewModel(Action<RemoteDesktopSessionViewModel> connectCommand, Action<RemoteDesktopSessionViewModel> cancelHandler)
        {
            _connectCommand = new RelayCommand(p => connectCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
        }
    }
}
