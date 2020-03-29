using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class WebConsoleConnectViewModel : ViewModelBase
    {
        public ICommand ConnectCommand { get; }
        public ICommand CancelCommand { get; }

        private string _url;
        public string Url
        {
            get => _url;
            set
            {
                if (value == _url)
                    return;

                _url = value;
                OnPropertyChanged();
            }
        }
        
        public ICollectionView UrlHistoryView { get; }

        public WebConsoleConnectViewModel(Action<WebConsoleConnectViewModel> connectCommand, Action<WebConsoleConnectViewModel> cancelHandler)
        {
            ConnectCommand = new RelayCommand(p => connectCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            UrlHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.WebConsole_UrlHistory);

            LoadSettings();
        }

        private void LoadSettings()
        {

        }
    }
}
