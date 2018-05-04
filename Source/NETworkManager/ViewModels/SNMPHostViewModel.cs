using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels
{
    public class SNMPHostViewModel : ViewModelBase
    {
        #region Variables
        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzTabItem> TabItems { get; private set; }

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
        public SNMPHostViewModel()
        {
            InterTabClient = new DragablzSNMPInterTabClient();

            TabItems = new ObservableCollection<DragablzTabItem>()
            {
                new DragablzTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new SNMPView (_tabId), _tabId)
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

        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem).View as SNMPView).CloseTab();
        }
        #endregion

        #region Methods
        private void AddTab()
        {
            _tabId++;

            TabItems.Add(new DragablzTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new SNMPView(_tabId), _tabId));

            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}