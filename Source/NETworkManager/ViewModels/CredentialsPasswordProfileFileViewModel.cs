using System;
using System.Security;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class CredentialsPasswordProfileFileViewModel : ViewModelBase
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
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        ProfileName = profileName;
        ShowWrongPassword = showWrongPassword;
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
    ///     Name of the profile file.
    /// </summary>
    public string ProfileName
    {
        get => _profileName;
        set
        {
            if (value == _profileName)
                return;

            _profileName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Show note that the password is wrong.
    /// </summary>
    public bool ShowWrongPassword
    {
        get => _showWrongPassword;
        set
        {
            if (value == _showWrongPassword)
                return;

            _showWrongPassword = value;
            OnPropertyChanged();
        }
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
        set
        {
            if (value == _isPasswordEmpty)
                return;

            _isPasswordEmpty = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Check if the passwords are valid.
    /// </summary>
    private void ValidatePassword()
    {
        IsPasswordEmpty = Password == null || Password.Length == 0;
    }
}