using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using log4net;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Utilities.ActiveDirectory;

namespace NETworkManager.ViewModels;

public sealed class ImportAdComputersViewModel : ViewModelBase
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ImportAdComputersViewModel));

    private readonly Window _parentWindow;
    private readonly Action _closeDialog;

    public ImportAdComputersViewModel(Window parentWindow, string groupName, Action closeDialog)
    {
        _parentWindow = parentWindow;
        _closeDialog = closeDialog;

        GroupNames = new ObservableCollection<string>(ProfileManager.GetGroupNames());
        GroupName = string.IsNullOrWhiteSpace(groupName)
            ? GroupNames.FirstOrDefault() ?? string.Empty
            : groupName;

        LdapSearchBase = SettingsManager.Current.Profiles_ImportLdapSearchBase ?? string.Empty;

        ImportCommand = new RelayCommand(_ => ImportAction(), Import_CanExecute);
        CancelCommand = new RelayCommand(_ => CancelAction());
    }

    public ObservableCollection<string> GroupNames { get; }

    public string LdapSearchBase
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string GroupName
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool ExcludeDisabledComputerAccounts
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

    public bool IsBusy
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ICommand ImportCommand { get; }

    public ICommand CancelCommand { get; }

    private bool Import_CanExecute(object parameter)
    {
        if (IsBusy)
            return false;

        if (string.IsNullOrWhiteSpace(LdapSearchBase) || string.IsNullOrWhiteSpace(GroupName))
            return false;

        var trimmedGroup = GroupName.Trim();
        if (trimmedGroup.StartsWith('~'))
            return false;

        return true;
    }

    private void CancelAction()
    {
        _closeDialog();
    }

    private async void ImportAction()
    {
        IsBusy = true;

        try
        {
            var searchBase = LdapSearchBase.Trim();
            var targetGroup = GroupName.Trim();

            IReadOnlyList<ActiveDirectoryComputerRecord> computers;

            try
            {
                computers = await Task.Run(() =>
                        ActiveDirectoryComputerSearcher.GetComputersInSubtree(searchBase,
                            ExcludeDisabledComputerAccounts))
                    .ConfigureAwait(true);
            }
            catch (Exception exception)
            {
                Log.Error("Active Directory computer import query failed.", exception);

                ConfigurationManager.OnDialogOpen();

                await DialogHelper.ShowMessageAsync(_parentWindow, Strings.Error,
                    $"{Strings.ActiveDirectoryImportFailed}{Environment.NewLine}{Environment.NewLine}{exception.Message}",
                    ChildWindowIcon.Error);

                ConfigurationManager.OnDialogClose();
                return;
            }

            SettingsManager.Current.Profiles_ImportLdapSearchBase = searchBase;
            SettingsManager.Save();

            var targetGroupInfo = ProfileManager.LoadedProfileFileData.Groups
                .FirstOrDefault(group => group.Name.Equals(targetGroup, StringComparison.OrdinalIgnoreCase));

            var existingProfileNames = new HashSet<string>(
                targetGroupInfo?.Profiles.Select(profile => profile.Name) ?? [],
                StringComparer.OrdinalIgnoreCase);

            var importedCount = 0;
            var skippedDuplicateCount = 0;
            var skippedNoDnsCount = 0;

            foreach (var computer in computers)
            {
                var dnsHostName = computer.DnsHostName.Trim();

                if (string.IsNullOrWhiteSpace(dnsHostName))
                {
                    skippedNoDnsCount++;
                    continue;
                }

                if (existingProfileNames.Contains(computer.ProfileName))
                {
                    skippedDuplicateCount++;
                    continue;
                }

                var profile = CreateRemoteDesktopProfileForImportedComputer(computer.ProfileName, dnsHostName,
                    targetGroup);

                ProfileManager.AddProfile(profile);
                existingProfileNames.Add(computer.ProfileName);
                importedCount++;
            }

            ProfileManager.Save();

            ConfigurationManager.OnDialogOpen();

            await DialogHelper.ShowMessageAsync(_parentWindow, Strings.ImportComputersFromActiveDirectory,
                string.Format(Strings.ActiveDirectoryImportSummary, importedCount, skippedDuplicateCount,
                    skippedNoDnsCount),
                ChildWindowIcon.Info);

            ConfigurationManager.OnDialogClose();

            _closeDialog();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static ProfileInfo CreateRemoteDesktopProfileForImportedComputer(string profileName, string dnsHostName,
        string targetGroup)
    {
        return new ProfileInfo
        {
            Name = profileName,
            Host = dnsHostName,
            Group = targetGroup,
            RemoteDesktop_Enabled = true,
            RemoteDesktop_InheritHost = true,
            RemoteDesktop_Host = dnsHostName,
            TagsCollection = new ObservableSetCollection<string>()
        };
    }
}
