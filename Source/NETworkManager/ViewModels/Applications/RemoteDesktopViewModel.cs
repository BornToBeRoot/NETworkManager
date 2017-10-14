using System.Collections.ObjectModel;
using NETworkManager.Controls;
using System;
using NETworkManager.Views.Applications;
using Dragablz;

namespace NETworkManager.ViewModels.Applications
{
    public class RemoteDesktopViewModel : ViewModelBase
    {
        public IInterTabClient InterTabClient { get; set; }
        public ObservableCollection<DragablzTabContent> TabContents = new ObservableCollection<DragablzTabContent>();

        public RemoteDesktopViewModel()
        {
            InterTabClient = new DragablzMainInterTabClient();   
        }

        public static Func<object> NewItemFactory
        {
            get { return () => new DragablzTabContent("Traceroute", new TracerouteView()); }
        }
    }
}