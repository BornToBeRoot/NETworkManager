using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using NETworkManager.Models.PowerShell;

namespace NETworkManager.ViewModels
{
    public class PowerShellSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly bool _isLoading;

        private string _applicationFilePath;
        public string ApplicationFilePath
        {
            get => _applicationFilePath;
            set
            {
                if (value == _applicationFilePath)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PowerShell_ApplicationFilePath = value;

                IsConfigured = !string.IsNullOrEmpty(value);

                _applicationFilePath = value;
                OnPropertyChanged();
            }
        }

        private string _defaultAdditionalCommandLine;
        public string DefaultAdditionalCommandLine
        {
            get => _defaultAdditionalCommandLine;
            set
            {
                if(value == _defaultAdditionalCommandLine)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PowerShell_DefaultAdditionalCommandLine = value;

                _defaultAdditionalCommandLine = value;
                OnPropertyChanged();
            }
        }

        private List<PowerShell.ExecutionPolicy> _executionPolicies = new List<PowerShell.ExecutionPolicy>();
        public List<PowerShell.ExecutionPolicy> ExecutionPolicies
        {
            get => _executionPolicies;
            set
            {
                if (value == _executionPolicies)
                    return;

                _executionPolicies = value;
                OnPropertyChanged();
            }
        }

        private PowerShell.ExecutionPolicy _executionPolicy;
        public PowerShell.ExecutionPolicy ExecutionPolicy
        {
            get => _executionPolicy;
            set
            {
                if (value == _executionPolicy)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PowerShell_DefaultExecutionPolicy = value;

                _executionPolicy = value;
                OnPropertyChanged();
            }
        }

        private bool _isConfigured;
        public bool IsConfigured
        {
            get => _isConfigured;
            set
            {
                if (value == _isConfigured)
                    return;

                _isConfigured = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public PowerShellSettingsViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ApplicationFilePath = SettingsManager.Current.PowerShell_ApplicationFilePath;
            IsConfigured = File.Exists(ApplicationFilePath);
            DefaultAdditionalCommandLine = SettingsManager.Current.PowerShell_DefaultAdditionalCommandLine;

            LoadExecutionPolicies();
        }

        private void LoadExecutionPolicies()
        {
            ExecutionPolicies = Enum.GetValues(typeof(PowerShell.ExecutionPolicy)).Cast<PowerShell.ExecutionPolicy>().ToList();
            ExecutionPolicy = ExecutionPolicies.FirstOrDefault(x => x == SettingsManager.Current.PowerShell_DefaultExecutionPolicy);
        }
        #endregion

        #region ICommands & Actions
        public ICommand BrowseFileCommand
        {
            get { return new RelayCommand(p => BrowseFileAction()); }
        }

        private void BrowseFileAction()
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = GlobalStaticConfiguration.ApplicationFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ApplicationFilePath = openFileDialog.FileName;
        }

        public ICommand ConfigureCommand
        {
            get { return new RelayCommand(p => ConfigureAction()); }
        }

        private void ConfigureAction()
        {
            Configure();
        }
        #endregion

        #region Methods
        private async void Configure()
        {
            try
            {
                Process.Start(SettingsManager.Current.PowerShell_ApplicationFilePath);
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Resources.Localization.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, settings);
            }
        }

        public void SetFilePathFromDragDrop(string filePath)
        {
            ApplicationFilePath = filePath;

            OnPropertyChanged(nameof(ApplicationFilePath));
        }
        #endregion
    }
}