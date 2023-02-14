using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using System.Collections.ObjectModel;

namespace NETworkManager.ViewModels
{
    public class SNTPServerViewModel : ViewModelBase
    {
        private readonly bool _isLoading;

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        private SNTPServerInfo _info;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _name = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private ObservableCollection<ServerInfo> _servers;
        public ObservableCollection<ServerInfo> Servers
        {
            get => _servers;
            set
            {
                if (value == _servers)
                    return;

                _servers = value;
                OnPropertyChanged();
            }
        }

        private bool _infoChanged;
        public bool InfoChanged
        {
            get => _infoChanged;
            set
            {
                if (value == _infoChanged)
                    return;

                _infoChanged = value;
                OnPropertyChanged();
            }
        }

        private bool _isEdited;
        public bool IsEdited
        {
            get => _isEdited;
            set
            {
                if (value == _isEdited)
                    return;

                _isEdited = value;
                OnPropertyChanged();
            }
        }

        public SNTPServerViewModel(Action<SNTPServerViewModel> saveCommand, Action<SNTPServerViewModel> cancelHandler, bool isEdited = false, SNTPServerInfo info = null)
        {
            _isLoading = true;

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            IsEdited = isEdited;

            _info = info ?? new SNTPServerInfo();

            Name = _info.Name;
            Servers = new ObservableCollection<ServerInfo>(_info.Servers);

            _isLoading = false;
        }
        public void Validate()
        {
            //InfoChanged = _info.Name != Name || _previousDNSServerAsString != DNSServers || _info.Port != Port;
        }
    }
}