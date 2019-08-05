using System.ComponentModel;

namespace NETworkManager.ViewModels
{
    public class WiFiViewModel : ViewModelBase
    {
        #region  Variables 

        #endregion

        #region Constructor, load settings

        public WiFiViewModel()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions

        #endregion

        #region Methods

        public void OnViewVisible()
        {

        }

        public void OnViewHide()
        {

        }
        #endregion

        #region Events
        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }
        #endregion
    }
}

