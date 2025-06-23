using System;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class OKCancelInfoMessageViewModel : ViewModelBase
{
    private readonly string _message;

    public OKCancelInfoMessageViewModel(Action<OKCancelInfoMessageViewModel> deleteCommand,
        Action<OKCancelInfoMessageViewModel> cancelHandler, string message)
    {
        DeleteCommand = new RelayCommand(_ => deleteCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        Message = message;
    }

    public ICommand DeleteCommand { get; }

    public ICommand CancelCommand { get; }

    public string Message
    {
        get => _message;
        private init
        {
            if (value == _message)
                return;

            _message = value;
            OnPropertyChanged();
        }
    }
}