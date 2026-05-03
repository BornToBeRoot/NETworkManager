using DnsClient;
using Lextm.SharpSnmpLib.Messaging;
using NETworkManager.Controls;
using NETworkManager.Models;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;
using NETworkManager.Models.PuTTY;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

// ReSharper disable InconsistentNaming

namespace NETworkManager.Settings;

/// <summary>
/// Represents the application settings, user preferences, and configuration data for all supported features and
/// modules. Supports property change notification for data binding and persistence scenarios.
/// </summary>
/// <remarks>The <see cref="SettingsInfo" /> class provides a centralized container for storing and managing user-configurable
/// options, operational parameters, and history collections for various application modules, such as network tools,
/// remote access, and calculators. It implements the INotifyPropertyChanged interface to enable data binding and
/// automatic UI updates when settings change. Most properties raise the PropertyChanged event when modified, allowing
/// consumers to track changes and persist settings as needed. This class is typically used as the main settings model
/// in applications that require user customization and state management across sessions.</remarks>
public class SettingsInfo : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    /// <remarks>This event is typically used to notify subscribers that a property value has been updated. It
    /// is commonly implemented in classes that support data binding or need to signal changes to property
    /// values.</remarks>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Helper method to raise the <see cref="PropertyChanged" /> event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        SettingsChanged = true;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Variables

    [JsonIgnore] public bool SettingsChanged { get; set; }

    /// <summary>
    /// Determines if the welcome dialog should be shown on application start.
    /// </summary>
    public bool WelcomeDialog_Show
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    /// <summary>
    /// Indicates if the update dialog should be shown on application start.
    /// Usually this is set to true if the application has been updated to a new version.
    /// </summary>
    public bool UpgradeDialog_Show
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
    /// Version of the settings file. Should be identical to the version of the application.
    /// It is used to determine if the settings file needs to be updated.
    /// </summary>
    public string Version
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
    /// Stores the date of the last backup of the settings file.
    /// </summary>
    public DateTime LastBackup
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = DateTime.MinValue;

    #region General

    // General   

    public ObservableSetCollection<ApplicationInfo> General_ApplicationList
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public int General_BackgroundJobInterval
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.General_BackgroundJobInterval;


    public int General_ThreadPoolAdditionalMinThreads
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.General_ThreadPoolAdditionalMinThreads;

    public int General_HistoryListEntries
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.General_HistoryListEntries;

    // Window

    public bool Window_ConfirmClose
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

    public bool Window_MinimizeInsteadOfTerminating
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

    public bool Window_MultipleInstances
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

    public bool Window_MinimizeToTrayInsteadOfTaskbar
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

    // TrayIcon

    public bool TrayIcon_AlwaysShowIcon
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

    // SplashScreen

    public bool SplashScreen_Enabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SplashScreen_Enabled;

    // Appearance

    public string Appearance_Theme
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Appearance_Theme;

    public string Appearance_Accent
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Appearance_Accent;

    public bool Appearance_UseCustomTheme
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Appearance_UseCustomTheme;

    public string Appearance_CustomThemeName
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

    public bool Appearance_PowerShellModifyGlobalProfile
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

    // Localization

    public string Localization_CultureCode
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

    // Network

    public bool Network_UseCustomDNSServer
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

    public string Network_CustomDNSServer
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

    public bool Network_ResolveHostnamePreferIPv4
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Network_ResolveHostnamePreferIPv4;

    // Status

    public bool Status_ShowWindowOnNetworkChange
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Status_ShowWindowOnNetworkChange;

    public int Status_WindowCloseTime
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Status_WindowCloseTime;

    // Autostart

    public bool Autostart_StartMinimizedInTray
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

    // HotKey

    public bool HotKey_ShowWindowEnabled
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

    public int HotKey_ShowWindowKey
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.HotKey_ShowWindowKey;

    public int HotKey_ShowWindowModifier
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.HotKey_ShowWindowModifier;

    // Update

    public bool Update_CheckForUpdatesAtStartup
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Update_CheckForUpdatesAtStartup;

    public bool Update_CheckForPreReleases
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Update_CheckForPreReleases;

    public bool Experimental_EnableExperimentalFeatures
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Experimental_EnableExperimentalFeatures;

    // Profiles

    public string Profiles_FolderLocation
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

    public string Profiles_LastSelected
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

    public bool Profiles_IsDailyBackupEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profiles_IsDailyBackupEnabled;

    public int Profiles_MaximumNumberOfBackups
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profiles_MaximumNumberOfBackups;

    // Settings

    public bool Settings_IsDailyBackupEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Settings_IsDailyBackupEnabled;

    public int Settings_MaximumNumberOfBackups
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Settings_MaximumNumberOfBackups;

    #endregion

    #region Others

    // Application view       

    public bool ExpandApplicationView
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

    #region Dashboard

    public string Dashboard_PublicIPv4Address
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Dashboard_PublicIPv4Address;

    public string Dashboard_PublicIPv6Address
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Dashboard_PublicIPv6Address;

    public bool Dashboard_CheckPublicIPAddress
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Dashboard_CheckPublicIPAddress;

    public bool Dashboard_UseCustomPublicIPv4AddressAPI
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

    public string Dashboard_CustomPublicIPv4AddressAPI
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

    public bool Dashboard_UseCustomPublicIPv6AddressAPI
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

    public string Dashboard_CustomPublicIPv6AddressAPI
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

    public bool Dashboard_CheckIPApiIPGeolocation
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Dashboard_CheckIPApiIPGeolocation;

    public bool Dashboard_CheckIPApiDNSResolver
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Dashboard_CheckIPApiDNSResolver;

    #endregion

    #region Network Interface

    public string NetworkInterface_InterfaceId
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

    public bool NetworkInterface_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double NetworkInterface_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public string NetworkInterface_ExportFilePath
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

    public ExportFileType NetworkInterface_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.NetworkInterface_ExportFileType;

    #endregion

    #region WiFi

    public string WiFi_InterfaceId
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

    public bool WiFi_Show2dot4GHzNetworks
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.WiFi_Show2dot4GHzNetworks;

    public bool WiFi_Show5GHzNetworks
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.WiFi_Show5GHzNetworks;

    public bool WiFi_Show6GHzNetworks
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.WiFi_Show6GHzNetworks;

    public bool WiFi_AutoRefreshEnabled
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

    public AutoRefreshTimeInfo WiFi_AutoRefreshTime
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.WiFi_AutoRefreshTime;

    public string WiFi_ExportFilePath
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

    public ExportFileType WiFi_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.WiFi_ExportFileType;

    #endregion

    #region IPScanner

    public bool IPScanner_ShowAllResults
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

    public int IPScanner_ICMPTimeout
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_ICMPTimeout;

    public int IPScanner_ICMPAttempts
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_ICMPAttempts;

    public int IPScanner_ICMPBuffer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_ICMPBuffer;

    public ObservableCollection<string> IPScanner_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public bool IPScanner_ResolveHostname
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_ResolveHostname;

    public bool IPScanner_PortScanEnabled
    {
        get;
        set
        {
            if (value == IPScanner_PortScanEnabled)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_PortScanEnabled;

    public string IPScanner_PortScanPorts
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_PortScanPorts;

    public int IPScanner_PortScanTimeout
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_PortScanTimeout;

    public bool IPScanner_NetBIOSEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_NetBIOSEnabled;

    public int IPScanner_NetBIOSTimeout
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_NetBIOSTimeout;

    public bool IPScanner_ResolveMACAddress
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

    public ObservableCollection<CustomCommandInfo> IPScanner_CustomCommands
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public int IPScanner_MaxHostThreads
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_MaxHostThreads;

    public int IPScanner_MaxPortThreads
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_MaxPortThreads;

    public bool IPScanner_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double IPScanner_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public string IPScanner_ExportFilePath
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

    public ExportFileType IPScanner_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPScanner_ExportFileType;

    #endregion

    #region Port Scanner

    public ObservableCollection<string> PortScanner_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<string> PortScanner_PortHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<PortProfileInfo> PortScanner_PortProfiles
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public bool PortScanner_ResolveHostname
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool PortScanner_ShowAllResults
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

    public int PortScanner_Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PortScanner_Timeout;

    public int PortScanner_MaxHostThreads
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PortScanner_MaxHostThreads;

    public int PortScanner_MaxPortThreads
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PortScanner_MaxPortThreads;

    public bool PortScanner_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double PortScanner_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public string PortScanner_ExportFilePath
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

    public ExportFileType PortScanner_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PortScanner_ExportFileType;

    #endregion

    #region Ping Monitor

    public ObservableCollection<string> PingMonitor_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public int PingMonitor_Buffer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PingMonitor_Buffer;

    public bool PingMonitor_DontFragment
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PingMonitor_DontFragment;

    public int PingMonitor_Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PingMonitor_Timeout;

    public int PingMonitor_TTL
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PingMonitor_TTL;

    public int PingMonitor_WaitTime
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PingMonitor_WaitTime;

    public bool PingMonitor_ExpandHostView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PingMonitor_ExpandHostView;

    public string PingMonitor_ExportFilePath
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

    public ExportFileType PingMonitor_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PingMonitor_ExportFileType;

    public bool PingMonitor_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double PingMonitor_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    #endregion

    #region Traceroute

    public ObservableCollection<string> Traceroute_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public int Traceroute_MaximumHops
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Traceroute_MaximumHops;

    public int Traceroute_Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Traceroute_Timeout;

    public int Traceroute_Buffer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Traceroute_Buffer;

    public bool Traceroute_ResolveHostname
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Traceroute_ResolveHostname;

    public bool Traceroute_CheckIPApiIPGeolocation
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Traceroute_CheckIPApiIPGeolocation;

    public bool Traceroute_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double Traceroute_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public string Traceroute_ExportFilePath
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

    public ExportFileType Traceroute_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Traceroute_ExportFileType;

    #endregion

    #region DNS Lookup

    public ObservableCollection<string> DNSLookup_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<DNSServerConnectionInfoProfile> DNSLookup_DNSServers
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    [Obsolete("Use DNSLookup_SelectedDNSServer_v2 instead.")]
    [field: Obsolete("Use DNSLookup_SelectedDNSServer_v2 instead.")]
    public DNSServerConnectionInfoProfile DNSLookup_SelectedDNSServer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = null;

    public string DNSLookup_SelectedDNSServer_v2
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

    public QueryClass DNSLookup_QueryClass
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.DNSLookup_QueryClass;

    public QueryType DNSLookup_QueryType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.DNSLookup_QueryType;

    public bool DNSLookup_AddDNSSuffix
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool DNSLookup_UseCustomDNSSuffix
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

    public string DNSLookup_CustomDNSSuffix
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

    public bool DNSLookup_Recursion
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool DNSLookup_UseCache
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

    public bool DNSLookup_UseTCPOnly
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.DNSLookup_UseTCPOnly;

    public int DNSLookup_Retries
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.DNSLookup_Retries;

    public int DNSLookup_Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.DNSLookup_Timeout;

    public bool DNSLookup_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double DNSLookup_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public string DNSLookup_ExportFilePath
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

    public ExportFileType DNSLookup_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.DNSLookup_ExportFileType;

    #endregion

    #region Remote Desktop

    public ObservableCollection<string> RemoteDesktop_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public bool RemoteDesktop_AdjustScreenAutomatically
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

    public bool RemoteDesktop_UseCurrentViewSize
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_UseCurrentViewSize;

    public bool RemoteDesktop_UseFixedScreenSize
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

    public int RemoteDesktop_ScreenWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_ScreenWidth;

    public int RemoteDesktop_ScreenHeight
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_ScreenHeight;

    public bool RemoteDesktop_UseCustomScreenSize
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

    public int RemoteDesktop_CustomScreenWidth
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

    public int RemoteDesktop_CustomScreenHeight
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

    public int RemoteDesktop_ColorDepth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_ColorDepth;

    public int RemoteDesktop_Port
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_Port;

    public bool RemoteDesktop_EnableCredSspSupport
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_EnableCredSspSupport;

    public uint RemoteDesktop_AuthenticationLevel
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_AuthenticationLevel;

    public bool RemoteDesktop_EnableGatewayServer
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

    public string RemoteDesktop_GatewayServerHostname
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

    public bool RemoteDesktop_GatewayServerBypassLocalAddresses
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_GatewayServerBypassLocalAddresses;

    public GatewayUserSelectedCredsSource RemoteDesktop_GatewayServerLogonMethod
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_GatewayServerLogonMethod;

    public bool RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer;

    public AudioRedirectionMode RemoteDesktop_AudioRedirectionMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_AudioRedirectionMode;

    public AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_AudioCaptureRedirectionMode;

    public KeyboardHookMode RemoteDesktop_KeyboardHookMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_KeyboardHookMode;

    public bool RemoteDesktop_RedirectClipboard
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_RedirectClipboard;

    public bool RemoteDesktop_RedirectDevices
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

    public bool RemoteDesktop_RedirectDrives
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

    public bool RemoteDesktop_RedirectPorts
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

    public bool RemoteDesktop_RedirectSmartCards
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

    public bool RemoteDesktop_RedirectPrinters
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

    public bool RemoteDesktop_PersistentBitmapCaching
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

    public bool RemoteDesktop_ReconnectIfTheConnectionIsDropped
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

    public NetworkConnectionType RemoteDesktop_NetworkConnectionType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.RemoteDesktop_NetworkConnectionType;

    public bool RemoteDesktop_DesktopBackground
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

    public bool RemoteDesktop_FontSmoothing
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

    public bool RemoteDesktop_DesktopComposition
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

    public bool RemoteDesktop_ShowWindowContentsWhileDragging
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

    public bool RemoteDesktop_MenuAndWindowAnimation
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

    public bool RemoteDesktop_VisualStyles
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

    public bool RemoteDesktop_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double RemoteDesktop_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    #endregion

    #region PowerShell

    public ObservableCollection<string> PowerShell_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string PowerShell_ApplicationFilePath
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

    public string PowerShell_Command
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PowerShell_Command;

    public string PowerShell_AdditionalCommandLine
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

    public ExecutionPolicy PowerShell_ExecutionPolicy
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PowerShell_ExecutionPolicy;

    public bool PowerShell_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double PowerShell_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    #endregion

    #region PuTTY

    public ObservableCollection<string> PuTTY_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ConnectionMode PuTTY_DefaultConnectionMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_DefaultConnectionMode;

    public string PuTTY_Username
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

    public string PuTTY_PrivateKeyFile
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

    public string PuTTY_Profile
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_DefaultProfile;

    public bool PuTTY_EnableSessionLog
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

    public LogMode PuTTY_LogMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_LogMode;

    public string PuTTY_LogPath
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_LogPath;

    public string PuTTY_LogFileName
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_LogFileName;

    public string PuTTY_AdditionalCommandLine
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

    public ObservableCollection<string> PuTTY_SerialLineHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<string> PuTTY_PortHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<string> PuTTY_BaudHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<string> PuTTY_UsernameHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<string> PuTTY_PrivateKeyFileHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<string> PuTTY_ProfileHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public bool PuTTY_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double PuTTY_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public string PuTTY_ApplicationFilePath
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

    public string PuTTY_SerialLine
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_SerialLine;

    public int PuTTY_SSHPort
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_SSHPort;

    public int PuTTY_TelnetPort
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_TelnetPort;


    public int PuTTY_BaudRate
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_BaudRate;

    public int PuTTY_RloginPort
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_RloginPort;

    public int PuTTY_RawPort
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.PuTTY_RawPort;

    #endregion

    #region TigerVNC

    public ObservableCollection<string> TigerVNC_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<int> TigerVNC_PortHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public bool TigerVNC_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double TigerVNC_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public string TigerVNC_ApplicationFilePath
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

    public int TigerVNC_Port
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.TigerVNC_DefaultVNCPort;

    #endregion

    #region Web Console

    public ObservableCollection<string> WebConsole_UrlHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public bool WebConsole_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double WebConsole_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public bool WebConsole_ShowAddressBar
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.WebConsole_ShowAddressBar;

    public bool WebConsole_IsStatusBarEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.WebConsole_IsStatusBarEnabled;

    public bool WebConsole_IsPasswordSaveEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.WebConsole_IsPasswordSaveEnabled;

    #endregion

    #region SNMP

    public ObservableCollection<string> SNMP_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<string> SNMP_OidHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<SNMPOIDProfileInfo> SNMP_OidProfiles
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public int SNMP_Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SNMP_Timeout;

    public WalkMode SNMP_WalkMode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SNMP_WalkMode;

    public int SNMP_Port
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = 161;

    public SNMPMode SNMP_Mode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SNMP_Mode;

    public SNMPVersion SNMP_Version
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SNMP_Version;

    public SNMPV3Security SNMP_Security
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SNMP_Security;

    public SNMPV3AuthenticationProvider SNMP_AuthenticationProvider
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SNMP_AuthenticationProvider;

    public SNMPV3PrivacyProvider SNMP_PrivacyProvider
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SNMP_PrivacyProvider;

    public bool SNMP_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double SNMP_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;


    public string SNMP_ExportFilePath
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

    public ExportFileType SNMP_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SNMP_ExportFileType;

    #endregion

    #region SNTP Lookup

    public ObservableCollection<ServerConnectionInfoProfile> SNTPLookup_SNTPServers
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ServerConnectionInfoProfile SNTPLookup_SelectedSNTPServer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    public int SNTPLookup_Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SNTPLookup_Timeout;

    public string SNTPLookup_ExportFilePath
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

    public ExportFileType SNTPLookup_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SNTPLookup_ExportFileType;

    #endregion

    #region Hosts File Editor

    public string HostsFileEditor_ExportFilePath
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

    public ExportFileType HostsFileEditor_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.HostsFileEditor_ExportFileType;

    #endregion

    #region Firewall
    public bool Firewall_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double Firewall_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public string Firewall_ExportFilePath
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

    public ExportFileType Firewall_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Firewall_ExportFileType;

    #endregion

    #region Discovery Protocol

    public DiscoveryProtocol DiscoveryProtocol_Protocol
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.DiscoveryProtocol_Protocol;

    public int DiscoveryProtocol_Duration
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.DiscoveryProtocol_Duration;

    public string DiscoveryProtocol_ExportFilePath
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

    public ExportFileType DiscoveryProtocol_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.DiscoveryProtocol_ExportFileType;

    #endregion

    #region WakeOnLAN

    public int WakeOnLAN_Port
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.WakeOnLAN_Port;

    public ObservableCollection<string> WakeOnLan_MACAddressHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<string> WakeOnLan_BroadcastHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public bool WakeOnLAN_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double WakeOnLAN_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    #endregion

    #region Whois

    public ObservableCollection<string> Whois_DomainHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public bool Whois_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double Whois_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public string Whois_ExportFilePath
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

    public ExportFileType Whois_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Whois_ExportFileType;

    #endregion

    #region IP Geolocation

    public ObservableCollection<string> IPGeolocation_HostHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public bool IPGeolocation_ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_ExpandProfileView;

    public double IPGeolocation_ProfileWidth
    {
        get;
        set
        {
            if (Math.Abs(value - field) < GlobalStaticConfiguration.Profile_FloatPointFix)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;

    public string IPGeolocation_ExportFilePath
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

    public ExportFileType IPGeolocation_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.IPGeolocation_ExportFileType;

    #endregion

    #region Subnet Calculator

    #region Calculator

    public ObservableCollection<string> SubnetCalculator_Calculator_SubnetHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    #endregion

    #region Subnetting

    public ObservableCollection<string> SubnetCalculator_Subnetting_SubnetHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<string> SubnetCalculator_Subnetting_NewSubnetmaskHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string SubnetCalculator_Subnetting_ExportFilePath
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

    public ExportFileType SubnetCalculator_Subnetting_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.SubnetCalculator_Subnetting_ExportFileType;

    #endregion

    #region WideSubnet

    public ObservableCollection<string> SubnetCalculator_WideSubnet_Subnet1
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<string> SubnetCalculator_WideSubnet_Subnet2
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    #endregion

    #endregion

    #region Bit Calculator

    public ObservableCollection<string> BitCalculator_InputHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public BitCaluclatorUnit BitCalculator_Unit
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.BitCalculator_Unit;


    public BitCaluclatorNotation BitCalculator_Notation
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.BitCalculator_Notation;

    public string BitCalculator_ExportFilePath
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

    public ExportFileType BitCalculator_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.BitCalculator_ExportFileType;

    #endregion

    #region Lookup

    public ObservableCollection<string> Lookup_OUI_SearchHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string Lookup_OUI_ExportFilePath
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

    public ExportFileType Lookup_OUI_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Lookup_OUI_ExportFileType;

    public ObservableCollection<string> Lookup_Port_SearchHistory
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string Lookup_Port_ExportFilePath
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

    public ExportFileType Lookup_Port_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Lookup_Port_ExportFileType;

    #endregion

    #region Connections

    public bool Connections_AutoRefreshEnabled
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

    public AutoRefreshTimeInfo Connections_AutoRefreshTime
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Connections_AutoRefreshTime;

    public string Connections_ExportFilePath
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

    public ExportFileType Connections_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Connections_ExportFileType;

    #endregion

    #region Listeners

    public bool Listeners_AutoRefreshEnabled
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

    public AutoRefreshTimeInfo Listeners_AutoRefreshTime
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Listeners_AutoRefreshTime;

    public string Listeners_ExportFilePath
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

    public ExportFileType Listeners_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Listeners_ExportFileType;

    #endregion

    #region NeighborTable

    public bool NeighborTable_AutoRefreshEnabled
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

    public AutoRefreshTimeInfo NeighborTable_AutoRefreshTime
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.NeighborTable_AutoRefreshTime;

    public string NeighborTable_ExportFilePath
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

    public ExportFileType NeighborTable_ExportFileType
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.NeighborTable_ExportFileType;

    public string NeighborTable_InterfaceName
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

    #endregion

    #region Constructor

    public SettingsInfo()
    {
        // General
        General_ApplicationList.CollectionChanged += CollectionChanged;

        // IP Scanner
        IPScanner_HostHistory.CollectionChanged += CollectionChanged;
        IPScanner_CustomCommands.CollectionChanged += CollectionChanged;

        // Port Scanner
        PortScanner_HostHistory.CollectionChanged += CollectionChanged;
        PortScanner_PortHistory.CollectionChanged += CollectionChanged;
        PortScanner_PortProfiles.CollectionChanged += CollectionChanged;

        // Ping Monitor
        PingMonitor_HostHistory.CollectionChanged += CollectionChanged;

        // Traceroute
        Traceroute_HostHistory.CollectionChanged += CollectionChanged;

        // DNS Lookup
        DNSLookup_HostHistory.CollectionChanged += CollectionChanged;
        DNSLookup_DNSServers.CollectionChanged += CollectionChanged;

        // Remote Desktop
        RemoteDesktop_HostHistory.CollectionChanged += CollectionChanged;

        // PowerShell
        PowerShell_HostHistory.CollectionChanged += CollectionChanged;

        // PuTTY
        PuTTY_HostHistory.CollectionChanged += CollectionChanged;
        PuTTY_SerialLineHistory.CollectionChanged += CollectionChanged;
        PuTTY_PortHistory.CollectionChanged += CollectionChanged;
        PuTTY_BaudHistory.CollectionChanged += CollectionChanged;
        PuTTY_UsernameHistory.CollectionChanged += CollectionChanged;
        PuTTY_PrivateKeyFileHistory.CollectionChanged += CollectionChanged;
        PuTTY_ProfileHistory.CollectionChanged += CollectionChanged;

        // TigerVNC
        TigerVNC_HostHistory.CollectionChanged += CollectionChanged;
        TigerVNC_PortHistory.CollectionChanged += CollectionChanged;

        // WebConsole
        WebConsole_UrlHistory.CollectionChanged += CollectionChanged;

        // SNMP
        SNMP_HostHistory.CollectionChanged += CollectionChanged;
        SNMP_OidHistory.CollectionChanged += CollectionChanged;
        SNMP_OidProfiles.CollectionChanged += CollectionChanged;

        // SNTP Lookup
        SNTPLookup_SNTPServers.CollectionChanged += CollectionChanged;

        // Wake on LAN
        WakeOnLan_MACAddressHistory.CollectionChanged += CollectionChanged;
        WakeOnLan_BroadcastHistory.CollectionChanged += CollectionChanged;

        // Whois
        Whois_DomainHistory.CollectionChanged += CollectionChanged;

        // IP Geolocation
        IPGeolocation_HostHistory.CollectionChanged += CollectionChanged;

        // Subnet Calculator > Calculator
        SubnetCalculator_Calculator_SubnetHistory.CollectionChanged += CollectionChanged;

        // Subnet Calculator > Subnetting
        SubnetCalculator_Subnetting_SubnetHistory.CollectionChanged += CollectionChanged;
        SubnetCalculator_Subnetting_NewSubnetmaskHistory.CollectionChanged += CollectionChanged;

        // Subnet Calculator > Supernetting
        SubnetCalculator_WideSubnet_Subnet1.CollectionChanged += CollectionChanged;
        SubnetCalculator_WideSubnet_Subnet2.CollectionChanged += CollectionChanged;

        // Bit Calculator
        BitCalculator_InputHistory.CollectionChanged += CollectionChanged;

        // Lookup > OUI
        Lookup_OUI_SearchHistory.CollectionChanged += CollectionChanged;

        // Lookup > Port
        Lookup_Port_SearchHistory.CollectionChanged += CollectionChanged;
    }

    private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        SettingsChanged = true;
    }

    #endregion
}