using System;
using System.Windows.Input;
using NETworkManager.Profiles;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class ProfileFileViewModel : ViewModelBase
{
    public ProfileFileViewModel(Action<ProfileFileViewModel> okCommand, Action<ProfileFileViewModel> cancelHandler,
        ProfileFileInfo info = null)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        if (info == null)
            return;

        Name = info.Name;

        IsEdit = true;
    }

    public ICommand OKCommand { get; }

    public ICommand CancelCommand { get; }

    public string Name
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

    public bool IsEdit
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
}