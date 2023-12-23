using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class SNMPOIDProfileViewModel : ViewModelBase
{
    private readonly SNMPOIDProfileInfo _info;
    private readonly bool _isLoading;

    private bool _infoChanged;

    private bool _isEdited;

    private SNMPMode _mode;

    private string _name;

    private string _oid;

    public SNMPOIDProfileViewModel(Action<SNMPOIDProfileViewModel> saveCommand,
        Action<SNMPOIDProfileViewModel> cancelHandler, bool isEdited = false, SNMPOIDProfileInfo info = null)
    {
        _isLoading = true;

        SaveCommand = new RelayCommand(_ => saveCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        Modes = new List<SNMPMode> { SNMPMode.Get, SNMPMode.Walk, SNMPMode.Set };

        IsEdited = isEdited;

        _info = info ?? new SNMPOIDProfileInfo();

        Name = _info.Name;
        OID = _info.OID;
        Mode = Modes.FirstOrDefault(x => x == _info.Mode);

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

    public List<SNMPMode> Modes { get; }

    public SNMPMode Mode
    {
        get => _mode;
        set
        {
            if (value == _mode)
                return;

            _mode = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();

            // Re-validate OID if mode changed
            OnPropertyChanged(nameof(OID));
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
        InfoChanged = _info.Name != Name || _info.OID != OID || _info.Mode != Mode;
    }
}