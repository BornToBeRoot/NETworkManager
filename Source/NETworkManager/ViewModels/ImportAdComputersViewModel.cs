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

    private string _ldapSearchBase = string.Empty;
    private string _targetGroupName = string.Empty;
    private bool _excludeDisabledComputerAccounts = true;
    private bool _isBusy;

    public ImportAdComputersViewModel(Window parentWindow, string suggestedTargetGroup, Action closeDialog)
    {
        _parentWindow = parentWindow;
        _closeDialog = closeDialog;

        GroupNames = new ObservableCollection<string>(ProfileManager.GetGroupNames());
        TargetGroupName = string.IsNullOrWhiteSpace(suggestedTargetGroup)
            ? GroupNames.FirstOrDefault() ?? string.Empty
            : suggestedTargetGroup;

        ImportCommand = new RelayCommand(_ => ImportAction(), Import_CanExecute);
        CancelCommand = new RelayCommand(_ => CancelAction());
    }

    public ObservableCollection<string> GroupNames { get; }

    public string LdapSearchBase
    {
        get => _ldapSearchBase;
        set
        {
            if (value == _ldapSearchBase)
                return;

            _ldapSearchBase = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string TargetGroupName
    {
        get => _targetGroupName;
        set
        {
            if (value == _targetGroupName)
                return;

            _targetGroupName = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool ExcludeDisabledComputerAccounts
    {
        get => _excludeDisabledComputerAccounts;
        set
        {
            if (value == _excludeDisabledComputerAccounts)
                return;

            _excludeDisabledComputerAccounts = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (value == _isBusy)
                return;

            _isBusy = value;
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

        if (string.IsNullOrWhiteSpace(LdapSearchBase) || string.IsNullOrWhiteSpace(TargetGroupName))
            return false;

        var trimmedGroup = TargetGroupName.Trim();
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
        if (ProfileManager.LoadedProfileFile == null)
        {
            ConfigurationManager.OnDialogOpen();

            await DialogHelper.ShowMessageAsync(_parentWindow, Strings.Error,
                Strings.ActiveDirectoryImportRequiresProfileFile, ChildWindowIcon.Error);

            ConfigurationManager.OnDialogClose();
            return;
        }

        IsBusy = true;

        try
        {
            var searchBase = LdapSearchBase.Trim();
            var targetGroup = TargetGroupName.Trim();

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
