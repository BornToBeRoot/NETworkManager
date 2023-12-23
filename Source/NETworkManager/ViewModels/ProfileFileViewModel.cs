using System;
using System.Windows.Input;
using NETworkManager.Profiles;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class ProfileFileViewModel : ViewModelBase
{
    private bool _isEdit;

    private string _name;

    public ProfileFileViewModel(Action<ProfileFileViewModel> addCommand, Action<ProfileFileViewModel> cancelHandler,
        ProfileFileInfo info = null)
    {
        AcceptCommand = new RelayCommand(_ => addCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        if (info == null)
            return;

        Name = info.Name;

        IsEdit = true;
    }

    public ICommand AcceptCommand { get; }

    public ICommand CancelCommand { get; }

    public string Name
    {
        get => _name;
        set
        {
            if (value == _name)
                return;

            _name = value;
            OnPropertyChanged();
        }
    }

    public bool IsEdit
    {
        get => _isEdit;
        set
        {
            if (value == _isEdit)
                return;

            _isEdit = value;
            OnPropertyChanged();
        }
    }
}