using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using NETworkManager.Models.PowerShell;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels
{
    public class AWSSessionManagerSettingsViewModel : ViewModelBase
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

        private string _additionalCommandLine;
        public string AdditionalCommandLine
        {
            get => _additionalCommandLine;
            set
            {
                if(value == _additionalCommandLine)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PowerShell_AdditionalCommandLine = value;

                _additionalCommandLine = value;
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
                    SettingsManager.Current.PowerShell_ExecutionPolicy = value;

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
        public AWSSessionManagerSettingsViewModel(IDialogCoordinator instance)
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
            AdditionalCommandLine = SettingsManager.Current.PowerShell_AdditionalCommandLine;

            LoadExecutionPolicies();
        }

        private void LoadExecutionPolicies()
        {
            ExecutionPolicies = System.Enum.GetValues(typeof(PowerShell.ExecutionPolicy)).Cast<PowerShell.ExecutionPolicy>().ToList();
            ExecutionPolicy = ExecutionPolicies.FirstOrDefault(x => x == SettingsManager.Current.PowerShell_ExecutionPolicy);
        }
        #endregion

        #region ICommands & Actions
        public ICommand BrowseFileCommand => new RelayCommand(p => BrowseFileAction());

        private void BrowseFileAction()
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = GlobalStaticConfiguration.ApplicationFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ApplicationFilePath = openFileDialog.FileName;
        }

        public ICommand ConfigureCommand => new RelayCommand(p => ConfigureAction());

        private void ConfigureAction()
        {
            Configure();
        }
        #endregion

        #region Methods
        private async Task Configure()
        {
            try
            {
                Process.Start(SettingsManager.Current.PowerShell_ApplicationFilePath);
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, settings);
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