using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels
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
                new DragablzPingTabItem(LocalizationManager.GetStringByKey("String_Header_Ping"), new PingView(_tabId), _tabId)
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

        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzPingTabItem).View as PingView).CloseTab();
        }
        #endregion

        #region Methods
        private void AddPingTab()
        {
            _tabId++;

            TabItems.Add(new DragablzPingTabItem(LocalizationManager.GetStringByKey("String_Header_Ping"), new PingView(_tabId), _tabId));
            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}