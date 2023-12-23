using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class WebConsoleConnectViewModel : ViewModelBase
{
    private string _url;

    public WebConsoleConnectViewModel(Action<WebConsoleConnectViewModel> connectCommand,
        Action<WebConsoleConnectViewModel> cancelHandler)
    {
        ConnectCommand = new RelayCommand(_ => connectCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        UrlHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.WebConsole_UrlHistory);

        LoadSettings();
    }

    public ICommand ConnectCommand { get; }
    public ICommand CancelCommand { get; }

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

    private void LoadSettings()
    {
    }
}