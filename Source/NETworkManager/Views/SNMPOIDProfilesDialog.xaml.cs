using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SNMPOIDProfilesDialog
{
    public SNMPOIDProfilesDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Need to be in loaded event, focusmanger won't work...
        TextBoxSearch.Focus();
    }

    private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var x = (SNMPOIDProfilesViewModel)DataContext;

        if (x.OKCommand.CanExecute(null))
            x.OKCommand.Execute(null);
    }
}
