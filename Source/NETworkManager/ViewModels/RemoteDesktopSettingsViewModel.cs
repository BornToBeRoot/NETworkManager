using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class RemoteDesktopSettingsViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private bool _adjustScreenAutomatically;
        public bool AdjustScreenAutomatically
        {
            get { return _adjustScreenAutomatically; }
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
            get { return _useCurrentViewSize; }
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
            get { return _useFixedScreenSize; }
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

        public List<string> ScreenResolutions
        {
            get
            {
                return new List<string>()
                {
                    "640x480",
                    "800x600",
                    "1024x768",
                    "1280x720",
                    "1280x768",
                    "1280x800",
                    "1280x1024",
                    "1366x768",
                    "1440x900",
                    "1400x1050",
                    "1680x1050",
                    "1920x1080"
                };
            }
        }

        private string _selectedScreenResolution;
        public string SelectedScreenResolution
        {
            get { return _selectedScreenResolution; }
            set
            {
                if (value == _selectedScreenResolution)
                    return;

                if (!_isLoading)
                {
                    string[] resolution = value.Split('x');

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
            get { return _useCustomScreenSize; }
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
            get { return _customScreenWidth; }
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
            get { return _customScreenHeight; }
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

        public List<int> ColorDepths
        {
            get
            {
                return new List<int>()
                {
                    15,
                    16,
                    24,
                    32
                };
            }
        }

        private int _selectedColorDepth;
        public int SelectedColorDepth
        {
            get { return _selectedColorDepth; }
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
            get { return _port; }
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
            get { return _enableCredSspSupport; }
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
            get { return _authenticationLevel; }
            set
            {
                if (value == _selectedColorDepth)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.RemoteDesktop_AuthenticationLevel = value;

                _authenticationLevel = value;
                OnPropertyChanged();
            }
        }

        // String must be formatted ...
        public string RDPAuthenticationLevelHelpMessage
        {
            get { return LocalizationManager.GetStringByKey("String_HelpMessage_RDPAuthenticationLevel"); }
        }

        private bool _redirectClipboard;
        public bool RedirectClipboard
        {
            get { return _redirectClipboard; }
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
            get { return _redirectDevices; }
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
            get { return _redirectDrives; }
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
            get { return _redirectPorts; }
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
            get { return _redirectSmartCards; }
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
            get { return _redirectPrinters; }
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
        #endregion

        #region Constructor, load settings
        public RemoteDesktopSettingsViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            AdjustScreenAutomatically = SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically;
            UseCurrentViewSize = SettingsManager.Current.RemoteDesktop_UseCurrentViewSize;
            UseFixedScreenSize = SettingsManager.Current.RemoteDesktop_UseFixedScreenSize;
            SelectedScreenResolution = ScreenResolutions.FirstOrDefault(x => x == string.Format("{0}x{1}", SettingsManager.Current.RemoteDesktop_ScreenWidth, SettingsManager.Current.RemoteDesktop_ScreenHeight));
            UseCustomScreenSize = SettingsManager.Current.RemoteDesktop_UseCustomScreenSize;
            CustomScreenWidth = SettingsManager.Current.RemoteDesktop_CustomScreenWidth.ToString();
            CustomScreenHeight = SettingsManager.Current.RemoteDesktop_CustomScreenHeight.ToString();
            SelectedColorDepth = ColorDepths.FirstOrDefault(x => x == SettingsManager.Current.RemoteDesktop_ColorDepth);
            Port = SettingsManager.Current.RemoteDesktop_Port;
            EnableCredSspSupport = SettingsManager.Current.RemoteDesktop_EnableCredSspSupport;
            AuthenticationLevel = SettingsManager.Current.RemoteDesktop_AuthenticationLevel;
            RedirectClipboard = SettingsManager.Current.RemoteDesktop_RedirectClipboard;
            RedirectDevices = SettingsManager.Current.RemoteDesktop_RedirectDevices;
            RedirectDrives = SettingsManager.Current.RemoteDesktop_RedirectDrives;
            RedirectPorts = SettingsManager.Current.RemoteDesktop_RedirectPorts;
            RedirectSmartCards = SettingsManager.Current.RemoteDesktop_RedirectSmartCards;
            RedirectPrinters = SettingsManager.Current.RemoteDesktop_RedirectPrinters;
        }
        #endregion
    }
}