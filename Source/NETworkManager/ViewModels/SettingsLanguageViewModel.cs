using System.Linq;
using System.Windows.Data;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System.ComponentModel;
using System;

namespace NETworkManager.ViewModels
{
    public class SettingsLanguageViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        ICollectionView _localizations;
        public ICollectionView Localizations
        {
            get { return _localizations; }
        }

        private string _cultureCode = string.Empty;

        private LocalizationInfo _localizationSelectedItem;
        public LocalizationInfo LocalizationSelectedItem
        {
            get { return _localizationSelectedItem; }
            set
            {
                if (value == _localizationSelectedItem)
                    return;

                if (!_isLoading && value != null) // Don't change if the value is null (can happen when a user searchs for a language....)
                {
                    LocalizationManager.Change(value);

                    SettingsManager.Current.Localization_CultureCode = value.Code;

                    RestartRequired = (value.Code != _cultureCode);
                }

                _localizationSelectedItem = value;
                OnPropertyChanged();
            }
        }

        private string _search;
        public string Search
        {
            get { return _search; }
            set
            {
                if (value == _search)
                    return;

                _search = value;

                Localizations.Refresh();

                OnPropertyChanged();
            }
        }

        private bool _restartRequired;
        public bool RestartRequired
        {
            get { return _restartRequired; }
            set
            {
                if (value == _restartRequired)
                    return;

                _restartRequired = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Construtor, LoadSettings
        public SettingsLanguageViewModel()
        {
            _localizations = CollectionViewSource.GetDefaultView(LocalizationManager.List);
            _localizations.SortDescriptions.Add(new SortDescription(nameof(LocalizationInfo.Name), ListSortDirection.Ascending));

            _localizations.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                LocalizationInfo info = o as LocalizationInfo;

                string search = Search.Trim();

                // Search by: Name
                return (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.NativeName.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            LocalizationSelectedItem = Localizations.Cast<LocalizationInfo>().FirstOrDefault(x => x.Code == LocalizationManager.Current.Code);

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            _cultureCode = SettingsManager.Current.Localization_CultureCode;
        }

        #endregion
    }
}