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
    public bool IsAdmin { get; }

    /// <summary>
    ///     Execution path of the application like "C:\Program Files\NETworkManager".
    /// </summary>
    public string ExecutionPath { get; }

    /// <summary>
    ///     Full path of the application like "C:\Program Files\NETworkManager\NETworkManager.exe"
    /// </summary>
    public string ApplicationFullName { get; }

    /// <summary>
    ///     Application name like "NETworkManager".
    /// </summary>
    public string ApplicationName { get; }

    /// <summary>
    ///     Indicates if the application is running in portable mode.
    /// </summary>
    public bool IsPortable { get; }

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

    public int IPScannerTabCount
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

    public int PortScannerTabCount
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

    public int TracerouteTabCount
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

    public int DNSLookupTabCount
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

    public int RemoteDesktopTabCount
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

    public bool IsRemoteDesktopWindowDragging
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

    public int PowerShellTabCount
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

    public bool IsPowerShellWindowDragging
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

    public int PuTTYTabCount
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

    public bool IsPuTTYWindowDragging
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

    public int TigerVNCTabCount
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

    public bool IsTigerVNCWindowDragging
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

    public int WebConsoleTabCount
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

    public bool IsWebConsoleWindowDragging
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

    public int SNMPTabCount
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

    public int SNTPLookupTabCount
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

    public int WhoisTabCount
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

    public int IPGeolocationTabCount
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

    /// <summary>
    ///     Indicates if the profile manager is enabled.
    /// </summary>
    public bool ProfileManagerIsEnabled
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

    /// <summary>
    ///     Indicates if the profile manager should show an unlock option.
    /// </summary>
    public bool ProfileManagerShowUnlock
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

    /// <summary>
    ///     Error message if the profile manager is not enabled.
    /// </summary>
    public string ProfileManagerErrorMessage
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Indicates if a child window is open.
    /// </summary>
    public bool IsChildWindowOpen
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

    /// <summary>
    /// Indicates if a profile filter popup is open.
    /// </summary>
    public bool IsProfileFilterPopupOpen
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

    /// <summary>
    ///     Indicates if there may be an airspace issue that needs to be fixed.
    /// </summary>
    public bool FixAirspace
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
}