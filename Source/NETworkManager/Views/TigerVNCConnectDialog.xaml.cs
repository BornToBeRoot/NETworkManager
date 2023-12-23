using System.Windows;

namespace NETworkManager.Views;

public partial class TigerVNCConnectDialog
{
    public TigerVNCConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ComboBoxHost.Focus();
    }
}