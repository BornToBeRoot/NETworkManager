using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Models.Settings;
using System;

namespace NETworkManager.ViewModels
{
    public class DNSLookupHostViewModel : ViewModelBase
    {
        #region Variables
        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzDNSLookupTabItem> TabItems { get; private set; }

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
        public DNSLookupHostViewModel()
        {
            InterTabClient = new DragablzDNSLookupInterTabClient();

            TabItems = new ObservableCollection<DragablzDNSLookupTabItem>()
            {
                new DragablzDNSLookupTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new DNSLookupView (_tabId), _tabId)
            };
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddDNSLookupTabCommand
        {
            get { return new RelayCommand(p => AddDNSLookupTabAction()); }
        }

        private void AddDNSLookupTabAction()
        {
            AddDNSLookupTab();
        }

        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzDNSLookupTabItem).View as DNSLookupView).CloseTab();
        }
        #endregion

        #region Methods
        private void AddDNSLookupTab()
        {
            _tabId++;

            TabItems.Add(new DragablzDNSLookupTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new DNSLookupView(_tabId), _tabId));

            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}