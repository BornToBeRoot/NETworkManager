using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class WikiOUILookupView : UserControl
    {
        WikiOUILookupViewModel viewModel = new WikiOUILookupViewModel();

        public WikiOUILookupView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
