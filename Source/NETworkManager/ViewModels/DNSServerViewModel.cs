using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using System.Collections.ObjectModel;

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

        private bool _useClassic;
        public bool UseClassic
        {
            get => _useClassic;
            set
            {
                if (_useClassic == value)
                    return;

                _useClassic = value;

                if (!_isLoading)
                    CheckInfoChanged();

                OnPropertyChanged();
            }
        }

        public ObservableCollection<DNSServerClassicInfo> _servers = new ObservableCollection<DNSServerClassicInfo>();
        public ObservableCollection<DNSServerClassicInfo> Servers
        {
            get => _servers;
            set
            {
                if (value != null && value == _servers)
                    return;

                _servers = value;
            }
        }

        private bool _useDoH;
        public bool UseDoH
        {
            get => _useDoH;
            set
            {
                if (_useDoH == value)
                    return;

                _useDoH = value;

                if (!_isLoading)
                    CheckInfoChanged();

                OnPropertyChanged();
            }
        }

        public ObservableCollection<DNSServerDoHInfo> _dohServers = new ObservableCollection<DNSServerDoHInfo>();
        public ObservableCollection<DNSServerDoHInfo> DoHServers
        {
            get => _dohServers;
            set
            {
                if (value != null && value == _dohServers)
                    return;

                _dohServers = value;
            }
        }

        private readonly DNSServerInfo _info;

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

            if (_info.UseDoHServer)
            {
                UseDoH = true;

                if (_info.DoHServers is not null)
                    DoHServers = new ObservableCollection<DNSServerDoHInfo>(_info.DoHServers);
            }
            else
            {
                UseClassic = true;

                if (_info.Servers is not null)
                    Servers = new ObservableCollection<DNSServerClassicInfo>(_info.Servers);
            }

            _isLoading = false;
        }

        public void CheckInfoChanged() => InfoChanged = true; // InfoChanged = _info.Name != Name || _previousDNSServerAsString != DNSServers || _info.Port != Port;
    }
}