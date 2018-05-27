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
using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace NETworkManager.ViewModels
{
    public class IPScannerHostViewModel : ViewModelBase
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
        ICollectionView _ipScannerProfiles;
        public ICollectionView IPScannerProfiles
        {
            get { return _ipScannerProfiles; }
        }

        private IPScannerProfileInfo _selectedProfile = new IPScannerProfileInfo();
        public IPScannerProfileInfo SelectedProfile
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

                IPScannerProfiles.Refresh();

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
                    SettingsManager.Current.IPScanner_ExpandProfileView = value;

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
                    SettingsManager.Current.IPScanner_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(dueToChangedSize: true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public IPScannerHostViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            InterTabClient = new DragablzInterTabClient(ApplicationViewManager.Name.IPScanner);

            TabItems = new ObservableCollection<DragablzTabItem>()
            {
                new DragablzTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new IPScannerView(_tabId), _tabId)
            };

            // Load profiles
            if (IPScannerProfileManager.Profiles == null)
                IPScannerProfileManager.Load();

            _ipScannerProfiles = CollectionViewSource.GetDefaultView(IPScannerProfileManager.Profiles);
            _ipScannerProfiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(IPScannerProfileInfo.Group)));
            _ipScannerProfiles.SortDescriptions.Add(new SortDescription(nameof(IPScannerProfileInfo.Group), ListSortDirection.Ascending));
            _ipScannerProfiles.SortDescriptions.Add(new SortDescription(nameof(IPScannerProfileInfo.Name), ListSortDirection.Ascending));
            _ipScannerProfiles.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                IPScannerProfileInfo info = o as IPScannerProfileInfo;

                string search = Search.Trim();

                // Search by: Name
                return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
            };

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.IPScanner_ExpandProfileView;

            if (ExpandProfileView)
                ProfileWidth = new GridLength(SettingsManager.Current.IPScanner_ProfileWidth);
            else
                ProfileWidth = new GridLength(40);

            _tempProfileWidth = SettingsManager.Current.IPScanner_ProfileWidth;
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

            IPScannerProfileViewModel ipScannerProfileViewModel = new IPScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                IPScannerProfileInfo ipScannerProfileInfo = new IPScannerProfileInfo
                {
                    Name = instance.Name,
                    IPRange = instance.IPRange,
                    Group = instance.Group
                };

                IPScannerProfileManager.AddProfile(ipScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, IPScannerProfileManager.GetProfileGroups());

            customDialog.Content = new IPScannerProfileDialog
            {
                DataContext = ipScannerProfileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand ScanProfileCommand
        {
            get { return new RelayCommand(p => ScanProfileAction()); }
        }

        private void ScanProfileAction()
        {
            AddTab(SelectedProfile.IPRange);
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

            IPScannerProfileViewModel ipScannerProfileViewModel = new IPScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                IPScannerProfileManager.RemoveProfile(SelectedProfile);

                IPScannerProfileInfo ipScannerProfileInfo = new IPScannerProfileInfo
                {
                    Name = instance.Name,
                    IPRange = instance.IPRange,
                    Group = instance.Group
                };

                IPScannerProfileManager.AddProfile(ipScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, IPScannerProfileManager.GetProfileGroups(), SelectedProfile);

            customDialog.Content = new IPScannerProfileDialog
            {
                DataContext = ipScannerProfileViewModel
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

            IPScannerProfileViewModel ipScannerProfileViewModel = new IPScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                IPScannerProfileInfo ipScannerProfileInfo = new IPScannerProfileInfo
                {
                    Name = instance.Name,
                    IPRange = instance.IPRange,
                    Group = instance.Group
                };

                IPScannerProfileManager.AddProfile(ipScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, IPScannerProfileManager.GetProfileGroups(), SelectedProfile);

            customDialog.Content = new IPScannerProfileDialog
            {
                DataContext = ipScannerProfileViewModel
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

                IPScannerProfileManager.RemoveProfile(SelectedProfile);
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

                IPScannerProfileManager.RenameGroup(instance.OldGroup, instance.Group);

                _ipScannerProfiles.Refresh();
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
            ((args.DragablzItem.Content as DragablzTabItem).View as IPScannerView).CloseTab();
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

            TabItems.Add(new DragablzTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new IPScannerView(_tabId, host), _tabId));

            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}