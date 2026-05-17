using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Utilities.ActiveDirectory;

namespace NETworkManager.ViewModels;

public sealed class ImportAdComputersViewModel : ViewModelBase
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ImportAdComputersViewModel));

    private readonly Action<IReadOnlyList<ProfileImportCandidate>, ImportAdComputersViewModel> _searchCompleted;

    public ImportAdComputersViewModel(Action<IReadOnlyList<ProfileImportCandidate>, ImportAdComputersViewModel> searchCompleted, Action cancelDialog, ImportAdComputersViewModel previousState = null)
    {
        _searchCompleted = searchCompleted;

        if (previousState != null)
        {
            LdapSearchBase = previousState.LdapSearchBase;
            LdapServer = previousState.LdapServer;
            LdapPort = previousState.LdapPort;
            UseSsl = previousState.UseSsl;
            AuthMode = previousState.AuthMode;
            Username = previousState.Username;
            Password = previousState.Password;
            ExcludeDisabledAccounts = previousState.ExcludeDisabledAccounts;
            AdditionalLdapFilter = previousState.AdditionalLdapFilter;
        }
        else
        {
            LdapSearchBase = SettingsManager.Current.Profiles_ImportActiveDirectorySearchBase ?? string.Empty;
            LdapServer = SettingsManager.Current.Profiles_ImportActiveDirectoryServer ?? string.Empty;
            LdapPort = SettingsManager.Current.Profiles_ImportActiveDirectoryPort;
            UseSsl = SettingsManager.Current.Profiles_ImportActiveDirectoryUseSsl;
            AuthMode = SettingsManager.Current.Profiles_ImportActiveDirectoryAuthMode;
            ExcludeDisabledAccounts = SettingsManager.Current.Profiles_ImportActiveDirectoryExcludeDisabledAccounts;
            AdditionalLdapFilter = SettingsManager.Current.Profiles_ImportActiveDirectoryAdditionalFilter ?? string.Empty;
        }

        SearchCommand = new RelayCommand(_ => SearchAction(), _ => Search_CanExecute());
        CancelCommand = new RelayCommand(_ => cancelDialog());
    }

    public string LdapSearchBase
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

    public string LdapServer
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

    public int LdapPort
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

    public bool UseSsl
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();

            LdapPort = value switch
            {
                // Auto-switch the well-known port when the user hasn't picked a custom one
                true when LdapPort == GlobalStaticConfiguration.Profiles_ImportActiveDirectoryPort_Ldap =>
                    GlobalStaticConfiguration.Profiles_ImportActiveDirectoryPort_Ldaps,
                false when LdapPort == GlobalStaticConfiguration.Profiles_ImportActiveDirectoryPort_Ldaps =>
                    GlobalStaticConfiguration.Profiles_ImportActiveDirectoryPort_Ldap,
                _ => LdapPort
            };
        }
    }

    public bool ExcludeDisabledAccounts
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

    public string AdditionalLdapFilter
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

    private ActiveDirectoryAuthenticationMode AuthMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsCurrentUserAuth));
            OnPropertyChanged(nameof(IsCustomAuth));
        }
    }

    public bool IsCurrentUserAuth
    {
        get => AuthMode == ActiveDirectoryAuthenticationMode.CurrentUser;
        set
        {
            if (value)
                AuthMode = ActiveDirectoryAuthenticationMode.CurrentUser;
        }
    }

    public bool IsCustomAuth
    {
        get => AuthMode == ActiveDirectoryAuthenticationMode.Custom;
        set
        {
            if (value)
                AuthMode = ActiveDirectoryAuthenticationMode.Custom;
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
        }
    } = string.Empty;

    public SecureString Password
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            IsPasswordEmpty = value == null || value.Length == 0;
        }
    } = new();

    public bool IsPasswordEmpty
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsSearching
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsStatusMessageDisplayed
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

    public string StatusMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ICommand SearchCommand { get; }

    public ICommand CancelCommand { get; }

    private bool Search_CanExecute()
    {
        return !IsSearching;
    }

    private async void SearchAction()
    {
        IsSearching = true;
        StatusMessage = Localization.Resources.Strings.SearchingActiveDirectoryDots;
        IsStatusMessageDisplayed = true;

        await Task.Delay(GlobalStaticConfiguration.ApplicationUIDelayInterval);

        var options = new ActiveDirectorySearchOptions
        {
            SearchBase = LdapSearchBase.Trim(),
            Server = LdapServer?.Trim() ?? string.Empty,
            Port = LdapPort,
            UseSsl = UseSsl,
            ExcludeDisabledAccounts = ExcludeDisabledAccounts,
            AdditionalFilter = AdditionalLdapFilter?.Trim() ?? string.Empty,
            Username = AuthMode == ActiveDirectoryAuthenticationMode.Custom ? Username.Trim() : string.Empty,
            Password = AuthMode == ActiveDirectoryAuthenticationMode.Custom ? Password : null
        };

        IReadOnlyList<ActiveDirectoryComputerRecord> computers;

        try
        {
            computers = await Task.Run(() =>
                ActiveDirectoryComputerSearcher.GetComputersInSubtree(options)).ConfigureAwait(true);

            IsSearching = false;
            IsStatusMessageDisplayed = false;
        }
        catch (Exception exception)
        {
            Log.Error("Active Directory search failed.", exception);

            IsSearching = false;
            StatusMessage = exception.Message;
            IsStatusMessageDisplayed = true;

            return;
        }

        if (computers.Count == 0)
        {
            StatusMessage = Localization.Resources.Strings.ActiveDirectoryNoComputersFound;
            IsStatusMessageDisplayed = true;

            return;
        }

        var importedAt = DateTime.Now.ToString("g", System.Globalization.CultureInfo.CurrentUICulture);
        var candidates = computers
            .Select(c => new ProfileImportCandidate(
                name: c.ProfileName,
                host: c.DnsHostName,
                description: string.Format(Localization.Resources.Strings.ActiveDirectory_ImportDescription, importedAt),
                importSource: ProfileImportSource.ActiveDirectory,
                importSourceId: c.ObjectGuid))
            .ToList();

        PersistSettings(options);

        _searchCompleted(candidates, this);

    }

    private void PersistSettings(ActiveDirectorySearchOptions options)
    {
        SettingsManager.Current.Profiles_ImportActiveDirectorySearchBase = options.SearchBase;
        SettingsManager.Current.Profiles_ImportActiveDirectoryServer = options.Server;
        SettingsManager.Current.Profiles_ImportActiveDirectoryPort = options.Port;
        SettingsManager.Current.Profiles_ImportActiveDirectoryUseSsl = options.UseSsl;
        SettingsManager.Current.Profiles_ImportActiveDirectoryExcludeDisabledAccounts = options.ExcludeDisabledAccounts;
        SettingsManager.Current.Profiles_ImportActiveDirectoryAdditionalFilter = options.AdditionalFilter;
        SettingsManager.Current.Profiles_ImportActiveDirectoryAuthMode = AuthMode;
    }
}
