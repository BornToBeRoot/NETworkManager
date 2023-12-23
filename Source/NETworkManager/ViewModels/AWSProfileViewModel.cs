using System;
using System.Windows.Input;
using NETworkManager.Models.AWS;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class AWSProfileViewModel : ViewModelBase
{
    private readonly AWSProfileInfo _info;
    private readonly bool _isLoading;

    private bool _infoChanged;

    private bool _isEdited;

    private bool _isEnabled;

    private string _profile;

    private string _region;

    public AWSProfileViewModel(Action<AWSProfileViewModel> saveCommand, Action<AWSProfileViewModel> cancelHandler,
        bool isEdited = false, AWSProfileInfo info = null)
    {
        _isLoading = true;

        SaveCommand = new RelayCommand(_ => saveCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        IsEdited = isEdited;

        _info = info ?? new AWSProfileInfo();

        IsEnabled = _info.IsEnabled;
        Profile = _info.Profile;
        Region = _info.Region;

        _isLoading = false;
    }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
                return;

            _isEnabled = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();
        }
    }

    public string Profile
    {
        get => _profile;
        set
        {
            if (_profile == value)
                return;

            _profile = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();
        }
    }

    public string Region
    {
        get => _region;
        set
        {
            if (_region == value)
                return;

            _region = value;

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
        InfoChanged = _info.IsEnabled != IsEnabled || _info.Profile != Profile || _info.Region != Region;
    }
}