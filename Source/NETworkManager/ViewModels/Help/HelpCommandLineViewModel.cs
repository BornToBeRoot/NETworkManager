using System.Windows.Input;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using System;
using NETworkManager.Models.Settings;
using System.Collections.ObjectModel;
using System.Linq;

namespace NETworkManager.ViewModels.Help
{
    public class HelpCommandLineViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        public ObservableCollection<LocalizationInfo> LanguageCollection { get; set; }

        private LocalizationInfo _localizationSelectedItem;
        public LocalizationInfo LocalizationSelectedItem
        {
            get { return _localizationSelectedItem; }
            set
            {
                if (value == _localizationSelectedItem)
                    return;

                if (!_isLoading)
                {
                    LocalizationManager.Change(value);

                    SettingsManager.Current.Localization_CultureCode = value.Code;
                }

                _localizationSelectedItem = value;
                OnPropertyChanged();
            }
        }

        private bool _displayWrongParameter;
        public bool DisplayWrongParameter
        {
            get { return _displayWrongParameter; }
            set
            {
                if (value == _displayWrongParameter)
                    return;

                _displayWrongParameter = value;
                OnPropertyChanged();
            }
        }

        private string _wrongParameter;
        public string WrongParameter
        {
            get { return _wrongParameter; }
            set
            {
                if (value == _wrongParameter)
                    return;

                _wrongParameter = value;
                OnPropertyChanged();
            }
        }

        private string _parameterHelp;
        public string ParameterHelp
        {
            get { return _parameterHelp; }
            set
            {
                if (value == _parameterHelp)
                    return;

                _parameterHelp = value;
                OnPropertyChanged();
            }
        }

        private string _parameterResetSettings;
        public string ParameterResetSettings
        {
            get { return _parameterResetSettings; }
            set
            {
                if (value == _parameterResetSettings)
                    return;

                _parameterResetSettings = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor, load settings
        public HelpCommandLineViewModel()
        {
            LoadSettings();

            if (!string.IsNullOrEmpty(CommandLineManager.Current.WrongParameter))
            {
                WrongParameter = CommandLineManager.Current.WrongParameter;
                DisplayWrongParameter = true;
            }

            ParameterHelp = CommandLineManager.ParameterHelp;
            ParameterResetSettings = CommandLineManager.ParameterResetSettings;

            _isLoading = false;
        }       

        private void LoadSettings()
        {
            LanguageCollection = new ObservableCollection<LocalizationInfo>(LocalizationManager.List);
            LocalizationSelectedItem = LanguageCollection.FirstOrDefault(x => x.Code == LocalizationManager.Current.Code);
        }
        #endregion
    }
}
