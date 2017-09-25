using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class WikiPortView : UserControl
    {
        WikiPortViewModel viewModel = new WikiPortViewModel(DialogCoordinator.Instance);

        public WikiPortView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
