using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Dialogs
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

        private void ValidatePassword()
        {
            IsNotEmpty = (Password != null && Password.Length > 0);
        }

        public CredentialsMasterPasswordViewModel(Action<CredentialsMasterPasswordViewModel> okCommand, Action<CredentialsMasterPasswordViewModel> cancelHandler)
        {
            _okCommand = new RelayCommand(p => okCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));
        }
    }
}