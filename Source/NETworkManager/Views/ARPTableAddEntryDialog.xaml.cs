using System.Windows;

namespace NETworkManager.Views;

public partial class ARPTableAddEntryDialog
{
    public ARPTableAddEntryDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        TextBoxIPAddress.Focus();
    }
}