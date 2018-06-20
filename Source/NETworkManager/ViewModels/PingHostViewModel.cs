using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Models.Settings;
using System.ComponentModel;
using System;
using System.Windows.Data;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace NETworkManager.ViewModels
{
    public class PingHostViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzTabItem> TabItems { get; private set; }

        private const string tagIdentifier = "tag=";

        private bool _isLoading = true;

        private int _tabId = 0;

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (value == _selectedTabIndex)
                    return;

                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }

        #region Profiles
        ICollectionView _profiles;
        public ICollectionView Profiles
        {
            get { return _profiles; }
        }

        private ProfileInfo _selectedProfile = new ProfileInfo();
        public ProfileInfo SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                if (value == _selectedProfile)
                    return;

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
                    SettingsManager.Current.Ping_ExpandProfileView = value;

                _expandProfileView = value;

                if (_canProfileWidthChange)
                    ResizeProfile(dueToChangedSize: false);

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
                    SettingsManager.Current.Ping_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(dueToChangedSize: true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public PingHostViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            InterTabClient = new DragablzInterTabClient(ApplicationViewManager.Name.Ping);

            TabItems = new ObservableCollection<DragablzTabItem>()
            {
                new DragablzTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new PingView(_tabId), _tabId)
            };

            _profiles = new CollectionViewSource { Source = ProfileManager.Profiles }.View;
            _profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            _profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            _profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            _profiles.Filter = o =>
            {
                ProfileInfo info = o as ProfileInfo;

                if (string.IsNullOrEmpty(Search))
                    return info.Ping_Enabled;

                string search = Search.Trim();

                // Search by: Name
                return (info.Ping_Enabled && info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.Ping_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.Ping_ExpandProfileView;

            if (ExpandProfileView)
                ProfileWidth = new GridLength(SettingsManager.Current.Ping_ProfileWidth);
            else
                ProfileWidth = new GridLength(40);

            _tempProfileWidth = SettingsManager.Current.Ping_ProfileWidth;
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddTabCommand
        {
            get { return new RelayCommand(p => AddTabAction()); }
        }

        private void AddTabAction()
        {
            AddTab();
        }

        public ICommand PingProfileCommand
        {
            get { return new RelayCommand(p => PingProfileAction()); }
        }

        private void PingProfileAction()
        {
            AddTab(SelectedProfile.Ping_Host);
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

        public ICommand DeleteProfileCommand
        {
            get { return new RelayCommand(p => DeleteProfileAction()); }
        }

        private async void DeleteProfileAction()
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

        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem).View as PingView).CloseTab();
        }
        #endregion

        #region Methods
        private void ResizeProfile(bool dueToChangedSize)
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

        public void AddTab(string host = null)
        {
            _tabId++;

            TabItems.Add(new DragablzTabItem(host ?? LocalizationManager.GetStringByKey("String_Header_NewTab"), new PingView(_tabId, host), _tabId));

            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}