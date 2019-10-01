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
                    CheckInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _dnsServer;
        public string DNSServer
        {
            get => _dnsServer;
            set
            {
                if (_dnsServer == value)
                    return;

                _dnsServer = value;

                if (!_isLoading)
                    CheckInfoChanged();

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
                    CheckInfoChanged();

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
                DNSServer = string.Join("; ", _info.Servers);

            _previousDNSServerAsString = DNSServer;

            Port = _info.Port;

            _isLoading = false;
        }

        public void CheckInfoChanged() => InfoChanged = _info.Name != Name || _previousDNSServerAsString != DNSServer || _info.Port != Port;
    }
}