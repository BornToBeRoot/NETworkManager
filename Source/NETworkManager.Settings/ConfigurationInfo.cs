using NETworkManager.Models;
using NETworkManager.Utilities;

namespace NETworkManager.Settings;

/*
 * This class is used to store static and dynamic configuration used in the application
 * across multiple windows, views, dialogs, etc.
 */
public class ConfigurationInfo : PropertyChangedBase
{
    #region Constructor

    /// <summary>
    ///     Create a new instance of <see cref="ConfigurationInfo" /> with static configuration.
    /// </summary>
    public ConfigurationInfo(bool isAdmin, string executionPath, string applicationFullName, string applicationName,
        bool isPortable)
    {
        IsAdmin = isAdmin;
        ExecutionPath = executionPath;
        ApplicationFullName = applicationFullName;
        ApplicationName = applicationName;
        IsPortable = isPortable;
    }

    #endregion

    #region Static properties set at startup

    /// <summary>
    ///     Indicates that the application is running as administrator.
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    ///     Execution path of the application like "C:\Program Files\NETworkManager".
    /// </summary>
    public string ExecutionPath { get; set; }

    /// <summary>
    ///     Full path of the application like "C:\Program Files\NETworkManager\NETworkManager.exe"
    /// </summary>
    public string ApplicationFullName { get; set; }

    /// <summary>
    ///     Application name like "NETworkManager".
    /// </summary>
    public string ApplicationName { get; set; }

    /// <summary>
    ///     Indicates if the application is running in portable mode.
    /// </summary>
    public bool IsPortable { get; set; }

    #endregion

    #region Dynamic properties set at runtime

    /// <summary>
    ///     Shows a reset notice if the settings were corrupted and reset.
    /// </summary>
    public bool ShowSettingsResetNoteOnStartup { get; set; }

    /// <summary>
    ///     Currently selected application.
    /// </summary>
    public ApplicationName CurrentApplication { get; set; } = Models.ApplicationName.None;

    /// <summary>
    ///     Indicates if Remote Desktop has tabs.
    /// </summary>
    public bool RemoteDesktopHasTabs { get; set; }

    /// <summary>
    ///     Indicates if PowerShell has tabs.
    /// </summary>
    public bool PowerShellHasTabs { get; set; }

    /// <summary>
    ///     Indicates if PuTTY has tabs.
    /// </summary>
    public bool PuTTYHasTabs { get; set; }

    /// <summary>
    ///     Indicates if AWS Session Manager has tabs.
    /// </summary>
    public bool AWSSessionManagerHasTabs { get; set; }

    /// <summary>
    ///     Indicates if TigerVNC has tabs.
    /// </summary>
    public bool TigerVNCHasTabs { get; set; }

    /// <summary>
    ///     Indicates if WebConsole has tabs.
    /// </summary>
    public bool WebConsoleHasTabs { get; set; }

    /// <summary>
    ///     Private variable for <see cref="ProfileManagerIsEnabled" />.
    /// </summary>
    private bool _profileManagerIsEnabled;

    /// <summary>
    ///     Indicates if the profile manager is enabled.
    /// </summary>
    public bool ProfileManagerIsEnabled
    {
        get => _profileManagerIsEnabled;
        set
        {
            if (value == _profileManagerIsEnabled)
                return;

            _profileManagerIsEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Private variable for <see cref="ProfileManagerShowUnlock" />.
    /// </summary>
    private bool _profileManagerShowUnlock;

    /// <summary>
    ///     Indicates if the profile manager should show an unlock option.
    /// </summary>
    public bool ProfileManagerShowUnlock
    {
        get => _profileManagerShowUnlock;
        set
        {
            if (value == _profileManagerShowUnlock)
                return;

            _profileManagerShowUnlock = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Private variable for <see cref="ProfileManagerErrorMessage" />.
    /// </summary>
    private string _profileManagerErrorMessage = string.Empty;

    /// <summary>
    ///     Error message if the profile manager is not enabled.
    /// </summary>
    public string ProfileManagerErrorMessage
    {
        get => _profileManagerErrorMessage;
        set
        {
            if (value == _profileManagerErrorMessage)
                return;

            _profileManagerErrorMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Private variable for <see cref="FixAirspace" />.
    /// </summary>
    private bool _fixAirspace;

    /// <summary>
    ///     Indicates if there may be an airspace issue that needs to be fixed.
    /// </summary>
    public bool FixAirspace
    {
        get => _fixAirspace;
        set
        {
            if (value == _fixAirspace)
                return;

            _fixAirspace = value;
            OnPropertyChanged();
        }
    }

    #endregion
}