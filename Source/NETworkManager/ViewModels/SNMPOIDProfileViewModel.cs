using NETworkManager.Utilities;
using System;
using System.Windows.Input;
using NETworkManager.Models.Network;

namespace NETworkManager.ViewModels;

public class SNMPOIDProfileViewModel : ViewModelBase
{
    private readonly bool _isLoading;

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    private string _name;
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

    private string _oid;
    public string OID
    {
        get => _oid;
        set
        {
            if (_oid == value)
                return;

            _oid = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();
        }
    }

    private readonly SNMPOIDProfileInfo _info;

    private bool _infoChanged;
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

    private bool _isEdited;
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

    public SNMPOIDProfileViewModel(Action<SNMPOIDProfileViewModel> saveCommand, Action<SNMPOIDProfileViewModel> cancelHandler, bool isEdited = false, SNMPOIDProfileInfo info = null)
    {
        _isLoading = true;

        SaveCommand = new RelayCommand(p => saveCommand(this));
        CancelCommand = new RelayCommand(p => cancelHandler(this));

        IsEdited = isEdited;

        _info = info ?? new SNMPOIDProfileInfo();

        Name = _info.Name;
        OID = _info.OID;

        _isLoading = false;
    }

    public void Validate()
    {
        InfoChanged = _info.Name != Name || _info.OID != OID;
    }
}
