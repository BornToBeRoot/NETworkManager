namespace NETworkManager.Views
{
    public partial class GroupDialog
    {
        public GroupDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Need to be in loaded event, focusmanger won't work...
            TextBoxName.Focus();
        }

        // Set name as hostname (if empty or identical)
        private string _oldName = string.Empty;

       

        private void TextBoxName_OnGotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            //_oldName = TextBoxName.Text;
        }

        private void TextBoxName_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            /*
            if (_oldName == TextBoxHost.Text)
                TextBoxHost.Text = TextBoxName.Text;

            _oldName = TextBoxName.Text;
            */
        } 
        
        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}