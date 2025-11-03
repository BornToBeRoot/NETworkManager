using System;
using System.Security;

namespace NETworkManager.ViewModels;

public class CredentialsPasswordProfileFileViewModel : DialogViewModelBase<CredentialsPasswordProfileFileViewModel>
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
    ///     Private variable for <see cref="ProfileName" />.
    /// </summary>
    private string _profileName;

    /// <summary>
    ///     Private variable for <see cref="ShowWrongPassword" />.
    /// </summary>
    private bool _showWrongPassword;

    /// <summary>
    ///     Initialize a new class <see cref="CredentialsPasswordProfileFileViewModel" /> with <see cref="OKCommand" /> and
    ///     <see cref="CancelCommand" />.
    /// </summary>
    /// <param name="okCommand"><see cref="OKCommand" /> which is executed on OK click.</param>
    /// <param name="cancelHandler"><see cref="CancelCommand" /> which is executed on cancel click.</param>
    /// <param name="profileName">Name of the profile file.</param>
    /// <param name="showWrongPassword">Show note that the password is wrong.</param>
    public CredentialsPasswordProfileFileViewModel(Action<CredentialsPasswordProfileFileViewModel> okCommand,
        Action<CredentialsPasswordProfileFileViewModel> cancelHandler, string profileName,
        bool showWrongPassword = false)
        : base(okCommand, cancelHandler)
    {
        ProfileName = profileName;
        ShowWrongPassword = showWrongPassword;
    }

    /// <summary>
    ///     Name of the profile file.
    /// </summary>
    public string ProfileName
    {
        get => _profileName;
        set => SetProperty(ref _profileName, value);
    }

    /// <summary>
    ///     Show note that the password is wrong.
    /// </summary>
    public bool ShowWrongPassword
    {
        get => _showWrongPassword;
        set => SetProperty(ref _showWrongPassword, value);
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