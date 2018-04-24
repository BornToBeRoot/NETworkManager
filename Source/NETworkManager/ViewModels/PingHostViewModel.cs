using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Models.Settings;
using System.ComponentModel;
using System;
using System.Windows.Data;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class PingHostViewModel : ViewModelBase
    {
        #region Variables
        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzPingTabItem> TabItems { get; private set; }

        private const string tagIdentifier = "tag=";
        
        private bool _isLoading = true;
        
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

        #region Sessions
        ICollectionView _pingProfiles;
        public ICollectionView PingProfiles
        {
            get { return _pingProfiles; }
        }

        private RemoteDesktopSessionInfo _selectedProfile = new RemoteDesktopSessionInfo();
        public RemoteDesktopSessionInfo SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                if (value == _selectedProfile)
                    return;

                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        private bool _expandProfileView;
        public bool ExpandProfileView
        {
            get { return _expandProfileView; }
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_ExpandProfileView = value;

                _expandProfileView = value;
                OnPropertyChanged();
            }
        }

        private string _search;
        public string Search
        {
            get { return _search; }
            set
            {
                if (value == _search)
                    return;

                _search = value;

                PingProfiles.Refresh();

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor
        public PingHostViewModel()
        {
            InterTabClient = new DragablzPingInterTabClient();

            TabItems = new ObservableCollection<DragablzPingTabItem>()
            {
                new DragablzPingTabItem(LocalizationManager.GetStringByKey("String_Header_Ping"), new PingView(_tabId), _tabId)
            };

            // Load profiles
            if (PingProfileManager.Profiles == null)
                PingProfileManager.Load();

            _pingProfiles = CollectionViewSource.GetDefaultView(PingProfileManager.Profiles);
            _pingProfiles.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            _pingProfiles.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
            _pingProfiles.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            _pingProfiles.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                PingProfileInfo info = o as PingProfileInfo;

                string search = Search.Trim();

                // Search by: Tag
                if (search.StartsWith(tagIdentifier, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(info.Tags))
                        return false;
                    else
                        return info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(tagIdentifier.Length, search.Length - tagIdentifier.Length).IndexOf(str, StringComparison.OrdinalIgnoreCase) > -1);
                }
                else // Search by: Name, Hostname
                {
                    return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
                }
            };

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.Ping_ExpandProfileView;
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddPingTabCommand
        {
            get { return new RelayCommand(p => AddPingTabAction()); }
        }

        private void AddPingTabAction()
        {
            AddPingTab();
        }

        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzPingTabItem).View as PingView).CloseTab();
        }
        #endregion

        #region Methods
        private void AddPingTab()
        {
            _tabId++;

            TabItems.Add(new DragablzPingTabItem(LocalizationManager.GetStringByKey("String_Header_Ping"), new PingView(_tabId), _tabId));
            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}