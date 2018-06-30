using System.Linq;
using System.Windows.Data;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System.ComponentModel;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class SettingsLanguageViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        ICollectionView _languages;
        public ICollectionView Languages
        {
            get { return _languages; }
        }

        private string _cultureCode = string.Empty;

        private LocalizationInfo _selectedLanguage;
        public LocalizationInfo SelectedLangauge
        {
            get { return _selectedLanguage; }
            set
            {
                if (value == _selectedLanguage)
                    return;

                if (!_isLoading && value != null) // Don't change if the value is null (can happen when a user searchs for a language....)
                {
                    LocalizationManager.Change(value);

                    SettingsManager.Current.Localization_CultureCode = value.Code;

                    RestartRequired = (value.Code != _cultureCode);
                }

                _selectedLanguage = value;
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

                Languages.Refresh();

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
            _languages = CollectionViewSource.GetDefaultView(LocalizationManager.List);
            _languages.SortDescriptions.Add(new SortDescription(nameof(LocalizationInfo.Name), ListSortDirection.Ascending));

            _languages.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                LocalizationInfo info = o as LocalizationInfo;

                string search = Search.Trim();

                // Search by: Name
                return (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.NativeName.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            SelectedLangauge = Languages.Cast<LocalizationInfo>().FirstOrDefault(x => x.Code == LocalizationManager.Current.Code);

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            _cultureCode = SettingsManager.Current.Localization_CultureCode;
        }
        #endregion

        #region ICommands & Actions
        public ICommand ClearSearchCommand
        {
            get { return new RelayCommand(p => ClearSearchAction()); }
        }

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }
        #endregion
    }
}