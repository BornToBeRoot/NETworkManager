namespace NETworkManager.Views;

public partial class PortProfileDialog
{
    public PortProfileDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBoxName.Focus();
    }
}
