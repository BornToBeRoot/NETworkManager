using MahApps.Metro.Controls;
using NETworkManager.ViewModels.Help;

namespace NETworkManager.Views.Help
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class HelpCommandLineWindow : MetroWindow
    {        
        HelpCommandLineViewModel viewModel = new HelpCommandLineViewModel();

        public HelpCommandLineWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}