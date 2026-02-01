using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// Represents the view model for managing port profiles.
/// </summary>
public class PortProfilesViewModel : ViewModelBase
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PortProfilesViewModel"/> class.
    /// </summary>
    /// <param name="okCommand">The action to execute when the OK command is invoked.</param>
    /// <param name="cancelHandler">The action to execute when the Cancel command is invoked.</param>
    public PortProfilesViewModel(Action<PortProfilesViewModel> okCommand, Action<PortProfilesViewModel> cancelHandler)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

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

        SelectedPortProfiles = new ArrayList {
            PortProfiles.Cast<PortProfileInfo>().FirstOrDefault()
        };
    }

    #endregion

    #region Variables

    /// <summary>
    /// Gets the command to confirm the selection.
    /// </summary>
    public ICommand OKCommand { get; }

    /// <summary>
    /// Gets the command to cancel the operation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Backing field for the Search property.
    /// </summary>
    private string _search;

    /// <summary>
    /// Gets or sets the search text to filter the port profiles.
    /// </summary>
    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;

            PortProfiles.Refresh();

            SelectedPortProfiles = new ArrayList {
                PortProfiles.Cast<PortProfileInfo>().FirstOrDefault()
            };

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection of port profiles.
    /// </summary>
    public ICollectionView PortProfiles { get; }

    /// <summary>
    /// Backing field for the SelectedPortProfiles property.
    /// </summary>
    private IList _selectedPortProfiles = new ArrayList();

    /// <summary>
    /// Gets or sets the list of selected port profiles.
    /// </summary>
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
    #endregion

    #region Methods
    /// <summary>
    /// Gets the selected port profiles as a typed collection.
    /// </summary>
    /// <returns>A collection of selected <see cref="PortProfileInfo"/>.</returns>
    public IEnumerable<PortProfileInfo> GetSelectedPortProfiles()
    {
        return [.. SelectedPortProfiles.Cast<PortProfileInfo>()];
    }
    #endregion
}