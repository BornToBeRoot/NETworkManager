using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsApplicationRemoteDesktopViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

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

                    SettingsManager.Current.RemoteDesktop_DesktopWidth = int.Parse(resolution[0]);
                    SettingsManager.Current.RemoteDesktop_DesktopHeight = int.Parse(resolution[1]);
                }

                _selectedScreenResolution = value;
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
            { if (value == _selectedColorDepth)
                    return;

                _selectedColorDepth = value;
                OnPropertyChanged();
            }
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
        public SettingsApplicationRemoteDesktopViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            SelectedScreenResolution = ScreenResolutions.FirstOrDefault(x => x == string.Format("{0}x{1}", SettingsManager.Current.RemoteDesktop_DesktopWidth, SettingsManager.Current.RemoteDesktop_DesktopHeight));
            SelectedColorDepth = ColorDepths.FirstOrDefault(x => x == SettingsManager.Current.RemoteDesktop_ColorDepth);
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