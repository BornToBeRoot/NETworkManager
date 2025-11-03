using System;
using System.Security;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class CredentialsChangePasswordViewModel : DialogViewModelBase<CredentialsChangePasswordViewModel>
{
    /// <summary>
    ///     Private variable for <see cref="IsPasswordEmpty" />.
    /// </summary>
    private bool _isPasswordEmpty = true;

    /// <summary>
    ///     Private variable for <see cref="IsRepeatedPasswordEqual" />.
    /// </summary>
    private bool _isRepeatedPasswordEqual;

    /// <summary>
    ///     Private variable for <see cref="NewPassword" />.
    /// </summary>
    private SecureString _newPassword = new();

    /// <summary>
    ///     Private variable for <see cref="NewPasswordRepeat" />.
    /// </summary>
    private SecureString _newPasswordRepeat = new();

    /// <summary>
    ///     Private variable for <see cref="Password" />.
    /// </summary>
    private SecureString _password = new();

    /// <summary>
    ///     Initialize a new class <see cref="CredentialsSetPasswordViewModel" /> with <see cref="OKCommand" /> and
    ///     <see cref="CancelCommand" />.
    /// </summary>
    /// <param name="okCommand"><see cref="OKCommand" /> which is executed on OK click.</param>
    /// <param name="cancelHandler"><see cref="CancelCommand" /> which is executed on cancel click.</param>
    public CredentialsChangePasswordViewModel(Action<CredentialsChangePasswordViewModel> okCommand,
        Action<CredentialsChangePasswordViewModel> cancelHandler)
        : base(okCommand, cancelHandler)
    {
    }

    /// <summary>
    ///     Password as <see cref="SecureString" />.
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
    ///     New password as <see cref="SecureString" />.
    /// </summary>
    public SecureString NewPassword
    {
        get => _newPassword;
        set
        {
            if (value == _newPassword)
                return;

            _newPassword = value;

            ValidatePassword();

            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Repeated new password as <see cref="SecureString" />.
    /// </summary>
    public SecureString NewPasswordRepeat
    {
        get => _newPasswordRepeat;
        set
        {
            if (value == _newPasswordRepeat)
                return;

            _newPasswordRepeat = value;

            ValidatePassword();

            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Indicate if one of the password fields are empty.
    /// </summary>
    public bool IsPasswordEmpty
    {
        get => _isPasswordEmpty;
        set => SetProperty(ref _isPasswordEmpty, value);
    }

    /// <summary>
    ///     Indicate if the <see cref="NewPassword" /> is equal to the <see cref="NewPasswordRepeat" />.
    /// </summary>
    public bool IsRepeatedPasswordEqual
    {
        get => _isRepeatedPasswordEqual;
        set => SetProperty(ref _isRepeatedPasswordEqual, value);
    }

    /// <summary>
    ///     Check if the passwords are valid and equal.
    /// </summary>
    private void ValidatePassword()
    {
        IsPasswordEmpty = Password == null || Password.Length == 0 || NewPassword == null || NewPassword.Length == 0 ||
                          NewPasswordRepeat == null || NewPasswordRepeat.Length == 0;

        IsRepeatedPasswordEqual = !IsPasswordEmpty && SecureStringHelper.ConvertToString(NewPassword)
            .Equals(SecureStringHelper.ConvertToString(NewPasswordRepeat));
    }
}