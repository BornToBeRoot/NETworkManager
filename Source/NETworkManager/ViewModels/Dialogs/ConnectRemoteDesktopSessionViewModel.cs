using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Network
{
    public class ConnectRemoteDesktopSessionViewModel : ViewModelBase
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
        
        public ConnectRemoteDesktopSessionViewModel(Action<ConnectRemoteDesktopSessionViewModel> connectCommand, Action<ConnectRemoteDesktopSessionViewModel> cancelHandler)
        {
            _connectCommand = new RelayCommand(p => connectCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
        }
    }
}
