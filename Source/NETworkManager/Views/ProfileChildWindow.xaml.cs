using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using NETworkManager.Localization.Resources;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class ProfileChildWindow
{
    // Set name as hostname (if empty or identical)
    private string _oldName = string.Empty;

    public ProfileChildWindow(Window parentWindow)
    {
        InitializeComponent();

        // Set the width and height of the child window based on the parent window size
        ChildWindowWidth = parentWindow.ActualWidth * 0.85;
        ChildWindowHeight = parentWindow.ActualHeight * 0.85;

        // Update the size of the child window when the parent window is resized
        parentWindow.SizeChanged += (_, _) =>
            {
                ChildWindowWidth = parentWindow.ActualWidth * 0.85;
                ChildWindowHeight = parentWindow.ActualHeight * 0.85;
            };
    }

    private void Firewall_ViewModelOnCommandExecuted(object sender, RoutedEventArgs args)
    {
        FirewallRuleGrid?.RestoreRuleGridFocus();
    }

    /// <summary>
    /// - Set the focus to Name on the General Tab.
    /// - Subscribe to ViewModel events as necessary.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxName.Focus();
            (DataContext as ProfileViewModel)
                ?.Firewall_ViewModel?.CommandExecuted += Firewall_ViewModelOnCommandExecuted;
        }));
    }

    private void TextBoxName_OnGotFocus(object sender, RoutedEventArgs e)
    {
        _oldName = TextBoxName.Text;
    }

    private void TextBoxName_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_oldName == TextBoxHost.Text)
            TextBoxHost.Text = TextBoxName.Text;

        _oldName = TextBoxName.Text;
    }

    private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    /// Handle application views when their tab is selected.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || !IsVisible)
            return;

        if (sender is not TabControl tabControl)
            return;
        if (tabControl.SelectedItem is not TabItem item)
            return;
        if (item.Header.ToString() == Strings.Firewall)
        {
            FirewallRuleGrid?.RestoreRuleGridFocus();
        }
        // else if (other TabItems ...)
    }

    private void ProfileChildWindow_OnUnloaded(object sender, RoutedEventArgs e)
    {
        (DataContext as ProfileViewModel)
            ?.Firewall_ViewModel?.CommandExecuted -= Firewall_ViewModelOnCommandExecuted;
    }
}