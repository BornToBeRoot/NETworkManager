using NETworkManager.Helpers;
using NETworkManager.Models.Settings;
using NETworkManager.Utils;
using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class CredentialViewModel : ViewModelBase
    {
        private bool _isLoading = true;

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

        private int _id;
        public int ID
        {
            get { return _id; }
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
            get { return _name; }
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
            get { return _username; }
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
            get { return _password; }
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

        private CredentialInfo _credentialInfo;

        private bool _credentialInfoChanged;
        public bool CredentialInfoChanged
        {
            get { return _credentialInfoChanged; }
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
            get { return _passwordIsEmpty; }
            set
            {
                if (value == _passwordIsEmpty)
                    return;

                _passwordIsEmpty = value;
                OnPropertyChanged();
            }
        }

        private bool _isBeingEdited;
        public bool IsBeingEdited
        {
            get { return _isBeingEdited; }
            set
            {
                if (value == _isBeingEdited)
                    return;

                _isBeingEdited = value;
                OnPropertyChanged();
            }
        }

        public CredentialViewModel(Action<CredentialViewModel> saveCommand, Action<CredentialViewModel> cancelHandler, int id, CredentialInfo credentialInfo = null)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _credentialInfo = credentialInfo ?? new CredentialInfo();

            ID = id;
            Name = _credentialInfo.Name;
            Username = _credentialInfo.Username;
            Password = _credentialInfo.Password;

            _isBeingEdited = credentialInfo != null;

            _isLoading = false;
        }

        public void HasCredentialInfoChanged()
        {
            CredentialInfoChanged = (_credentialInfo.Name != Name) || (_credentialInfo.Username != Username) || (SecureStringHelper.ConvertToString(_credentialInfo.Password) != SecureStringHelper.ConvertToString(Password));
        }

        private void ValidatePassword()
        {
            PasswordIsEmpty = (Password == null || Password.Length == 0);
        }

    }
}