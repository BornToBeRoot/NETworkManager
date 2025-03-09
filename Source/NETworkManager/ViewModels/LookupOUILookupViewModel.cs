﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Lookup;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class LookupOUILookupViewModel : ViewModelBase
{
    #region Constructor, Load settings

    public LookupOUILookupViewModel(IDialogCoordinator instance)
    {
        _dialogCoordinator = instance;

        // Search history
        SearchHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Lookup_OUI_SearchHistory);

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(OUIInfo.MACAddress), ListSortDirection.Ascending));
    }

    #endregion

    #region Methods

    private void AddSearchToHistory(string macAddressOrVendor)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.Lookup_OUI_SearchHistory.ToList(),
            macAddressOrVendor, SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.Lookup_OUI_SearchHistory.Clear();
        OnPropertyChanged(
            nameof(Search)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.Lookup_OUI_SearchHistory.Add(x));
    }

    #endregion

    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(LookupOUILookupViewModel));
    
    private readonly IDialogCoordinator _dialogCoordinator;

    private string _search;

    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;
            OnPropertyChanged();
        }
    }

    private bool _hasError;

    public bool HasError
    {
        get => _hasError;
        set
        {
            if (value == _hasError)
                return;

            _hasError = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView SearchHistoryView { get; }

    private bool _isRunning;

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value == _isRunning)
                return;

            _isRunning = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<OUIInfo> _results = new();

    public ObservableCollection<OUIInfo> Results
    {
        get => _results;
        set
        {
            if (value != null && value == _results)
                return;

            _results = value;
        }
    }

    public ICollectionView ResultsView { get; }

    private OUIInfo _selectedResult;

    public OUIInfo SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (value == _selectedResult)
                return;

            _selectedResult = value;
            OnPropertyChanged();
        }
    }

    private IList _selectedResults = new ArrayList();

    public IList SelectedResults
    {
        get => _selectedResults;
        set
        {
            if (Equals(value, _selectedResults))
                return;

            _selectedResults = value;
            OnPropertyChanged();
        }
    }

    private bool _nothingFound;

    public bool NothingFound
    {
        get => _nothingFound;
        set
        {
            if (value == _nothingFound)
                return;

            _nothingFound = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands & Actions

    public ICommand OUILookupCommand => new RelayCommand(_ => OUILookupAction(), OUILookup_CanExecute);

    private bool OUILookup_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&            
               !ConfigurationManager.Current.IsChildWindowOpen && 
               !HasError;
    }

    private async void OUILookupAction()
    {
        IsRunning = true;

        Results.Clear();

        var macAddresses = new HashSet<string>();
        var vendors = new HashSet<string>();

        // Parse the input
        foreach (var search in Search.Split(';'))
        {
            var searchTrim = search.Trim();

            if (Regex.IsMatch(searchTrim, RegexHelper.MACAddressRegex) ||
                Regex.IsMatch(searchTrim, RegexHelper.MACAddressFirst3BytesRegex))
                macAddresses.Add(searchTrim);
            else
                vendors.Add(searchTrim);
        }

        // Temporary collection to avoid duplicate entries
        var results = new HashSet<OUIInfo>();

        // Get OUI information's by MAC-Address
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator (Doesn't work with async/await)
        foreach (var macAddress in macAddresses)
        foreach (var info in await OUILookup.LookupByMacAddressAsync(macAddress))
            results.Add(info);

        // Get OUI information's by Vendor
        foreach (var info in await OUILookup.SearchByVendorsAsync(vendors)) results.Add(info);

        // Add the results to the collection
        foreach (var result in results)
            Results.Add(result);

        // Show a message if no vendor was found
        NothingFound = Results.Count == 0;

        // Add the MAC-Address or Vendor to the history
        AddSearchToHistory(Search);

        IsRunning = false;
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    private async Task ExportAction()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.Export
        };

        var exportViewModel = new ExportViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    instance.ExportAll
                        ? Results
                        : new ObservableCollection<OUIInfo>(SelectedResults.Cast<OUIInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);
                
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.Lookup_OUI_ExportFileType = instance.FileType;
            SettingsManager.Current.Lookup_OUI_ExportFilePath = instance.FilePath;
        }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new[]
        {
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        }, true, SettingsManager.Current.Lookup_OUI_ExportFileType, SettingsManager.Current.Lookup_OUI_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    #endregion
}