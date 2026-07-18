using Dragablz;
using log4net;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.PowerShell;
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
using PowerShellProfile = NETworkManager.Profiles.Application.PowerShell;

namespace NETworkManager.ViewModels;

public class PowerShellHostViewModel : ProfileHostViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(PowerShellHostViewModel));

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

    public PowerShellHostViewModel()
    {
        // Check if PowerShell executable is configured
        CheckExecutable();

        // Try to find PowerShell executable

        if (!IsExecutableConfigured)
            TryFindExecutable();

        WriteDefaultProfileToRegistry();

        InterTabClient = new DragablzInterTabClient(ApplicationName.PowerShell);
        InterTabPartition = nameof(ApplicationName.PowerShell);

        TabItems = [];

        InitializeProfileHost();

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.PowerShell;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.PowerShell_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.PowerShell_Host;

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
        if (view is PowerShellControl control)
            return control.IsConnected;

        return false;
    }

    public ICommand ReconnectCommand => new RelayCommand(ReconnectAction);

    private void ReconnectAction(object view)
    {
        if (view is not PowerShellControl control)
            return;

        if (control.ReconnectCommand.CanExecute(null))
            control.ReconnectCommand.Execute(null);
    }

    public ICommand ResizeWindowCommand => new RelayCommand(ResizeWindowAction, IsConnected_CanExecute);

    private void ResizeWindowAction(object view)
    {
        if (view is PowerShellControl control)
            control.ResizeEmbeddedWindow();
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

    public ICommand ClearSearchCommand => new RelayCommand(_ => ClearSearchAction());

    private void ClearSearchAction()
    {
        Search = string.Empty;
    }

    public ICommand OpenSettingsCommand => new RelayCommand(_ => OpenSettingsAction());

    private static void OpenSettingsAction()
    {
        EventSystem.RedirectToSettings();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Check if the executable is configured and exists.
    /// </summary>
    private void CheckExecutable()
    {
        IsExecutableConfigured = !string.IsNullOrEmpty(SettingsManager.Current.PowerShell_ApplicationFilePath) &&
                                 File.Exists(SettingsManager.Current.PowerShell_ApplicationFilePath);

        if (IsExecutableConfigured)
            Log.Info($"PowerShell executable found: \"{SettingsManager.Current.PowerShell_ApplicationFilePath}\"");
        else
            Log.Warn("PowerShell executable not found!");
    }

    /// <summary>
    /// Try to find executable.
    /// </summary>
    private void TryFindExecutable()
    {
        Log.Info("Try to find PowerShell executable...");

        var applicationFilePath = ApplicationHelper.Find(PowerShell.PwshFileName);

        // Workaround for: https://github.com/BornToBeRoot/NETworkManager/issues/3223
        if (applicationFilePath.EndsWith("AppData\\Local\\Microsoft\\WindowsApps\\pwsh.exe"))
        {
            Log.Info("Found pwsh.exe in AppData (Microsoft Store installation). Trying to resolve real path...");

            var realPwshPath = FindRealPwshPath(applicationFilePath);

            if (realPwshPath != null)
                applicationFilePath = realPwshPath;
        }

        // Fallback to Windows PowerShell
        if (string.IsNullOrEmpty(applicationFilePath))
        {
            Log.Warn("Failed to resolve pwsh.exe path. Falling back to Windows PowerShell.");

            applicationFilePath = ApplicationHelper.Find(PowerShell.WindowsPowerShellFileName);
        }

        SettingsManager.Current.PowerShell_ApplicationFilePath = applicationFilePath;

        CheckExecutable();

        if (!IsExecutableConfigured)
            Log.Warn("Install PowerShell or configure the path in the settings.");
    }

    /// <summary>
    /// Resolves the actual installation path of a PowerShell executable that was installed via the
    /// Microsoft Store / WindowsApps and therefore appears as a proxy stub in the user's AppData.
    ///
    /// Typical input is a path like:
    /// <c>C:\Users\{USERNAME}\AppData\Local\Microsoft\WindowsApps\pwsh.exe</c>
    ///
    /// This helper attempts to locate the corresponding real executable under the Program Files
    /// WindowsApps package layout, e.g.:
    /// <c>C:\Program Files\WindowsApps\Microsoft.PowerShell_7.*_8wekyb3d8bbwe\pwsh.exe</c>.
    ///
    /// Workaround for: https://github.com/BornToBeRoot/NETworkManager/issues/3223
    /// </summary>
    /// <param name="path">Path to the pwsh proxy stub, typically located under the current user's <c>%LocalAppData%\Microsoft\WindowsApps\pwsh.exe</c>.</param>
    /// <returns>Full path to the real pwsh executable under Program Files WindowsApps when found; otherwise null.</returns>
    private string FindRealPwshPath(string path)
    {
        try
        {
            var command = "(Get-Command pwsh).Source";

            ProcessStartInfo psi = new()
            {
                FileName = path,
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(psi);

            string output = process.StandardOutput.ReadToEnd();

            if (!process.WaitForExit(10000))
            {
                process.Kill();
                Log.Warn("Timeout while trying to resolve real pwsh path.");

                return null;
            }

            if (string.IsNullOrEmpty(output))
                return null;

            output = output.Replace(@"\\", @"\")
                           .Replace(@"\r", string.Empty)
                           .Replace(@"\n", string.Empty)
                           .Replace("\r\n", string.Empty)
                           .Replace("\n", string.Empty)
                           .Replace("\r", string.Empty);

            return output.Trim();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to resolve real pwsh path: {ex.Message}");

            return null;
        }
    }

    private Task Connect(string host = null)
    {
        var childWindow = new PowerShellConnectChildWindow();

        var childWindowViewModel = new PowerShellConnectViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();

            // Create profile info
            var sessionInfo = new PowerShellSessionInfo
            {
                EnableRemoteConsole = instance.EnableRemoteConsole,
                Host = instance.Host,
                Command = instance.Command,
                AdditionalCommandLine = PlaceholderHelper.Resolve(instance.AdditionalCommandLine,
                    (PlaceholderHelper.Host, instance.Host)),
                ExecutionPolicy = instance.ExecutionPolicy
            };

            // Add to history
            // Note: The history can only be updated after the values have been read.
            //       Otherwise, in some cases, incorrect values are taken over.
            AddHostToHistory(instance.Host);

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
        Connect(PowerShellProfile.CreateSessionInfo(SelectedProfile),
            SelectedProfile.Name);
    }

    private void ConnectProfileExternal()
    {
        var sessionInfo =
            PowerShellProfile.CreateSessionInfo(SelectedProfile);

        Process.Start(new ProcessStartInfo
        {
            FileName = SettingsManager.Current.PowerShell_ApplicationFilePath,
            Arguments = PowerShell.BuildCommandLine(sessionInfo)
        });
    }

    private void Connect(PowerShellSessionInfo sessionInfo, string header = null)
    {
        sessionInfo.ApplicationFilePath = SettingsManager.Current.PowerShell_ApplicationFilePath;

        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(
            header ?? (sessionInfo.EnableRemoteConsole ? sessionInfo.Host : Strings.PowerShell),
            new PowerShellControl(tabId, sessionInfo), tabId));

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

        SettingsManager.Current.PowerShell_HostHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.PowerShell_HostHistory.ToList(), host,
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
        if (!SettingsManager.Current.Appearance_PowerShellModifyGlobalProfile)
            return;

        if (!IsExecutableConfigured)
            return;

        Log.Debug("Write PowerShell profile to registry...");

        PowerShell.WriteDefaultProfileToRegistry(
            SettingsManager.Current.Appearance_Theme,
            SettingsManager.Current.PowerShell_ApplicationFilePath);
    }

    #endregion

    #region Event

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.PowerShell_ApplicationFilePath):
                CheckExecutable();
                WriteDefaultProfileToRegistry();
                break;
            case nameof(SettingsInfo.Appearance_PowerShellModifyGlobalProfile):
            case nameof(SettingsInfo.Appearance_Theme):
                WriteDefaultProfileToRegistry();
                break;
        }
    }

    #endregion
}
