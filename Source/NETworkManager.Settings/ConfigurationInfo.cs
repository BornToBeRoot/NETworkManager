using System.Diagnostics;
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

    private int _ipScannerTabCount;
    public int IPScannerTabCount
    {
        get => _ipScannerTabCount;
        set
        {
            if (value == _ipScannerTabCount)
                return;

            Debug.WriteLine("IPScanner current tabs: " + value);

            _ipScannerTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _portScannerTabCount;
    public int PortScannerTabCount
    {
        get => _portScannerTabCount;
        set
        {
            if (value == _portScannerTabCount)
                return;

            Debug.WriteLine("Port Scanner current tabs: " + value);

            _portScannerTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _tracerouteTabCount;
    public int TracerouteTabCount
    {
        get => _tracerouteTabCount;
        set
        {
            if (value == _tracerouteTabCount)
                return;

            Debug.WriteLine("Traceroute current tabs: " + value);

            _tracerouteTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _dnsLookupTabCount;
    public int DNSLookupTabCount
    {
        get => _dnsLookupTabCount;
        set
        {
            if (value == _dnsLookupTabCount)
                return;

            Debug.WriteLine("DNS Lookup current tabs: " + value);

            _dnsLookupTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _remoteDesktopTabCount;
    public int RemoteDesktopTabCount
    {
        get => _remoteDesktopTabCount;
        set
        {
            if (value == _remoteDesktopTabCount)
                return;

            Debug.WriteLine("Remote Desktop current tabs: " + value);

            _remoteDesktopTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _powerShellTabCount;
    public int PowerShellTabCount
    {
        get => _powerShellTabCount;
        set
        {
            if (value == _powerShellTabCount)
                return;

            Debug.WriteLine("PowerShell current tabs: " + value);

            _powerShellTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _puTTYTabCount;
    public int PuTTYTabCount
    {
        get => _puTTYTabCount;
        set
        {
            if (value == _puTTYTabCount)
                return;

            Debug.WriteLine("PuTTY current tabs: " + value);

            _puTTYTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _awsSessionManagerTabCount;
    public int AWSSessionManagerTabCount
    {
        get => _awsSessionManagerTabCount;
        set
        {
            if (value == _awsSessionManagerTabCount)
                return;

            Debug.WriteLine("AWS SSM current tabs: " + value);

            _awsSessionManagerTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _tigerVNCTabCount;
    public int TigerVNCTabCount
    {
        get => _tigerVNCTabCount;
        set
        {
            if (value == _tigerVNCTabCount)
                return;

            Debug.WriteLine("TigerVNC current tabs: " + value);

            _tigerVNCTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _webConsoleTabCount;
    public int WebConsoleTabCount
    {
        get => _webConsoleTabCount;
        set
        {
            if (value == _webConsoleTabCount)
                return;

            Debug.WriteLine("WebConsole current tabs: " + value);

            _webConsoleTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _snmpTabCount;
    public int SNMPTabCount
    {
        get => _snmpTabCount;
        set
        {
            if (value == _snmpTabCount)
                return;

            Debug.WriteLine("SNMP current tabs: " + value);

            _snmpTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _sntpLookupTabCount;
    public int SNTPLookupTabCount
    {
        get => _sntpLookupTabCount;
        set
        {
            if (value == _sntpLookupTabCount)
                return;

            Debug.WriteLine("SNTP Lookup current tabs: " + value);

            _sntpLookupTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _whoisTabCount;
    public int WhoisTabCount
    {
        get => _whoisTabCount;
        set
        {
            if (value == _whoisTabCount)
                return;

            Debug.WriteLine("Whois current tabs: " + value);

            _whoisTabCount = value;
            OnPropertyChanged();
        }
    }

    private int _ipGeolocationTabCount;
    public int IPGeolocationTabCount
    {
        get => _ipGeolocationTabCount;
        set
        {
            if (value == _ipGeolocationTabCount)
                return;

            Debug.WriteLine("IP Geolocation current tabs: " + value);

            _ipGeolocationTabCount = value;
            OnPropertyChanged();
        }
    }

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