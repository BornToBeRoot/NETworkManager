using System.Windows;

namespace NETworkManager.Views;

public partial class SNMPOIDProfileDialog
{
    public SNMPOIDProfileDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        TextBoxName.Focus();
    }
}