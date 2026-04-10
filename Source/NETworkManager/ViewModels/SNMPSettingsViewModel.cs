using Lextm.SharpSnmpLib.Messaging;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class SNMPSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public ICollectionView OIDProfiles { get; }

    public SNMPOIDProfileInfo SelectedOIDProfile
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    public List<WalkMode> WalkModes { get; private set; }

    public WalkMode WalkMode
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_WalkMode = value;

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
                SettingsManager.Current.SNMP_Timeout = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public int Port
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_Port = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Contructor, load settings

    public SNMPSettingsViewModel()
    {
        _isLoading = true;

        OIDProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_OidProfiles);
        OIDProfiles.SortDescriptions.Add(new SortDescription(nameof(SNMPOIDProfileInfo.Name),
            ListSortDirection.Ascending));

        SelectedOIDProfile = OIDProfiles.Cast<SNMPOIDProfileInfo>().FirstOrDefault();

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        WalkModes = Enum.GetValues(typeof(WalkMode)).Cast<WalkMode>().OrderBy(x => x.ToString()).ToList();
        WalkMode = WalkModes.First(x => x == SettingsManager.Current.SNMP_WalkMode);
        Timeout = SettingsManager.Current.SNMP_Timeout;
        Port = SettingsManager.Current.SNMP_Port;
    }

    #endregion

    #region ICommand & Actions

    public ICommand AddOIDProfileCommand => new RelayCommand(_ => AddOIDProfileAction());

    private void AddOIDProfileAction()
    {
        AddOIDProfile().ConfigureAwait(false);
    }

    public ICommand EditOIDProfileCommand => new RelayCommand(_ => EditOIDProfileAction());

    private void EditOIDProfileAction()
    {
        EditOIDProfile().ConfigureAwait(false);
    }

    public ICommand DeleteOIDProfileCommand => new RelayCommand(_ => DeleteOIDProfileAction());

    private void DeleteOIDProfileAction()
    {
        DeleteOIDProfile().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private async Task AddOIDProfile()
    {
        var childWindow = new SNMPOIDProfileChildWindow();

        var childWindowViewModel = new SNMPOIDProfileViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            SettingsManager.Current.SNMP_OidProfiles.Add(new SNMPOIDProfileInfo(
                instance.Name,
                instance.OID,
                instance.Mode)
             );
        }, async _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.AddOIDProfile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    public async Task EditOIDProfile()
    {
        var childWindow = new SNMPOIDProfileChildWindow();

        var childWindowViewModel = new SNMPOIDProfileViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            SettingsManager.Current.SNMP_OidProfiles.Remove(SelectedOIDProfile);
            SettingsManager.Current.SNMP_OidProfiles.Add(new SNMPOIDProfileInfo(instance.Name, instance.OID,
                instance.Mode));
        }, async _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, true, SelectedOIDProfile);

        childWindow.Title = Strings.EditOIDProfile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    private async Task DeleteOIDProfile()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.DeleteOIDProfile,
            Strings.DeleteOIDProfileMessage,
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        SettingsManager.Current.SNMP_OidProfiles.Remove(SelectedOIDProfile);
    }

    #endregion
}