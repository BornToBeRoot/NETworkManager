namespace NETworkManager.Views;

public partial class ProfileFileDialog
{
    public ProfileFileDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBoxName.Focus();
    }
}
