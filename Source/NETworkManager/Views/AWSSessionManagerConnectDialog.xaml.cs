using System.Windows;

namespace NETworkManager.Views;

public partial class AWSSessionManagerConnectDialog
{
    public AWSSessionManagerConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ComboBoxInstanceID.Focus();
    }
}