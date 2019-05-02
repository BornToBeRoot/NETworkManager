using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class RemoteDesktopSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private bool _adjustScreenAutomatically;
        public bool AdjustScreenAutomatically
        {
            get => _adjustScreenAutomatically;
            set
            {
                if (value == _adjustScreenAutomatically)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically = value;

                _adjustScreenAutomatically = value;
                OnPropertyChanged();
            }
        }

        private bool _useCurrentViewSize;
        public bool UseCurrentViewSize
        {
            get => _useCurrentViewSize;
            set
            {
                if (value == _useCurrentViewSize)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_UseCurrentViewSize = value;

                _useCurrentViewSize = value;
                OnPropertyChanged();
            }
        }

        private bool _useFixedScreenSize;
        public bool UseFixedScreenSize
        {
            get => _useFixedScreenSize;
            set
            {
                if (value == _useFixedScreenSize)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_UseFixedScreenSize = value;

                _useFixedScreenSize = value;
                OnPropertyChanged();
            }
        }

        public List<string> ScreenResolutions => RemoteDesktop.ScreenResolutions;

        private string _selectedScreenResolution;
        public string SelectedScreenResolution
        {
            get => _selectedScreenResolution;
            set
            {
                if (value == _selectedScreenResolution)
                    return;

                if (!_isLoading)
                {
                    var resolution = value.Split('x');

                    SettingsManager.Current.RemoteDesktop_ScreenWidth = int.Parse(resolution[0]);
                    SettingsManager.Current.RemoteDesktop_ScreenHeight = int.Parse(resolution[1]);
                }

                _selectedScreenResolution = value;
                OnPropertyChanged();
            }
        }

        private bool _useCustomScreenSize;
        public bool UseCustomScreenSize
        {
            get => _useCustomScreenSize;
            set
            {
                if (value == _useCustomScreenSize)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_UseCustomScreenSize = value;

                _useCustomScreenSize = value;
                OnPropertyChanged();
            }
        }

        private string _customScreenWidth;
        public string CustomScreenWidth
        {
            get => _customScreenWidth;
            set
            {
                if (value == _customScreenWidth)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_CustomScreenWidth = int.Parse(value);

                _customScreenWidth = value;
                OnPropertyChanged();
            }
        }

        private string _customScreenHeight;
        public string CustomScreenHeight
        {
            get => _customScreenHeight;
            set
            {
                if (value == _customScreenHeight)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_CustomScreenHeight = int.Parse(value);

                _customScreenHeight = value;
                OnPropertyChanged();
            }
        }

        public List<int> ColorDepths => RemoteDesktop.ColorDepths;

        private int _selectedColorDepth;
        public int SelectedColorDepth
        {
            get => _selectedColorDepth;
            set
            {
                if (value == _selectedColorDepth)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_ColorDepth = value;

                _selectedColorDepth = value;
                OnPropertyChanged();
            }
        }

        private int _port;
        public int Port
        {
            get => _port;
            set
            {
                if (value == _port)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_Port = value;

                _port = value;
                OnPropertyChanged();
            }
        }

        private bool _enableCredSspSupport;
        public bool EnableCredSspSupport
        {
            get => _enableCredSspSupport;
            set
            {
                if (value == _enableCredSspSupport)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_EnableCredSspSupport = value;

                _enableCredSspSupport = value;
                OnPropertyChanged();
            }
        }

        private uint _authenticationLevel;
        public uint AuthenticationLevel
        {
            get => _authenticationLevel;
            set
            {
                if (value == _authenticationLevel)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_AuthenticationLevel = value;

                _authenticationLevel = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<RemoteDesktop.AudioRedirectionMode> AudioRedirectionModes => System.Enum.GetValues(typeof(RemoteDesktop.AudioRedirectionMode)).Cast<RemoteDesktop.AudioRedirectionMode>();

        private RemoteDesktop.AudioRedirectionMode _audioRedirectionMode;
        public RemoteDesktop.AudioRedirectionMode AudioRedirectionMode
        {
            get => _audioRedirectionMode;
            set
            {
                if (Equals(value, _audioRedirectionMode))
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_AudioRedirectionMode = value;

                _audioRedirectionMode = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<RemoteDesktop.AudioCaptureRedirectionMode> AudioCaptureRedirectionModes => System.Enum.GetValues(typeof(RemoteDesktop.AudioCaptureRedirectionMode)).Cast<RemoteDesktop.AudioCaptureRedirectionMode>();

        private RemoteDesktop.AudioCaptureRedirectionMode _audioCaptureRedirectionMode;
        public RemoteDesktop.AudioCaptureRedirectionMode AudioCaptureRedirectionMode
        {
            get => _audioCaptureRedirectionMode;
            set
            {
                if (Equals(value, _audioCaptureRedirectionMode))
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_AudioCaptureRedirectionMode = value;

                _audioCaptureRedirectionMode = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<RemoteDesktop.KeyboardHookMode> KeyboardHookModes => System.Enum.GetValues(typeof(RemoteDesktop.KeyboardHookMode)).Cast<RemoteDesktop.KeyboardHookMode>();

        private RemoteDesktop.KeyboardHookMode _keyboardHookMode;
        public RemoteDesktop.KeyboardHookMode KeyboardHookMode
        {
            get => _keyboardHookMode;
            set
            {
                if (Equals(value, _keyboardHookMode))
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_KeyboardHookMode = value;

                _keyboardHookMode = value;
                OnPropertyChanged();
            }
        }

        private bool _redirectClipboard;
        public bool RedirectClipboard
        {
            get => _redirectClipboard;
            set
            {
                if (value == _redirectClipboard)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_RedirectClipboard = value;

                _redirectClipboard = value;
                OnPropertyChanged();
            }
        }

        private bool _redirectDevices;
        public bool RedirectDevices
        {
            get => _redirectDevices;
            set
            {
                if (value == _redirectDevices)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_RedirectDevices = value;

                _redirectDevices = value;
                OnPropertyChanged();
            }
        }

        private bool _redirectDrives;
        public bool RedirectDrives
        {
            get => _redirectDrives;
            set
            {
                if (value == _redirectDrives)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_RedirectDrives = value;

                _redirectDrives = value;
                OnPropertyChanged();
            }
        }

        private bool _redirectPorts;
        public bool RedirectPorts
        {
            get => _redirectPorts;
            set
            {
                if (value == _redirectPorts)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_RedirectPorts = value;

                _redirectPorts = value;
                OnPropertyChanged();
            }
        }

        private bool _redirectSmartCards;
        public bool RedirectSmartCards
        {
            get => _redirectSmartCards;
            set
            {
                if (value == _redirectSmartCards)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_RedirectSmartCards = value;

                _redirectSmartCards = value;
                OnPropertyChanged();
            }
        }

        private bool _redirectPrinters;
        public bool RedirectPrinters
        {
            get => _redirectPrinters;
            set
            {
                if (value == _redirectPrinters)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_RedirectPrinters = value;

                _redirectPrinters = value;
                OnPropertyChanged();
            }
        }

        private bool _persistentBitmapCaching;
        public bool PersistentBitmapCaching
        {
            get => _persistentBitmapCaching;
            set
            {
                if (value == _persistentBitmapCaching)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_PersistentBitmapCaching = value;

                _persistentBitmapCaching = value;
                OnPropertyChanged();
            }
        }

        private bool _reconnectIfTheConnectionIsDropped;
        public bool ReconnectIfTheConnectionIsDropped
        {
            get => _reconnectIfTheConnectionIsDropped;
            set
            {
                if (value == _reconnectIfTheConnectionIsDropped)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_ReconnectIfTheConnectionIsDropped = value;

                _reconnectIfTheConnectionIsDropped = value;
                OnPropertyChanged();
            }
        }
        public IEnumerable<RemoteDesktop.NetworkConnectionType> NetworkConnectionTypes => System.Enum.GetValues(typeof(RemoteDesktop.NetworkConnectionType)).Cast<RemoteDesktop.NetworkConnectionType>();

        private RemoteDesktop.NetworkConnectionType _networkConnectionType;
        public RemoteDesktop.NetworkConnectionType NetworkConnectionType
        {
            get => _networkConnectionType;
            set
            {
                if (Equals(value, _networkConnectionType))
                    return;

                if (!_isLoading)
                {
                    ChangeNetworkConnectionTypeSettings(value);
                    SettingsManager.Current.RemoteDesktop_NetworkConnectionType = value;
                }

                _networkConnectionType = value;
                OnPropertyChanged();
            }
        }

        private bool _desktopBackground;
        public bool DesktopBackground
        {
            get => _desktopBackground;
            set
            {
                if (value == _desktopBackground)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_DesktopBackground = value;

                _desktopBackground = value;
                OnPropertyChanged();
            }
        }

        private bool _fontSmoothing;
        public bool FontSmoothing
        {
            get => _fontSmoothing;
            set
            {
                if (value == _fontSmoothing)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_FontSmoothing = value;

                _fontSmoothing = value;
                OnPropertyChanged();
            }
        }

        private bool _desktopComposition;
        public bool DesktopComposition
        {
            get => _desktopComposition;
            set
            {
                if (value == _desktopComposition)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_DesktopComposition = value;

                _desktopComposition = value;
                OnPropertyChanged();
            }
        }

        private bool _showWindowContentsWhileDragging;
        public bool ShowWindowContentsWhileDragging
        {
            get => _showWindowContentsWhileDragging;
            set
            {
                if (value == _showWindowContentsWhileDragging)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_ShowWindowContentsWhileDragging = value;

                _showWindowContentsWhileDragging = value;
                OnPropertyChanged();
            }
        }

        private bool _menuAndWindowAnimation;
        public bool MenuAndWindowAnimation
        {
            get => _menuAndWindowAnimation;
            set
            {
                if (value == _menuAndWindowAnimation)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_MenuAndWindowAnimation = value;

                _menuAndWindowAnimation = value;
                OnPropertyChanged();
            }
        }

        private bool _visualStyles;
        public bool VisualStyles
        {
            get => _visualStyles;
            set
            {
                if (value == _visualStyles)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_VisualStyles = value;

                _visualStyles = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public RemoteDesktopSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            AdjustScreenAutomatically = SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically;
            UseCurrentViewSize = SettingsManager.Current.RemoteDesktop_UseCurrentViewSize;
            UseFixedScreenSize = SettingsManager.Current.RemoteDesktop_UseFixedScreenSize;
            SelectedScreenResolution = ScreenResolutions.FirstOrDefault(x => x == $"{SettingsManager.Current.RemoteDesktop_ScreenWidth}x{SettingsManager.Current.RemoteDesktop_ScreenHeight}");
            UseCustomScreenSize = SettingsManager.Current.RemoteDesktop_UseCustomScreenSize;
            CustomScreenWidth = SettingsManager.Current.RemoteDesktop_CustomScreenWidth.ToString();
            CustomScreenHeight = SettingsManager.Current.RemoteDesktop_CustomScreenHeight.ToString();
            SelectedColorDepth = ColorDepths.FirstOrDefault(x => x == SettingsManager.Current.RemoteDesktop_ColorDepth);
            Port = SettingsManager.Current.RemoteDesktop_Port;
            EnableCredSspSupport = SettingsManager.Current.RemoteDesktop_EnableCredSspSupport;
            AuthenticationLevel = SettingsManager.Current.RemoteDesktop_AuthenticationLevel;
            AudioRedirectionMode = AudioRedirectionModes.FirstOrDefault(x => x == SettingsManager.Current.RemoteDesktop_AudioRedirectionMode);
            AudioCaptureRedirectionMode = AudioCaptureRedirectionModes.FirstOrDefault(x => x == SettingsManager.Current.RemoteDesktop_AudioCaptureRedirectionMode);
            KeyboardHookMode = KeyboardHookModes.FirstOrDefault(x => x == SettingsManager.Current.RemoteDesktop_KeyboardHookMode);
            RedirectClipboard = SettingsManager.Current.RemoteDesktop_RedirectClipboard;
            RedirectDevices = SettingsManager.Current.RemoteDesktop_RedirectDevices;
            RedirectDrives = SettingsManager.Current.RemoteDesktop_RedirectDrives;
            RedirectPorts = SettingsManager.Current.RemoteDesktop_RedirectPorts;
            RedirectSmartCards = SettingsManager.Current.RemoteDesktop_RedirectSmartCards;
            RedirectPrinters = SettingsManager.Current.RemoteDesktop_RedirectPrinters;
            PersistentBitmapCaching = SettingsManager.Current.RemoteDesktop_PersistentBitmapCaching;
            ReconnectIfTheConnectionIsDropped = SettingsManager.Current.RemoteDesktop_ReconnectIfTheConnectionIsDropped;
            NetworkConnectionType = NetworkConnectionTypes.FirstOrDefault(x => x == SettingsManager.Current.RemoteDesktop_NetworkConnectionType);
            DesktopBackground = SettingsManager.Current.RemoteDesktop_DesktopBackground;
            FontSmoothing = SettingsManager.Current.RemoteDesktop_FontSmoothing;
            DesktopComposition = SettingsManager.Current.RemoteDesktop_DesktopComposition;
            ShowWindowContentsWhileDragging = SettingsManager.Current.RemoteDesktop_ShowWindowContentsWhileDragging;
            MenuAndWindowAnimation = SettingsManager.Current.RemoteDesktop_MenuAndWindowAnimation;
            VisualStyles = SettingsManager.Current.RemoteDesktop_VisualStyles;
        }
        #endregion

        #region Methods
        private void ChangeNetworkConnectionTypeSettings(RemoteDesktop.NetworkConnectionType networkConnectionType)
        {
            switch (networkConnectionType)
            {
                case RemoteDesktop.NetworkConnectionType.Modem:
                    DesktopBackground = false;
                    FontSmoothing = false;
                    DesktopComposition = false;
                    ShowWindowContentsWhileDragging = false;
                    MenuAndWindowAnimation = false;
                    VisualStyles = false;
                    break;
                case RemoteDesktop.NetworkConnectionType.BroadbandLow:
                    DesktopBackground = false;
                    FontSmoothing = false;
                    DesktopComposition = false;
                    ShowWindowContentsWhileDragging = false;
                    MenuAndWindowAnimation = false;
                    VisualStyles = true;
                    break;
                case RemoteDesktop.NetworkConnectionType.Satellite:
                case RemoteDesktop.NetworkConnectionType.BroadbandHigh:
                    DesktopBackground = false;
                    FontSmoothing = false;
                    DesktopComposition = true;
                    ShowWindowContentsWhileDragging = false;
                    MenuAndWindowAnimation = false;
                    VisualStyles = true;
                    break;
                case RemoteDesktop.NetworkConnectionType.WAN:
                case RemoteDesktop.NetworkConnectionType.LAN:
                    DesktopBackground = true;
                    FontSmoothing = true;
                    DesktopComposition = true;
                    ShowWindowContentsWhileDragging = true;
                    MenuAndWindowAnimation = true;
                    VisualStyles = true;
                    break;
            }
        }
        #endregion
    }
}