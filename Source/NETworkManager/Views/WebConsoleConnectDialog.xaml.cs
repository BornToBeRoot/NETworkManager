using System.Windows;

namespace NETworkManager.Views;

public partial class WebConsoleConnectDialog
{
    public WebConsoleConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ComboBoxUrl.Focus();
    }
}