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

#pragma warning disable CS0414 // Field is assigned but its value is never used
    private readonly bool _isLoading;
    private bool _isViewActive = true;
#pragma warning restore CS0414 // Field is assigned but its value is never used

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
    public SNTPLookupHostViewModel()
    {
        _isLoading = true;
        
        InterTabClient = new DragablzInterTabClient(ApplicationName.SNTPLookup);

        TabItems = new ObservableCollection<DragablzTabItem>
        {
            new(Localization.Resources.Strings.NewTab, new SNTPLookupView (_tabId), _tabId)
        };
        
        LoadSettings();

        _isLoading = false;
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
        _tabId++;

        TabItems.Add(new DragablzTabItem(Localization.Resources.Strings.NewTab, new SNTPLookupView(_tabId), _tabId));

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
    #endregion
}