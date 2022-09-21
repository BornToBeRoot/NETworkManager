using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.Network;

namespace NETworkManager.ViewModels
{
    public class PortProfileViewModel : ViewModelBase
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

        private string _ports;
        public string Ports
        {
            get => _ports;
            set
            {
                if (_ports == value)
                    return;

                _ports = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private readonly PortProfileInfo _info;

        private string _previousPortAsString;

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

        public PortProfileViewModel(Action<PortProfileViewModel> saveCommand, Action<PortProfileViewModel> cancelHandler, bool isEdited = false, PortProfileInfo info = null)
        {
            _isLoading = true;

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            IsEdited = isEdited;

            _info = info ?? new PortProfileInfo();

            Name = _info.Name;

            // List to string
            if (_info.Ports != null)
                Ports = _info.Ports;

            _previousPortAsString = Ports;

            _isLoading = false;
        }

        public void Validate()
        {
            InfoChanged = _info.Name != Name || _previousPortAsString != Ports;
        }
    }
}