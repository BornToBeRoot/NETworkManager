using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using System.Linq;
using NETworkManager.Views;
using System.ComponentModel;
using System.Windows.Data;
using System;
using NETworkManager.Utilities;
using System.Windows;
using NETworkManager.Profiles;
using NETworkManager.Models.WebConsole;
using System.Windows.Threading;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NETworkManager.ViewModels;

public class WebConsoleHostViewModel : ViewModelBase, IProfileManager
{
    #region Variables
    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly DispatcherTimer _searchDispatcherTimer = new();

    public IInterTabClient InterTabClient { get; }
    public ObservableCollection<DragablzTabItem> TabItems { get; }

    private readonly bool _isLoading = true;
    private bool _isViewActive = true;

    /// <summary>
    /// Private variable for <see cref="IsRuntimeAvailable"/>.
    /// </summary>
    private bool _isRuntimeAvailable;

    /// <summary>
    /// Variable indicates if the Edge WebView2 runtime is available.
    /// </summary>
    public bool IsRuntimeAvailable
    {
        get => _isRuntimeAvailable;
        set
        {
            if (value == _isRuntimeAvailable)
                return;

            _isRuntimeAvailable = value;
            OnPropertyChanged();
        }
    }

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

    #region Profiles
    public ICollectionView _profiles;
    public ICollectionView Profiles
    {
        get => _profiles;
        set
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
                SettingsManager.Current.WebConsole_ExpandProfileView = value;

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
                SettingsManager.Current.WebConsole_ProfileWidth = value.Value;

            _profileWidth = value;

            if (_canProfileWidthChange)
                ResizeProfile(true);

            OnPropertyChanged();
        }
    }
    #endregion
    #endregion

    #region Constructor, load settings
    public WebConsoleHostViewModel(IDialogCoordinator instance)
    {
        _dialogCoordinator = instance;

        try
        {
            string version = CoreWebView2Environment.GetAvailableBrowserVersionString();
            IsRuntimeAvailable = true;
        }
        catch (WebView2RuntimeNotFoundException)
        {
            IsRuntimeAvailable = false;
        }

        InterTabClient = new DragablzInterTabClient(ApplicationName.WebConsole);

        TabItems = new ObservableCollection<DragablzTabItem>();

        // Profiles
        SetProfilesView();

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        LoadSettings();

        SettingsManager.Current.PropertyChanged += Current_PropertyChanged;

        _isLoading = false;
    }

    private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
    }

    private void LoadSettings()
    {
        ExpandProfileView = SettingsManager.Current.WebConsole_ExpandProfileView;

        ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.WebConsole_ProfileWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.WebConsole_ProfileWidth;
    }
    #endregion

    #region ICommand & Actions
    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as WebConsoleControl)?.CloseTab();
    }

    public ICommand ConnectCommand => new RelayCommand(p => ConnectAction());

    private void ConnectAction()
    {
        Connect();
    }

    public ICommand ReloadCommand => new RelayCommand(ReloadAction);

    private void ReloadAction(object view)
    {
        if (view is WebConsoleControl control)
        {
            if (control.ReloadCommand.CanExecute(null))
                control.ReloadCommand.Execute(null);
        }
    }
    
    public ICommand ConnectProfileCommand => new RelayCommand(p => ConnectProfileAction(), ConnectProfile_CanExecute);

    private bool ConnectProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void ConnectProfileAction()
    {
        ConnectProfile();
    }

    public ICommand AddProfileCommand => new RelayCommand(p => AddProfileAction());

    private void AddProfileAction()
    {
        ProfileDialogManager.ShowAddProfileDialog(this, _dialogCoordinator, null, null, ApplicationName.WebConsole);
    }

    private bool ModifyProfile_CanExecute(object obj) => SelectedProfile != null && !SelectedProfile.IsDynamic;

    public ICommand EditProfileCommand => new RelayCommand(p => EditProfileAction(), ModifyProfile_CanExecute);

    private void EditProfileAction()
    {
        ProfileDialogManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile);
    }

    public ICommand CopyAsProfileCommand => new RelayCommand(p => CopyAsProfileAction(), ModifyProfile_CanExecute);

    private void CopyAsProfileAction()
    {
        ProfileDialogManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile);
    }

    public ICommand DeleteProfileCommand => new RelayCommand(p => DeleteProfileAction(), ModifyProfile_CanExecute);

    private void DeleteProfileAction()
    {
        ProfileDialogManager.ShowDeleteProfileDialog(this, _dialogCoordinator, new List<ProfileInfo> { SelectedProfile });
    }

    public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

    private void EditGroupAction(object group)
    {
        ProfileDialogManager.ShowEditGroupDialog(this, _dialogCoordinator, ProfileManager.GetGroup(group.ToString()));
    }

    public ICommand ClearSearchCommand => new RelayCommand(p => ClearSearchAction());

    private void ClearSearchAction()
    {
        Search = string.Empty;
    }

    public ICommand OpenSettingsCommand => new RelayCommand(p => OpenSettingsAction());

    private static void OpenSettingsAction()
    {
        EventSystem.RedirectToSettings();
    }

    public ICommand OpenWebsiteCommand => new RelayCommand(OpenWebsiteAction);

    private static void OpenWebsiteAction(object url)
    {
        ExternalProcessStarter.OpenUrl((string)url);
    }
    #endregion

    #region Methods
    private async Task Connect()
    {
        var customDialog = new CustomDialog
        {
            Title = Localization.Resources.Strings.Connect
        };

        var connectViewModel = new WebConsoleConnectViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            ConfigurationManager.Current.IsDialogOpen = false;

            // Create profile info
            var info = new WebConsoleSessionInfo
            {
                Url = instance.Url
            };

            // Add to history
            // Note: The history can only be updated after the values have been read.
            //       Otherwise, in some cases, incorrect values are taken over.
            AddUrlToHistory(instance.Url);

            Connect(info);
        }, async instance =>
         {
             await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
             ConfigurationManager.Current.IsDialogOpen = false;
         });

        customDialog.Content = new WebConsoleConnectDialog
        {
            DataContext = connectViewModel
        };

        ConfigurationManager.Current.IsDialogOpen = true;
        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private void ConnectProfile()
    {
        Connect(NETworkManager.Profiles.Application.WebConsole.CreateSessionInfo(SelectedProfile), SelectedProfile.Name);
    }

    private void Connect(WebConsoleSessionInfo sessionInfo, string header = null)
    {
        TabItems.Add(new DragablzTabItem(header ?? sessionInfo.Url, new WebConsoleControl(sessionInfo)));

        SelectedTabIndex = TabItems.Count - 1;
    }

    // Modify history list
    private static void AddUrlToHistory(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        SettingsManager.Current.WebConsole_UrlHistory = new ObservableCollection<string>(ListHelper.Modify(SettingsManager.Current.WebConsole_UrlHistory.ToList(), url, SettingsManager.Current.General_HistoryListEntries));
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
        _isViewActive = true;

        RefreshProfiles();
    }

    public void OnViewHide()
    {
        _isViewActive = false;
    }

    private void SetProfilesView(ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource { Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.WebConsole_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name) }.View;

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

            // Search by: Name, WebConsole_Url
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.WebConsole_Url.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Set specific profile or first if null
        SelectedProfile = null;

        if (profile != null)
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault(x => x.Equals(profile)) ??
                Profiles.Cast<ProfileInfo>().FirstOrDefault();
        else
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault();
    }

    public void RefreshProfiles()
    {
        if (!_isViewActive)
            return;

        SetProfilesView(SelectedProfile);
    }

    public void OnProfileManagerDialogOpen()
    {
        ConfigurationManager.Current.IsDialogOpen = true;
    }

    public void OnProfileManagerDialogClose()
    {
        ConfigurationManager.Current.IsDialogOpen = false;
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