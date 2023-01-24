using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Profiles;
using System.Windows.Threading;
using NETworkManager.Models;
using System.Collections.Generic;

namespace NETworkManager.ViewModels
{
    public class SNTPLookupHostViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;        

        public IInterTabClient InterTabClient { get; }
        public ObservableCollection<DragablzTabItem> TabItems { get; }

        private readonly bool _isLoading = true;
        private bool _isViewActive = true;

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

        #region Constructor, load settings
        public SNTPLookupHostViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;

            InterTabClient = new DragablzInterTabClient(ApplicationName.SNTPLookup);

            TabItems = new ObservableCollection<DragablzTabItem>
            {
                new DragablzTabItem(Localization.Resources.Strings.NewTab, new SNTPLookupView (_tabId), _tabId)
            };
            
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddTabCommand => new RelayCommand(p => AddTabAction());

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

            TabItems.Add(new DragablzTabItem(host ?? Localization.Resources.Strings.NewTab, new SNTPLookupView(_tabId, host), _tabId));

            SelectedTabIndex = TabItems.Count - 1;
        }

        public void OnViewVisible()
        {
            _isViewActive = true;
        }

        public void OnViewHide()
        {
            _isViewActive = false;
        }

        public void RefreshProfiles()
        {
            if (!_isViewActive)
                return;
        }

        public void OnProfileDialogOpen()
        {

        }

        public void OnProfileDialogClose()
        {

        }
        #endregion

        #region Event
        
        #endregion
    }
}