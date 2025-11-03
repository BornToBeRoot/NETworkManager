using System;
using System.Security;

namespace NETworkManager.ViewModels;

public class CredentialsPasswordViewModel : DialogViewModelBase<CredentialsPasswordViewModel>
{
    /// <summary>
    ///     Private variable for <see cref="IsPasswordEmpty" />.
    /// </summary>
    private bool _isPasswordEmpty = true;

    /// <summary>
    ///     Private variable for <see cref="Password" />.
    /// </summary>
    private SecureString _password = new();

    /// <summary>
    ///     Initialize a new class <see cref="CredentialsPasswordViewModel" /> with <see cref="OKCommand" /> and
    ///     <see cref="CancelCommand" />.
    /// </summary>
    /// <param name="okCommand"><see cref="OKCommand" /> which is executed on OK click.</param>
    /// <param name="cancelHandler"><see cref="CancelCommand" /> which is executed on cancel click.</param>
    public CredentialsPasswordViewModel(Action<CredentialsPasswordViewModel> okCommand,
        Action<CredentialsPasswordViewModel> cancelHandler)
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
    ///     Indicate if one of the password fields are empty.
    /// </summary>
    public bool IsPasswordEmpty
    {
        get => _isPasswordEmpty;
        set => SetProperty(ref _isPasswordEmpty, value);
    }

    /// <summary>
    ///     Check if the passwords are valid.
    /// </summary>
    private void ValidatePassword()
    {
        IsPasswordEmpty = Password == null || Password.Length == 0;
    }
}