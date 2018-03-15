using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views.Applications;
using System.Windows;

namespace NETworkManager.ViewModels.Applications
{
    public class PingHostViewModel : ViewModelBase
    {
        #region Variables
        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzPingTabItem> TabItems { get; private set; }

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
        public PingHostViewModel()
        {
            InterTabClient = new DragablzPingInterTabClient();

            TabItems = new ObservableCollection<DragablzPingTabItem>()
            {
                new DragablzPingTabItem(Application.Current.Resources["String_Header_Ping"] as string, new PingView(_tabId), _tabId)
            };
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddPingTabCommand
        {
            get { return new RelayCommand(p => AddPingTabAction()); }
        }

        private void AddPingTabAction()
        {
            AddPingTab();
        }
        #endregion

        #region Methods
        private void AddPingTab()
        {
            _tabId++;

            TabItems.Add(new DragablzPingTabItem(Application.Current.Resources["String_Header_Ping"] as string, new PingView(_tabId), _tabId));
            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}