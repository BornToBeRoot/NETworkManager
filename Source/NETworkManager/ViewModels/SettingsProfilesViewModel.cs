using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Profile;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class SettingsProfilesViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public bool IsPortable => ConfigurationManager.Current.IsPortable;

        private string _location;
        public string Location
        {
            get => _location;
            set
            {
                if (value == _location)
                    return;

                _location = value;
                OnPropertyChanged();
            }
        }

        private bool _movingFiles;
        public bool MovingFiles
        {
            get => _movingFiles;
            set
            {
                if (value == _movingFiles)
                    return;

                _movingFiles = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _profileFiles;
        public ICollectionView ProfileFiles
        {
            get => _profileFiles;
            set
            {
                if (value == _profileFiles)
                    return;

                _profileFiles = value;
                OnPropertyChanged();
            }
        }

        private ProfileFileInfo _selectedProfileFile;
        public ProfileFileInfo SelectedProfileFile
        {
            get => _selectedProfileFile;
            set
            {
                if (Equals(value, _selectedProfileFile))
                    return;

                _selectedProfileFile = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsProfilesViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;

            ProfileFiles = new CollectionViewSource { Source = ProfileManager.ProfileFiles }.View;
            ProfileFiles.SortDescriptions.Add(new SortDescription(nameof(ProfileFileInfo.Name), ListSortDirection.Ascending));

            LoadSettings();
        }

        private void LoadSettings()
        {
            Location = ProfileManager.GetProfilesLocation();
        }
        #endregion

        #region ICommands & Actions
        public ICommand BrowseLocationFolderCommand => new RelayCommand(p => BrowseLocationFolderAction());

        private void BrowseLocationFolderAction()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (Directory.Exists(Location))
                    dialog.SelectedPath = Location;

                var dialogResult = dialog.ShowDialog();

                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                    Location = dialog.SelectedPath;
            }
        }

        public ICommand OpenLocationCommand => new RelayCommand(p => OpenLocationAction());

        private static void OpenLocationAction()
        {
            Process.Start("explorer.exe", ProfileManager.GetProfilesLocation());
        }

        public ICommand ChangeLocationCommand => new RelayCommand(p => ChangeLocationAction());

        private async void ChangeLocationAction()
        {
            MovingFiles = true;

            // Get files from new location and check if there are files with the same name
            var containsFile = Directory.GetFiles(Location).Where(x => Path.GetExtension(x) == ProfileManager.ProfilesFileExtension).Count() > 0;

            var copyFiles = false;

            // Check if settings file exists in new location
            if (containsFile)
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Resources.Localization.Strings.Overwrite;
                settings.NegativeButtonText = Resources.Localization.Strings.Cancel;
                settings.FirstAuxiliaryButtonText = Resources.Localization.Strings.UseOther;
                settings.DefaultButtonFocus = MessageDialogResult.FirstAuxiliary;

                var result = await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Overwrite, Resources.Localization.Strings.OverwriteSettingsInTheDestinationFolder, MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, AppearanceManager.MetroDialog);

                switch (result)
                {
                    case MessageDialogResult.Negative:
                        MovingFiles = false;
                        return;
                    case MessageDialogResult.Affirmative:
                        copyFiles = true;
                        break;
                }
            }

            if (copyFiles)
            {
                try
                {
                    await ProfileManager.MoveProfilesAsync(Location);

                    // Show the user some awesome animation to indicate we are working on it :)
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;

                    settings.AffirmativeButtonText = Resources.Localization.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, settings);
                }
            }

            SettingsManager.Current.Profiles_CustomProfilesLocation = Location;

            Location = string.Empty;
            Location = SettingsManager.Current.Profiles_CustomProfilesLocation;

            MovingFiles = false;
        }

        public ICommand RestoreDefaultProfilesLocationCommand => new RelayCommand(p => RestoreDefaultProfilesLocationAction());

        private void RestoreDefaultProfilesLocationAction()
        {
            Location = ProfileManager.GetDefaultProfilesLocation();
        }

        public ICommand AddProfileFileCommand => new RelayCommand(p => AddProfileFileAction());

        private async void AddProfileFileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.AddProfileFile
            };

            var profileFileViewModel = new ProfileFileViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.AddProfileFile(instance.Name);
            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new ProfileFileDialog
            {
                DataContext = profileFileViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditProfileFileCommand => new RelayCommand(p => EditProfileFileAction());

        private async void EditProfileFileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.EditProfileFile
            };

            var profileFileViewModel = new ProfileFileViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RenameProfileFile(SelectedProfileFile, instance.Name);
            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, SelectedProfileFile);

            customDialog.Content = new ProfileFileDialog
            {
                DataContext = profileFileViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DeleteProfileFileCommand => new RelayCommand(p => DeleteProfileFileAction(), DeleteProfileFile_CanExecute);

        private bool DeleteProfileFile_CanExecute(object obj)
        {
            return ProfileFiles.Cast<ProfileFileInfo>().Count() > 1;
        }

        private async void DeleteProfileFileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.Confirm
            };

            var confirmRemoveViewModel = new ConfirmRemoveViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.DeleteProfileFile(SelectedProfileFile);
            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Resources.Localization.Strings.DeleteProfileFile);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        public void SaveAndCheckSettings()
        {
            // Save everything
            if (ProfileManager.ProfilesChanged)
                ProfileManager.Save();
        }

        public void SetLocationPathFromDragDrop(string path)
        {
            Location = path;

            OnPropertyChanged(nameof(Location));
        }
        #endregion
    }
}