using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class PortProfileViewModel : ViewModelBase
{
    private readonly PortProfileInfo _info;
    private readonly bool _isLoading;

    private readonly string _previousPortAsString;

    private bool _infoChanged;

    private bool _isEdited;

    private string _name;

    private string _ports;

    public PortProfileViewModel(Action<PortProfileViewModel> saveCommand, Action<PortProfileViewModel> cancelHandler,
        bool isEdited = false, PortProfileInfo info = null)
    {
        _isLoading = true;

        SaveCommand = new RelayCommand(_ => saveCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        IsEdited = isEdited;

        _info = info ?? new PortProfileInfo();

        Name = _info.Name;

        // List to string
        if (_info.Ports != null)
            Ports = _info.Ports;

        _previousPortAsString = Ports;

        _isLoading = false;
    }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;

            _name = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();
        }
    }

    public string Ports
    {
        get => _ports;
        set
        {
            if (_ports == value)
                return;

            _ports = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();
        }
    }

    public bool InfoChanged
    {
        get => _infoChanged;
        set
        {
            if (value == _infoChanged)
                return;

            _infoChanged = value;
            OnPropertyChanged();
        }
    }

    public bool IsEdited
    {
        get => _isEdited;
        set
        {
            if (value == _isEdited)
                return;

            _isEdited = value;
            OnPropertyChanged();
        }
    }

    private void Validate()
    {
        InfoChanged = _info.Name != Name || _previousPortAsString != Ports;
    }
}