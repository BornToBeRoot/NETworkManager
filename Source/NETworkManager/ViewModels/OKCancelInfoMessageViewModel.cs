using System;

namespace NETworkManager.ViewModels;

public class OKCancelInfoMessageViewModel : DialogViewModelBase<OKCancelInfoMessageViewModel>
{
    public OKCancelInfoMessageViewModel(Action<OKCancelInfoMessageViewModel> okCommand,
        Action<OKCancelInfoMessageViewModel> cancelHandler, string message, string okButtonText = null, string cancelButtonText = null, ChildWindowIcon icon = ChildWindowIcon.Info)
        : base(okCommand, cancelHandler)
    {
        Message = message;

        OKButtonText = okButtonText ?? Localization.Resources.Strings.OK;
        CancelButtonText = cancelButtonText ?? Localization.Resources.Strings.Cancel;
        Icon = icon;
    }

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

    private readonly string _cancelButtonText;
    public string CancelButtonText
    {
        get => _cancelButtonText;
        private init
        {
            if (value == _cancelButtonText)
                return;

            _cancelButtonText = value;
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