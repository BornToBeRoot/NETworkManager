using System.Windows;

namespace NETworkManager.Views;

public partial class RemoteDesktopConnectDialog
{
    public RemoteDesktopConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ComboBoxHost.Focus();
    }
}