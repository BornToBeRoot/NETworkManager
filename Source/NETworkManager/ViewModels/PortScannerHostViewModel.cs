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

namespace NETworkManager.ViewModels
{
    public class PortScannerHostViewModel : ViewModelBase
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
        ICollectionView _portScannerProfiles;
        public ICollectionView PortScannerProfiles
        {
            get { return _portScannerProfiles; }
        }

        private PortScannerProfileInfo _selectedProfile = new PortScannerProfileInfo();
        public PortScannerProfileInfo SelectedProfile
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

        private bool _expandProfileView;
        public bool ExpandProfileView
        {
            get { return _expandProfileView; }
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ExpandProfileView = value;

                _expandProfileView = value;
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

                PortScannerProfiles.Refresh();

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor
        public PortScannerHostViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            InterTabClient = new DragablzPortScannerInterTabClient();

            TabItems = new ObservableCollection<DragablzTabItem>()
            {
                new DragablzTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new PortScannerView(_tabId), _tabId)
            };

            // Load profiles
            if (PortScannerProfileManager.Profiles == null)
                PortScannerProfileManager.Load();

            _portScannerProfiles = CollectionViewSource.GetDefaultView(PortScannerProfileManager.Profiles);
            _portScannerProfiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PortScannerProfileInfo.Group)));
            _portScannerProfiles.SortDescriptions.Add(new SortDescription(nameof(PortScannerProfileInfo.Group), ListSortDirection.Ascending));
            _portScannerProfiles.SortDescriptions.Add(new SortDescription(nameof(PortScannerProfileInfo.Name), ListSortDirection.Ascending));
            _portScannerProfiles.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                PortScannerProfileInfo info = o as PortScannerProfileInfo;

                string search = Search.Trim();

                // Search by: Name
                return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
            };

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.PortScanner_ExpandProfileView;
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

        public ICommand ScanProfileCommand
        {
            get { return new RelayCommand(p => ScanProfileAction()); }
        }

        private void ScanProfileAction()
        {
            AddTab(SelectedProfile.Hostname, SelectedProfile.Ports);
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

            PortScannerProfileViewModel portScannerProfileViewModel = new PortScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                PortScannerProfileInfo portScannerProfileInfo = new PortScannerProfileInfo
                {
                    Name = instance.Name,
                    Hostname = instance.Hostname,
                    Ports = instance.Ports,
                    Group = instance.Group
                };

                PortScannerProfileManager.AddProfile(portScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, PortScannerProfileManager.GetProfileGroups());

            customDialog.Content = new PortScannerProfileDialog
            {
                DataContext = portScannerProfileViewModel
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

            PortScannerProfileViewModel portScannerProfileViewModel = new PortScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                PortScannerProfileManager.RemoveProfile(SelectedProfile);

                PortScannerProfileInfo portScannerProfileInfo = new PortScannerProfileInfo
                {
                    Name = instance.Name,
                    Hostname = instance.Hostname,
                    Ports = instance.Ports,
                    Group = instance.Group
                };

                PortScannerProfileManager.AddProfile(portScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, PortScannerProfileManager.GetProfileGroups(), SelectedProfile);

            customDialog.Content = new PortScannerProfileDialog
            {
                DataContext = portScannerProfileViewModel
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

            PortScannerProfileViewModel portScannerProfileViewModel = new PortScannerProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                PortScannerProfileInfo portScannerProfileInfo = new PortScannerProfileInfo
                {
                    Name = instance.Name,
                    Hostname = instance.Hostname,
                    Ports = instance.Ports,
                    Group = instance.Group
                };

                PortScannerProfileManager.AddProfile(portScannerProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, PortScannerProfileManager.GetProfileGroups(), SelectedProfile);

            customDialog.Content = new PortScannerProfileDialog
            {
                DataContext = portScannerProfileViewModel
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

                PortScannerProfileManager.RemoveProfile(SelectedProfile);
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

                PortScannerProfileManager.RenameGroup(instance.OldGroup, instance.Group);

                _portScannerProfiles.Refresh();
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
            ((args.DragablzItem.Content as DragablzTabItem).View as PortScannerView).CloseTab();
        }
        #endregion

        #region Methods
        public void AddTab(string host = null, string ports = null)
        {
            _tabId++;

            TabItems.Add(new DragablzTabItem(host ?? LocalizationManager.GetStringByKey("String_Header_NewTab"), new PortScannerView(_tabId, host, ports), _tabId));

            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}