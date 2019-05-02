using NETworkManager.Utilities;
using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class CredentialsMasterPasswordViewModel : ViewModelBase
    {
        public ICommand OkCommand { get; }

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

        private bool _isPasswordEmpty;
        public bool IsPasswordEmpty
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

        public CredentialsMasterPasswordViewModel(Action<CredentialsMasterPasswordViewModel> okCommand, Action<CredentialsMasterPasswordViewModel> cancelHandler)
        {
            OkCommand = new RelayCommand(p => okCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));
        }

        private void ValidatePassword() => IsPasswordEmpty = Password == null || Password.Length == 0;
    }
}