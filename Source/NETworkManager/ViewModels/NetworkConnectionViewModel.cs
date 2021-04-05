using NETworkManager.Settings;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels
{
    public class NetworkConnectionViewModel : ViewModelBase
    {
        #region  Variables 
        public bool CheckPublicIPAddress => SettingsManager.Current.Dashboard_CheckPublicIPAddress;
        #endregion

        #region Constructor, load settings

        public NetworkConnectionViewModel()
        {
            // Detect if network address or status changed...
            NetworkChange.NetworkAvailabilityChanged += (sender, args) => CheckConnectionAsync();
            NetworkChange.NetworkAddressChanged += (sender, args) => CheckConnectionAsync();

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            CheckConnectionAsync();
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions

        #endregion

        #region Methods
        public void CheckConnectionAsync()
        {
            Task.Run(() => CheckConnection());
        }

        public void CheckConnection()
        {

        }

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
            switch (e.PropertyName)
            {
                case nameof(SettingsInfo.Dashboard_CheckPublicIPAddress):
                    OnPropertyChanged(nameof(CheckPublicIPAddress));
                    break;
            }
        }
        #endregion
    }
}

