namespace NETworkManager.Views;

public partial class CustomCommandDialog
{
    public CustomCommandDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBoxName.Focus();
    }
}
