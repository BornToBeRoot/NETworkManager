using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
///     View model of the mDNS Discovery settings: the service type profiles (like the port profiles of the Port
///     Scanner settings).
/// </summary>
public class MDNSDiscoverySettingsViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    ///     Gets the collection view of the service type profiles.
    /// </summary>
    public ICollectionView ServiceTypeProfiles { get; }

    /// <summary>
    ///     Gets or sets the selected service type profile.
    /// </summary>
    public MDNSServiceTypeProfileInfo SelectedServiceTypeProfile
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
    ///     Gets or sets the selected service type profiles (several can be deleted at once).
    /// </summary>
    public IList SelectedServiceTypeProfiles
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();

    #endregion

    #region Constructor

    /// <summary>
    ///     Initializes a new instance of the <see cref="MDNSDiscoverySettingsViewModel" /> class.
    /// </summary>
    public MDNSDiscoverySettingsViewModel()
    {
        // Own view (not the default view of the collection), sort and selection only apply to the settings
        ServiceTypeProfiles = new CollectionViewSource
            { Source = SettingsManager.Current.MDNSDiscovery_ServiceTypeProfiles }.View;
        ServiceTypeProfiles.SortDescriptions.Add(
            new SortDescription(nameof(MDNSServiceTypeProfileInfo.Name), ListSortDirection.Ascending));

        SelectFirstServiceTypeProfile();
    }

    #endregion

    #region ICommand & Actions

    public ICommand AddServiceTypeProfileCommand => new RelayCommand(_ => AddServiceTypeProfileAction());

    private void AddServiceTypeProfileAction()
    {
        _ = AddServiceTypeProfile();
    }

    public ICommand EditServiceTypeProfileCommand =>
        new RelayCommand(_ => EditServiceTypeProfileAction(), EditServiceTypeProfile_CanExecute);

    /// <summary>
    ///     Checks if exactly one service type profile is selected (e.g. F2 without a selection or with several).
    /// </summary>
    private bool EditServiceTypeProfile_CanExecute(object parameter)
    {
        return SelectedServiceTypeProfile != null && SelectedServiceTypeProfiles.Count == 1;
    }

    private void EditServiceTypeProfileAction()
    {
        _ = EditServiceTypeProfile();
    }

    public ICommand DeleteServiceTypeProfileCommand =>
        new RelayCommand(_ => DeleteServiceTypeProfileAction(), DeleteServiceTypeProfile_CanExecute);

    /// <summary>
    ///     Checks if at least one service type profile is selected (e.g. Del without a selection).
    /// </summary>
    private bool DeleteServiceTypeProfile_CanExecute(object parameter)
    {
        return SelectedServiceTypeProfiles.Count > 0;
    }

    private void DeleteServiceTypeProfileAction()
    {
        _ = DeleteServiceTypeProfile();
    }

    #endregion

    #region Methods

    private Task AddServiceTypeProfile()
    {
        var childWindow = new MDNSServiceTypeProfileChildWindow();

        var childWindowViewModel = new MDNSServiceTypeProfileViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            SettingsManager.Current.MDNSDiscovery_ServiceTypeProfiles.Add(
                new MDNSServiceTypeProfileInfo(instance.Name.Trim(), instance.ServiceTypes.Trim()));
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.MDNSDiscovery_AddServiceTypeProfile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    private Task EditServiceTypeProfile()
    {
        var childWindow = new MDNSServiceTypeProfileChildWindow();

        var profile = SelectedServiceTypeProfile;

        var childWindowViewModel = new MDNSServiceTypeProfileViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            // Same ID, kept when the profile is edited
            var editedProfile = new MDNSServiceTypeProfileInfo(instance.Name.Trim(), instance.ServiceTypes.Trim());

            if (profile.Id != Guid.Empty)
                editedProfile.Id = profile.Id;

            SettingsManager.Current.MDNSDiscovery_ServiceTypeProfiles.Remove(profile);
            SettingsManager.Current.MDNSDiscovery_ServiceTypeProfiles.Add(editedProfile);
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, profile);

        childWindow.Title = Strings.MDNSDiscovery_EditServiceTypeProfile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    ///     Deletes the selected service type profiles (after a confirmation that lists them).
    /// </summary>
    private async Task DeleteServiceTypeProfile()
    {
        var profiles = SelectedServiceTypeProfiles.Cast<MDNSServiceTypeProfileInfo>().ToList();

        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            profiles.Count == 1
                ? Strings.MDNSDiscovery_DeleteServiceTypeProfile
                : Strings.MDNSDiscovery_DeleteServiceTypeProfiles,
            (profiles.Count == 1
                ? Strings.MDNSDiscovery_DeleteServiceTypeProfileMessage
                : Strings.MDNSDiscovery_DeleteServiceTypeProfilesMessage) +
            Environment.NewLine + Environment.NewLine +
            DialogHelper.FormatItemList([.. profiles.Select(x => x.Name)]),
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        foreach (var profile in profiles)
            SettingsManager.Current.MDNSDiscovery_ServiceTypeProfiles.Remove(profile);

        SelectFirstServiceTypeProfile();
    }

    /// <summary>
    ///     Selects the first service type profile (also in the list of selected profiles, which the grid applies).
    /// </summary>
    private void SelectFirstServiceTypeProfile()
    {
        SelectedServiceTypeProfile = ServiceTypeProfiles.Cast<MDNSServiceTypeProfileInfo>().FirstOrDefault();
        SelectedServiceTypeProfiles = SelectedServiceTypeProfile != null
            ? new ArrayList { SelectedServiceTypeProfile }
            : new ArrayList();
    }

    #endregion
}
