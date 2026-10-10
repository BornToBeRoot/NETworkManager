using log4net;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.RDAP;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class RDAPSettingsViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(RDAPSettingsViewModel));

    private readonly bool _isLoading;

    /// <summary>
    ///     Status of the cached IANA bootstrap files.
    /// </summary>
    public IReadOnlyList<RDAPBootstrapCacheInfo> BootstrapFiles
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

    public string CachePath => GlobalStaticConfiguration.RDAP_CachePath;

    public bool IsUpdating
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

    public bool FollowReferral
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RDAP_FollowReferral = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public int Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RDAP_Timeout = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public RDAPSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;

        // Refresh the table if a query downloaded the files in the meantime.
        RDAPBootstrapService.GetInstance().CacheInfoChanged += (_, _) =>
            Application.Current?.Dispatcher.InvokeAsync(RefreshBootstrapFiles);

        RefreshBootstrapFiles();
    }

    private void LoadSettings()
    {
        FollowReferral = SettingsManager.Current.RDAP_FollowReferral;
        Timeout = SettingsManager.Current.RDAP_Timeout;
    }

    private void RefreshBootstrapFiles()
    {
        _ = LoadBootstrapFilesAsync();
    }

    private async Task LoadBootstrapFilesAsync()
    {
        try
        {
            BootstrapFiles = await RDAPBootstrapService.GetInstance().GetCacheInfosAsync(CachePath);
        }
        catch (Exception ex)
        {
            Log.Error("Could not load RDAP bootstrap cache info.", ex);
        }
    }

    #endregion

    #region ICommands & Actions

    public ICommand UpdateNowCommand => new RelayCommand(_ => UpdateNowAction());

    private void UpdateNowAction()
    {
        _ = UpdateBootstrapFilesAsync();
    }

    private async Task UpdateBootstrapFilesAsync()
    {
        IsUpdating = true;
        IsStatusMessageDisplayed = false;

        try
        {
            await RDAPBootstrapService.GetInstance().UpdateNowAsync(CachePath);

            StatusMessage = Strings.BootstrapFilesAreUpToDate;
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(Strings.RDAPBootstrapUnavailableX, ex.Message);
        }

        IsStatusMessageDisplayed = true;

        await LoadBootstrapFilesAsync();

        IsUpdating = false;
    }

    public ICommand OpenLocationCommand => new RelayCommand(_ => OpenLocationAction());

    private void OpenLocationAction()
    {
        Directory.CreateDirectory(CachePath);

        Process.Start("explorer.exe", CachePath);
    }

    #endregion
}
