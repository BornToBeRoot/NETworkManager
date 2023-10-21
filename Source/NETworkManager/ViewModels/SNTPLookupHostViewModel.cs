using System;
using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Models;

namespace NETworkManager.ViewModels;

public class SNTPLookupHostViewModel : ViewModelBase
{
    #region Variables
 public IInterTabClient InterTabClient { get; }
    public ObservableCollection<DragablzTabItem> TabItems { get; }

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
    public SNTPLookupHostViewModel()
    {
        InterTabClient = new DragablzInterTabClient(ApplicationName.SNTPLookup);

        var tabId = Guid.NewGuid();
        
        TabItems = new ObservableCollection<DragablzTabItem>
        {
            new(Localization.Resources.Strings.NewTab, new SNTPLookupView (tabId), tabId)
        };
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        
    }
    #endregion

    #region ICommand & Actions
    public ICommand AddTabCommand => new RelayCommand(_ => AddTabAction());

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

    private void AddTab()
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(Localization.Resources.Strings.NewTab, new SNTPLookupView(tabId), tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    public void OnViewVisible()
    {
        
    }

    public void OnViewHide()
    {
        
    }
    #endregion
}