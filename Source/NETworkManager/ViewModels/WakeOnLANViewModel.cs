using NETworkManager.Models.Settings;
using System.Net;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using NETworkManager.Models.Network;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Views;
using NETworkManager.Utilities;
using System.Threading.Tasks;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class WakeOnLANViewModel : ViewModelBase
    {
        #region  Variables 
        private IDialogCoordinator dialogCoordinator;

        private bool _isLoading = true;

        private bool _isSending;
        public bool IsSending
        {
            get { return _isSending; }
            set
            {
                if (value == _isSending)
                    return;

                _isSending = value;
                OnPropertyChanged();
            }
        }

        private string _MACAddress;
        public string MACAddress
        {
            get { return _MACAddress; }
            set
            {
                if (value == _MACAddress)
                    return;

                _MACAddress = value;
                OnPropertyChanged();
            }
        }

        private bool _macAddressHasError;
        public bool MACAddressHasError
        {
            get { return _macAddressHasError; }
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
            get { return _broadcast; }
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
            get { return _broadcastHasError; }
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
            get { return _port; }
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
            get { return _portHasError; }
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
            get { return _displayStatusMessage; }
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
            get { return _statusMessage; }
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        #region Clients
        ICollectionView _profiles;
        public ICollectionView Profiles
        {
            get { return _profiles; }
        }

        private ProfileInfo _selectedProfile;
        public ProfileInfo SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                if (value == _selectedProfile)
                    return;

                if (value != null && !IsSending)
                {
                    MACAddress = value.WakeOnLAN_MACAddress;
                    Broadcast = value.WakeOnLAN_Broadcast;
                    Port = value.WakeOnLAN_Port;
                }

                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        private string _search;
        public string Search
        {
            get { return _search; }
            set
            {
                if (value == _search)
                    return;

                _search = value;

                Profiles.Refresh();

                OnPropertyChanged();
            }
        }

        private bool _canProfileWidthChange = true;
        private double _tempProfileWidth;

        private bool _expandProfileView;
        public bool ExpandProfileView
        {
            get { return _expandProfileView; }
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.WakeOnLAN_ExpandClientView = value;

                _expandProfileView = value;

                if (_canProfileWidthChange)
                    ResizeClient(dueToChangedSize: false);

                OnPropertyChanged();
            }
        }

        private GridLength _profileWidth;
        public GridLength ProfileWidth
        {
            get { return _profileWidth; }
            set
            {
                if (value == _profileWidth)
                    return;

                if (!_isLoading && value.Value != 40) // Do not save the size when collapsed
                    SettingsManager.Current.WakeOnLAN_ClientWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeClient(dueToChangedSize: true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public WakeOnLANViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            _profiles = new CollectionViewSource { Source = ProfileManager.Profiles }.View;
            _profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            _profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            _profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            _profiles.Filter = o =>
            {
                ProfileInfo info = o as ProfileInfo;

                if (string.IsNullOrEmpty(Search))
                    return info.WakeOnLAN_Enabled;

                string search = Search.Trim();

                // Search by: Name
                return (info.WakeOnLAN_Enabled && info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.WakeOnLAN_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();


            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Port = SettingsManager.Current.WakeOnLAN_DefaultPort;
            ExpandProfileView = SettingsManager.Current.WakeOnLAN_ExpandClientView;

            if (ExpandProfileView)
                ProfileWidth = new GridLength(SettingsManager.Current.WakeOnLAN_ClientWidth);
            else
                ProfileWidth = new GridLength(40);

            _tempProfileWidth = SettingsManager.Current.WakeOnLAN_ClientWidth;
        }
        #endregion

        #region ICommands & Actions
        public ICommand WakeUpCommand
        {
            get { return new RelayCommand(p => WakeUpAction(), WakeUpAction_CanExecute); }
        }

        private bool WakeUpAction_CanExecute(object parameter)
        {
            return !MACAddressHasError && !BroadcastHasError && !PortHasError;
        }

        private void WakeUpAction()
        {
            WakeOnLANInfo info = new WakeOnLANInfo
            {
                MagicPacket = WakeOnLAN.CreateMagicPacket(MACAddress),
                Broadcast = IPAddress.Parse(Broadcast),
                Port = Port
            };

            WakeUp(info);
        }

        public ICommand WakeUpProfileCommand
        {
            get { return new RelayCommand(p => WakeUpProfileAction()); }
        }

        private void WakeUpProfileAction()
        {
            WakeOnLANInfo info = new WakeOnLANInfo
            {
                MagicPacket = WakeOnLAN.CreateMagicPacket(SelectedProfile.WakeOnLAN_MACAddress),
                Broadcast = IPAddress.Parse(SelectedProfile.WakeOnLAN_Broadcast),
                Port = SelectedProfile.WakeOnLAN_Port
            };

            WakeUp(info);
        }

        public ICommand AddProfileCommand
        {
            get { return new RelayCommand(p => AddProfileAction()); }
        }

        private async void AddProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_AddProfile")
            };

            ProfileViewModel profileViewModel = new ProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, ProfileManager.GetGroups());

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditProfileCommand
        {
            get { return new RelayCommand(p => EditProfileAction()); }
        }

        private async void EditProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_EditProfile")
            };

            ProfileViewModel profileViewModel = new ProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RemoveProfile(SelectedProfile);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, ProfileManager.GetGroups(), SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand CopyAsProfileCommand
        {
            get { return new RelayCommand(p => CopyAsProfileAction()); }
        }

        private async void CopyAsProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_CopyProfile")
            };

            ProfileViewModel profileViewModel = new ProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, ProfileManager.GetGroups(), SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DeleteClientCommand
        {
            get { return new RelayCommand(p => DeleteClientAction()); }
        }

        private async void DeleteClientAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_DeleteProfile")
            };

            ConfirmRemoveViewModel confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RemoveProfile(SelectedProfile);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, LocalizationManager.GetStringByKey("String_DeleteProfileMessage"));

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditGroupCommand
        {
            get { return new RelayCommand(p => EditGroupAction(p)); }
        }

        private async void EditGroupAction(object group)
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_EditGroup")
            };

            GroupViewModel editGroupViewModel = new GroupViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RenameGroup(instance.OldGroup, instance.Group);

                _profiles.Refresh();
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, group.ToString());

            customDialog.Content = new GroupDialog
            {
                DataContext = editGroupViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand ClearSearchCommand
        {
            get { return new RelayCommand(p => ClearSearchAction()); }
        }

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

                StatusMessage = LocalizationManager.GetStringByKey("String_MagicPacketSuccessfulSended");
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
                if (ProfileWidth.Value == 40)
                    ExpandProfileView = false;
                else
                    ExpandProfileView = true;
            }
            else
            {
                if (ExpandProfileView)
                {
                    if (_tempProfileWidth == 40)
                        ProfileWidth = new GridLength(250);
                    else
                        ProfileWidth = new GridLength(_tempProfileWidth);
                }
                else
                {
                    _tempProfileWidth = ProfileWidth.Value;
                    ProfileWidth = new GridLength(40);
                }
            }

            _canProfileWidthChange = true;
        }
        #endregion
    }
}
