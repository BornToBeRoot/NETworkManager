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
        public HTTPHeadersHostViewModel()
        {
            InterTabClient = new DragablzInterTabClient(ApplicationViewManager.Name.HTTPHeaders);

            TabItems = new ObservableCollection<DragablzTabItem>
            {
                new DragablzTabItem(Resources.Localization.Strings.NewTab, new HTTPHeadersView (_tabId), _tabId)
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
            ((args.DragablzItem.Content as DragablzTabItem)?.View as HTTPHeadersView)?.CloseTab();
        }
        #endregion

        #region Methods
        private void AddTab()
        {
            _tabId++;

            TabItems.Add(new DragablzTabItem(Resources.Localization.Strings.NewTab, new HTTPHeadersView(_tabId), _tabId));

            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}