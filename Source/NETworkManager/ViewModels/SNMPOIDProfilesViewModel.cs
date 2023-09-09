using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class SNMPOIDProfilesViewModel : ViewModelBase
{
    private readonly bool _isLoading;

    public ICommand OKCommand { get; }

    public ICommand CancelCommand { get; }


    private string _search;
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

    private SNMPOIDProfileInfo _selectedOIDProfile;
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

    public SNMPOIDProfilesViewModel(Action<SNMPOIDProfilesViewModel> okCommand, Action<SNMPOIDProfilesViewModel> cancelHandler)
    {
        _isLoading = true;

        OKCommand = new RelayCommand(p => okCommand(this));
        CancelCommand = new RelayCommand(p => cancelHandler(this));

        OIDProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_OidProfiles);
        OIDProfiles.SortDescriptions.Add(new SortDescription(nameof(SNMPOIDProfileInfo.Name), ListSortDirection.Ascending));
        OIDProfiles.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not SNMPOIDProfileInfo info)
                return false;

            var search = Search.Trim();

            // Search: Name, OID
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.OID.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        _isLoading = false;
    }
}
