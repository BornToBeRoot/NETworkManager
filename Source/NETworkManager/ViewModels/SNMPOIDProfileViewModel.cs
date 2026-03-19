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
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();
        }
    }

    public string OID
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();
        }
    }

    public List<SNMPMode> Modes { get; }

    public SNMPMode Mode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();

            // Re-validate OID if mode changed
            OnPropertyChanged(nameof(OID));
        }
    }

    public bool InfoChanged
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsEdited
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    private void Validate()
    {
        InfoChanged = _info.Name != Name || _info.OID != OID || _info.Mode != Mode;
    }
}