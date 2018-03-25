using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class PuTTYSettingsViewModel : ViewModelBase
    {
        #region Variables
        private const string ApplicationFileExtensionFilter = "Application (*.exe)|*.exe";

        private bool _isLoading = true;

        private string _puTTYLocation;
        public string PuTTYLocation
        {
            get { return _puTTYLocation; }
            set
            {
                if (value == _puTTYLocation)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PuTTY_PuTTYLocation = value;

                _puTTYLocation = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public PuTTYSettingsViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            PuTTYLocation = SettingsManager.Current.PuTTY_PuTTYLocation;
        }
        #endregion

        #region ICommands & Actions
        public ICommand BrowseFileCommand
        {
            get { return new RelayCommand(p => BrowseFileAction()); }
        }

        private void BrowseFileAction()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = ApplicationFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                PuTTYLocation = openFileDialog.FileName;
        }
        #endregion
    }
}