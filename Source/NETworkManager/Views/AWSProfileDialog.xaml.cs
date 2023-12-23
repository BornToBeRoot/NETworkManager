using System.Windows;

namespace NETworkManager.Views;

public partial class AWSProfileDialog
{
    public AWSProfileDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        TextBoxProfile.Focus();
    }
}