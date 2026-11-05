using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using NETworkManager.Models.Network;

namespace NETworkManager.ViewModels;

/// <summary>
///     Override of the SNMP version and credentials in the profile and group dialog (SNMP tool, IP Scanner; see
///     <see cref="Controls.SNMPCredentialsControl" />): v1 / v2c with a community, v3 with a user. The passwords stay
///     unchanged unless a new one is entered.
/// </summary>
public class SNMPCredentialsViewModel : ViewModelBase, IHasInputError
{
    /// <summary>
    ///     Creates the values of the dialog from a profile or group.
    /// </summary>
    /// <param name="isOverridden">The version and credentials are overridden.</param>
    /// <param name="version">SNMP version.</param>
    /// <param name="community">Community (v1 / v2c).</param>
    /// <param name="security">Security level (v3).</param>
    /// <param name="username">Username (v3).</param>
    /// <param name="authenticationProvider">Authentication provider (v3).</param>
    /// <param name="auth">Authentication password (v3).</param>
    /// <param name="privacyProvider">Privacy provider (v3).</param>
    /// <param name="priv">Privacy password (v3).</param>
    /// <param name="isProfileFileEncrypted">The profile file is encrypted (no warning for the passwords).</param>
    public SNMPCredentialsViewModel(bool isOverridden, SNMPVersion version, SecureString community,
        SNMPV3Security security, string username, SNMPV3AuthenticationProvider authenticationProvider,
        SecureString auth, SNMPV3PrivacyProvider privacyProvider, SecureString priv, bool isProfileFileEncrypted)
    {
        Versions = Enum.GetValues<SNMPVersion>().ToList();
        Securities = [SNMPV3Security.NoAuthNoPriv, SNMPV3Security.AuthNoPriv, SNMPV3Security.AuthPriv];
        AuthenticationProviders = Enum.GetValues<SNMPV3AuthenticationProvider>().ToList();
        PrivacyProviders = Enum.GetValues<SNMPV3PrivacyProvider>().ToList();

        IsOverridden = isOverridden;
        Version = version;
        Community = community;
        Security = security;
        Username = username;
        AuthenticationProvider = authenticationProvider;
        Auth = auth;
        PrivacyProvider = privacyProvider;
        Priv = priv;
        IsProfileFileEncrypted = isProfileFileEncrypted;
    }

    /// <summary>
    ///     The version and credentials are overridden (otherwise the ones of the group / the settings are used).
    /// </summary>
    public bool IsOverridden
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

    public IReadOnlyList<SNMPVersion> Versions { get; }

    public SNMPVersion Version
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsV3));
            OnPropertyChanged(nameof(HasAuth));
            OnPropertyChanged(nameof(HasPriv));
            OnPropertyChanged(nameof(HasError));
        }
    }

    /// <summary>
    ///     v3: a user instead of a community.
    /// </summary>
    public bool IsV3 => Version == SNMPVersion.V3;

    /// <summary>
    ///     v3 with authentication (AuthNoPriv, AuthPriv): the authentication provider and password are shown.
    /// </summary>
    public bool HasAuth => IsV3 && Security != SNMPV3Security.NoAuthNoPriv;

    /// <summary>
    ///     v3 with privacy (AuthPriv): the privacy provider and password are shown.
    /// </summary>
    public bool HasPriv => IsV3 && Security == SNMPV3Security.AuthPriv;

    /// <summary>
    ///     No community is set (the watermark shows an example instead of "Not changed").
    /// </summary>
    public bool IsCommunityEmpty { get; private set; } = true;

    public SecureString Community
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();

            IsCommunityEmpty = IsEmpty(value);
            OnPropertyChanged(nameof(IsCommunityEmpty));
            OnPropertyChanged(nameof(HasError));
        }
    }

    public IReadOnlyList<SNMPV3Security> Securities { get; }

    public SNMPV3Security Security
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasAuth));
            OnPropertyChanged(nameof(HasPriv));
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

    public IReadOnlyList<SNMPV3AuthenticationProvider> AuthenticationProviders { get; }

    public SNMPV3AuthenticationProvider AuthenticationProvider
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
    ///     No authentication password is set.
    /// </summary>
    public bool IsAuthEmpty { get; private set; } = true;

    public SecureString Auth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();

            IsAuthEmpty = IsEmpty(value);
            OnPropertyChanged(nameof(IsAuthEmpty));
            OnPropertyChanged(nameof(HasError));
        }
    }

    public IReadOnlyList<SNMPV3PrivacyProvider> PrivacyProviders { get; }

    public SNMPV3PrivacyProvider PrivacyProvider
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
    ///     No privacy password is set.
    /// </summary>
    public bool IsPrivEmpty { get; private set; } = true;

    public SecureString Priv
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();

            IsPrivEmpty = IsEmpty(value);
            OnPropertyChanged(nameof(IsPrivEmpty));
            OnPropertyChanged(nameof(HasError));
        }
    }

    /// <summary>
    ///     The profile file is encrypted (no warning that the passwords are saved as plain text).
    /// </summary>
    public bool IsProfileFileEncrypted { get; }

    /// <summary>
    ///     A text box of the control has invalid input (set by the control): WPF doesn't pass invalid input to the
    ///     view model, which keeps the last valid value (e.g. the first character of a cleared username).
    /// </summary>
    public bool HasInputError
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
    ///     Overridden, but a value is missing (the dialog can't be saved): the community (v1 / v2c), the username or a
    ///     password of the security level (v3).
    /// </summary>
    public bool HasError => IsOverridden && (IsV3
        ? HasInputError || string.IsNullOrWhiteSpace(Username) || (HasAuth && IsAuthEmpty) || (HasPriv && IsPrivEmpty)
        : IsCommunityEmpty);

    /// <summary>
    ///     Community of the profile / group: only for v1 / v2c (an override of v3 has none).
    /// </summary>
    public SecureString CommunityToSave => IsOverridden && !IsV3 ? Community : new SecureString();

    /// <summary>
    ///     Authentication password of the profile / group: only for v3 with authentication.
    /// </summary>
    public SecureString AuthToSave => IsOverridden && HasAuth ? Auth : new SecureString();

    /// <summary>
    ///     Privacy password of the profile / group: only for v3 with privacy.
    /// </summary>
    public SecureString PrivToSave => IsOverridden && HasPriv ? Priv : new SecureString();

    private static bool IsEmpty(SecureString value)
    {
        // Length: not decrypted to a managed string
        return value == null || value.Length == 0;
    }
}
