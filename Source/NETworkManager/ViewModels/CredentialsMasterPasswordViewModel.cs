using NETworkManager.Utils;
using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class CredentialsMasterPasswordViewModel : ViewModelBase
    {
        private readonly ICommand _okCommand;
        public ICommand OKCommand
        {
            get { return _okCommand; }
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

        public CredentialsMasterPasswordViewModel(Action<CredentialsMasterPasswordViewModel> okCommand, Action<CredentialsMasterPasswordViewModel> cancelHandler)
        {
            _okCommand = new RelayCommand(p => okCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
        }

        private void ValidatePassword()
        {
            PasswordIsEmpty = (Password == null || Password.Length == 0);
        }
    }
}