namespace NETworkManager.Views;

public partial class GroupDialog
{
    public GroupDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBoxName.Focus();
    }
    
    private void ScrollViewer_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
    {
        e.Handled = true;
    }
}