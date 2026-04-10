using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class SNMPOIDProfilesViewModel : ViewModelBase
{
    public SNMPOIDProfilesViewModel(Action<SNMPOIDProfilesViewModel> okCommand,
        Action<SNMPOIDProfilesViewModel> cancelHandler)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

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

        SelectedOIDProfile = OIDProfiles.Cast<SNMPOIDProfileInfo>().FirstOrDefault();
    }

    public ICommand OKCommand { get; }
    public ICommand CancelCommand { get; }

    public string Search
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OIDProfiles.Refresh();

            SelectedOIDProfile = OIDProfiles.Cast<SNMPOIDProfileInfo>().FirstOrDefault();

            OnPropertyChanged();
        }
    }

    public ICollectionView OIDProfiles { get; }

    public SNMPOIDProfileInfo SelectedOIDProfile
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }
}