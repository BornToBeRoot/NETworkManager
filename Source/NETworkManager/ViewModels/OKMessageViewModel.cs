using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class OKMessageViewModel : ViewModelBase
{
    public OKMessageViewModel(Action<OKMessageViewModel> okCommand, string message, string okButtonText = null)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));

        Message = message;

        OKButtonText = okButtonText ?? Localization.Resources.Strings.OK;
    }

    public ICommand OKCommand { get; }

    private readonly string _message;

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

    private readonly string _okButtonText;

    public string OKButtonText
    {
        get => _okButtonText;
        private init
        {
            if (value == _okButtonText)
                return;

            _okButtonText = value;
            OnPropertyChanged();
        }
    }
}
