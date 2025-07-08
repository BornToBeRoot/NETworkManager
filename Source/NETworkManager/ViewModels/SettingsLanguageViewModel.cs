﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Localization;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class SettingsLanguageViewModel : ViewModelBase
{
    #region Construtor, LoadSettings

    public SettingsLanguageViewModel()
    {
        _isLoading = true;

        Languages = CollectionViewSource.GetDefaultView(LocalizationManager.List);
        Languages.SortDescriptions.Add(new SortDescription(nameof(LocalizationInfo.IsOfficial),
            ListSortDirection.Descending));
        Languages.SortDescriptions.Add(new SortDescription(nameof(LocalizationInfo.Name), ListSortDirection.Ascending));

        Languages.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not LocalizationInfo info)
                return false;

            var search = Search.Trim();

            // Search by: Name, NativeName
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.NativeName.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        SelectedLanguage = Languages.Cast<LocalizationInfo>()
            .FirstOrDefault(x => x.Code == LocalizationManager.GetInstance().Current.Code);

        _isLoading = false;
    }

    #endregion

    #region Variables

    private readonly bool _isLoading;

    public ICollectionView Languages { get; }

    private LocalizationInfo _selectedLanguage;

    public LocalizationInfo SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value == _selectedLanguage)
                return;

            if (!_isLoading &&
                value != null) // Don't change if the value is null (can happen when a user search for a language....)
            {
                LocalizationManager.GetInstance().Change(value);

                SettingsManager.Current.Localization_CultureCode = value.Code;
            }

            _selectedLanguage = value;
            OnPropertyChanged();
        }
    }

    private string _search;

    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;

            Languages.Refresh();

            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands & Actions

    public ICommand ClearSearchCommand => new RelayCommand(_ => ClearSearchAction());

    private void ClearSearchAction()
    {
        Search = string.Empty;
    }

    #endregion
}