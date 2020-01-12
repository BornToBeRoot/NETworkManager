using NETworkManager.Models.Settings;
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

        private bool _ignoreCertificateErrors;
        public bool IgnoreCertificateErrors
        {
            get => _ignoreCertificateErrors;
            set
            {
                if (value == _ignoreCertificateErrors)
                    return;

                _ignoreCertificateErrors = value;
                OnPropertyChanged();
            }
        }

        public WebConsoleConnectViewModel(Action<WebConsoleConnectViewModel> connectCommand, Action<WebConsoleConnectViewModel> cancelHandler)
        {
            ConnectCommand = new RelayCommand(p => connectCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            UrlHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.WebConsole_UrlHistory);

            LoadSettings();
        }

        private void LoadSettings()
        {
            IgnoreCertificateErrors = SettingsManager.Current.WebConsole_IgnoreCertificateErrors;
        }
    }
}
