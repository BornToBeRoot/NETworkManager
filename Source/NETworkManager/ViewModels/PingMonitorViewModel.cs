using NETworkManager.Models.Settings;
using System.Net;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using NETworkManager.Models.Network;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Utilities;
using System.Threading.Tasks;
using System.Linq;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using NETworkManager.Views;
using DnsClient;
using System.Net.Sockets;
using System.Diagnostics;

namespace NETworkManager.ViewModels
{
    public class PingMonitorViewModel : ViewModelBase, IProfileManager
    {
        #region  Variables 
        private readonly IDialogCoordinator _dialogCoordinator;
        private LookupClient DnsLookupClient = new LookupClient();

        private readonly bool _isLoading;

        private int _hostId;

        private string _host;
        public string Host
        {
            get => _host;
            set
            {
                if (value == _host)
                    return;

                _host = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView HostHistoryView { get; }

        private ObservableCollection<PingMonitorClientView> _hosts = new ObservableCollection<PingMonitorClientView>();
        public ObservableCollection<PingMonitorClientView> Hosts
        {
            get => _hosts;
            set
            {
                if (value != null && value == _hosts)
                    return;

                _hosts = value;
            }
        }

        public ICollectionView HostsView { get; }


        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get => _displayStatusMessage;
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        #region Profiles
        public ICollectionView Profiles { get; }

        private ProfileInfo _selectedProfile;
        public ProfileInfo SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (value == _selectedProfile)
                    return;

                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        private string _search;
        public string Search
        {
            get => _search;
            set
            {
                if (value == _search)
                    return;

                _search = value;

                RefreshProfiles();

                OnPropertyChanged();
            }
        }

        private bool _canProfileWidthChange = true;
        private double _tempProfileWidth;

        private bool _expandProfileView;
        public bool ExpandProfileView
        {
            get => _expandProfileView;
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_ExpandProfileView = value;

                _expandProfileView = value;

                if (_canProfileWidthChange)
                    ResizeProfile(false);

                OnPropertyChanged();
            }
        }

        private GridLength _profileWidth;
        public GridLength ProfileWidth
        {
            get => _profileWidth;
            set
            {
                if (value == _profileWidth)
                    return;

                if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix) // Do not save the size when collapsed
                    SettingsManager.Current.Ping_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public PingMonitorViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            // Host history
            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PingMonitor_HostHistory);

            // Hosts
            HostsView = CollectionViewSource.GetDefaultView(Hosts);

            // Profiles
            Profiles = new CollectionViewSource { Source = ProfileManager.Profiles }.View;
            Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            Profiles.Filter = o =>
            {
                if (!(o is ProfileInfo info))
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return info.PingMonitor_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.PingMonitor_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                // Search by: Name, Ping_Host
                return info.PingMonitor_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.Ping_Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.PingMonitor_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();
            
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.PingMonitor_ExpandProfileView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.PingMonitor_ProfileWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

            _tempProfileWidth = SettingsManager.Current.PingMonitor_ProfileWidth;
        }
        #endregion

        #region ICommands & Actions
        public ICommand AddHostCommand => new RelayCommand(p => AddHostAction());

        private void AddHostAction()
        {
            AddHost(Host);

            // Add the hostname or ip address to the history
            AddHostToHistory(Host);
        }

        public ICommand AddProfileCommand => new RelayCommand(p => AddProfileAction());

        private void AddProfileAction()
        {
            ProfileManager.ShowAddProfileDialog(this, _dialogCoordinator);
        }

        public ICommand EditProfileCommand => new RelayCommand(p => EditProfileAction());

        private void EditProfileAction()
        {
            ProfileManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand CopyAsProfileCommand => new RelayCommand(p => CopyAsProfileAction());

        private void CopyAsProfileAction()
        {
            ProfileManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand DeleteProfileCommand => new RelayCommand(p => DeleteProfileAction());

        private void DeleteProfileAction()
        {
            ProfileManager.ShowDeleteProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

        private void EditGroupAction(object group)
        {
            ProfileManager.ShowEditGroupDialog(this, _dialogCoordinator, group.ToString());
        }

        public ICommand ClearSearchCommand => new RelayCommand(p => ClearSearchAction());

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }
        #endregion

        #region Methods

        private async void AddHost(string host)
        {
            _hostId++;

            // Try to parse the string into an IP-Address
            var hostIsIP = IPAddress.TryParse(Host, out var ipAddress);

            if (!hostIsIP) // Lookup
            {
                try
                {
                    // Try to resolve the hostname
                    var ipHostEntrys = await DnsLookupClient.GetHostEntryAsync(host);

                    if(ipHostEntrys.AddressList.Length == 0)
                    {
                        StatusMessage = string.Format(Resources.Localization.Strings.CouldNotResolveHostnameFor, Host);
                        DisplayStatusMessage = true;

                        return;
                    }

                    foreach (var ip in ipHostEntrys.AddressList)
                    {
                        switch (ip.AddressFamily)
                        {
                            // ToDo: Setting
                            case AddressFamily.InterNetwork when SettingsManager.Current.Ping_ResolveHostnamePreferIPv4:
                                ipAddress = ip;
                                break;
                            // ToDo: Setting
                            case AddressFamily.InterNetworkV6 when !SettingsManager.Current.Ping_ResolveHostnamePreferIPv4:
                                ipAddress = ip;
                                break;
                        }
                    }

                    // Fallback --> If we could not resolve our prefered ip protocol for the hostname
                    foreach (var ip in ipHostEntrys.AddressList)
                    {
                        ipAddress = ip;
                        break;
                    }
                }
                catch // This will catch DNS resolve errors
                {
                    StatusMessage = string.Format(Resources.Localization.Strings.CouldNotResolveHostnameFor, Host);
                    DisplayStatusMessage = true;

                    return;
                }
            }
            else // Reverse lookup
            {
                try
                {
                    var x = await DnsLookupClient.GetHostNameAsync(ipAddress);

                    if (!string.IsNullOrEmpty(x))
                        host = x;
                }
                catch
                {

                }
            }                        

            Debug.WriteLine("Add new host:");
            Debug.WriteLine(host);
            Debug.WriteLine(ipAddress);

            Hosts.Add(new PingMonitorClientView(_hostId, new PingMonitorOptions(host, ipAddress)));
        }

        private void AddHostToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.PingMonitor_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PingMonitor_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PingMonitor_HostHistory.Add(x));
        }

        private void ResizeProfile(bool dueToChangedSize)
        {
            _canProfileWidthChange = false;

            if (dueToChangedSize)
            {
                ExpandProfileView = Math.Abs(ProfileWidth.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix;
            }
            else
            {
                if (ExpandProfileView)
                {
                    ProfileWidth = Math.Abs(_tempProfileWidth - GlobalStaticConfiguration.Profile_WidthCollapsed) < GlobalStaticConfiguration.FloatPointFix ? new GridLength(GlobalStaticConfiguration.Profile_DefaultWidthExpanded) : new GridLength(_tempProfileWidth);
                }
                else
                {
                    _tempProfileWidth = ProfileWidth.Value;
                    ProfileWidth = new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);
                }
            }

            _canProfileWidthChange = true;
        }

        public void OnViewVisible()
        {
            RefreshProfiles();
        }

        public void OnViewHide()
        {

        }

        public void RefreshProfiles()
        {
            Profiles.Refresh();
        }

        public void OnProfileDialogOpen()
        {

        }

        public void OnProfileDialogClose()
        {

        }
        #endregion
    }
}
