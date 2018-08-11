using NETworkManager.Models.Settings;
using System.ComponentModel;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class SubnetCalculatorHostViewModel : ViewModelBase
    {
        #region Variables
        public bool ShowCurrentApplicationTitle => SettingsManager.Current.Window_ShowCurrentApplicationTitle;

        #endregion

        #region Constructor
        public SubnetCalculatorHostViewModel()
        {
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
        }
        #endregion

        #region Events
        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.Window_ShowCurrentApplicationTitle))
                OnPropertyChanged(nameof(ShowCurrentApplicationTitle));
        }
        #endregion
    }
}
