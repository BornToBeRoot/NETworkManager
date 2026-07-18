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

public class TracerouteHostViewModel : ProfileHostViewModelBase
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

    public TracerouteHostViewModel()
    {
        InterTabClient = new DragablzInterTabClient(ApplicationName.Traceroute);
        InterTabPartition = nameof(ApplicationName.Traceroute);

        var tabId = Guid.NewGuid();

        TabItems =
        [
            new DragablzTabItem(Strings.NewTab, new TracerouteView(tabId), tabId)
        ];

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.Traceroute;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.Traceroute_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.Traceroute_Host;

    #endregion

    #region ICommand & Actions

    public ICommand AddTabCommand => new RelayCommand(_ => AddTabAction());

    private void AddTabAction()
    {
        AddTab();
    }

    public ICommand TraceProfileCommand => new RelayCommand(_ => TraceProfileAction(), TraceProfile_CanExecute);

    private bool TraceProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void TraceProfileAction()
    {
        AddTab(SelectedProfile);
    }

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as IDragablzTabItem)?.CloseTab();
    }

    #endregion

    #region Methods

    public void AddTab(string host = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(host ?? Strings.NewTab, new TracerouteView(tabId, host),
            tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    public void AddTab(ProfileInfo profile)
    {
        AddTab(profile.Traceroute_Host);
    }

    #endregion
}
