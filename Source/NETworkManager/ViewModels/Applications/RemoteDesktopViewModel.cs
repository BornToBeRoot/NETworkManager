using System.Collections.ObjectModel;
using NETworkManager.Controls;
using NETworkManager.Views.Applications;
using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Applications
{
    public class RemoteDesktopViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        public IInterTabClient InterTabClient { get; private set; } = new DragablzMainInterTabClient();
        public ObservableCollection<DragablzTabContent> TabContents { get; private set; } = new ObservableCollection<DragablzTabContent>();

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (value == _selectedTabIndex)
                    return;

                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public RemoteDesktopViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;
        }
        #endregion

        #region ICommand & Actions
        public ICommand NewRDPSessionCommand
        {
            get { return new RelayCommand(p => NewRDPSessionAction()); }
        }

        private void NewRDPSessionAction()
        {
            TabContents.Add(new DragablzTabContent("Traceroute", new TracerouteView()));
            SelectedTabIndex = TabContents.Count - 1;
        }
        #endregion
    }
}