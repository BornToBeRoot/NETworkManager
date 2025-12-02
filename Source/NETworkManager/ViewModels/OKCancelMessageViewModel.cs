using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// Represents the ViewModel for a message dialog with OK and Cancel options.
/// </summary>
public class OKCancelMessageViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OKCancelMessageViewModel"/> class.
    /// </summary>
    /// <param name="okCommand">The action to execute when the OK button is clicked.</param>
    /// <param name="cancelHandler">The action to execute when the Cancel button is clicked.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="icon">The icon to display in the message window.</param>
    /// <param name="okButtonText">The text for the OK button.</param>
    /// <param name="cancelButtonText">The text for the Cancel button.</param>
    public OKCancelMessageViewModel(Action<OKCancelMessageViewModel> okCommand,
        Action<OKCancelMessageViewModel> cancelHandler, string message, ChildWindowIcon icon = ChildWindowIcon.Info, string okButtonText = null, string cancelButtonText = null)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        Message = message;

        OKButtonText = okButtonText ?? Localization.Resources.Strings.OK;
        CancelButtonText = cancelButtonText ?? Localization.Resources.Strings.Cancel;
    }

    /// <summary>
    /// Gets the command for the OK button.
    /// </summary>
    public ICommand OKCommand { get; }

    /// <summary>
    /// Gets the command for the Cancel button.
    /// </summary>
    public ICommand CancelCommand { get; }

    private readonly string _message;

    /// <summary>
    /// Gets the message to display.
    /// </summary>
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

    /// <summary>
    /// Gets the text for the OK button.
    /// </summary>
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

    /// <summary>
    /// Gets the text for the Cancel button.
    /// </summary>
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

    /// <summary>
    /// Gets the icon to display.
    /// </summary>
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