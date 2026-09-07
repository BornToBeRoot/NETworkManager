using NETworkManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class ServerConnectionInfoProfileChildWindow
{
    public ServerConnectionInfoProfileChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var isNameReadOnly = (DataContext as ServerConnectionInfoProfileViewModel)?.IsNameReadOnly ?? false;

        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            // Focus() alone doesn't move the caret - place it after any existing text (e.g. when
            // editing an existing profile) instead of leaving it at the start.
            if (isNameReadOnly)
            {
                TextBoxServer.Focus();
                TextBoxServer.CaretIndex = TextBoxServer.Text.Length;
            }
            else
            {
                TextBoxName.Focus();
                TextBoxName.CaretIndex = TextBoxName.Text.Length;
            }
        }));
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = (ServerConnectionInfoProfileViewModel)DataContext;
    }

    /// <summary>
    ///     Pressing enter in the server or port textbox adds the server (like clicking the Add button)
    ///     instead of triggering the window's default button (Save), as long as a server is entered.
    ///     If the server field is empty, falls through to the default behavior (Save, if enabled) instead.
    ///     Note: once a server is entered, enter is always intercepted here - even if currently invalid -
    ///     so it can never fall through to Save, which is enabled independently of the server/port
    ///     textboxes' validity and would otherwise silently discard an in-progress, invalid entry.
    /// </summary>
    private void TextBoxServerOrPort_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        if (DataContext is not ServerConnectionInfoProfileViewModel viewModel)
            return;

        if (string.IsNullOrEmpty(viewModel.Server))
            return;

        e.Handled = true;

        if (Validation.GetHasError(TextBoxServer) || Validation.GetHasError(TextBoxPort))
            return;

        viewModel.AddServerCommand.Execute(null);
    }
}