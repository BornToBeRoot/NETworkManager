using NETworkManager.Models.Settings;
using System.Net;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using NETworkManager.Models.Network;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Utilities;
using System.Threading.Tasks;
using System.Linq;
using MahApps.Metro.Controls;

namespace NETworkManager.ViewModels
{
    public class WakeOnLANViewModel : ViewModelBase, IProfileManagerViewModel
    {
        #region  Variables 
        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly bool _isLoading;

        private bool _isSending;
        public bool IsSending
        {
            get => _isSending;
            set
            {
                if (value == _isSending)
                    return;

                _isSending = value;
                OnPropertyChanged();
            }
        }

        private string _macAddress;
        public string MACAddress
        {
            get => _macAddress;
            set
            {
                if (value == _macAddress)
                    return;

                _macAddress = value;
                OnPropertyChanged();
            }
        }

        private bool _macAddressHasError;
        public bool MACAddressHasError
        {
            get => _macAddressHasError;
            set
            {
                if (value == _macAddressHasError)
                    return;

                _macAddressHasError = value;
                OnPropertyChanged();
            }
        }

        private string _broadcast;
        public string Broadcast
        {
            get => _broadcast;
            set
            {
                if (value == _broadcast)
                    return;

                _broadcast = value;
                OnPropertyChanged();
            }
        }

        private bool _broadcastHasError;
        public bool BroadcastHasError
        {
            get => _broadcastHasError;
            set
            {
                if (value == _broadcastHasError)
                    return;

                _broadcastHasError = value;
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

                _port = value;
                OnPropertyChanged();
            }
        }

        private bool _portHasError;
        public bool PortHasError
        {
            get => _portHasError;
            set
            {
                if (value == _portHasError)
                    return;

                _portHasError = value;
                OnPropertyChanged();
            }
        }

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get => _displayStatusMessage;
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
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

        #region Profiles
        public ICollectionView Profiles { get; }

        private ProfileInfo _selectedProfile;
        public ProfileInfo SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (value == _selectedProfile)
                    return;

                if (value != null && !IsSending)
                {
                    MACAddress = value.WakeOnLAN_MACAddress;
                    Broadcast = value.WakeOnLAN_Broadcast;
                    Port = value.WakeOnLAN_OverridePort ? value.WakeOnLAN_Port : SettingsManager.Current.WakeOnLAN_Port;
                }

                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        private string _search;
        public string Search
        {
            get => _search;
            set
            {
                if (value == _search)
                    return;

                _search = value;

                RefreshProfiles();

                OnPropertyChanged();
            }
        }

        private bool _canProfileWidthChange = true;
        private double _tempProfileWidth;

        private bool _expandProfileView;
        public bool ExpandProfileView
        {
            get => _expandProfileView;
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.WakeOnLAN_ExpandClientView = value;

                _expandProfileView = value;

                if (_canProfileWidthChange)
                    ResizeClient(false);

                OnPropertyChanged();
            }
        }

        private GridLength _profileWidth;
        public GridLength ProfileWidth
        {
            get => _profileWidth;
            set
            {
                if (value == _profileWidth)
                    return;

                if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix) // Do not save the size when collapsed
                    SettingsManager.Current.WakeOnLAN_ClientWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeClient(true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public WakeOnLANViewModel(IDialogCoordinator instance)
        {
            _isLoading = true; 

            _dialogCoordinator = instance;

            Profiles = new CollectionViewSource { Source = ProfileManager.Profiles }.View;
            Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            Profiles.Filter = o =>
            {
                if (!(o is ProfileInfo info))
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return info.WakeOnLAN_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.WakeOnLAN_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                // Search by: Name, WakeOnLAN_MACAddress
                return info.WakeOnLAN_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.WakeOnLAN_MACAddress.Replace("-", "").Replace(":", "").IndexOf(search.Replace("-", "").Replace(":", ""), StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.WakeOnLAN_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();


            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Port = SettingsManager.Current.WakeOnLAN_Port;
            ExpandProfileView = SettingsManager.Current.WakeOnLAN_ExpandClientView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.WakeOnLAN_ClientWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

            _tempProfileWidth = SettingsManager.Current.WakeOnLAN_ClientWidth;
        }
        #endregion

        #region ICommands & Actions
        public ICommand WakeUpCommand => new RelayCommand(p => WakeUpAction(), WakeUpAction_CanExecute);

        private bool WakeUpAction_CanExecute(object parameter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen && !MACAddressHasError && !BroadcastHasError && !PortHasError;

        private void WakeUpAction()
        {
            var info = new WakeOnLANInfo
            {
                MagicPacket = WakeOnLAN.CreateMagicPacket(MACAddress),
                Broadcast = IPAddress.Parse(Broadcast),
                Port = Port
            };

            WakeUp(info);
        }

        public ICommand WakeUpProfileCommand => new RelayCommand(p => WakeUpProfileAction());

        private void WakeUpProfileAction()
        {
            WakeUp(WakeOnLAN.CreateWakeOnLANInfo(SelectedProfile));
        }

        public ICommand AddProfileCommand => new RelayCommand(p => AddProfileAction());

        private void AddProfileAction()
        {
            ProfileManager.ShowAddProfileDialog(this, _dialogCoordinator);
        }

        public ICommand EditProfileCommand => new RelayCommand(p => EditProfileAction());

        private void EditProfileAction()
        {
            ProfileManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand CopyAsProfileCommand => new RelayCommand(p => CopyAsProfileAction());

        private void CopyAsProfileAction()
        {
            ProfileManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand DeleteProfileCommand => new RelayCommand(p => DeleteProfileAction());

        private void DeleteProfileAction()
        {
            ProfileManager.ShowDeleteProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

        private void EditGroupAction(object group)
        {
            ProfileManager.ShowEditGroupDialog(this, _dialogCoordinator, group.ToString());
        }

        public ICommand ClearSearchCommand => new RelayCommand(p => ClearSearchAction());

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }
        #endregion

        #region Methods
        private async void WakeUp(WakeOnLANInfo info)
        {
            DisplayStatusMessage = false;
            IsSending = true;

            try
            {
                WakeOnLAN.Send(info);

                await Task.Delay(2000); // Make the user happy, let him see a reload animation (and he cannot spam the send command)

                StatusMessage = Resources.Localization.Strings.MagicPacketSentMessage;
                DisplayStatusMessage = true;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }
            finally
            {
                IsSending = false;
            }
        }

        private void ResizeClient(bool dueToChangedSize)
        {
            _canProfileWidthChange = false;

            if (dueToChangedSize)
            {
                ExpandProfileView = Math.Abs(ProfileWidth.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix;
            }
            else
            {
                if (ExpandProfileView)
                {
                    ProfileWidth = Math.Abs(_tempProfileWidth - GlobalStaticConfiguration.Profile_WidthCollapsed) < GlobalStaticConfiguration.FloatPointFix ? new GridLength(GlobalStaticConfiguration.Profile_DefaultWidthExpanded) : new GridLength(_tempProfileWidth);
                }
                else
                {
                    _tempProfileWidth = ProfileWidth.Value;
                    ProfileWidth = new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);
                }
            }

            _canProfileWidthChange = true;
        }

        public void OnViewVisible()
        {
            RefreshProfiles();
        }

        public void OnViewHide()
        {

        }

        public void RefreshProfiles()
        {
            Profiles.Refresh();
        }

        public void OnProfileDialogOpen()
        {

        }

        public void OnProfileDialogClose()
        {

        }
        #endregion
    }
}
