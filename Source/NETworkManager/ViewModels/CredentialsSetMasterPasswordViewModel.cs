using NETworkManager.Utilities;
using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class CredentialsSetMasterPasswordViewModel : ViewModelBase
    {
        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        private SecureString _password = new SecureString();
        public SecureString Password
        {
            get => _password;
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
            get => _passwordRepeat;
            set
            {
                if (value == _passwordRepeat)
                    return;

                _passwordRepeat = value;

                ValidatePassword();

                OnPropertyChanged();
            }
        }

        private bool _isPasswordEmpty = true;
        public bool IsPasswordIsEmpty
        {
            get => _isPasswordEmpty;
            set
            {
                if (value == _isPasswordEmpty)
                    return;

                _isPasswordEmpty = value;
                OnPropertyChanged();
            }
        }

        private bool _isRepeatPasswordsEqual;
        public bool IsRepeatPasswordsEqual
        {
            get => _isRepeatPasswordsEqual;
            set
            {
                if (value == _isRepeatPasswordsEqual)
                    return;

                _isRepeatPasswordsEqual = value;
                OnPropertyChanged();
            }
        }

        private void ValidatePassword()
        {
            IsPasswordIsEmpty = ((Password == null || Password.Length == 0) || (PasswordRepeat == null || PasswordRepeat.Length == 0));

            IsRepeatPasswordsEqual = !IsPasswordIsEmpty && SecureStringHelper.ConvertToString(Password).Equals(SecureStringHelper.ConvertToString(PasswordRepeat));
        }

        public CredentialsSetMasterPasswordViewModel(Action<CredentialsSetMasterPasswordViewModel> saveCommand, Action<CredentialsSetMasterPasswordViewModel> cancelHandler)
        {
            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));
        }
    }
}