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

        private bool _isDialogOpen { get; set; }
        public bool IsDialogOpen
        {
            get { return _isDialogOpen; }
            set
            {
                if (value == _isDialogOpen)
                    return;

                _isDialogOpen = value;
                OnPropertyChanged();
            }
        }

        public ConfigurationInfo()
        {

        }
    }
}
