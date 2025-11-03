using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using NETworkManager.Models.Network;
using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class PortProfilesViewModel : DialogViewModelBase<PortProfilesViewModel>
{
    private string _search;

    private IList _selectedPortProfiles = new ArrayList();

    public PortProfilesViewModel(Action<PortProfilesViewModel> okCommand, Action<PortProfilesViewModel> cancelHandler)
        : base(okCommand, cancelHandler)
    {
        PortProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortProfiles);
        PortProfiles.SortDescriptions.Add(
            new SortDescription(nameof(PortProfileInfo.Name), ListSortDirection.Ascending));
        PortProfiles.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not PortProfileInfo info)
                return false;

            var search = Search.Trim();

            // Search: Name, Ports
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Ports.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
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

            PortProfiles.Refresh();

            OnPropertyChanged();
        }
    }

    public ICollectionView PortProfiles { get; }

    public IList SelectedPortProfiles
    {
        get => _selectedPortProfiles;
        set
        {
            if (Equals(value, _selectedPortProfiles))
                return;

            _selectedPortProfiles = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<PortProfileInfo> GetSelectedPortProfiles()
    {
        return new List<PortProfileInfo>(SelectedPortProfiles.Cast<PortProfileInfo>());
    }
}