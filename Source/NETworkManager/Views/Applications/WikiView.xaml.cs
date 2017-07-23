using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class WikiView : UserControl
    {
        WikiViewModel viewModel = new WikiViewModel(DialogCoordinator.Instance);

        public WikiView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
