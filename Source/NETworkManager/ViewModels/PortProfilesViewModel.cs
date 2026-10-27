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
        // Only with a selected profile - also for Enter in the grid (the OK button is disabled without a selection)
        OKCommand = new RelayCommand(_ => okCommand(this), _ => SelectedPortProfiles.Count > 0);
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        // Own view (not the default view of the collection, which the settings show too) - sort and filter only
        // apply to this dialog
        PortProfiles = new CollectionViewSource { Source = SettingsManager.Current.PortScanner_PortProfiles }.View;
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

        SelectedPortProfiles = CreateFirstProfileSelection();
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
    /// Gets or sets the search text to filter the port profiles.
    /// </summary>
    public string Search
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            PortProfiles.Refresh();

            SelectedPortProfiles = CreateFirstProfileSelection();

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection of port profiles.
    /// </summary>
    public ICollectionView PortProfiles { get; }

    /// <summary>
    /// Gets or sets the list of selected port profiles.
    /// </summary>
    public IList SelectedPortProfiles
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

    #region Methods
    /// <summary>
    /// Gets the selected port profiles as a typed collection.
    /// </summary>
    /// <returns>A collection of selected <see cref="PortProfileInfo"/>.</returns>
    public IEnumerable<PortProfileInfo> GetSelectedPortProfiles()
    {
        return [.. SelectedPortProfiles.OfType<PortProfileInfo>()];
    }

    /// <summary>
    /// Creates the selection with the first shown profile - empty if no profile is shown (no profiles or no match for
    /// the search), never a <see langword="null" /> entry.
    /// </summary>
    /// <returns>Selection.</returns>
    private ArrayList CreateFirstProfileSelection()
    {
        var firstProfile = PortProfiles.Cast<PortProfileInfo>().FirstOrDefault();

        return firstProfile != null ? new ArrayList { firstProfile } : new ArrayList();
    }
    #endregion
}