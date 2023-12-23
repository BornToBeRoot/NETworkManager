using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Views;

public partial class ProfileDialog
{
    // Set name as hostname (if empty or identical)
    private string _oldName = string.Empty;

    public ProfileDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        TextBoxName.Focus();
    }

    private void TextBoxName_OnGotFocus(object sender, RoutedEventArgs e)
    {
        _oldName = TextBoxName.Text;
    }

    private void TextBoxName_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_oldName == TextBoxHost.Text)
            TextBoxHost.Text = TextBoxName.Text;

        _oldName = TextBoxName.Text;
    }

    private void ScrollViewer_ManipulationBoundaryFeedback(object sender,
        ManipulationBoundaryFeedbackEventArgs e)
    {
        e.Handled = true;
    }
}