using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views.Applications;
using System.Windows;

namespace NETworkManager.ViewModels.Applications
{
    public class PuTTYViewModel : ViewModelBase
    {
        #region Variables
        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzPuTTYTabItem> TabItems { get; private set; }

        private int _tabId = 0;

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
        public PuTTYViewModel()
        {
            InterTabClient = new DragablzPuTTYInterTabClient();
            TabItems = new ObservableCollection<DragablzPuTTYTabItem>();
        }
        #endregion

        #region ICommand & Actions
        public ICommand ConnectNewSessionCommand
        {
            get { return new RelayCommand(p => ConnectNewSessionAction()); }
        }

        private void ConnectNewSessionAction()
        {
            ConnectSession();
        }
        #endregion         
        
        private void ConnectSession()
        {
            TabItems.Add(new DragablzPuTTYTabItem("Test header", new PuTTYControl()));
            SelectedTabIndex = TabItems.Count - 1;
        }
    }
}