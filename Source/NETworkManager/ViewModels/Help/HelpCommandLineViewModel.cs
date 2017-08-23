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

        private bool _displayWrongParameterNote;
        public bool DisplayWrongParameterNote
        {
            get { return _displayWrongParameterNote; }
            set
            {
                if (value == _displayWrongParameterNote)
                    return;

                _displayWrongParameterNote = value;
                OnPropertyChanged();
            }
        }

        private string _wrongParameterNote;
        public string WrongParameterNote
        {
            get { return _wrongParameterNote; }
            set
            {
                if (value == _wrongParameterNote)
                    return;

                _wrongParameterNote = value;
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
                WrongParameterNote = CommandLineManager.Current.WrongParameter;
                DisplayWrongParameterNote = true;
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
