using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class DNSLookupHostViewModel : ViewModelBase
    {
        #region Variables
        public IInterTabClient InterTabClient { get; }
        public ObservableCollection<DragablzTabItem> TabItems { get; }

        private int _tabId;

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
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
        public DNSLookupHostViewModel()
        {
            InterTabClient = new DragablzInterTabClient(ApplicationViewManager.Name.DNSLookup);

            TabItems = new ObservableCollection<DragablzTabItem>
            {
                new DragablzTabItem(Resources.Localization.Strings.NewTab, new DNSLookupView (_tabId), _tabId)
            };
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddTabCommand
        {
            get { return new RelayCommand(p => AddTabAction()); }
        }

        private void AddTabAction()
        {
            AddTab();
        }

        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private static void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem)?.View as DNSLookupView)?.CloseTab();
        }
        #endregion

        #region Methods
        public void AddTab(string host = null)
        {
            _tabId++;

            TabItems.Add(new DragablzTabItem(host ?? Resources.Localization.Strings.NewTab, new DNSLookupView(_tabId, host), _tabId));
            
            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}