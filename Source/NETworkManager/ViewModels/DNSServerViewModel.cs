using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.Network;

namespace NETworkManager.ViewModels
{
    public class DNSServerViewModel : ViewModelBase
    {
        private readonly bool _isLoading;

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

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

        private string _dnsServers;
        public string DNSServers
        {
            get => _dnsServers;
            set
            {
                if (_dnsServers == value)
                    return;

                _dnsServers = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private int _port;
        public int Port
        {
            get => _port;
            set
            {
                if (_port == value)
                    return;

                _port = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private readonly DNSServerInfo _info;

        private string _previousDNSServerAsString;

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

        public DNSServerViewModel(Action<DNSServerViewModel> saveCommand, Action<DNSServerViewModel> cancelHandler, bool isEdited = false, DNSServerInfo info = null)
        {
            _isLoading = true;

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            IsEdited = isEdited;

            _info = info ?? new DNSServerInfo();

            Name = _info.Name;

            // List to string
            if (_info.Servers != null)
                DNSServers = string.Join("; ", _info.Servers);

            _previousDNSServerAsString = DNSServers;

            Port = _info.Port;

            _isLoading = false;
        }

        public void Validate()
        {
            InfoChanged = _info.Name != Name || _previousDNSServerAsString != DNSServers || _info.Port != Port;
        }
    }
}