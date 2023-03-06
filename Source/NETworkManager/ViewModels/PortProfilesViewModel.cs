using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels;

public class PortProfilesViewModel : ViewModelBase
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

            PortProfiles.Refresh();

            OnPropertyChanged();
        }
    }

    public ICollectionView PortProfiles { get; }

    private IList _selectedPortProfiles = new ArrayList();
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

    public PortProfilesViewModel(Action<PortProfilesViewModel> okCommand, Action<PortProfilesViewModel> cancelHandler)
    {
        _isLoading = true;

        OKCommand = new RelayCommand(p => okCommand(this));
        CancelCommand = new RelayCommand(p => cancelHandler(this));

        PortProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortProfiles);
        PortProfiles.SortDescriptions.Add(new SortDescription(nameof(PortProfileInfo.Name), ListSortDirection.Ascending));
        PortProfiles.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not PortProfileInfo info)
                return false;

            var search = Search.Trim();

            // Search: Name, Ports
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.Ports.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        _isLoading = false;
    }

    public List<PortProfileInfo> GetSelectedPortProfiles()
    {
        return new List<PortProfileInfo>(SelectedPortProfiles.Cast<PortProfileInfo>());
    }
}
