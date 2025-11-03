using System;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Models.Network;
using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class SNMPOIDProfilesViewModel : DialogViewModelBase<SNMPOIDProfilesViewModel>
{
    private string _search;

    private SNMPOIDProfileInfo _selectedOIDProfile;

    public SNMPOIDProfilesViewModel(Action<SNMPOIDProfilesViewModel> okCommand,
        Action<SNMPOIDProfilesViewModel> cancelHandler)
        : base(okCommand, cancelHandler)
    {
        OIDProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_OidProfiles);
        OIDProfiles.SortDescriptions.Add(new SortDescription(nameof(SNMPOIDProfileInfo.Name),
            ListSortDirection.Ascending));
        OIDProfiles.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not SNMPOIDProfileInfo info)
                return false;

            var search = Search.Trim();

            // Search: Name, OID
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.OID.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
        };
    }

    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;

            OIDProfiles.Refresh();

            OnPropertyChanged();
        }
    }

    public ICollectionView OIDProfiles { get; }

    public SNMPOIDProfileInfo SelectedOIDProfile
    {
        get => _selectedOIDProfile;
        set
        {
            if (Equals(value, _selectedOIDProfile))
                return;

            _selectedOIDProfile = value;
            OnPropertyChanged();
        }
    }
}