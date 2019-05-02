using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class CredentialViewModel : ViewModelBase
    {
        private readonly bool _isLoading;

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        private Guid _id;
        public Guid Id
        {
            get => _id;
            set
            {
                if (_id == value)
                    return;

                _id = value;
                OnPropertyChanged();
            }
        }

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
                    HasCredentialInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (_username == value)
                    return;

                _username = value;

                if (!_isLoading)
                    HasCredentialInfoChanged();

                OnPropertyChanged();
            }
        }

        private SecureString _password = new SecureString();
        public SecureString Password
        {
            get => _password;
            set
            {
                if (value == _password)
                    return;

                _password = value;

                if (!_isLoading)
                {
                    HasCredentialInfoChanged();
                    ValidatePassword();
                }

                OnPropertyChanged();
            }
        }

        private readonly CredentialInfo _credentialInfo;

        private bool _credentialInfoChanged;
        public bool CredentialInfoChanged
        {
            get => _credentialInfoChanged;
            set
            {
                if (value == _credentialInfoChanged)
                    return;

                _credentialInfoChanged = value;
                OnPropertyChanged();
            }
        }

        private bool _passwordIsEmpty;
        public bool PasswordIsEmpty
        {
            get => _passwordIsEmpty;
            set
            {
                if (value == _passwordIsEmpty)
                    return;

                _passwordIsEmpty = value;
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

        public CredentialViewModel(Action<CredentialViewModel> saveCommand, Action<CredentialViewModel> cancelHandler, bool isEdited = false, CredentialInfo credentialInfo = null)
        {
            _isLoading = true;

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            _isEdited = isEdited;

            _credentialInfo = credentialInfo ?? new CredentialInfo();

            Id = _credentialInfo.ID;
            Name = _credentialInfo.Name;
            Username = _credentialInfo.Username;
            Password = _credentialInfo.Password;

            _isLoading = false;
        }

        public void HasCredentialInfoChanged() => CredentialInfoChanged = (_credentialInfo.Name != Name) || (_credentialInfo.Username != Username) || (SecureStringHelper.ConvertToString(_credentialInfo.Password) != SecureStringHelper.ConvertToString(Password));

        private void ValidatePassword() => PasswordIsEmpty = Password == null || Password.Length == 0;
    }
}