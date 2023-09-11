using NETworkManager.Settings;
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
using NETworkManager.Profiles;
using System.Windows.Threading;
using System.Collections.Generic;
using NETworkManager.Models;
using Amazon.EC2.Model;

namespace NETworkManager.ViewModels;

public class WakeOnLANViewModel : ViewModelBase, IProfileManager
{
    #region  Variables 
    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly DispatcherTimer _searchDispatcherTimer = new();

    private readonly bool _isLoading;
    private bool _isViewActive = true;

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value == _isRunning)
                return;

            _isRunning = value;
            OnPropertyChanged();
        }
    }
    public ICollectionView MACAddressHistoryView { get; }

    private string _macAddress;
    public string MACAddress
    {
        get => _macAddress;
        set
        {
            if (value == _macAddress)
                return;

            _macAddress = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView BroadcastHistoryView { get; }

    private string _broadcast;
    public string Broadcast
    {
        get => _broadcast;
        set
        {
            if (value == _broadcast)
                return;

            _broadcast = value;
            OnPropertyChanged();
        }
    }

    private bool _isStatusMessageDisplayed;
    public bool IsStatusMessageDisplayed
    {
        get => _isStatusMessageDisplayed;
        set
        {
            if (value == _isStatusMessageDisplayed)
                return;

            _isStatusMessageDisplayed = value;
            OnPropertyChanged();
        }
    }

    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (value == _statusMessage)
                return;

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    #region Profiles

    private ICollectionView _profiles;
    public ICollectionView Profiles
    {
        get => _profiles;
        private set
        {
            if (value == _profiles)
                return;

            _profiles = value;
            OnPropertyChanged();
        }
    }

    private ProfileInfo _selectedProfile = new();
    public ProfileInfo SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (value == _selectedProfile)
                return;

            if (value != null && !IsRunning)
            {
                MACAddress = value.WakeOnLAN_MACAddress;
                Broadcast = value.WakeOnLAN_Broadcast;
            }

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

            // Start searching...
            IsSearching = true;
            _searchDispatcherTimer.Start();

            OnPropertyChanged();
        }
    }

    private bool _isSearching;
    public bool IsSearching
    {
        get => _isSearching;
        set
        {
            if (value == _isSearching)
                return;

            _isSearching = value;
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
                SettingsManager.Current.WakeOnLAN_ExpandProfileView = value;

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

            if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.Profile_FloatPointFix) // Do not save the size when collapsed
                SettingsManager.Current.WakeOnLAN_ProfileWidth = value.Value;

            _profileWidth = value;

            if (_canProfileWidthChange)
                ResizeProfile(true);

            OnPropertyChanged();
        }
    }
    #endregion
    #endregion

    #region Constructor, load settings
    public WakeOnLANViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;
        
        _dialogCoordinator = instance;

        MACAddressHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.WakeOnLan_MACAddressHistory);
        BroadcastHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.WakeOnLan_BroadcastHistory);

        // Profiles
        SetProfilesView();

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ExpandProfileView = SettingsManager.Current.WakeOnLAN_ExpandProfileView;

        ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.WakeOnLAN_ProfileWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.WakeOnLAN_ProfileWidth;
    }
    #endregion

    #region ICommands & Actions
    public ICommand WakeUpCommand => new RelayCommand(_ => WakeUpAction(), WakeUpAction_CanExecute);

    private bool WakeUpAction_CanExecute(object parameter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

    private void WakeUpAction()
    {
        var info = new WakeOnLANInfo
        {
            MagicPacket = WakeOnLAN.CreateMagicPacket(MACAddress),
            Broadcast = IPAddress.Parse(Broadcast),
            Port = SettingsManager.Current.WakeOnLAN_Port
        };

        AddMACAddressToHistory(MACAddress);
        AddBroadcastToHistory(Broadcast);

        WakeUp(info).ConfigureAwait(false);
    }

    public ICommand WakeUpProfileCommand => new RelayCommand(_ => WakeUpProfileAction());

    private void WakeUpProfileAction()
    {
        WakeUp(NETworkManager.Profiles.Application.WakeOnLAN.CreateInfo(SelectedProfile)).ConfigureAwait(false);
    }

    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    private void AddProfileAction()
    {
        ProfileDialogManager.ShowAddProfileDialog(this, _dialogCoordinator, null, null, ApplicationName.WakeOnLAN).ConfigureAwait(false);
    }

    private bool ModifyProfile_CanExecute(object obj) => SelectedProfile is { IsDynamic: false };

    public ICommand EditProfileCommand => new RelayCommand(_ => EditProfileAction(), ModifyProfile_CanExecute);

    private void EditProfileAction()
    {
        ProfileDialogManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile).ConfigureAwait(false);
    }

    public ICommand CopyAsProfileCommand => new RelayCommand(_ => CopyAsProfileAction(), ModifyProfile_CanExecute);

    private void CopyAsProfileAction()
    {
        ProfileDialogManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile).ConfigureAwait(false);
    }

    public ICommand DeleteProfileCommand => new RelayCommand(_ => DeleteProfileAction(), ModifyProfile_CanExecute);

    private void DeleteProfileAction()
    {
        ProfileDialogManager.ShowDeleteProfileDialog(this, _dialogCoordinator, new List<ProfileInfo> { SelectedProfile }).ConfigureAwait(false);
    }

    public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

    private void EditGroupAction(object group)
    {
        ProfileDialogManager.ShowEditGroupDialog(this, _dialogCoordinator, ProfileManager.GetGroup(group.ToString())).ConfigureAwait(false);
    }

    public ICommand ClearSearchCommand => new RelayCommand(_ => ClearSearchAction());

    private void ClearSearchAction()
    {
        Search = string.Empty;
    }
    #endregion

    #region Methods
    private async Task WakeUp(WakeOnLANInfo info)
    {
        IsStatusMessageDisplayed = false;
        IsRunning = true;

        try
        {
            WakeOnLAN.Send(info);

            // Make the user happy, let him see a reload animation (and he cannot spam the reload command)
            await Task.Delay(2000);

            StatusMessage = Localization.Resources.Strings.MagicPacketSentMessage;
            IsStatusMessageDisplayed = true;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }

        IsRunning = false;
    }

    private void AddMACAddressToHistory(string macAddress)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.WakeOnLan_MACAddressHistory.ToList(), macAddress, SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.WakeOnLan_MACAddressHistory.Clear();
        OnPropertyChanged(nameof(MACAddress)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.WakeOnLan_MACAddressHistory.Add(x));
    }

    private void AddBroadcastToHistory(string broadcast)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.WakeOnLan_BroadcastHistory.ToList(), broadcast, SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.WakeOnLan_BroadcastHistory.Clear();
        OnPropertyChanged(nameof(Broadcast)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.WakeOnLan_BroadcastHistory.Add(x));
    }

    private void ResizeProfile(bool dueToChangedSize)
    {
        _canProfileWidthChange = false;

        if (dueToChangedSize)
        {
            ExpandProfileView = Math.Abs(ProfileWidth.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.Profile_FloatPointFix;
        }
        else
        {
            if (ExpandProfileView)
            {
                ProfileWidth = Math.Abs(_tempProfileWidth - GlobalStaticConfiguration.Profile_WidthCollapsed) < GlobalStaticConfiguration.Profile_FloatPointFix ? new GridLength(GlobalStaticConfiguration.Profile_DefaultWidthExpanded) : new GridLength(_tempProfileWidth);
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
        _isViewActive = true;

        RefreshProfiles();
    }

    public void OnViewHide()
    {
        _isViewActive = false;
    }

    private void SetProfilesView(ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource { Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.WakeOnLAN_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name) }.View;

        Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));

        Profiles.Filter = o =>
        {
            if (o is not ProfileInfo info)
                return false;

            if (string.IsNullOrEmpty(Search))
                return true;

            var search = Search.Trim();

            // Search by: Tag=xxx (exact match, ignore case)
            /*
            if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                return !string.IsNullOrEmpty(info.Tags) && info.PingMonitor_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));
            */

            // Search by: Name, WakeOnLAN_MACAddress
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.WakeOnLAN_MACAddress.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Set specific profile or first if null
        SelectedProfile = null;

        if (profile != null)
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault(x => x.Equals(profile)) ??
                Profiles.Cast<ProfileInfo>().FirstOrDefault();
        else
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault();
    }

    private void RefreshProfiles()
    {
        if (!_isViewActive)
            return;

        SetProfilesView(SelectedProfile);
    }
    #endregion

    #region Event
    private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
    {
        RefreshProfiles();
    }

    private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
    {
        _searchDispatcherTimer.Stop();

        RefreshProfiles();

        IsSearching = false;
    }
    #endregion
}
