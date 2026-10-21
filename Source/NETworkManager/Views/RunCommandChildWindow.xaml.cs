using NETworkManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.Views;

/// <summary>
///     Run command dialog. The input keeps the focus; Up/Down select a suggestion, Tab completes it, Enter runs it and
///     Escape closes the dialog.
/// </summary>
public partial class RunCommandChildWindow
{
    public RunCommandChildWindow()
    {
        InitializeComponent();
    }

    private RunCommandViewModel ViewModel => (RunCommandViewModel)DataContext;

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate { TextBoxInput.Focus(); }));
    }

    private void TextBoxInput_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                ViewModel.SelectNext();
                break;
            case Key.Up:
                ViewModel.SelectPrevious();
                break;
            case Key.Tab:
                ViewModel.Complete();
                TextBoxInput.CaretIndex = TextBoxInput.Text.Length;
                break;
            case Key.Enter:
                ViewModel.Run();
                break;
            case Key.Escape:
                ViewModel.CancelCommand.Execute(null);
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void ListBoxSuggestions_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (ItemsControl.ContainerFromElement(ListBoxSuggestions, (DependencyObject)e.OriginalSource) is not
            ListBoxItem item)
            return;

        ViewModel.SelectedSuggestion = (RunCommandSuggestion)item.DataContext;
        ViewModel.Run();

        TextBoxInput.Focus();
    }

    private void ListBoxSuggestions_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListBoxSuggestions.SelectedItem != null)
            ListBoxSuggestions.ScrollIntoView(ListBoxSuggestions.SelectedItem);
    }
}
