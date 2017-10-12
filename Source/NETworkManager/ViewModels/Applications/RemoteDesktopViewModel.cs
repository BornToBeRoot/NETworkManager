using System.Collections.ObjectModel;
using NETworkManager.Controls;
using System;
using NETworkManager.Views.Applications;

namespace NETworkManager.ViewModels.Applications
{
    public class RemoteDesktopViewModel : ViewModelBase
    {
        public ObservableCollection<DragablzTabContent> TabContents = new ObservableCollection<DragablzTabContent>();

        public RemoteDesktopViewModel()
        {
            
        }

        public static Func<object> NewItemFactory
        {
            get { return () => new DragablzTabContent("test", new TracerouteView()); }
        }
    }
}