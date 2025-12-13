using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;
using NETworkManager.Views;
using System.Threading.Tasks;
using System.Windows;

namespace NETworkManager
{
    /// <summary>
    /// Helper class for showing dialog messages.
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Displays a modal message dialog with an OK button, allowing the user to acknowledge the message before
        /// continuing.
        /// </summary>
        /// <remarks>The dialog is shown as a child window of the specified parent. The method is
        /// asynchronous and returns when the dialog is dismissed by the user.</remarks>
        /// <param name="parentWindow">The parent window that will host the message dialog. Cannot be null.</param>
        /// <param name="title">The title text displayed in the message dialog window. Cannot be null.</param>
        /// <param name="message">The main message content shown in the dialog. Cannot be null.</param>
        /// <param name="icon">The icon to display in the dialog, indicating the message type. Defaults to Info if not specified.</param>
        /// <param name="confirmButtonText">The text to display on the confirm button. If null, a default value is used.</param>
        /// <returns>A task that completes when the user closes the message dialog.</returns>
        public static Task ShowMessageAsync(Window parentWindow, string title, string message, ChildWindowIcon icon = ChildWindowIcon.Info, string confirmButtonText = null)
        {            
            confirmButtonText ??= Strings.OK;

            var childWindow = new MessageChildWindow();

            var childWindowViewModel = new MessageViewModel(_ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;
            }, message, icon, confirmButtonText);

            childWindow.Title = title;

            childWindow.DataContext = childWindowViewModel;

            ConfigurationManager.Current.IsChildWindowOpen = true;

            return parentWindow.ShowChildWindowAsync(childWindow);
        }

        /// <summary>
        /// Displays an asynchronous modal dialog with OK and Cancel buttons, allowing the user to confirm or cancel an
        /// action.
        /// </summary>
        /// <remarks>The dialog is shown as a child window of the specified parent. The method does not
        /// return until the user closes the dialog. Custom button text and icon can be provided to tailor the dialog to
        /// specific scenarios.</remarks>
        /// <param name="parentWindow">The parent window that hosts the child dialog. Cannot be null.</param>
        /// <param name="title">The title text displayed in the dialog window.</param>
        /// <param name="message">The message content shown to the user in the dialog.</param>
        /// <param name="icon">The icon displayed in the dialog to indicate the message type. Defaults to Info.</param>
        /// <param name="confirmButtonText">The text label for the confirm button. If null, a default value is used.</param>
        /// <param name="cancelButtonText">The text label for the cancel button. If null, a default value is used.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user
        /// clicks OK; otherwise, <see langword="false"/>.</returns>
        public static async Task<bool> ShowConfirmationMessageAsync(Window parentWindow, string title, string message, ChildWindowIcon icon = ChildWindowIcon.Info, string confirmButtonText = null, string cancelButtonText = null)
        {
            confirmButtonText ??= Strings.OK;
            cancelButtonText ??= Strings.Cancel;

            var result = false;

            var childWindow = new MessageConfirmationChildWindow();

            var childWindowViewModel = new MessageConfirmationViewModel(_ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;

                result = true;
            },
            _ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;
            },
            message, icon, confirmButtonText, cancelButtonText);

            childWindow.Title = title;
            childWindow.DataContext = childWindowViewModel;

            ConfigurationManager.Current.IsChildWindowOpen = true;

            await parentWindow.ShowChildWindowAsync(childWindow);

            return result;
        }
    }
}
