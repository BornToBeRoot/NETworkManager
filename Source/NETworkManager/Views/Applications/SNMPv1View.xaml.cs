using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class SNMPv1View : UserControl
    {
        SNMPv1ViewModel viewModel = new SNMPv1ViewModel();

        public SNMPv1View()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
