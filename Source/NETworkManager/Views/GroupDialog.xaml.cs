using System.Windows;
using System.Windows.Input;

namespace NETworkManager.Views;

public partial class GroupDialog
{
    public GroupDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        TextBoxName.Focus();
    }

    private void ScrollViewer_ManipulationBoundaryFeedback(object sender,
        ManipulationBoundaryFeedbackEventArgs e)
    {
        e.Handled = true;
    }
}