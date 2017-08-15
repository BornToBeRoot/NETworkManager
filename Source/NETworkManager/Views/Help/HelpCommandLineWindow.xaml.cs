using MahApps.Metro.Controls;

namespace NETworkManager.Views.Help
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class HelpCommandLineWindow : MetroWindow
    {
        public HelpCommandLineWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}