using System.Globalization;
using System.Security;
using NETworkManager.Utilities;
using NETworkManager.Validators;

namespace NETworkManager.ViewModels;

/// <summary>
///     Windows credentials (username, domain, password) of a profile or group in the profile and group dialog (Remote
///     Desktop, IP Scanner; see <see cref="Controls.WindowsCredentialsControl" />). The password stays unchanged unless a
///     new one is entered.
/// </summary>
public class WindowsCredentialsViewModel : ViewModelBase
{
    /// <summary>
    ///     Creates the values of the dialog from a profile or group.
    /// </summary>
    /// <param name="useCredentials">The credentials are used (otherwise the ones of the group / the current user).</param>
    /// <param name="username">Username.</param>
    /// <param name="domain">Domain (optional).</param>
    /// <param name="password">Password.</param>
    /// <param name="isProfileFileEncrypted">The profile file is encrypted (no warning for the password).</param>
    public WindowsCredentialsViewModel(bool useCredentials, string username, string domain, SecureString password,
        bool isProfileFileEncrypted)
    {
        UseCredentials = useCredentials;
        Username = username;
        Domain = domain;
        Password = password;
        IsProfileFileEncrypted = isProfileFileEncrypted;
    }

    /// <summary>
    ///     The credentials are used.
    /// </summary>
    public bool UseCredentials
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public string Username
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public string Domain
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    /// <summary>
    ///     No password is set (the watermark shows an example instead of "Not changed").
    /// </summary>
    public bool IsPasswordEmpty { get; private set; } = true;

    public SecureString Password
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();

            IsPasswordEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));
            OnPropertyChanged(nameof(IsPasswordEmpty));
            OnPropertyChanged(nameof(HasError));
        }
    }

    /// <summary>
    ///     The profile file is encrypted (no warning that the password is saved as plain text).
    /// </summary>
    public bool IsProfileFileEncrypted { get; }

    /// <summary>
    ///     Used, but a value is invalid (the dialog can't be saved): the username is empty or contains spaces, the
    ///     domain is invalid or the password is empty. Same rules as the validators of the text boxes.
    /// </summary>
    public bool HasError => UseCredentials &&
                            (!IsValid(new EmptyValidator(), Username) || !IsValid(new NoSpacesValidator(), Username) ||
                             !IsValid(new EmptyOrWindowsDomainValidator(), Domain) || IsPasswordEmpty);

    /// <summary>
    ///     Username of the profile / group: empty if the credentials are not used (no sensitive information is kept).
    /// </summary>
    public string UsernameToSave => UseCredentials ? Username : string.Empty;

    /// <summary>
    ///     Domain of the profile / group: empty if the credentials are not used.
    /// </summary>
    public string DomainToSave => UseCredentials ? Domain : string.Empty;

    /// <summary>
    ///     Password of the profile / group: empty if the credentials are not used.
    /// </summary>
    public SecureString PasswordToSave => UseCredentials ? Password : new SecureString();

    private static bool IsValid(System.Windows.Controls.ValidationRule validator, string value)
    {
        return validator.Validate(value, CultureInfo.CurrentCulture).IsValid;
    }
}
