using NETworkManager.ViewModels;

namespace NETworkManager.Views
{ 
    // ReSharper disable once UnusedMember.Global, called from App.xaml.cs
    public partial class CommandLineHelpWindow
    {
        private readonly CommandLineHelpViewModel _viewModel = new CommandLineHelpViewModel();

        public CommandLineHelpWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}