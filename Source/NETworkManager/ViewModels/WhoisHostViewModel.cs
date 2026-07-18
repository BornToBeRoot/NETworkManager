using Dragablz;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Profiles;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class WhoisHostViewModel : ProfileHostViewModelBase
{
    #region Variables

    public IInterTabClient InterTabClient { get; }

    public string InterTabPartition
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DragablzTabItem> TabItems { get; }

    public int SelectedTabIndex
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor

    public WhoisHostViewModel()
    {
        InterTabClient = new DragablzInterTabClient(ApplicationName.Whois);
        InterTabPartition = nameof(ApplicationName.Whois);

        var tabId = Guid.NewGuid();

        TabItems =
        [
            new DragablzTabItem(Strings.NewTab, new WhoisView(tabId), tabId)
        ];

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.Whois;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.Whois_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.Whois_Domain;

    #endregion

    #region ICommand & Actions

    public ICommand AddTabCommand => new RelayCommand(_ => AddTabAction());

    private void AddTabAction()
    {
        AddTab();
    }

    public ICommand QueryProfileCommand => new RelayCommand(_ => QueryProfileAction(), QueryProfile_CanExecute);

    private bool QueryProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void QueryProfileAction()
    {
        AddTab(SelectedProfile.Whois_Domain);
    }

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private static void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as IDragablzTabItem)?.CloseTab();
    }

    #endregion

    #region Methods

    private void AddTab(string domain = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(domain ?? Strings.NewTab, new WhoisView(tabId, domain),
            tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    #endregion
}
