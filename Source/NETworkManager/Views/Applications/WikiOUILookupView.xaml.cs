using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class WikiOUILookupView : UserControl
    {
        WikiOUILookupViewModel viewModel = new WikiOUILookupViewModel(DialogCoordinator.Instance);

        public WikiOUILookupView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
