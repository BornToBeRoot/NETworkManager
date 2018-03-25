using MahApps.Metro.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class HelpCommandLineWindow : MetroWindow
    {        
        CommandLineHelpViewModel viewModel = new CommandLineHelpViewModel();

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