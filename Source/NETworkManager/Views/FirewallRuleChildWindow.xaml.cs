using System;
using System.Windows;
using System.Windows.Threading;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class FirewallRuleChildWindow
{
    public FirewallRuleChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxName.Focus();
        }));
    }

    private void TextBoxProgram_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);

        if (files != null && DataContext is FirewallRuleViewModel viewModel)
            viewModel.Program = files[0];
    }

    private void TextBoxProgram_PreviewDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }
}