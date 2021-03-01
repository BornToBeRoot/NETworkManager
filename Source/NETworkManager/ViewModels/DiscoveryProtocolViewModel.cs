using System.Collections.Generic;
using System.Windows.Input;
using System.Net.NetworkInformation;
using System;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using NETworkManager.Models.Network;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using NETworkManager.Utilities;
using System.Windows;
using MahApps.Metro.Controls;
using static NETworkManager.Models.Network.DiscoveryProtocol;
using System.Windows.Threading;

namespace NETworkManager.ViewModels
{
    public class DiscoveryProtocolViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private DiscoveryProtocol _discoveryProtocol = new DiscoveryProtocol();
        private readonly bool _isLoading;
        System.Timers.Timer _remainingTimer;
        private int _secondsRemaining;

        private bool _firstRun = true;
        public bool FirstRun
        {
            get => _firstRun;
            set
            {
                if (value == _firstRun)
                    return;

                _firstRun = value;
                OnPropertyChanged();
            }
        }

        private bool _isNetworkInteraceLoading;
        public bool IsNetworkInterfaceLoading
        {
            get => _isNetworkInteraceLoading;
            set
            {
                if (value == _isNetworkInteraceLoading)
                    return;

                _isNetworkInteraceLoading = value;
                OnPropertyChanged();
            }
        }

        private List<NetworkInterfaceInfo> _networkInterfaces;
        public List<NetworkInterfaceInfo> NetworkInterfaces
        {
            get => _networkInterfaces;
            set
            {
                if (value == _networkInterfaces)
                    return;

                _networkInterfaces = value;
                OnPropertyChanged();
            }
        }

        private NetworkInterfaceInfo _selectedNetworkInterface;
        public NetworkInterfaceInfo SelectedNetworkInterface
        {
            get => _selectedNetworkInterface;
            set
            {
                if (value == _selectedNetworkInterface)
                    return;

                if (value != null)
                {
                    if (!_isLoading)
                        SettingsManager.Current.DiscoveryProtocol_InterfaceId = value.Id;

                    CanCapture = value.IsOperational;
                }

                _selectedNetworkInterface = value;
                OnPropertyChanged();
            }
        }

        private List<Protocol> _protocols = new List<Protocol>();
        public List<Protocol> Protocols
        {
            get => _protocols;
            set
            {
                if (value == _protocols)
                    return;

                _protocols = value;
                OnPropertyChanged();
            }
        }

        private Protocol _selectedProtocol;
        public Protocol SelectedProtocol
        {
            get => _selectedProtocol;
            set
            {
                if (value == _selectedProtocol)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DiscoveryProtocol_Protocol = value;

                _selectedProtocol = value;
                OnPropertyChanged();
            }
        }

        private List<int> _durations;
        public List<int> Durations
        {
            get => _durations;
            set
            {
                if (value == _durations)
                    return;

                _durations = value;
                OnPropertyChanged();
            }
        }

        private int _selectedDuration;
        public int SelectedDuration
        {
            get => _selectedDuration;
            set
            {
                if (value == _selectedDuration)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DiscoveryProtocol_Duration = value;

                _selectedDuration = value;
                OnPropertyChanged();
            }
        }

        private bool _canCapture;
        public bool CanCapture
        {
            get => _canCapture;
            set
            {
                if (value == _canCapture)
                    return;

                _canCapture = value;
                OnPropertyChanged();
            }
        }

        private bool _isCapturing;
        public bool IsCapturing
        {
            get => _isCapturing;
            set
            {
                if (value == _isCapturing)
                    return;

                _isCapturing = value;
                OnPropertyChanged();
            }
        }

        private string _timeRemainingMessage;
        public string TimeRemainingMessage
        {
            get => _timeRemainingMessage;
            set
            {
                if (value == _timeRemainingMessage)
                    return;

                _timeRemainingMessage = value;
                OnPropertyChanged();
            }
        }
                
