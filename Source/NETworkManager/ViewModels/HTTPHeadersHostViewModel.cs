using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels
{
    public class HTTPHeadersHostViewModel : ViewModelBase
    {
        #region Variables
        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzHTTPHeadersTabItem> TabItems { get; private set; }

        private const string tagIdentifier = "tag=";

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
        public HTTPHeadersHostViewModel()
        {
            InterTabClient = new DragablzHTTPHeadersInterTabClient();

            TabItems = new ObservableCollection<DragablzHTTPHeadersTabItem>()
            {
                new DragablzHTTPHeadersTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new HTTPHeadersView (_tabId), _tabId)
            };
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddHTTPHeadersTabCommand
        {
            get { return new RelayCommand(p => AddHTTPHeadersTabAction()); }
        }

        private void AddHTTPHeadersTabAction()
        {
            AddHTTPHeadersTab();
        }

        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzHTTPHeadersTabItem).View as HTTPHeadersView).CloseTab();
        }
        #endregion

        #region Methods
        private void AddHTTPHeadersTab()
        {
            _tabId++;

            TabItems.Add(new DragablzHTTPHeadersTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new HTTPHeadersView(_tabId), _tabId));

            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}