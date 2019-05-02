using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Linq;
using System.Security;
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
                    HasDNSServerInfoChanged();

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
                    HasDNSServerInfoChanged();

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
                    HasDNSServerInfoChanged();

                OnPropertyChanged();
            }
        }

        private readonly DNSServerInfo _dnsServerInfo;

        private string _previousDNSServerAsString;

        private bool _dnsServerInfoChanged;
        public bool DNSServerInfoChanged
        {
            get => _dnsServerInfoChanged;
            set
            {
                if (value == _dnsServerInfoChanged)
                    return;

                _dnsServerInfoChanged = value;
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

        public DNSServerViewModel(Action<DNSServerViewModel> saveCommand, Action<DNSServerViewModel> cancelHandler, bool isEdited = false, DNSServerInfo dnsServerInfo = null)
        {
            _isLoading = true;

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            _isEdited = isEdited;

            _dnsServerInfo = dnsServerInfo ?? new DNSServerInfo();

            Name = _dnsServerInfo.Name;

            // List to string
            if (_dnsServerInfo.Server != null)
                DNSServer = string.Join("; ", _dnsServerInfo.Server);

            _previousDNSServerAsString = DNSServer;

            Port = _dnsServerInfo.Port;

            _isLoading = false;
        }

        public void HasDNSServerInfoChanged() => DNSServerInfoChanged = _dnsServerInfo.Name != null || _previousDNSServerAsString != DNSServer || _dnsServerInfo.Port != Port;
    }
}