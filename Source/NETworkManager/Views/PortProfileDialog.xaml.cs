using System.Windows;

namespace NETworkManager.Views;

public partial class PortProfileDialog
{
    public PortProfileDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        TextBoxName.Focus();
    }
}