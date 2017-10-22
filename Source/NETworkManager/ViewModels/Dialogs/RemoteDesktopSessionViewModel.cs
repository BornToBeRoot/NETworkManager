using NETworkManager.Models.Settings;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Network
{
    public class RemoteDesktopSessionViewModel : ViewModelBase
    {
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
                OnPropertyChanged();
            }
        }

        public RemoteDesktopSessionViewModel(Action<RemoteDesktopSessionViewModel> saveCommand, Action<RemoteDesktopSessionViewModel> cancelHandler)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
        }

        public RemoteDesktopSessionViewModel(Action<RemoteDesktopSessionViewModel> saveCommand, Action<RemoteDesktopSessionViewModel> cancelHandler, RemoteDesktopSessionInfo info)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            Name = info.Name;
            Hostname = info.Hostname;
        }
    }
}
