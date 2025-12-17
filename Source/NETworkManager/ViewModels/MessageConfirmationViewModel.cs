using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// Represents the ViewModel for a message dialog with OK and Cancel options.
/// </summary>
public class MessageConfirmationViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageConfirmationViewModel"/> class.
    /// </summary>
    /// <param name="confirmCommand">The action to execute when the confirm button is clicked.</param>
    /// <param name="cancelHandler">The action to execute when the Cancel button is clicked.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="icon">The icon to display in the message window.</param>
    /// <param name="confirmButtonText">The text for the confirm button.</param>
    /// <param name="cancelButtonText">The text for the cancel button.</param>
    public MessageConfirmationViewModel(Action<MessageConfirmationViewModel> confirmCommand,
        Action<MessageConfirmationViewModel> cancelHandler, string message, ChildWindowIcon icon = ChildWindowIcon.Info, string confirmButtonText = null, string cancelButtonText = null)
    {
        ConfirmCommand = new RelayCommand(_ => confirmCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        Message = message;

        ConfirmButtonText = confirmButtonText ?? Localization.Resources.Strings.OK;
        CancelButtonText = cancelButtonText ?? Localization.Resources.Strings.Cancel;
    }

    /// <summary>
    /// Gets the command for the OK button.
    /// </summary>
    public ICommand ConfirmCommand { get; }

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

    private readonly string _confirmButtonText;

    /// <summary>
    /// Gets the text for the confirm button.
    /// </summary>
    public string ConfirmButtonText
    {
        get => _confirmButtonText;
        private init
        {
            if (value == _confirmButtonText)
                return;

            _confirmButtonText = value;
            OnPropertyChanged();
        }
    }

    private readonly string _cancelButtonText;

    /// <summary>
    /// Gets the text for the cancel button.
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