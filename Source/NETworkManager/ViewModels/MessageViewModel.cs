using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// Represents the ViewModel for a message dialog with an OK option.
/// </summary>
public class MessageViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageViewModel"/> class.
    /// </summary>
    /// <param name="confirmCommand">The action to execute when the confirm button is clicked.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="icon">The icon to display in the message window.</param>
    /// <param name="confirmButtonText">The text for the OK button.</param>
    public MessageViewModel(Action<MessageViewModel> confirmCommand, string message, ChildWindowIcon icon = ChildWindowIcon.Info, string confirmButtonText = null)
    {
        ConfirmCommand = new RelayCommand(_ => confirmCommand(this));

        Message = message;

        ConfirmButtonText = confirmButtonText ?? Localization.Resources.Strings.OK;
        Icon = icon;
    }

    /// <summary>
    /// Gets the command for the OK button.
    /// </summary>
    public ICommand ConfirmCommand { get; }

    /// <summary>
    /// Gets the message to display.
    /// </summary>
    public string Message
    {
        get;
        private init
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the text for the confirm button.
    /// </summary>
    public string ConfirmButtonText
    {
        get;
        private init
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the icon to display.
    /// </summary>
    public ChildWindowIcon Icon
    {
        get;
        private init
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }
}