        private bool _isStatusMessageDisplayed;
        public bool IsStatusMessageDisplayed
        {
            get => _isStatusMessageDisplayed;
            set
            {
                if (value == _isStatusMessageDisplayed)
                    return;

                _isStatusMessageDisplayed = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _discoveryPackageReceived;
        public bool DiscoveryPackageReceived
        {
            get => _discoveryPackageReceived;
            set
            {
                if (value == _discoveryPackageReceived)
                    return;

                _discoveryPackageReceived = value;
                OnPropertyChanged();
            }
        }

        private DiscoveryProtocolPackageInfo _discoveryPackage;
        public DiscoveryProtocolPackageInfo DiscoveryPackage
        {
            get => _discoveryPackage;
            set
            {
                if (value == _discoveryPackage)
                    return;

                _discoveryPackage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public DiscoveryProtocolViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            LoadNetworkInterfaces();

            // Detect if network address or status changed...
            NetworkChange.NetworkAvailabilityChanged += (sender, args) => ReloadNetworkInterfacesAction();
            NetworkChange.NetworkAddressChanged += (sender, args) => ReloadNetworkInterfacesAction();
            
            _discoveryProtocol.PackageReceived += _discoveryProtocol_PackageReceived;
            _discoveryProtocol.ErrorReceived += _discoveryProtocol_ErrorReceived;
            _discoveryProtocol.WarningReceived += _discoveryProtocol_WarningReceived;
            _discoveryProtocol.Complete += _discoveryProtocol_Complete;

            _remainingTimer = new System.Timers.Timer
            {
                Interval = 1000
            };

            _remainingTimer.Elapsed += Timer_Elapsed;

            LoadSettings();

            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        private async void LoadNetworkInterfaces()
        {
            IsNetworkInterfaceLoading = true;

            NetworkInterfaces = await Models.Network.NetworkInterface.GetNetworkInterfacesAsync();

            // Get the last selected interface, if it is still available on this machine...
            if (NetworkInterfaces.Count > 0)
            {
                var info = NetworkInterfaces.FirstOrDefault(s => s.Id == SettingsManager.Current.DiscoveryProtocol_InterfaceId);

                SelectedNetworkInterface = info ?? NetworkInterfaces[0];
            }

            IsNetworkInterfaceLoading = false;
        }

        private void LoadSettings()
        {
            Protocols = System.Enum.GetValues(typeof(Protocol)).Cast<Protocol>().OrderBy(x => x.ToString()).ToList();
            SelectedProtocol = Protocols.FirstOrDefault(x => x == SettingsManager.Current.DiscoveryProtocol_Protocol);
            Durations = new List<int>() { 15, 30, 60, 90, 120 };
            SelectedDuration = Durations.FirstOrDefault(x => x == SettingsManager.Current.DiscoveryProtocol_Duration);
        }
        #endregion

        #region ICommands & Actions
        public ICommand ReloadNetworkInterfacesCommand => new RelayCommand(p => ReloadNetworkInterfacesAction(), ReloadNetworkInterfaces_CanExecute);

        private bool ReloadNetworkInterfaces_CanExecute(object obj) => !IsNetworkInterfaceLoading && Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private async void ReloadNetworkInterfacesAction()
        {
            IsNetworkInterfaceLoading = true;

            await Task.Delay(2000); // Make the user happy, let him see a reload animation (and he cannot spam the reload command)

            var id = string.Empty;

            if (SelectedNetworkInterface != null)
                id = SelectedNetworkInterface.Id;

            NetworkInterfaces = await Models.Network.NetworkInterface.GetNetworkInterfacesAsync();

            // Change interface...
            SelectedNetworkInterface = string.IsNullOrEmpty(id) ? NetworkInterfaces.FirstOrDefault() : NetworkInterfaces.FirstOrDefault(x => x.Id == id);

            IsNetworkInterfaceLoading = false;
        }

        public ICommand OpenNetworkConnectionsCommand => new RelayCommand(p => OpenNetworkConnectionsAction());

        public async void OpenNetworkConnectionsAction()
        {
            try
            {
                Process.Start("NCPA.cpl");
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }
        }

        public ICommand RestartAsAdminCommand => new RelayCommand(p => RestartAsAdminAction());

        public async void RestartAsAdminAction()
        {
            try
            {
                (Application.Current.MainWindow as MainWindow).RestartApplication(true);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }
        }

        public ICommand CaptureCommand => new RelayCommand(p => CaptureAction());

        public async void CaptureAction()
        {
            if (FirstRun)
                FirstRun = false;

            IsStatusMessageDisplayed = false;
            StatusMessage = string.Empty;

            DiscoveryPackageReceived = false;

            IsCapturing = true;

            int duration = SelectedDuration + 2; // Capture 2 seconds more than the user chose

            _secondsRemaining = duration + 1; // Init powershell etc. takes some time... 

            TimeRemainingMessage = string.Format(Localization.Resources.Strings.XXSecondsRemainingDots, _secondsRemaining);

            _remainingTimer.Start();

            try
            {
                _discoveryProtocol.CaptureAsync(SelectedNetworkInterface.Name, duration, SelectedProtocol);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }            
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                TimeRemainingMessage = string.Format(Localization.Resources.Strings.XXSecondsRemainingDots, _secondsRemaining);

                if (_secondsRemaining > 0)
                    _secondsRemaining--;
            }));
        }
        #endregion

        #region Methods   
        public void OnViewVisible()
        {

        }

        public void OnViewHide()
        {

        }
        #endregion

        #region Events
        private void _discoveryProtocol_PackageReceived(object sender, DiscoveryProtocolPackageArgs e)
        {
            DiscoveryPackage = DiscoveryProtocolPackageInfo.Parse(e);

            DiscoveryPackageReceived = true;
        }

        private void _discoveryProtocol_WarningReceived(object sender, DiscoveryProtocolWarningArgs e)
        {
            if (!string.IsNullOrEmpty(StatusMessage))
                StatusMessage += Environment.NewLine;

            StatusMessage += e.Message;

            IsStatusMessageDisplayed = true;
        }

        private void _discoveryProtocol_ErrorReceived(object sender, DiscoveryProtocolErrorArgs e)
        {            
            if (!string.IsNullOrEmpty(StatusMessage))
                StatusMessage += Environment.NewLine;

            StatusMessage += e.Message;

            IsStatusMessageDisplayed = true;
        }

        private void _discoveryProtocol_Complete(object sender, EventArgs e)
        {
            _remainingTimer.Stop();
            IsCapturing = false;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
        #endregion
    }
}
