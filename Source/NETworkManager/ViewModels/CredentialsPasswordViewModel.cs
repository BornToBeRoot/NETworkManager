using NETworkManager.Utilities;
using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class CredentialsPasswordViewModel : ViewModelBase
    {
        /// <summary>
        /// Command which is called when the OK button is clicked.
        /// </summary>
        public ICommand OKCommand { get; }

        /// <summary>
        /// Command which is called when the cancel button is clicked.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Private variable for <see cref="Password"/>.
        /// </summary>
        private SecureString _password = new SecureString();

        /// <summary>
        /// Password as secure string.
        /// </summary>
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

        /// <summary>
        /// Private variable for <see cref="IsPasswordEmpty"/>.
        /// </summary>
        private bool _isPasswordEmpty;

        /// <summary>
        /// Indicate if one of the password fields are empty.
        /// </summary>
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


        /// <summary>
        /// Initalizes a new class <see cref="CredentialsPasswordViewModel"/> with <see cref="OKCommand" /> and <see cref="CancelCommand"/>.
        /// </summary>
        /// <param name="okCommand"><see cref="OKCommand"/> which is executed on OK click.</param>
        /// <param name="cancelHandler"><see cref="CancelCommand"/> which is executed on cancel click.</param>
        public CredentialsPasswordViewModel(Action<CredentialsPasswordViewModel> okCommand, Action<CredentialsPasswordViewModel> cancelHandler)
        {
            OKCommand = new RelayCommand(p => okCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));
        }

        /// <summary>
        /// Check if the passwords are valid.
        /// </summary>
        private void ValidatePassword() => IsPasswordEmpty = Password == null || Password.Length == 0;
    }
}