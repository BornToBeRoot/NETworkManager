using System;
using System.Security;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for changing the credentials password.
/// </summary>
public class CredentialsChangePasswordViewModel : ViewModelBase
{
    /// <summary>
    ///     Initialize a new class <see cref="CredentialsChangePasswordViewModel" /> with <see cref="OKCommand" /> and
    ///     <see cref="CancelCommand" />.
    /// </summary>
    /// <param name="okCommand"><see cref="OKCommand" /> which is executed on OK click.</param>
    /// <param name="cancelHandler"><see cref="CancelCommand" /> which is executed on cancel click.</param>
    public CredentialsChangePasswordViewModel(Action<CredentialsChangePasswordViewModel> okCommand,
        Action<CredentialsChangePasswordViewModel> cancelHandler)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));
    }

    /// <summary>
    ///     Command which is called when the OK button is clicked.
    /// </summary>
    public ICommand OKCommand { get; }

    /// <summary>
    ///     Command which is called when the cancel button is clicked.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    ///     Password as <see cref="SecureString" />.
    /// </summary>
    public SecureString Password
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            ValidatePassword();

            OnPropertyChanged();
        }
    } = new();

    /// <summary>
    ///     New password as <see cref="SecureString" />.
    /// </summary>
    public SecureString NewPassword
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            ValidatePassword();

            OnPropertyChanged();
        }
    } = new();

    /// <summary>
    ///     Repeated new password as <see cref="SecureString" />.
    /// </summary>
    public SecureString NewPasswordRepeat
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            ValidatePassword();

            OnPropertyChanged();
        }
    } = new();

    /// <summary>
    ///     Indicate if one of the password fields are empty.
    /// </summary>
    public bool IsPasswordEmpty
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    /// <summary>
    ///     Indicate if the <see cref="NewPassword" /> is equal to the <see cref="NewPasswordRepeat" />.
    /// </summary>
    public bool IsRepeatedPasswordEqual
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
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