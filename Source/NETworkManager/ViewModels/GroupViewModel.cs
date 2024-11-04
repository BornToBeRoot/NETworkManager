﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;
using NETworkManager.Models.PuTTY;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Profiles;
using NETworkManager.Utilities;

// ReSharper disable InconsistentNaming

namespace NETworkManager.ViewModels;

public class GroupViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading = true;

    public bool IsProfileFileEncrypted => ProfileManager.LoadedProfileFile.IsEncrypted;

    public ICollectionView GroupViews { get; }
    public GroupInfo Group { get; }

    private IReadOnlyCollection<string> _groups { get; }

    #region General

    private bool _nameIsValid = true;

    public bool NameIsValid
    {
        get => _nameIsValid;
        set
        {
            if (value == _nameIsValid)
                return;

            _nameIsValid = value;
            OnPropertyChanged();
        }
    }

    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            if (value == _name)
                return;

            // Check name for duplicate...
            if (_groups.Contains(value, StringComparer.OrdinalIgnoreCase) &&
                !value.Equals(Group.Name, StringComparison.OrdinalIgnoreCase))
                NameIsValid = false;
            else
                NameIsValid = true;

            _name = value;
            OnPropertyChanged();
        }
    }

    private string _description;

    public string Description
    {
        get => _description;
        set
        {
            if (value == _description)
                return;

            _description = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Remote Desktop

    private bool _remoteDesktop_UseCredentials;

    public bool RemoteDesktop_UseCredentials
    {
        get => _remoteDesktop_UseCredentials;
        set
        {
            if (value == _remoteDesktop_UseCredentials)
                return;

            _remoteDesktop_UseCredentials = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_Username;

    public string RemoteDesktop_Username
    {
        get => _remoteDesktop_Username;
        set
        {
            if (value == _remoteDesktop_Username)
                return;

            _remoteDesktop_Username = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_Domain;

    public string RemoteDesktop_Domain
    {
        get => _remoteDesktop_Domain;
        set
        {
            if (value == _remoteDesktop_Domain)
                return;

            _remoteDesktop_Domain = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_IsPasswordEmpty = true; // Initial it's empty

    public bool RemoteDesktop_IsPasswordEmpty
    {
        get => _remoteDesktop_IsPasswordEmpty;
        set
        {
            if (value == _remoteDesktop_IsPasswordEmpty)
                return;

            _remoteDesktop_IsPasswordEmpty = value;
            OnPropertyChanged();
        }
    }

    private SecureString _remoteDesktop_Password;

    public SecureString RemoteDesktop_Password
    {
        get => _remoteDesktop_Password;
        set
        {
            if (value == _remoteDesktop_Password)
                return;

            // Validate the password string
            RemoteDesktop_IsPasswordEmpty =
                value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            _remoteDesktop_Password = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideDisplay;

    public bool RemoteDesktop_OverrideDisplay
    {
        get => _remoteDesktop_OverrideDisplay;
        set
        {
            if (value == _remoteDesktop_OverrideDisplay)
                return;

            _remoteDesktop_OverrideDisplay = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_AdjustScreenAutomatically;

    public bool RemoteDesktop_AdjustScreenAutomatically
    {
        get => _remoteDesktop_AdjustScreenAutomatically;
        set
        {
            if (value == _remoteDesktop_AdjustScreenAutomatically)
                return;

            _remoteDesktop_AdjustScreenAutomatically = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_UseCurrentViewSize;

    public bool RemoteDesktop_UseCurrentViewSize
    {
        get => _remoteDesktop_UseCurrentViewSize;
        set
        {
            if (value == _remoteDesktop_UseCurrentViewSize)
                return;

            _remoteDesktop_UseCurrentViewSize = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_UseFixedScreenSize;

    public bool RemoteDesktop_UseFixedScreenSize
    {
        get => _remoteDesktop_UseFixedScreenSize;
        set
        {
            if (value == _remoteDesktop_UseFixedScreenSize)
                return;

            _remoteDesktop_UseFixedScreenSize = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<string> RemoteDesktop_ScreenResolutions => RemoteDesktop.ScreenResolutions;

    public int RemoteDesktop_ScreenWidth;
    public int RemoteDesktop_ScreenHeight;

    private string _remoteDesktop_SelectedScreenResolution;

    public string RemoteDesktop_SelectedScreenResolution
    {
        get => _remoteDesktop_SelectedScreenResolution;
        set
        {
            if (value == _remoteDesktop_SelectedScreenResolution)
                return;

            var resolution = value.Split('x');

            RemoteDesktop_ScreenWidth = int.Parse(resolution[0]);
            RemoteDesktop_ScreenHeight = int.Parse(resolution[1]);

            _remoteDesktop_SelectedScreenResolution = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_UseCustomScreenSize;

    public bool RemoteDesktop_UseCustomScreenSize
    {
        get => _remoteDesktop_UseCustomScreenSize;
        set
        {
            if (value == _remoteDesktop_UseCustomScreenSize)
                return;

            _remoteDesktop_UseCustomScreenSize = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_CustomScreenWidth;

    public string RemoteDesktop_CustomScreenWidth
    {
        get => _remoteDesktop_CustomScreenWidth;
        set
        {
            if (value == _remoteDesktop_CustomScreenWidth)
                return;

            _remoteDesktop_CustomScreenWidth = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_CustomScreenHeight;

    public string RemoteDesktop_CustomScreenHeight
    {
        get => _remoteDesktop_CustomScreenHeight;
        set
        {
            if (value == _remoteDesktop_CustomScreenHeight)
                return;

            _remoteDesktop_CustomScreenHeight = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideColorDepth;

    public bool RemoteDesktop_OverrideColorDepth
    {
        get => _remoteDesktop_OverrideColorDepth;
        set
        {
            if (value == _remoteDesktop_OverrideColorDepth)
                return;

            _remoteDesktop_OverrideColorDepth = value;
            OnPropertyChanged();
        }
    }

    public List<int> RemoteDesktop_ColorDepths => RemoteDesktop.ColorDepths;

    private int _remoteDesktop_SelectedColorDepth;

    public int RemoteDesktop_SelectedColorDepth
    {
        get => _remoteDesktop_SelectedColorDepth;
        set
        {
            if (value == _remoteDesktop_SelectedColorDepth)
                return;

            _remoteDesktop_SelectedColorDepth = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverridePort;

    public bool RemoteDesktop_OverridePort
    {
        get => _remoteDesktop_OverridePort;
        set
        {
            if (value == _remoteDesktop_OverridePort)
                return;

            _remoteDesktop_OverridePort = value;
            OnPropertyChanged();
        }
    }

    private int _remoteDesktop_Port;

    public int RemoteDesktop_Port
    {
        get => _remoteDesktop_Port;
        set
        {
            if (value == _remoteDesktop_Port)
                return;

            _remoteDesktop_Port = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideCredSspSupport;

    public bool RemoteDesktop_OverrideCredSspSupport
    {
        get => _remoteDesktop_OverrideCredSspSupport;
        set
        {
            if (value == _remoteDesktop_OverrideCredSspSupport)
                return;

            _remoteDesktop_OverrideCredSspSupport = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_EnableCredSspSupport;

    public bool RemoteDesktop_EnableCredSspSupport
    {
        get => _remoteDesktop_EnableCredSspSupport;
        set
        {
            if (value == _remoteDesktop_EnableCredSspSupport)
                return;

            _remoteDesktop_EnableCredSspSupport = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideAuthenticationLevel;

    public bool RemoteDesktop_OverrideAuthenticationLevel
    {
        get => _remoteDesktop_OverrideAuthenticationLevel;
        set
        {
            if (value == _remoteDesktop_OverrideAuthenticationLevel)
                return;

            _remoteDesktop_OverrideAuthenticationLevel = value;
            OnPropertyChanged();
        }
    }

    private uint _remoteDesktop_AuthenticationLevel;

    public uint RemoteDesktop_AuthenticationLevel
    {
        get => _remoteDesktop_AuthenticationLevel;
        set
        {
            if (value == _remoteDesktop_AuthenticationLevel)
                return;

            _remoteDesktop_AuthenticationLevel = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideGatewayServer;

    public bool RemoteDesktop_OverrideGatewayServer
    {
        get => _remoteDesktop_OverrideGatewayServer;
        set
        {
            if (value == _remoteDesktop_OverrideGatewayServer)
                return;

            _remoteDesktop_OverrideGatewayServer = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_EnableGatewayServer;

    public bool RemoteDesktop_EnableGatewayServer
    {
        get => _remoteDesktop_EnableGatewayServer;
        set
        {
            if (value == _remoteDesktop_EnableGatewayServer)
                return;

            _remoteDesktop_EnableGatewayServer = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_GatewayServerHostname;

    public string RemoteDesktop_GatewayServerHostname
    {
        get => _remoteDesktop_GatewayServerHostname;
        set
        {
            if (value == _remoteDesktop_GatewayServerHostname)
                return;

            _remoteDesktop_GatewayServerHostname = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_GatewayServerBypassLocalAddresses;

    public bool RemoteDesktop_GatewayServerBypassLocalAddresses
    {
        get => _remoteDesktop_GatewayServerBypassLocalAddresses;
        set
        {
            if (value == _remoteDesktop_GatewayServerBypassLocalAddresses)
                return;

            _remoteDesktop_GatewayServerBypassLocalAddresses = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<GatewayUserSelectedCredsSource> RemoteDesktop_GatewayServerLogonMethods =>
        Enum.GetValues(typeof(GatewayUserSelectedCredsSource)).Cast<GatewayUserSelectedCredsSource>();

    private GatewayUserSelectedCredsSource _remoteDesktop_GatewayServerLogonMethod;

    public GatewayUserSelectedCredsSource RemoteDesktop_GatewayServerLogonMethod
    {
        get => _remoteDesktop_GatewayServerLogonMethod;
        set
        {
            if (Equals(value, _remoteDesktop_GatewayServerLogonMethod))
                return;

            _remoteDesktop_GatewayServerLogonMethod = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_GatewayServerShareCredentialsWithRemoteComputer;

    public bool RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer
    {
        get => _remoteDesktop_GatewayServerShareCredentialsWithRemoteComputer;
        set
        {
            if (value == _remoteDesktop_GatewayServerShareCredentialsWithRemoteComputer)
                return;

            _remoteDesktop_GatewayServerShareCredentialsWithRemoteComputer = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_UseGatewayServerCredentials;

    public bool RemoteDesktop_UseGatewayServerCredentials
    {
        get => _remoteDesktop_UseGatewayServerCredentials;
        set
        {
            if (value == _remoteDesktop_UseGatewayServerCredentials)
                return;

            _remoteDesktop_UseGatewayServerCredentials = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_GatewayServerUsername;

    public string RemoteDesktop_GatewayServerUsername
    {
        get => _remoteDesktop_GatewayServerUsername;
        set
        {
            if (value == _remoteDesktop_GatewayServerUsername)
                return;

            _remoteDesktop_GatewayServerUsername = value;
            OnPropertyChanged();
        }
    }

    private string _remoteDesktop_GatewayServerDomain;

    public string RemoteDesktop_GatewayServerDomain
    {
        get => _remoteDesktop_GatewayServerDomain;
        set
        {
            if (value == _remoteDesktop_GatewayServerDomain)
                return;

            _remoteDesktop_GatewayServerDomain = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_IsGatewayServerPasswordEmpty = true; // Initial it's empty

    public bool RemoteDesktop_IsGatewayServerPasswordEmpty
    {
        get => _remoteDesktop_IsGatewayServerPasswordEmpty;
        set
        {
            if (value == _remoteDesktop_IsGatewayServerPasswordEmpty)
                return;

            _remoteDesktop_IsGatewayServerPasswordEmpty = value;
            OnPropertyChanged();
        }
    }

    private SecureString _remoteDesktop_GatewayServerPassword;

    public SecureString RemoteDesktop_GatewayServerPassword
    {
        get => _remoteDesktop_GatewayServerPassword;
        set
        {
            if (value == _remoteDesktop_GatewayServerPassword)
                return;

            // Validate the password string
            RemoteDesktop_IsGatewayServerPasswordEmpty =
                value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            _remoteDesktop_GatewayServerPassword = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideAudioRedirectionMode;

    public bool RemoteDesktop_OverrideAudioRedirectionMode
    {
        get => _remoteDesktop_OverrideAudioRedirectionMode;
        set
        {
            if (value == _remoteDesktop_OverrideAudioRedirectionMode)
                return;

            _remoteDesktop_OverrideAudioRedirectionMode = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<AudioRedirectionMode> RemoteDesktop_AudioRedirectionModes =>
        Enum.GetValues(typeof(AudioRedirectionMode)).Cast<AudioRedirectionMode>();

    private AudioRedirectionMode _remoteDesktop_AudioRedirectionMode;

    public AudioRedirectionMode RemoteDesktop_AudioRedirectionMode
    {
        get => _remoteDesktop_AudioRedirectionMode;
        set
        {
            if (Equals(value, _remoteDesktop_AudioRedirectionMode))
                return;

            _remoteDesktop_AudioRedirectionMode = value;
            OnPropertyChanged();
        }
    }


    private bool _remoteDesktop_OverrideAudioCaptureRedirectionMode;

    public bool RemoteDesktop_OverrideAudioCaptureRedirectionMode
    {
        get => _remoteDesktop_OverrideAudioCaptureRedirectionMode;
        set
        {
            if (value == _remoteDesktop_OverrideAudioCaptureRedirectionMode)
                return;

            _remoteDesktop_OverrideAudioCaptureRedirectionMode = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<AudioCaptureRedirectionMode> RemoteDesktop_AudioCaptureRedirectionModes =>
        Enum.GetValues(typeof(AudioCaptureRedirectionMode)).Cast<AudioCaptureRedirectionMode>();

    private AudioCaptureRedirectionMode _remoteDesktop_AudioCaptureRedirectionMode;

    public AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode
    {
        get => _remoteDesktop_AudioCaptureRedirectionMode;
        set
        {
            if (Equals(value, _remoteDesktop_AudioCaptureRedirectionMode))
                return;

            _remoteDesktop_AudioCaptureRedirectionMode = value;
            OnPropertyChanged();
        }
    }


    private bool _remoteDesktop_OverrideApplyWindowsKeyCombinations;

    public bool RemoteDesktop_OverrideApplyWindowsKeyCombinations
    {
        get => _remoteDesktop_OverrideApplyWindowsKeyCombinations;
        set
        {
            if (value == _remoteDesktop_OverrideApplyWindowsKeyCombinations)
                return;

            _remoteDesktop_OverrideApplyWindowsKeyCombinations = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<KeyboardHookMode> RemoteDesktop_KeyboardHookModes =>
        Enum.GetValues(typeof(KeyboardHookMode)).Cast<KeyboardHookMode>();

    private KeyboardHookMode _remoteDesktop_KeyboardHookMode;

    public KeyboardHookMode RemoteDesktop_KeyboardHookMode
    {
        get => _remoteDesktop_KeyboardHookMode;
        set
        {
            if (Equals(value, _remoteDesktop_KeyboardHookMode))
                return;

            _remoteDesktop_KeyboardHookMode = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectClipboard;

    public bool RemoteDesktop_OverrideRedirectClipboard
    {
        get => _remoteDesktop_OverrideRedirectClipboard;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectClipboard)
                return;

            _remoteDesktop_OverrideRedirectClipboard = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectClipboard;

    public bool RemoteDesktop_RedirectClipboard
    {
        get => _remoteDesktop_RedirectClipboard;
        set
        {
            if (value == _remoteDesktop_RedirectClipboard)
                return;

            _remoteDesktop_RedirectClipboard = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectDevices;

    public bool RemoteDesktop_OverrideRedirectDevices
    {
        get => _remoteDesktop_OverrideRedirectDevices;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectDevices)
                return;

            _remoteDesktop_OverrideRedirectDevices = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectDevices;

    public bool RemoteDesktop_RedirectDevices
    {
        get => _remoteDesktop_RedirectDevices;
        set
        {
            if (value == _remoteDesktop_RedirectDevices)
                return;

            _remoteDesktop_RedirectDevices = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectDrives;

    public bool RemoteDesktop_OverrideRedirectDrives
    {
        get => _remoteDesktop_OverrideRedirectDrives;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectDrives)
                return;

            _remoteDesktop_OverrideRedirectDrives = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectDrives;

    public bool RemoteDesktop_RedirectDrives
    {
        get => _remoteDesktop_RedirectDrives;
        set
        {
            if (value == _remoteDesktop_RedirectDrives)
                return;

            _remoteDesktop_RedirectDrives = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectPorts;

    public bool RemoteDesktop_OverrideRedirectPorts
    {
        get => _remoteDesktop_OverrideRedirectPorts;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectPorts)
                return;

            _remoteDesktop_OverrideRedirectPorts = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectPorts;

    public bool RemoteDesktop_RedirectPorts
    {
        get => _remoteDesktop_RedirectPorts;
        set
        {
            if (value == _remoteDesktop_RedirectPorts)
                return;

            _remoteDesktop_RedirectPorts = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectSmartcards;

    public bool RemoteDesktop_OverrideRedirectSmartcards
    {
        get => _remoteDesktop_OverrideRedirectSmartcards;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectSmartcards)
                return;

            _remoteDesktop_OverrideRedirectSmartcards = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectSmartCards;

    public bool RemoteDesktop_RedirectSmartCards
    {
        get => _remoteDesktop_RedirectSmartCards;
        set
        {
            if (value == _remoteDesktop_RedirectSmartCards)
                return;

            _remoteDesktop_RedirectSmartCards = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideRedirectPrinters;

    public bool RemoteDesktop_OverrideRedirectPrinters
    {
        get => _remoteDesktop_OverrideRedirectPrinters;
        set
        {
            if (value == _remoteDesktop_OverrideRedirectPrinters)
                return;

            _remoteDesktop_OverrideRedirectPrinters = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_RedirectPrinters;

    public bool RemoteDesktop_RedirectPrinters
    {
        get => _remoteDesktop_RedirectPrinters;
        set
        {
            if (value == _remoteDesktop_RedirectPrinters)
                return;

            _remoteDesktop_RedirectPrinters = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverridePersistentBitmapCaching;

    public bool RemoteDesktop_OverridePersistentBitmapCaching
    {
        get => _remoteDesktop_OverridePersistentBitmapCaching;
        set
        {
            if (value == _remoteDesktop_OverridePersistentBitmapCaching)
                return;

            _remoteDesktop_OverridePersistentBitmapCaching = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_PersistentBitmapCaching;

    public bool RemoteDesktop_PersistentBitmapCaching
    {
        get => _remoteDesktop_PersistentBitmapCaching;
        set
        {
            if (value == _remoteDesktop_PersistentBitmapCaching)
                return;

            _remoteDesktop_PersistentBitmapCaching = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped;

    public bool RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped
    {
        get => _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped;
        set
        {
            if (value == _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped)
                return;

            _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_ReconnectIfTheConnectionIsDropped;

    public bool RemoteDesktop_ReconnectIfTheConnectionIsDropped
    {
        get => _remoteDesktop_ReconnectIfTheConnectionIsDropped;
        set
        {
            if (value == _remoteDesktop_ReconnectIfTheConnectionIsDropped)
                return;

            _remoteDesktop_ReconnectIfTheConnectionIsDropped = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_OverrideNetworkConnectionType;

    public bool RemoteDesktop_OverrideNetworkConnectionType
    {
        get => _remoteDesktop_OverrideNetworkConnectionType;
        set
        {
            if (value == _remoteDesktop_OverrideNetworkConnectionType)
                return;

            _remoteDesktop_OverrideNetworkConnectionType = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<NetworkConnectionType> RemoteDesktop_NetworkConnectionTypes =>
        Enum.GetValues(typeof(NetworkConnectionType)).Cast<NetworkConnectionType>();

    private NetworkConnectionType _remoteDesktop_NetworkConnectionType;

    public NetworkConnectionType RemoteDesktop_NetworkConnectionType
    {
        get => _remoteDesktop_NetworkConnectionType;
        set
        {
            if (Equals(value, _remoteDesktop_NetworkConnectionType))
                return;

            if (!_isLoading)
                ChangeNetworkConnectionTypeSettings(value);

            _remoteDesktop_NetworkConnectionType = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_DesktopBackground;

    public bool RemoteDesktop_DesktopBackground
    {
        get => _remoteDesktop_DesktopBackground;
        set
        {
            if (value == _remoteDesktop_DesktopBackground)
                return;

            _remoteDesktop_DesktopBackground = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_FontSmoothing;

    public bool RemoteDesktop_FontSmoothing
    {
        get => _remoteDesktop_FontSmoothing;
        set
        {
            if (value == _remoteDesktop_FontSmoothing)
                return;

            _remoteDesktop_FontSmoothing = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_DesktopComposition;

    public bool RemoteDesktop_DesktopComposition
    {
        get => _remoteDesktop_DesktopComposition;
        set
        {
            if (value == _remoteDesktop_DesktopComposition)
                return;

            _remoteDesktop_DesktopComposition = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_ShowWindowContentsWhileDragging;

    public bool RemoteDesktop_ShowWindowContentsWhileDragging
    {
        get => _remoteDesktop_ShowWindowContentsWhileDragging;
        set
        {
            if (value == _remoteDesktop_ShowWindowContentsWhileDragging)
                return;

            _remoteDesktop_ShowWindowContentsWhileDragging = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_MenuAndWindowAnimation;

    public bool RemoteDesktop_MenuAndWindowAnimation
    {
        get => _remoteDesktop_MenuAndWindowAnimation;
        set
        {
            if (value == _remoteDesktop_MenuAndWindowAnimation)
                return;

            _remoteDesktop_MenuAndWindowAnimation = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteDesktop_VisualStyles;

    public bool RemoteDesktop_VisualStyles
    {
        get => _remoteDesktop_VisualStyles;
        set
        {
            if (value == _remoteDesktop_VisualStyles)
                return;

            _remoteDesktop_VisualStyles = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region PowerShell

    private bool _powerShell_OverrideCommand;

    public bool PowerShell_OverrideCommand
    {
        get => _powerShell_OverrideCommand;
        set
        {
            if (value == _powerShell_OverrideCommand)
                return;

            _powerShell_OverrideCommand = value;
            OnPropertyChanged();
        }
    }

    private string _powerShell_Command;

    public string PowerShell_Command
    {
        get => _powerShell_Command;
        set
        {
            if (value == _powerShell_Command)
                return;

            _powerShell_Command = value;
            OnPropertyChanged();
        }
    }

    private bool _powerShell_OverrideAdditionalCommandLine;

    public bool PowerShell_OverrideAdditionalCommandLine
    {
        get => _powerShell_OverrideAdditionalCommandLine;
        set
        {
            if (value == _powerShell_OverrideAdditionalCommandLine)
                return;

            _powerShell_OverrideAdditionalCommandLine = value;
            OnPropertyChanged();
        }
    }

    private string _powerShell_AdditionalCommandLine;

    public string PowerShell_AdditionalCommandLine
    {
        get => _powerShell_AdditionalCommandLine;
        set
        {
            if (value == _powerShell_AdditionalCommandLine)
                return;

            _powerShell_AdditionalCommandLine = value;
            OnPropertyChanged();
        }
    }

    private bool _powerShell_OverrideExecutionPolicy;

    public bool PowerShell_OverrideExecutionPolicy
    {
        get => _powerShell_OverrideExecutionPolicy;
        set
        {
            if (value == _powerShell_OverrideExecutionPolicy)
                return;

            _powerShell_OverrideExecutionPolicy = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<ExecutionPolicy> PowerShell_ExecutionPolicies { get; set; }

    private ExecutionPolicy _powerShell_ExecutionPolicy;

    public ExecutionPolicy PowerShell_ExecutionPolicy
    {
        get => _powerShell_ExecutionPolicy;
        set
        {
            if (value == _powerShell_ExecutionPolicy)
                return;

            _powerShell_ExecutionPolicy = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region PuTTY

    private bool _puTTY_OverrideUsername;

    public bool PuTTY_OverrideUsername
    {
        get => _puTTY_OverrideUsername;
        set
        {
            if (value == _puTTY_OverrideUsername)
                return;

            _puTTY_OverrideUsername = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY__Username;

    public string PuTTY_Username
    {
        get => _puTTY__Username;
        set
        {
            if (value == _puTTY__Username)
                return;

            _puTTY__Username = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverridePrivateKeyFile;

    public bool PuTTY_OverridePrivateKeyFile
    {
        get => _puTTY_OverridePrivateKeyFile;
        set
        {
            if (value == _puTTY_OverridePrivateKeyFile)
                return;

            _puTTY_OverridePrivateKeyFile = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY__PrivateKeyFile;

    public string PuTTY_PrivateKeyFile
    {
        get => _puTTY__PrivateKeyFile;
        set
        {
            if (value == _puTTY__PrivateKeyFile)
                return;

            _puTTY__PrivateKeyFile = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideProfile;

    public bool PuTTY_OverrideProfile
    {
        get => _puTTY_OverrideProfile;
        set
        {
            if (value == _puTTY_OverrideProfile)
                return;

            _puTTY_OverrideProfile = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_Profile;

    public string PuTTY_Profile
    {
        get => _puTTY_Profile;
        set
        {
            if (value == _puTTY_Profile)
                return;

            _puTTY_Profile = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideEnableLog;

    public bool PuTTY_OverrideEnableLog
    {
        get => _puTTY_OverrideEnableLog;
        set
        {
            if (value == _puTTY_OverrideEnableLog)
                return;

            _puTTY_OverrideEnableLog = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_EnableLog;

    public bool PuTTY_EnableLog
    {
        get => _puTTY_EnableLog;
        set
        {
            if (value == _puTTY_EnableLog)
                return;

            _puTTY_EnableLog = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideLogMode;

    public bool PuTTY_OverrideLogMode
    {
        get => _puTTY_OverrideLogMode;
        set
        {
            if (value == _puTTY_OverrideLogMode)
                return;

            _puTTY_OverrideLogMode = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<LogMode> PuTTY_LogModes => Enum.GetValues(typeof(LogMode)).Cast<LogMode>();

    private LogMode _puTTY_LogMode;

    public LogMode PuTTY_LogMode
    {
        get => _puTTY_LogMode;
        set
        {
            if (Equals(value, _puTTY_LogMode))
                return;

            _puTTY_LogMode = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideLogPath;

    public bool PuTTY_OverrideLogPath
    {
        get => _puTTY_OverrideLogPath;
        set
        {
            if (value == _puTTY_OverrideLogPath)
                return;

            _puTTY_OverrideLogPath = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_LogPath;

    public string PuTTY_LogPath
    {
        get => _puTTY_LogPath;
        set
        {
            if (value == _puTTY_LogPath)
                return;

            _puTTY_LogPath = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideLogFileName;

    public bool PuTTY_OverrideLogFileName
    {
        get => _puTTY_OverrideLogFileName;
        set
        {
            if (value == _puTTY_OverrideLogFileName)
                return;

            _puTTY_OverrideLogFileName = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_LogFileName;

    public string PuTTY_LogFileName
    {
        get => _puTTY_LogFileName;
        set
        {
            if (value == _puTTY_LogFileName)
                return;

            _puTTY_LogFileName = value;
            OnPropertyChanged();
        }
    }

    private bool _puTTY_OverrideAdditionalCommandLine;

    public bool PuTTY_OverrideAdditionalCommandLine
    {
        get => _puTTY_OverrideAdditionalCommandLine;
        set
        {
            if (value == _puTTY_OverrideAdditionalCommandLine)
                return;

            _puTTY_OverrideAdditionalCommandLine = value;
            OnPropertyChanged();
        }
    }

    private string _puTTY_AdditionalCommandLine;

    public string PuTTY_AdditionalCommandLine
    {
        get => _puTTY_AdditionalCommandLine;
        set
        {
            if (value == _puTTY_AdditionalCommandLine)
                return;

            _puTTY_AdditionalCommandLine = value;
            OnPropertyChanged();
        }
    }

    private ConnectionMode _puTTY_ConnectionMode;

    public ConnectionMode PuTTY_ConnectionMode
    {
        get => _puTTY_ConnectionMode;
        set
        {
            if (value == _puTTY_ConnectionMode)
                return;

            _puTTY_ConnectionMode = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region AWS Session Manager

    private bool _awsSessionManager_OverrideProfile;

    public bool AWSSessionManager_OverrideProfile
    {
        get => _awsSessionManager_OverrideProfile;
        set
        {
            if (value == _awsSessionManager_OverrideProfile)
                return;

            _awsSessionManager_OverrideProfile = value;
            OnPropertyChanged();
        }
    }

    private string _awsSessionManager_Profile;

    public string AWSSessionManager_Profile
    {
        get => _awsSessionManager_Profile;
        set
        {
            if (value == _awsSessionManager_Profile)
                return;

            _awsSessionManager_Profile = value;
            OnPropertyChanged();
        }
    }

    private bool _awsSessionManager_OverrideRegion;

    public bool AWSSessionManager_OverrideRegion
    {
        get => _awsSessionManager_OverrideRegion;
        set
        {
            if (value == _awsSessionManager_OverrideRegion)
                return;

            _awsSessionManager_OverrideRegion = value;
            OnPropertyChanged();
        }
    }

    private string _awsSessionManager_Region;

    public string AWSSessionManager_Region
    {
        get => _awsSessionManager_Region;
        set
        {
            if (value == _awsSessionManager_Region)
                return;

            _awsSessionManager_Region = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region TigerVNC

    private bool _tigerVNC_OverridePort;

    public bool TigerVNC_OverridePort
    {
        get => _tigerVNC_OverridePort;
        set
        {
            if (value == _tigerVNC_OverridePort)
                return;

            _tigerVNC_OverridePort = value;
            OnPropertyChanged();
        }
    }

    private int _tigerVNC_Port;

    public int TigerVNC_Port
    {
        get => _tigerVNC_Port;
        set
        {
            if (value == _tigerVNC_Port)
                return;

            _tigerVNC_Port = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region SNMP

    private bool _snmp_OverrideOIDAndMode;

    public bool SNMP_OverrideOIDAndMode
    {
        get => _snmp_OverrideOIDAndMode;
        set
        {
            if (value == _snmp_OverrideOIDAndMode)
                return;

            _snmp_OverrideOIDAndMode = value;
            OnPropertyChanged();
        }
    }

    private string _snmp_OID;

    public string SNMP_OID
    {
        get => _snmp_OID;
        set
        {
            if (value == _snmp_OID)
                return;

            _snmp_OID = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPMode> SNMP_Modes { get; set; }

    private SNMPMode _snmp_Mode;

    public SNMPMode SNMP_Mode
    {
        get => _snmp_Mode;
        set
        {
            if (value == _snmp_Mode)
                return;

            _snmp_Mode = value;
            OnPropertyChanged();

            // Re-validate OID if mode changed
            OnPropertyChanged(nameof(SNMP_OID));
        }
    }

    private bool _snmp_OverrideVersionAndAuth;

    public bool SNMP_OverrideVersionAndAuth
    {
        get => _snmp_OverrideVersionAndAuth;
        set
        {
            if (value == _snmp_OverrideVersionAndAuth)
                return;

            _snmp_OverrideVersionAndAuth = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPVersion> SNMP_Versions { get; }

    private SNMPVersion _snmp_Version;

    public SNMPVersion SNMP_Version
    {
        get => _snmp_Version;
        set
        {
            if (value == _snmp_Version)
                return;

            _snmp_Version = value;
            OnPropertyChanged();
        }
    }

    private bool _snmp_IsCommunityEmpty = true; // Initial it's empty

    public bool SNMP_IsCommunityEmpty
    {
        get => _snmp_IsCommunityEmpty;
        set
        {
            if (value == _snmp_IsCommunityEmpty)
                return;

            _snmp_IsCommunityEmpty = value;
            OnPropertyChanged();
        }
    }

    private SecureString _snmp_Community;

    public SecureString SNMP_Community
    {
        get => _snmp_Community;
        set
        {
            if (value == _snmp_Community)
                return;

            // Validate the password string
            SNMP_IsCommunityEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            _snmp_Community = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3Security> SNMP_Securities { get; }

    private SNMPV3Security _snmp_Security;

    public SNMPV3Security SNMP_Security
    {
        get => _snmp_Security;
        set
        {
            if (value == _snmp_Security)
                return;

            _snmp_Security = value;
            OnPropertyChanged();
        }
    }

    private string _snmp_Username;

    public string SNMP_Username
    {
        get => _snmp_Username;
        set
        {
            if (value == _snmp_Username)
                return;

            _snmp_Username = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3AuthenticationProvider> SNMP_AuthenticationProviders { get; }

    private SNMPV3AuthenticationProvider _snmp_AuthenticationProvider;

    public SNMPV3AuthenticationProvider SNMP_AuthenticationProvider
    {
        get => _snmp_AuthenticationProvider;
        set
        {
            if (value == _snmp_AuthenticationProvider)
                return;

            _snmp_AuthenticationProvider = value;
            OnPropertyChanged();
        }
    }

    private bool _snmp_IsAuthEmpty = true; // Initial it's empty

    public bool SNMP_IsAuthEmpty
    {
        get => _snmp_IsAuthEmpty;
        set
        {
            if (value == _snmp_IsAuthEmpty)
                return;

            _snmp_IsAuthEmpty = value;
            OnPropertyChanged();
        }
    }

    private SecureString _snmp_Auth;

    public SecureString SNMP_Auth
    {
        get => _snmp_Auth;
        set
        {
            if (value == _snmp_Auth)
                return;

            // Validate the password string
            SNMP_IsAuthEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            _snmp_Auth = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3PrivacyProvider> SNMP_PrivacyProviders { get; }

    private SNMPV3PrivacyProvider _snmp_PrivacyProvider;

    public SNMPV3PrivacyProvider SNMP_PrivacyProvider
    {
        get => _snmp_PrivacyProvider;
        set
        {
            if (value == _snmp_PrivacyProvider)
                return;

            _snmp_PrivacyProvider = value;
            OnPropertyChanged();
        }
    }

    private bool _snmp_IsPrivEmpty = true; // Initial it's empty

    public bool SNMP_IsPrivEmpty
    {
        get => _snmp_IsPrivEmpty;
        set
        {
            if (value == _snmp_IsPrivEmpty)
                return;

            _snmp_IsPrivEmpty = value;
            OnPropertyChanged();
        }
    }

    private SecureString _snmp_Priv;

    public SecureString SNMP_Priv
    {
        get => _snmp_Priv;
        set
        {
            if (value == _snmp_Priv)
                return;

            // Validate the password string
            SNMP_IsPrivEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            _snmp_Priv = value;
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

        // AWS Session Manager
        AWSSessionManager_OverrideProfile = groupInfo.AWSSessionManager_OverrideProfile;
        AWSSessionManager_Profile = groupInfo.AWSSessionManager_Profile;
        AWSSessionManager_OverrideRegion = groupInfo.AWSSessionManager_OverrideRegion;
        AWSSessionManager_Region = groupInfo.AWSSessionManager_Region;

        // TigerVNC
        TigerVNC_OverridePort = groupInfo.TigerVNC_OverridePort;
        TigerVNC_Port = groupInfo.TigerVNC_Port;

        // SNMP
        SNMP_OverrideOIDAndMode = groupInfo.SNMP_OverrideOIDAndMode;
        SNMP_OID = groupInfo.SNMP_OID;
        SNMP_Modes = new List<SNMPMode> { SNMPMode.Get, SNMPMode.Walk, SNMPMode.Set };
        SNMP_Mode = SNMP_Modes.FirstOrDefault(x => x == groupInfo.SNMP_Mode);
        SNMP_OverrideVersionAndAuth = groupInfo.SNMP_OverrideVersionAndAuth;
        SNMP_Versions = Enum.GetValues(typeof(SNMPVersion)).Cast<SNMPVersion>().ToList();
        SNMP_Version = SNMP_Versions.FirstOrDefault(x => x == groupInfo.SNMP_Version);
        SNMP_Community = groupInfo.SNMP_Community;
        SNMP_Securities = new List<SNMPV3Security>
            { SNMPV3Security.NoAuthNoPriv, SNMPV3Security.AuthNoPriv, SNMPV3Security.AuthPriv };
        SNMP_Security = SNMP_Securities.FirstOrDefault(x => x == groupInfo.SNMP_Security);
        SNMP_Username = groupInfo.SNMP_Username;
        SNMP_AuthenticationProviders = Enum.GetValues(typeof(SNMPV3AuthenticationProvider))
            .Cast<SNMPV3AuthenticationProvider>().ToList();
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