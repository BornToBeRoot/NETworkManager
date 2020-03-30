using NETworkManager.ViewModels;

namespace NETworkManager
{ 
    // ReSharper disable once UnusedMember.Global, called from App.xaml.cs
    public partial class CommandLineWindow
    {
        private readonly CommandLineViewModel _viewModel = new CommandLineViewModel();

        public CommandLineWindow()
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