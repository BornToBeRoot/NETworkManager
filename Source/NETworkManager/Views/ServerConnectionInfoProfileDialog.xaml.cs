using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views;

public partial class ServerConnectionInfoProfileDialog
{
    public ServerConnectionInfoProfileDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBoxName.Focus();
    }

    private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = (ServerConnectionInfoProfileViewModel)DataContext;
    }
}
