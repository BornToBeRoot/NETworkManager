using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class WikiPortLookupView : UserControl
    {
        WikiPortLookupViewModel viewModel = new WikiPortLookupViewModel();

        public WikiPortLookupView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}