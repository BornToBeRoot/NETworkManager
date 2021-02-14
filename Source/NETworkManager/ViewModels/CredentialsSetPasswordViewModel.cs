using NETworkManager.Utilities;
using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class CredentialsSetPasswordViewModel : ViewModelBase
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
        /// Private variable for <see cref="PasswordRepeat"/>.
        /// </summary>
        private SecureString _passwordRepeat = new SecureString();

        /// <summary>
        /// Repeated password as secure string.
        /// </summary>
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

        /// <summary>
        /// Private variable for <see cref="IsPasswordEmpty"/>.
        /// </summary>
        private bool _isPasswordEmpty = true;

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
        /// Private variable for <see cref="IsRepeatedPasswordEqual"/>.
        /// </summary>
        private bool _isRepeatedPasswordEqual;

        /// <summary>
        /// Indicate if the <see cref="PasswordRepeat"/> is equal to the <see cref="Password"/>.
        /// </summary>
        public bool IsRepeatedPasswordEqual
        {
            get => _isRepeatedPasswordEqual;
            set
            {
                if (value == _isRepeatedPasswordEqual)
                    return;

                _isRepeatedPasswordEqual = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Check if the passwords are valid and equal.
        /// </summary>
        private void ValidatePassword()
        {
            IsPasswordEmpty = Password == null || Password.Length == 0 || PasswordRepeat == null || PasswordRepeat.Length == 0;

            IsRepeatedPasswordEqual = !IsPasswordEmpty && SecureStringHelper.ConvertToString(Password).Equals(SecureStringHelper.ConvertToString(PasswordRepeat));
        }

        /// <summary>
        /// Initalizes a new class <see cref="CredentialsSetPasswordViewModel"/> with <see cref="OKCommand" /> and <see cref="CancelCommand"/>.
        /// </summary>
        /// <param name="okCommand"><see cref="OKCommand"/> which is executed on OK click.</param>
        /// <param name="cancelHandler"><see cref="CancelCommand"/> which is executed on cancel click.</param>
        public CredentialsSetPasswordViewModel(Action<CredentialsSetPasswordViewModel> okCommand, Action<CredentialsSetPasswordViewModel> cancelHandler)
        {
            OKCommand = new RelayCommand(p => okCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));
        }
    }
}