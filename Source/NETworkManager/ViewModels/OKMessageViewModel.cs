using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class OKMessageViewModel : ViewModelBase
{
    public OKMessageViewModel(Action<OKMessageViewModel> okCommand, string message, string okButtonText = null, ChildWindowIcon icon = ChildWindowIcon.Info)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));

        Message = message;

        OKButtonText = okButtonText ?? Localization.Resources.Strings.OK;
        Icon = icon;
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

    private ChildWindowIcon _icon;

    public ChildWindowIcon Icon
    {
        get => _icon;
        private init
        {
            if (value == _icon)
                return;

            _icon = value;
            OnPropertyChanged();
        }
    }
}
