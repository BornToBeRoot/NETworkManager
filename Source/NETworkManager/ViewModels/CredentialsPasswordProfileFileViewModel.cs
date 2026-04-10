using NETworkManager.Utilities;
using System;
using System.Security;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for entering the password for a profile file.
/// </summary>
public class CredentialsPasswordProfileFileViewModel : ViewModelBase
{
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
    ///     Show note that the password is wrong.
    /// </summary>
    public bool ShowWrongPassword
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
    ///     Check if the passwords are valid.
    /// </summary>
    private void ValidatePassword()
    {
        IsPasswordEmpty = Password == null || Password.Length == 0;
    }
}