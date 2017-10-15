using System.Collections.ObjectModel;
using NETworkManager.Controls;
using System;
using NETworkManager.Views.Applications;
using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace NETworkManager.ViewModels.Applications
{
    public class RemoteDesktopViewModel : ViewModelBase
    {
        private IDialogCoordinator dialogCoordinator;
        public IInterTabClient InterTabClient { get; set; }
        public ObservableCollection<DragablzTabContent> TabContents = new ObservableCollection<DragablzTabContent>();

        public RemoteDesktopViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;
            InterTabClient = new DragablzMainInterTabClient();
        }

        public static Func<object> NewItemFactory
        {
            get {
                return () => new DragablzTabContent("Traceroute", new TracerouteView());
            }
        }
    }
}