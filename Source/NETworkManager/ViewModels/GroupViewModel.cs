using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;
using NETworkManager.Models.PuTTY;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Profiles;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Windows.Data;
using System.Windows.Input;

// ReSharper disable InconsistentNaming

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for a profile group.
/// </summary>
public class GroupViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading = true;

    /// <summary>
    /// Gets a value indicating whether the profile file is encrypted.
    /// </summary>
    public bool IsProfileFileEncrypted => ProfileManager.LoadedProfileFile.IsEncrypted;

    /// <summary>
    /// Gets the collection view of group views.
    /// </summary>
    public ICollectionView GroupViews { get; }

    /// <summary>
    /// Gets the group info.
    /// </summary>
    public GroupInfo Group { get; }

    /// <summary>
    /// The list of existing group names.
    /// </summary>
    private IReadOnlyCollection<string> _groups { get; }

    #region General

    /// <summary>
    /// Gets or sets a value indicating whether the name is valid.
    /// </summary>
    public bool NameIsValid
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
    /// Gets or sets the name of the group.
    /// </summary>
    public string Name
    {
        get;
        set
        {
            if (value == field)
                return;

            // Check name for duplicate...
            if (_groups.Contains(value, StringComparer.OrdinalIgnoreCase) &&
                !value.Equals(Group.Name, StringComparison.OrdinalIgnoreCase))
                NameIsValid = false;
            else
                NameIsValid = true;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the description of the group.
    /// </summary>
    public string Description
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

    #region Remote Desktop

    /// <summary>
    /// Gets or sets a value indicating whether to use credentials for Remote Desktop.
    /// </summary>
    public bool RemoteDesktop_UseCredentials
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
    /// Gets or sets the username for Remote Desktop.
    /// </summary>
    public string RemoteDesktop_Username
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
    /// Gets or sets the domain for Remote Desktop.
    /// </summary>
    public string RemoteDesktop_Domain
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

    public bool RemoteDesktop_IsPasswordEmpty
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

    public SecureString RemoteDesktop_Password
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the password string
            RemoteDesktop_IsPasswordEmpty =
                value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_AdminSession
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

    public bool RemoteDesktop_OverrideDisplay
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
    }

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

    public IEnumerable<string> RemoteDesktop_ScreenResolutions => RemoteDesktop.ScreenResolutions;

    public int RemoteDesktop_ScreenWidth;
    public int RemoteDesktop_ScreenHeight;

    public string RemoteDesktop_SelectedScreenResolution
    {
        get;
        set
        {
            if (value == field)
                return;

            var resolution = value.Split('x');

            RemoteDesktop_ScreenWidth = int.Parse(resolution[0]);
            RemoteDesktop_ScreenHeight = int.Parse(resolution[1]);

            field = value;
            OnPropertyChanged();
        }
    }

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

    public string RemoteDesktop_CustomScreenWidth
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

    public string RemoteDesktop_CustomScreenHeight
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

    public bool RemoteDesktop_OverrideColorDepth
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

    public List<int> RemoteDesktop_ColorDepths => RemoteDesktop.ColorDepths;

    public int RemoteDesktop_SelectedColorDepth
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

    public bool RemoteDesktop_OverridePort
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
    }

    public bool RemoteDesktop_OverrideCredSspSupport
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
    }

    public bool RemoteDesktop_OverrideAuthenticationLevel
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
    }

    public bool RemoteDesktop_OverrideGatewayServer
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
    }

    public IEnumerable<GatewayUserSelectedCredsSource> RemoteDesktop_GatewayServerLogonMethods =>
        Enum.GetValues(typeof(GatewayUserSelectedCredsSource)).Cast<GatewayUserSelectedCredsSource>();

    public GatewayUserSelectedCredsSource RemoteDesktop_GatewayServerLogonMethod
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

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
    }

    public bool RemoteDesktop_UseGatewayServerCredentials
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

    public string RemoteDesktop_GatewayServerUsername
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

    public string RemoteDesktop_GatewayServerDomain
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

    public bool RemoteDesktop_IsGatewayServerPasswordEmpty
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

    public SecureString RemoteDesktop_GatewayServerPassword
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the password string
            RemoteDesktop_IsGatewayServerPasswordEmpty =
                value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideAudioRedirectionMode
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

    public IEnumerable<AudioRedirectionMode> RemoteDesktop_AudioRedirectionModes =>
        Enum.GetValues(typeof(AudioRedirectionMode)).Cast<AudioRedirectionMode>();

    public AudioRedirectionMode RemoteDesktop_AudioRedirectionMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }


    public bool RemoteDesktop_OverrideAudioCaptureRedirectionMode
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

    public IEnumerable<AudioCaptureRedirectionMode> RemoteDesktop_AudioCaptureRedirectionModes =>
        Enum.GetValues(typeof(AudioCaptureRedirectionMode)).Cast<AudioCaptureRedirectionMode>();

    public AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }


    public bool RemoteDesktop_OverrideApplyWindowsKeyCombinations
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

    public IEnumerable<KeyboardHookMode> RemoteDesktop_KeyboardHookModes =>
        Enum.GetValues(typeof(KeyboardHookMode)).Cast<KeyboardHookMode>();

    public KeyboardHookMode RemoteDesktop_KeyboardHookMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoteDesktop_OverrideRedirectClipboard
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
    }

    public bool RemoteDesktop_OverrideRedirectDevices
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

    public bool RemoteDesktop_OverrideRedirectDrives
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

    public bool RemoteDesktop_OverrideRedirectPorts
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

    public bool RemoteDesktop_OverrideRedirectSmartcards
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

    public bool RemoteDesktop_OverrideRedirectPrinters
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

    public bool RemoteDesktop_OverridePersistentBitmapCaching
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

    public bool RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped
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

    public bool RemoteDesktop_OverrideNetworkConnectionType
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

    public IEnumerable<NetworkConnectionType> RemoteDesktop_NetworkConnectionTypes =>
        Enum.GetValues(typeof(NetworkConnectionType)).Cast<NetworkConnectionType>();

    public NetworkConnectionType RemoteDesktop_NetworkConnectionType
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            if (!_isLoading)
                ChangeNetworkConnectionTypeSettings(value);

            field = value;
            OnPropertyChanged();
        }
    }

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

    #endregion

    #region PowerShell

    public bool PowerShell_OverrideCommand
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
    }

    public bool PowerShell_OverrideAdditionalCommandLine
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

    public bool PowerShell_OverrideExecutionPolicy
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

    public IEnumerable<ExecutionPolicy> PowerShell_ExecutionPolicies { get; set; }

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
    }

    #endregion

    #region PuTTY

    public bool PuTTY_OverrideUsername
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

    public bool PuTTY_OverridePrivateKeyFile
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

    public bool PuTTY_OverrideProfile
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
    }

    public bool PuTTY_OverrideEnableLog
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

    public bool PuTTY_EnableLog
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

    public bool PuTTY_OverrideLogMode
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

    public IEnumerable<LogMode> PuTTY_LogModes => Enum.GetValues(typeof(LogMode)).Cast<LogMode>();

    public LogMode PuTTY_LogMode
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool PuTTY_OverrideLogPath
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
    }

    public bool PuTTY_OverrideLogFileName
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
    }

    public bool PuTTY_OverrideAdditionalCommandLine
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

    public ConnectionMode PuTTY_ConnectionMode
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

    #region TigerVNC

    public bool TigerVNC_OverridePort
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
    }

    #endregion

    #region SNMP

    public bool SNMP_OverrideOIDAndMode
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

    public string SNMP_OID
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

    public IEnumerable<SNMPMode> SNMP_Modes { get; set; }

    public SNMPMode SNMP_Mode
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();

            // Re-validate OID if mode changed
            OnPropertyChanged(nameof(SNMP_OID));
        }
    }

    public bool SNMP_OverrideVersionAndAuth
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

    public IEnumerable<SNMPVersion> SNMP_Versions { get; }

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
    }

    public bool SNMP_IsCommunityEmpty
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

    public SecureString SNMP_Community
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the password string
            SNMP_IsCommunityEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3Security> SNMP_Securities { get; }

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
    }

    public string SNMP_Username
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

    public IEnumerable<SNMPV3AuthenticationProvider> SNMP_AuthenticationProviders { get; }

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
    }

    public bool SNMP_IsAuthEmpty
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

    public SecureString SNMP_Auth
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the password string
            SNMP_IsAuthEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3PrivacyProvider> SNMP_PrivacyProviders { get; }

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
    }

    public bool SNMP_IsPrivEmpty
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

    public SecureString SNMP_Priv
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the password string
            SNMP_IsPrivEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region Constructor

    public GroupViewModel(Action<GroupViewModel> saveCommand, Action<GroupViewModel> cancelHandler,
        IReadOnlyCollection<string> groups, GroupEditMode editMode = GroupEditMode.Add, GroupInfo group = null)
    {
        // Load the view
        GroupViews = new CollectionViewSource { Source = GroupViewManager.List }.View;
        GroupViews.SortDescriptions.Add(new SortDescription(nameof(GroupViewInfo.Name), ListSortDirection.Ascending));

        SaveCommand = new RelayCommand(_ => saveCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        var groupInfo = group ?? new GroupInfo();

        Group = groupInfo;
        _groups = groups;

        // General
        Name = groupInfo.Name;
        Description = groupInfo.Description;

        // Remote Desktop
        RemoteDesktop_UseCredentials = groupInfo.RemoteDesktop_UseCredentials;
        RemoteDesktop_Username = groupInfo.RemoteDesktop_Username;
        RemoteDesktop_Domain = groupInfo.RemoteDesktop_Domain;
        RemoteDesktop_Password = groupInfo.RemoteDesktop_Password;
        RemoteDesktop_AdminSession = groupInfo.RemoteDesktop_AdminSession;
        RemoteDesktop_OverrideDisplay = groupInfo.RemoteDesktop_OverrideDisplay;
        RemoteDesktop_AdjustScreenAutomatically = groupInfo.RemoteDesktop_AdjustScreenAutomatically;
        RemoteDesktop_UseCurrentViewSize = groupInfo.RemoteDesktop_UseCurrentViewSize;
        RemoteDesktop_UseFixedScreenSize = groupInfo.RemoteDesktop_UseFixedScreenSize;
        RemoteDesktop_SelectedScreenResolution = RemoteDesktop_ScreenResolutions.FirstOrDefault(x =>
            x == $"{groupInfo.RemoteDesktop_ScreenWidth}x{groupInfo.RemoteDesktop_ScreenHeight}");
        RemoteDesktop_UseCustomScreenSize = groupInfo.RemoteDesktop_UseCustomScreenSize;
        RemoteDesktop_CustomScreenWidth = groupInfo.RemoteDesktop_CustomScreenWidth.ToString();
        RemoteDesktop_CustomScreenHeight = groupInfo.RemoteDesktop_CustomScreenHeight.ToString();
        RemoteDesktop_OverrideColorDepth = groupInfo.RemoteDesktop_OverrideColorDepth;
        RemoteDesktop_SelectedColorDepth =
            RemoteDesktop_ColorDepths.FirstOrDefault(x => x == groupInfo.RemoteDesktop_ColorDepth);
        RemoteDesktop_OverridePort = groupInfo.RemoteDesktop_OverridePort;
        RemoteDesktop_Port = groupInfo.RemoteDesktop_Port;
        RemoteDesktop_OverrideCredSspSupport = groupInfo.RemoteDesktop_OverrideCredSspSupport;
        RemoteDesktop_EnableCredSspSupport = groupInfo.RemoteDesktop_EnableCredSspSupport;
        RemoteDesktop_OverrideAuthenticationLevel = groupInfo.RemoteDesktop_OverrideAuthenticationLevel;
        RemoteDesktop_AuthenticationLevel = groupInfo.RemoteDesktop_AuthenticationLevel;
        RemoteDesktop_OverrideGatewayServer = groupInfo.RemoteDesktop_OverrideGatewayServer;
        RemoteDesktop_EnableGatewayServer = groupInfo.RemoteDesktop_EnableGatewayServer;
        RemoteDesktop_GatewayServerHostname = groupInfo.RemoteDesktop_GatewayServerHostname;
        RemoteDesktop_GatewayServerBypassLocalAddresses = groupInfo.RemoteDesktop_GatewayServerBypassLocalAddresses;
        RemoteDesktop_GatewayServerLogonMethod = groupInfo.RemoteDesktop_GatewayServerLogonMethod;
        RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer =
            groupInfo.RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer;
        RemoteDesktop_UseGatewayServerCredentials = groupInfo.RemoteDesktop_UseGatewayServerCredentials;
        RemoteDesktop_GatewayServerUsername = groupInfo.RemoteDesktop_GatewayServerUsername;
        RemoteDesktop_GatewayServerDomain = groupInfo.RemoteDesktop_GatewayServerDomain;
        RemoteDesktop_GatewayServerPassword = groupInfo.RemoteDesktop_GatewayServerPassword;
        RemoteDesktop_OverrideAudioRedirectionMode = groupInfo.RemoteDesktop_OverrideAudioRedirectionMode;
        RemoteDesktop_AudioRedirectionMode =
            RemoteDesktop_AudioRedirectionModes.FirstOrDefault(x => x == groupInfo.RemoteDesktop_AudioRedirectionMode);
        RemoteDesktop_OverrideAudioCaptureRedirectionMode = groupInfo.RemoteDesktop_OverrideAudioCaptureRedirectionMode;
        RemoteDesktop_AudioCaptureRedirectionMode =
            RemoteDesktop_AudioCaptureRedirectionModes.FirstOrDefault(x =>
                x == groupInfo.RemoteDesktop_AudioCaptureRedirectionMode);
        RemoteDesktop_OverrideApplyWindowsKeyCombinations = groupInfo.RemoteDesktop_OverrideApplyWindowsKeyCombinations;
        RemoteDesktop_KeyboardHookMode =
            RemoteDesktop_KeyboardHookModes.FirstOrDefault(x => x == groupInfo.RemoteDesktop_KeyboardHookMode);
        RemoteDesktop_OverrideRedirectClipboard = groupInfo.RemoteDesktop_OverrideRedirectClipboard;
        RemoteDesktop_RedirectClipboard = groupInfo.RemoteDesktop_RedirectClipboard;
        RemoteDesktop_OverrideRedirectDevices = groupInfo.RemoteDesktop_OverrideRedirectDevices;
        RemoteDesktop_RedirectDevices = groupInfo.RemoteDesktop_RedirectDevices;
        RemoteDesktop_OverrideRedirectDrives = groupInfo.RemoteDesktop_OverrideRedirectDrives;
        RemoteDesktop_RedirectDrives = groupInfo.RemoteDesktop_RedirectDrives;
        RemoteDesktop_OverrideRedirectPorts = groupInfo.RemoteDesktop_OverrideRedirectPorts;
        RemoteDesktop_RedirectPorts = groupInfo.RemoteDesktop_RedirectPorts;
        RemoteDesktop_OverrideRedirectSmartcards = groupInfo.RemoteDesktop_OverrideRedirectSmartcards;
        RemoteDesktop_RedirectSmartCards = groupInfo.RemoteDesktop_RedirectSmartCards;
        RemoteDesktop_OverrideRedirectPrinters = groupInfo.RemoteDesktop_OverrideRedirectPrinters;
        RemoteDesktop_RedirectPrinters = groupInfo.RemoteDesktop_RedirectPrinters;
        RemoteDesktop_OverridePersistentBitmapCaching = groupInfo.RemoteDesktop_OverridePersistentBitmapCaching;
        RemoteDesktop_PersistentBitmapCaching = groupInfo.RemoteDesktop_PersistentBitmapCaching;
        RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped =
            groupInfo.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped;
        RemoteDesktop_ReconnectIfTheConnectionIsDropped = groupInfo.RemoteDesktop_ReconnectIfTheConnectionIsDropped;
        RemoteDesktop_NetworkConnectionType =
            RemoteDesktop_NetworkConnectionTypes.FirstOrDefault(x =>
                x == groupInfo.RemoteDesktop_NetworkConnectionType);
        RemoteDesktop_DesktopBackground = groupInfo.RemoteDesktop_DesktopBackground;
        RemoteDesktop_FontSmoothing = groupInfo.RemoteDesktop_FontSmoothing;
        RemoteDesktop_DesktopComposition = groupInfo.RemoteDesktop_DesktopComposition;
        RemoteDesktop_ShowWindowContentsWhileDragging = groupInfo.RemoteDesktop_ShowWindowContentsWhileDragging;
        RemoteDesktop_MenuAndWindowAnimation = groupInfo.RemoteDesktop_MenuAndWindowAnimation;
        RemoteDesktop_VisualStyles = groupInfo.RemoteDesktop_VisualStyles;

        // PowerShell
        PowerShell_OverrideCommand = groupInfo.PowerShell_OverrideCommand;
        PowerShell_Command = groupInfo.PowerShell_Command;
        PowerShell_OverrideAdditionalCommandLine = groupInfo.PowerShell_OverrideAdditionalCommandLine;
        PowerShell_AdditionalCommandLine = groupInfo.PowerShell_AdditionalCommandLine;
        PowerShell_OverrideExecutionPolicy = groupInfo.PowerShell_OverrideExecutionPolicy;
        PowerShell_ExecutionPolicies = Enum.GetValues(typeof(ExecutionPolicy)).Cast<ExecutionPolicy>().ToList();
        PowerShell_ExecutionPolicy =
            PowerShell_ExecutionPolicies.FirstOrDefault(x => x == groupInfo.PowerShell_ExecutionPolicy);

        // PuTTY
        PuTTY_OverrideUsername = groupInfo.PuTTY_OverrideUsername;
        PuTTY_Username = groupInfo.PuTTY_Username;
        PuTTY_OverridePrivateKeyFile = groupInfo.PuTTY_OverridePrivateKeyFile;
        PuTTY_PrivateKeyFile = groupInfo.PuTTY_PrivateKeyFile;
        PuTTY_OverrideProfile = groupInfo.PuTTY_OverrideProfile;
        PuTTY_Profile = groupInfo.PuTTY_Profile;
        PuTTY_OverrideEnableLog = groupInfo.PuTTY_OverrideEnableLog;
        PuTTY_EnableLog = groupInfo.PuTTY_EnableLog;
        PuTTY_OverrideLogMode = groupInfo.PuTTY_OverrideLogMode;
        PuTTY_LogMode = PuTTY_LogModes.FirstOrDefault(x => x == groupInfo.PuTTY_LogMode);
        PuTTY_OverrideLogPath = groupInfo.PuTTY_OverrideLogPath;
        PuTTY_LogPath = groupInfo.PuTTY_LogPath;
        PuTTY_OverrideLogFileName = groupInfo.PuTTY_OverrideLogFileName;
        PuTTY_LogFileName = groupInfo.PuTTY_LogFileName;
        PuTTY_OverrideAdditionalCommandLine = groupInfo.PuTTY_OverrideAdditionalCommandLine;
        PuTTY_AdditionalCommandLine = groupInfo.PuTTY_AdditionalCommandLine;

        // TigerVNC
        TigerVNC_OverridePort = groupInfo.TigerVNC_OverridePort;
        TigerVNC_Port = groupInfo.TigerVNC_Port;

        // SNMP
        SNMP_OverrideOIDAndMode = groupInfo.SNMP_OverrideOIDAndMode;
        SNMP_OID = groupInfo.SNMP_OID;
        SNMP_Modes = [SNMPMode.Get, SNMPMode.Walk, SNMPMode.Set];
        SNMP_Mode = SNMP_Modes.FirstOrDefault(x => x == groupInfo.SNMP_Mode);
        SNMP_OverrideVersionAndAuth = groupInfo.SNMP_OverrideVersionAndAuth;
        SNMP_Versions = Enum.GetValues(typeof(SNMPVersion)).Cast<SNMPVersion>().ToList();
        SNMP_Version = SNMP_Versions.FirstOrDefault(x => x == groupInfo.SNMP_Version);
        SNMP_Community = groupInfo.SNMP_Community;
        SNMP_Securities = [SNMPV3Security.NoAuthNoPriv, SNMPV3Security.AuthNoPriv, SNMPV3Security.AuthPriv];
        SNMP_Security = SNMP_Securities.FirstOrDefault(x => x == groupInfo.SNMP_Security);
        SNMP_Username = groupInfo.SNMP_Username;
        SNMP_AuthenticationProviders = [.. Enum.GetValues(typeof(SNMPV3AuthenticationProvider)).Cast<SNMPV3AuthenticationProvider>()];
        SNMP_AuthenticationProvider =
            SNMP_AuthenticationProviders.FirstOrDefault(x => x == groupInfo.SNMP_AuthenticationProvider);
        SNMP_Auth = groupInfo.SNMP_Auth;
        SNMP_PrivacyProviders = Enum.GetValues(typeof(SNMPV3PrivacyProvider)).Cast<SNMPV3PrivacyProvider>().ToList();
        SNMP_PrivacyProvider = SNMP_PrivacyProviders.FirstOrDefault(x => x == groupInfo.SNMP_PrivacyProvider);
        SNMP_Priv = groupInfo.SNMP_Priv;

        _isLoading = false;
    }

    #endregion

    #region Methods

    private void ChangeNetworkConnectionTypeSettings(NetworkConnectionType connectionSpeed)
    {
        switch (connectionSpeed)
        {
            case NetworkConnectionType.Modem:
                RemoteDesktop_DesktopBackground = false;
                RemoteDesktop_FontSmoothing = false;
                RemoteDesktop_DesktopComposition = false;
                RemoteDesktop_ShowWindowContentsWhileDragging = false;
                RemoteDesktop_MenuAndWindowAnimation = false;
                RemoteDesktop_VisualStyles = false;
                break;
            case NetworkConnectionType.BroadbandLow:
                RemoteDesktop_DesktopBackground = false;
                RemoteDesktop_FontSmoothing = false;
                RemoteDesktop_DesktopComposition = false;
                RemoteDesktop_ShowWindowContentsWhileDragging = false;
                RemoteDesktop_MenuAndWindowAnimation = false;
                RemoteDesktop_VisualStyles = true;
                break;
            case NetworkConnectionType.Satellite:
            case NetworkConnectionType.BroadbandHigh:
                RemoteDesktop_DesktopBackground = false;
                RemoteDesktop_FontSmoothing = false;
                RemoteDesktop_DesktopComposition = true;
                RemoteDesktop_ShowWindowContentsWhileDragging = false;
                RemoteDesktop_MenuAndWindowAnimation = false;
                RemoteDesktop_VisualStyles = true;
                break;
            case NetworkConnectionType.WAN:
            case NetworkConnectionType.LAN:
                RemoteDesktop_DesktopBackground = true;
                RemoteDesktop_FontSmoothing = true;
                RemoteDesktop_DesktopComposition = true;
                RemoteDesktop_ShowWindowContentsWhileDragging = true;
                RemoteDesktop_MenuAndWindowAnimation = true;
                RemoteDesktop_VisualStyles = true;
                break;
        }
    }

    #endregion

    #region ICommand & Actions

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    #endregion
}