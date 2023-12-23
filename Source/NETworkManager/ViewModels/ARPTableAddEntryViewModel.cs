using System;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class ArpTableAddEntryViewModel : ViewModelBase
{
    private string _ipAddress;

    private string _macAddress;

    public ArpTableAddEntryViewModel(Action<ArpTableAddEntryViewModel> addCommand,
        Action<ArpTableAddEntryViewModel> cancelHandler)
    {
        AddCommand = new RelayCommand(_ => addCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));
    }

    public ICommand AddCommand { get; }

    public ICommand CancelCommand { get; }

    public string IPAddress
    {
        get => _ipAddress;
        set
        {
            if (value == _ipAddress)
                return;

            _ipAddress = value;
            OnPropertyChanged();
        }
    }

    public string MACAddress
    {
        get => _macAddress;
        set
        {
            if (value == _macAddress)
                return;

            _macAddress = value;
            OnPropertyChanged();
        }
    }
}