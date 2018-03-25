using NETworkManager.Utilities;
using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels
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

        private bool _passwordIsEmpty = true;
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

        private bool _passwordsMatch = false;
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
            PasswordIsEmpty = ((Password == null || Password.Length == 0) || (PasswordRepeat == null || PasswordRepeat.Length == 0));

            PasswordsMatch = PasswordIsEmpty ? false : SecureStringHelper.ConvertToString(Password).Equals(SecureStringHelper.ConvertToString(PasswordRepeat));
        }

        public CredentialsSetMasterPasswordViewModel(Action<CredentialsSetMasterPasswordViewModel> saveCommand, Action<CredentialsSetMasterPasswordViewModel> cancelHandler)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
        }
    }
}