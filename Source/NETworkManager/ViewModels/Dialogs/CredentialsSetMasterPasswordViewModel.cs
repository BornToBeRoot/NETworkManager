using NETworkManager.Helpers;
using System;
using System.Diagnostics;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Dialogs
{
    public class CredentialsSetMasterPasswordViewModel : ViewModelBase
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

        private SecureString _password = new SecureString();
        public SecureString Password
        {
            get { return _password; }
            set
            {
                if (value == _password)
                    return;

                _password = value;

                ValidatePassword();

                OnPropertyChanged();
            }
        }

        private SecureString _passwordRepeat = new SecureString();
        public SecureString PasswordRepeat
        {
            get { return _passwordRepeat; }
            set
            {
                if (value == _passwordRepeat)
                    return;

                _passwordRepeat = value;

                ValidatePassword();

                OnPropertyChanged();
            }
        }

        private bool _isNotEmpty;
        public bool IsNotEmpty
        {
            get { return _isNotEmpty; }
            set
            {
                if (value == _isNotEmpty)
                    return;

                _isNotEmpty = value;
                OnPropertyChanged();
            }
        }

        private bool _passwordsMatch;
        public bool PasswordsMatch
        {
            get { return _passwordsMatch; }
            set
            {
                if (value == _passwordsMatch)
                    return;

                _passwordsMatch = value;
                OnPropertyChanged();
            }
        }

        private void ValidatePassword()
        {
            IsNotEmpty = ((Password != null && Password.Length > 0) && (PasswordRepeat != null && PasswordRepeat.Length > 0));

            PasswordsMatch = IsNotEmpty ? SecureStringHelper.ConvertToString(Password).Equals(SecureStringHelper.ConvertToString(PasswordRepeat)) : false;
        }

        public CredentialsSetMasterPasswordViewModel(Action<CredentialsSetMasterPasswordViewModel> saveCommand, Action<CredentialsSetMasterPasswordViewModel> cancelHandler)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
        }
    }
}