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

namespace NETworkManager.ViewModels
{
    public class TracerouteHostViewModel : ViewModelBase
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
        ICollectionView _tracerouteProfiles;
        public ICollectionView TracerouteProfiles
        {
            get { return _tracerouteProfiles; }
        }

        private TracerouteProfileInfo _selectedProfile = new TracerouteProfileInfo();
        public TracerouteProfileInfo SelectedProfile
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
                    SettingsManager.Current.Traceroute_ExpandProfileView = value;

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

                TracerouteProfiles.Refresh();

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor
        public TracerouteHostViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            InterTabClient = new DragablzTracerouteInterTabClient();

            TabItems = new ObservableCollection<DragablzTabItem>()
            {
                new DragablzTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new TracerouteView(_tabId), _tabId)
            };

            // Load profiles
            if (TracerouteProfileManager.Profiles == null)
                TracerouteProfileManager.Load();

            _tracerouteProfiles = CollectionViewSource.GetDefaultView(TracerouteProfileManager.Profiles);
            _tracerouteProfiles.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            _tracerouteProfiles.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
            _tracerouteProfiles.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            _tracerouteProfiles.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                TracerouteProfileInfo info = o as TracerouteProfileInfo;

                string search = Search.Trim();

                // Search by: Tag
                if (search.StartsWith(tagIdentifier, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(info.Tags))
                        return false;
                    else
                        return info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(tagIdentifier.Length, search.Length - tagIdentifier.Length).IndexOf(str, StringComparison.OrdinalIgnoreCase) > -1);
                }
                else // Search by: Name, Hostname
                {
                    return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
                }
            };

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.Traceroute_ExpandProfileView;
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

            TracerouteProfileViewModel tracerouteProfileViewModel = new TracerouteProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                TracerouteProfileInfo tracerouteProfileInfo = new TracerouteProfileInfo
                {
                    Name = instance.Name,
                    Host = instance.Host,
                    Group = instance.Group,
                    Tags = instance.Tags
                };

                TracerouteProfileManager.AddProfile(tracerouteProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, TracerouteProfileManager.GetProfileGroups());

            customDialog.Content = new TracerouteProfileDialog
            {
                DataContext = tracerouteProfileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand TraceProfileCommand
        {
            get { return new RelayCommand(p => TraceProfileAction()); }
        }

        private void TraceProfileAction()
        {
            AddTab(SelectedProfile.Host);
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

            TracerouteProfileViewModel tracerouteProfileViewModel = new TracerouteProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                TracerouteProfileManager.RemoveProfile(SelectedProfile);

                TracerouteProfileInfo tracerouteProfileInfo = new TracerouteProfileInfo
                {
                    Name = instance.Name,
                    Host = instance.Host,
                    Group = instance.Group,
                    Tags = instance.Tags
                };

                TracerouteProfileManager.AddProfile(tracerouteProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, TracerouteProfileManager.GetProfileGroups(), SelectedProfile);

            customDialog.Content = new TracerouteProfileDialog
            {
                DataContext = tracerouteProfileViewModel
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

            TracerouteProfileViewModel tracerouteProfileViewModel = new TracerouteProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                TracerouteProfileInfo tracerouteProfileInfo = new TracerouteProfileInfo
                {
                    Name = instance.Name,
                    Host = instance.Host,
                    Group = instance.Group,
                    Tags = instance.Tags
                };

                TracerouteProfileManager.AddProfile(tracerouteProfileInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, TracerouteProfileManager.GetProfileGroups(), SelectedProfile);

            customDialog.Content = new TracerouteProfileDialog
            {
                DataContext = tracerouteProfileViewModel
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

                TracerouteProfileManager.RemoveProfile(SelectedProfile);
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

                TracerouteProfileManager.RenameGroup(instance.OldGroup, instance.Group);

                _tracerouteProfiles.Refresh();
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

        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem).View as TracerouteView).CloseTab();
        }
        #endregion

        #region Methods
        private void AddTab(string host = null)
        {
            _tabId++;

            TabItems.Add(new DragablzTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new TracerouteView(_tabId, host), _tabId));

            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}