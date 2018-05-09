using NETworkManager.ViewModels;
using System;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class DNSLookupHostView : UserControl
    {
        DNSLookupHostViewModel viewModel = new DNSLookupHostViewModel();

        public DNSLookupHostView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public void AddTab(string host)
        {
            viewModel.AddTab(host);
        }
    }
}
