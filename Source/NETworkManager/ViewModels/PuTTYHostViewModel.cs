using Dragablz;
using log4net;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.PuTTY;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PuTTYProfile = NETworkManager.Settings.Application.PuTTY;

namespace NETworkManager.ViewModels;

public class PuTTYHostViewModel : ProfileHostViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(PuTTYHostViewModel));

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

    public bool IsExecutableConfigured
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

    public bool HeaderContextMenuIsOpen
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

    private bool _textBoxSearchIsFocused;

    /// <summary>
    /// Gets or sets a value indicating whether the profile context menu is open. Bound to
    /// <see cref="Views.Controls.ProfileExpanderPanel.ProfileContextMenuIsOpen"/> so <see cref="FocusEmbeddedWindow"/>
    /// can avoid stealing focus while it is open.
    /// </summary>
    public bool ProfileContextMenuIsOpen
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

    #region Constructor, load settings

    public PuTTYHostViewModel()
    {
        // Check if PuTTY executable is configured
        CheckExecutable();

        // Try to find PuTTY executable
        if (!IsExecutableConfigured)
            TryFindExecutable();

        WriteDefaultProfileToRegistry();

        InterTabClient = new DragablzInterTabClient(ApplicationName.PuTTY);
        InterTabPartition = nameof(ApplicationName.PuTTY);

        TabItems = [];

        InitializeProfileHost();

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.PuTTY;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.PuTTY_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.PuTTY_HostOrSerialLine;

    /// <summary>
    /// Also mirrors the popup state into <see cref="ConfigurationManager"/> so the main window can suppress
    /// global focus-stealing behavior (e.g. re-focusing the embedded terminal) while the filter popup is open.
    /// </summary>
    public new ICommand OpenProfileFilterCommand => new RelayCommand(_ => OpenProfileFilterAction());

    private void OpenProfileFilterAction()
    {
        ConfigurationManager.Current.IsProfileFilterPopupOpen = true;

        ProfileFilterIsOpen = true;
    }

    /// <summary>
    /// Called when the tag-filter popup closes (including an implicit close, e.g. clicking away), to keep
    /// <see cref="ConfigurationManager.Current"/> in sync.
    /// </summary>
    public ICommand ProfileFilterPopupClosedCommand => new RelayCommand(_ => OnProfileFilterClosed());

    public void OnProfileFilterClosed()
    {
        ConfigurationManager.Current.IsProfileFilterPopupOpen = false;
    }

    public void OnProfileManagerDialogOpen()
    {
        ConfigurationManager.OnDialogOpen();
    }

    public void OnProfileManagerDialogClose()
    {
        ConfigurationManager.OnDialogClose();
    }

    #endregion

    #region ICommand & Actions

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as IDragablzTabItem)?.CloseTab();
    }

    private bool Connect_CanExecute(object obj)
    {
        return IsExecutableConfigured;
    }

    public ICommand ConnectCommand => new RelayCommand(_ => ConnectAction(), Connect_CanExecute);

    private void ConnectAction()
    {
        _ = Connect();
    }

    private bool IsConnected_CanExecute(object view)
    {
        if (view is PuTTYControl control)
            return control.IsConnected;

        return false;
    }

    public ICommand ReconnectCommand => new RelayCommand(ReconnectAction);

    private void ReconnectAction(object view)
    {
        if (view is not PuTTYControl control)
            return;

        if (control.ReconnectCommand.CanExecute(null))
            control.ReconnectCommand.Execute(null);
    }

    public ICommand ResizeWindowCommand => new RelayCommand(ResizeWindowAction, IsConnected_CanExecute);

    private void ResizeWindowAction(object view)
    {
        if (view is PuTTYControl control)
            control.ResizeEmbeddedWindow();
    }

    public ICommand RestartSessionCommand => new RelayCommand(RestartSessionAction, IsConnected_CanExecute);

    private void RestartSessionAction(object view)
    {
        if (view is PuTTYControl control)
            control.RestartSession();
    }

    public ICommand ConnectProfileCommand => new RelayCommand(_ => ConnectProfileAction(), ConnectProfile_CanExecute);

    private bool ConnectProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void ConnectProfileAction()
    {
        ConnectProfile();
    }

    public ICommand ConnectProfileExternalCommand => new RelayCommand(_ => ConnectProfileExternalAction());

    private void ConnectProfileExternalAction()
    {
        ConnectProfileExternal();
    }

    public ICommand TextBoxSearchGotFocusCommand
    {
        get { return new RelayCommand(_ => _textBoxSearchIsFocused = true); }
    }

    public ICommand TextBoxSearchLostFocusCommand
    {
        get { return new RelayCommand(_ => _textBoxSearchIsFocused = false); }
    }

    public ICommand OpenSettingsCommand => new RelayCommand(_ => OpenSettingsAction());

    private static void OpenSettingsAction()
    {
        EventSystem.RedirectToSettings();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Check if executable is configured and exists.
    /// </summary>
    private void CheckExecutable()
    {
        IsExecutableConfigured = !string.IsNullOrEmpty(SettingsManager.Current.PuTTY_ApplicationFilePath) &&
                                 File.Exists(SettingsManager.Current.PuTTY_ApplicationFilePath);

        if (IsExecutableConfigured)
            Log.Info($"PuTTY executable configured: \"{SettingsManager.Current.PuTTY_ApplicationFilePath}\"");
        else
            Log.Warn("PuTTY executable not found!");
    }

    /// <summary>
    /// Try to find executable.
    /// </summary>
    private void TryFindExecutable()
    {
        Log.Info("Try to find PuTTY executable...");

        SettingsManager.Current.PuTTY_ApplicationFilePath = ApplicationHelper.Find(Models.PuTTY.PuTTY.FileName);

        CheckExecutable();

        if (!IsExecutableConfigured)
            Log.Warn("Install PuTTY or configure the path in the settings.");
    }

    private Task Connect(string host = null)
    {
        var childWindow = new PuTTYConnectChildWindow();

        var childWindowViewModel = new PuTTYConnectViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();

            // Create profile info
            var sessionInfo = new PuTTYSessionInfo
            {
                HostOrSerialLine = instance.ConnectionMode == ConnectionMode.Serial
                    ? instance.SerialLine
                    : instance.Host,
                Mode = instance.ConnectionMode,
                PortOrBaud = instance.ConnectionMode == ConnectionMode.Serial ? instance.Baud : instance.Port,
                Username = instance.Username,
                PrivateKey = instance.PrivateKeyFile,
                Profile = instance.Profile,
                EnableLog = SettingsManager.Current.PuTTY_EnableSessionLog,
                LogMode = SettingsManager.Current.PuTTY_LogMode,
                LogFileName = SettingsManager.Current.PuTTY_LogFileName,
                LogPath = PuTTYProfile.LogPath,
                AdditionalCommandLine = PlaceholderHelper.Resolve(instance.AdditionalCommandLine,
                    (PlaceholderHelper.Host, instance.ConnectionMode == ConnectionMode.Serial ? null : instance.Host))
            };

            // Add to history
            // Note: The history can only be updated after the values have been read.
            //       Otherwise, in some cases, incorrect values are taken over.
            AddHostToHistory(instance.Host);
            AddSerialLineToHistory(instance.SerialLine);
            AddPortToHistory(instance.Port);
            AddBaudToHistory(instance.Baud);
            AddUsernameToHistory(instance.Username);
            AddPrivateKeyToHistory(instance.PrivateKeyFile);
            AddProfileToHistory(instance.Profile);

            Connect(sessionInfo);
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();
        }, host);

        childWindow.Title = Strings.Connect;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        ConfigurationManager.OnDialogOpen();

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    private void ConnectProfile()
    {
        Connect(NETworkManager.Profiles.Application.PuTTY.CreateSessionInfo(SelectedProfile), SelectedProfile.Name);
    }

    private void ConnectProfileExternal()
    {
        // Create log path
        DirectoryHelper.CreateWithEnvironmentVariables(PuTTYProfile.LogPath);

        var sessionInfo = NETworkManager.Profiles.Application.PuTTY.CreateSessionInfo(SelectedProfile);

        ProcessStartInfo info = new()
        {
            FileName = SettingsManager.Current.PuTTY_ApplicationFilePath,
            Arguments = PuTTY.BuildCommandLine(sessionInfo)
        };

        Process.Start(info);
    }

    private void Connect(PuTTYSessionInfo sessionInfo, string header = null)
    {
        // Must be added here. So that it works with profiles and the connect dialog.
        sessionInfo.ApplicationFilePath = SettingsManager.Current.PuTTY_ApplicationFilePath;

        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(header ?? sessionInfo.HostOrSerialLine, new PuTTYControl(tabId, sessionInfo),
            tabId));

        // Select the added tab
        SelectedTabIndex = TabItems.Count - 1;
    }

    public void AddTab(string host)
    {
        _ = Connect(host);
    }

    // Modify history list
    private static void AddHostToHistory(string host)
    {
        if (string.IsNullOrEmpty(host))
            return;

        SettingsManager.Current.PuTTY_HostHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_HostHistory.ToList(), host,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddSerialLineToHistory(string serialLine)
    {
        if (string.IsNullOrEmpty(serialLine))
            return;

        SettingsManager.Current.PuTTY_SerialLineHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_SerialLineHistory.ToList(), serialLine,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddPortToHistory(int port)
    {
        if (port == 0)
            return;

        SettingsManager.Current.PuTTY_PortHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_PortHistory.ToList(), port.ToString(),
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddBaudToHistory(int baud)
    {
        if (baud == 0)
            return;

        SettingsManager.Current.PuTTY_BaudHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_BaudHistory.ToList(), baud.ToString(),
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddUsernameToHistory(string username)
    {
        if (string.IsNullOrEmpty(username))
            return;

        SettingsManager.Current.PuTTY_UsernameHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_UsernameHistory.ToList(), username,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddPrivateKeyToHistory(string privateKey)
    {
        if (string.IsNullOrEmpty(privateKey))
            return;

        SettingsManager.Current.PuTTY_PrivateKeyFileHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_PrivateKeyFileHistory.ToList(), privateKey,
                SettingsManager.Current.General_HistoryListEntries));
    }

    private static void AddProfileToHistory(string profile)
    {
        if (string.IsNullOrEmpty(profile))
            return;

        SettingsManager.Current.PuTTY_ProfileHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PuTTY_ProfileHistory.ToList(), profile,
                SettingsManager.Current.General_HistoryListEntries));
    }

    public void FocusEmbeddedWindow()
    {
        /* Don't continue if
           - Search TextBox is focused
           - Header ContextMenu is opened
           - Profile ContextMenu is opened
        */
        if (_textBoxSearchIsFocused || HeaderContextMenuIsOpen || ProfileContextMenuIsOpen)
            return;

        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        if (window == null)
            return;

        // Find all TabablzControl in the active window
        foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
        {
            // Skip if no items
            if (tabablzControl.Items.Count == 0)
                continue;

            // Focus embedded window in the selected tab
            (((DragablzTabItem)tabablzControl.SelectedItem)?.View as IEmbeddedWindow)?.FocusEmbeddedWindow();

            break;
        }
    }

    public override void OnViewVisible()
    {
        base.OnViewVisible();

        FocusEmbeddedWindow();
    }

    private void WriteDefaultProfileToRegistry()
    {
        if (!IsExecutableConfigured)
            return;

        Log.Debug("Write PuTTY profile to registry...");

        PuTTY.WriteDefaultProfileToRegistry(SettingsManager.Current.Appearance_Theme);
    }

    #endregion

    #region Event

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.PuTTY_ApplicationFilePath):
                CheckExecutable();
                break;
            case nameof(SettingsInfo.Appearance_Theme):
                WriteDefaultProfileToRegistry();
                break;
        }
    }

    #endregion
}
