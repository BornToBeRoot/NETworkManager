using Dragablz;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Profiles.Application;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class SNMPHostViewModel : ProfileHostViewModelBase
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

    public SNMPHostViewModel()
    {
        InterTabClient = new DragablzInterTabClient(ApplicationName.SNMP);
        InterTabPartition = nameof(ApplicationName.SNMP);

        TabItems = [];
        AddTab();

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.SNMP;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.SNMP_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.SNMP_Host;

    #endregion

    #region ICommand & Actions

    public ICommand AddTabCommand => new RelayCommand(_ => AddTabAction());

    private void AddTabAction()
    {
        AddTab();
    }

    public ICommand AddTabProfileCommand => new RelayCommand(_ => AddTabProfileAction(), AddTabProfile_CanExecute);

    private bool AddTabProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void AddTabProfileAction()
    {
        AddTab(SelectedProfile);
    }

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private static void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as IDragablzTabItem)?.CloseTab();
    }

    #endregion

    #region Methods

    private void AddTab(SNMPSessionInfo sessionInfo, string header = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(header ?? sessionInfo.Host ?? Strings.NewTab,
            new SNMPView(tabId, sessionInfo), tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    private void AddTab()
    {
        var sessionInfo = SNMP.CreateSessionInfo();

        AddTab(sessionInfo);
    }

    public void AddTab(string host)
    {
        var sessionInfo = SNMP.CreateSessionInfo();

        sessionInfo.Host = host;

        AddTab(sessionInfo);
    }

    private void AddTab(ProfileInfo profile)
    {
        var sessionInfo = SNMP.CreateSessionInfo(profile);

        AddTab(sessionInfo, profile.Name);
    }

    #endregion
}
