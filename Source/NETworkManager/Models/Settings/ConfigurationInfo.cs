using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NETworkManager.Models.Settings
{
    public class ConfigurationInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsAdmin { get; set; }
        public string ExecutionPath { get; set; }
        public string ApplicationFullName { get; set; }
        public string ApplicationName { get; set; }

        private bool _fixAirspace { get; set; }
        public bool FixAirspace
        {
            get { return _fixAirspace; }
            set
            {
                if (value == _fixAirspace)
                    return;

                _fixAirspace = value;
                OnPropertyChanged();
            }
        }

        public ConfigurationInfo()
        {

        }
    }
}
