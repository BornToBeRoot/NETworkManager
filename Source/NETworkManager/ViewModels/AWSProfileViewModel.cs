using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.AWS;

namespace NETworkManager.ViewModels
{
    public class AWSProfileViewModel : ViewModelBase
    {
        private readonly bool _isLoading;

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                    return;

                _isEnabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _profile;
        public string Profile
        {
            get => _profile;
            set
            {
                if (_profile == value)
                    return;

                _profile = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _region;
        public string Region
        {
            get => _region;
            set
            {
                if (_region == value)
                    return;

                _region = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private readonly AWSProfileInfo _info;

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

        public AWSProfileViewModel(Action<AWSProfileViewModel> saveCommand, Action<AWSProfileViewModel> cancelHandler, bool isEdited = false, AWSProfileInfo info = null)
        {
            _isLoading = true;

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            IsEdited = isEdited;

            _info = info ?? new AWSProfileInfo();
            
            IsEnabled = _info.IsEnabled;
            Profile = _info.Profile;
            Region = _info.Region;

            _isLoading = false;
        }

        public void Validate()
        {
            InfoChanged = _info.IsEnabled != IsEnabled || _info.Profile != Profile || _info.Region != Region;
        }
    }
}