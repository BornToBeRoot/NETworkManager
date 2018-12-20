using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class TightVNCSettingsViewModel : ViewModelBase
    {
        #region Variables
        private const string ApplicationFileExtensionFilter = "Application (*.exe)|*.exe";
        private readonly IDialogCoordinator _dialogCoordinator;
        
        private readonly bool _isLoading;

        private string _tightVNCLocation;
        public string TightVNCLocation
        {
            get => _tightVNCLocation;
            set
            {
                if (value == _tightVNCLocation)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.TightVNC_ApplicationFilePath = value;

                // Path to tightvnc is configured....
                IsTightVNCConfigured = !string.IsNullOrEmpty(value);

                _tightVNCLocation = value;                               
                OnPropertyChanged();
            }
        }

        private bool _isTightVNCConfigured;
        public bool IsTightVNCConfigured
        {
            get => _isTightVNCConfigured;
            set
            {
                if (value == _isTightVNCConfigured)
                    return;

                _isTightVNCConfigured = value;
                OnPropertyChanged();
            }
        }
        
        private int _vncPort;
        public int VNCPort
        {
            get => _vncPort;
            set
            {
                if (value == _vncPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.TightVNC_VNCPort = value;

                _vncPort = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public TightVNCSettingsViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            TightVNCLocation = SettingsManager.Current.TightVNC_ApplicationFilePath;
            IsTightVNCConfigured = File.Exists(TightVNCLocation);
            VNCPort = SettingsManager.Current.TightVNC_VNCPort;
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
                Filter = ApplicationFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                TightVNCLocation = openFileDialog.FileName;
        }

        public ICommand ConfigureTightVNCCommand
        {
            get { return new RelayCommand(p => ConfigurePuTTYAction()); }
        }

        private void ConfigurePuTTYAction()
        {
            ConfigureTightVNC();
        }
        #endregion

        #region Methods
        private async void ConfigureTightVNC()
        {
            try
            {
                Process.Start(SettingsManager.Current.TightVNC_ApplicationFilePath);
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
            TightVNCLocation = filePath;

            OnPropertyChanged(nameof(TightVNCLocation));
        }
        #endregion
    }
}