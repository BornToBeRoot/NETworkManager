using System;
using System.Security;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class CredentialsSetPasswordViewModel : DialogViewModelBase<CredentialsSetPasswordViewModel>
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
    ///     Private variable for <see cref="Password" />.
    /// </summary>
    private SecureString _password = new();

    /// <summary>
    ///     Private variable for <see cref="PasswordRepeat" />.
    /// </summary>
    private SecureString _passwordRepeat = new();

    /// <summary>
    ///     Initialize a new class <see cref="CredentialsSetPasswordViewModel" /> with <see cref="OKCommand" /> and
    ///     <see cref="CancelCommand" />.
    /// </summary>
    /// <param name="okCommand"><see cref="OKCommand" /> which is executed on OK click.</param>
    /// <param name="cancelHandler"><see cref="CancelCommand" /> which is executed on cancel click.</param>
    public CredentialsSetPasswordViewModel(Action<CredentialsSetPasswordViewModel> okCommand,
        Action<CredentialsSetPasswordViewModel> cancelHandler)
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
    ///     Repeated password as <see cref="SecureString" />.
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
    ///     Indicate if one of the password fields are empty.
    /// </summary>
    public bool IsPasswordEmpty
    {
        get => _isPasswordEmpty;
        set => SetProperty(ref _isPasswordEmpty, value);
    }

    /// <summary>
    ///     Indicate if the <see cref="PasswordRepeat" /> is equal to the <see cref="Password" />.
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
        IsPasswordEmpty = Password == null || Password.Length == 0 || PasswordRepeat == null ||
                          PasswordRepeat.Length == 0;

        IsRepeatedPasswordEqual = !IsPasswordEmpty && SecureStringHelper.ConvertToString(Password)
            .Equals(SecureStringHelper.ConvertToString(PasswordRepeat));
    }
}