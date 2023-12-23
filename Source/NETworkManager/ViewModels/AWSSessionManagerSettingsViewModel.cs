using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.AWS;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class AWSSessionManagerSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly bool _isLoading;

    private bool _enableSyncInstanceIDsFromAWS;

    public bool EnableSyncInstanceIDsFromAWS
    {
        get => _enableSyncInstanceIDsFromAWS;
        set
        {
            if (value == _enableSyncInstanceIDsFromAWS)
                return;

            if (!_isLoading)
                SettingsManager.Current.AWSSessionManager_EnableSyncInstanceIDsFromAWS = value;

            _enableSyncInstanceIDsFromAWS = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView AWSProfiles { get; }

    private AWSProfileInfo _selectedAWSProfile = new();

    public AWSProfileInfo SelectedAWSProfile
    {
        get => _selectedAWSProfile;
        set
        {
            if (value == _selectedAWSProfile)
                return;

            _selectedAWSProfile = value;
            OnPropertyChanged();
        }
    }

    private bool _syncOnlyRunningInstancesFromAWS;

    public bool SyncOnlyRunningInstancesFromAWS
    {
        get => _syncOnlyRunningInstancesFromAWS;
        set
        {
            if (value == _syncOnlyRunningInstancesFromAWS)
                return;

            if (!_isLoading)
                SettingsManager.Current.AWSSessionManager_SyncOnlyRunningInstancesFromAWS = value;

            _syncOnlyRunningInstancesFromAWS = value;
            OnPropertyChanged();
        }
    }

    private string _profile;

    public string Profile
    {
        get => _profile;
        set
        {
            if (value == _profile)
                return;

            if (!_isLoading)
                SettingsManager.Current.AWSSessionManager_Profile = value;

            _profile = value;
            OnPropertyChanged();
        }
    }

    private string _region;

    public string Region
    {
        get => _region;
        set
        {
            if (value == _region)
                return;

            if (!_isLoading)
                SettingsManager.Current.AWSSessionManager_Region = value;

            _region = value;
            OnPropertyChanged();
        }
    }

    private string _applicationFilePath;

    public string ApplicationFilePath
    {
        get => _applicationFilePath;
        set
        {
            if (value == _applicationFilePath)
                return;

            if (!_isLoading)
                SettingsManager.Current.AWSSessionManager_ApplicationFilePath = value;

            IsConfigured = !string.IsNullOrEmpty(value);

            _applicationFilePath = value;
            OnPropertyChanged();
        }
    }

    private bool _isConfigured;

    public bool IsConfigured
    {
        get => _isConfigured;
        set
        {
            if (value == _isConfigured)
                return;

            _isConfigured = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Contructor, load settings

    public AWSSessionManagerSettingsViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        AWSProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.AWSSessionManager_AWSProfiles);
        AWSProfiles.SortDescriptions.Add(new SortDescription(nameof(AWSProfileInfo.Profile),
            ListSortDirection.Ascending));
        AWSProfiles.SortDescriptions.Add(
            new SortDescription(nameof(AWSProfileInfo.Region), ListSortDirection.Ascending));

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        EnableSyncInstanceIDsFromAWS = SettingsManager.Current.AWSSessionManager_EnableSyncInstanceIDsFromAWS;
        SyncOnlyRunningInstancesFromAWS = SettingsManager.Current.AWSSessionManager_SyncOnlyRunningInstancesFromAWS;
        Profile = SettingsManager.Current.AWSSessionManager_Profile;
        Region = SettingsManager.Current.AWSSessionManager_Region;
        ApplicationFilePath = SettingsManager.Current.AWSSessionManager_ApplicationFilePath;
        IsConfigured = File.Exists(ApplicationFilePath);
    }

    #endregion

    #region ICommands & Actions

    public ICommand AddAWSProfileCommand => new RelayCommand(_ => AddAWSProfileAction());

    private void AddAWSProfileAction()
    {
        AddAWSProfile().ConfigureAwait(false);
    }

    public ICommand EditAWSProfileCommand => new RelayCommand(_ => EditAWSProfileAction());

    private void EditAWSProfileAction()
    {
        EditAWSProfile().ConfigureAwait(false);
    }

    public ICommand DeleteAWSProfileCommand => new RelayCommand(_ => DeleteAWSProfileAction());

    private void DeleteAWSProfileAction()
    {
        DeleteAWSProfile().ConfigureAwait(false);
    }

    public ICommand BrowseFileCommand => new RelayCommand(_ => BrowseFileAction());

    private void BrowseFileAction()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = GlobalStaticConfiguration.ApplicationFileExtensionFilter
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
            ApplicationFilePath = openFileDialog.FileName;
    }

    public ICommand ConfigureCommand => new RelayCommand(_ => ConfigureAction());

    private void ConfigureAction()
    {
        Configure().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private async Task AddAWSProfile()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.AddAWSProfile
        };

        var viewModel = new AWSProfileViewModel(instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.AWSSessionManager_AWSProfiles.Add(new AWSProfileInfo(instance.IsEnabled,
                instance.Profile, instance.Region));
        }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); });

        customDialog.Content = new AWSProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    public async Task EditAWSProfile()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.EditAWSProfile
        };

        var viewModel = new AWSProfileViewModel(instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.AWSSessionManager_AWSProfiles.Remove(SelectedAWSProfile);
            SettingsManager.Current.AWSSessionManager_AWSProfiles.Add(new AWSProfileInfo(instance.IsEnabled,
                instance.Profile, instance.Region));
        }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, true, SelectedAWSProfile);

        customDialog.Content = new AWSProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private async Task DeleteAWSProfile()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.DeleteAWSProfile
        };

        var viewModel = new ConfirmDeleteViewModel(_ =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.AWSSessionManager_AWSProfiles.Remove(SelectedAWSProfile);
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            Strings.DeleteAWSProfileMessage);

        customDialog.Content = new ConfirmDeleteDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private async Task Configure()
    {
        try
        {
            Process.Start(SettingsManager.Current.AWSSessionManager_ApplicationFilePath);
        }
        catch (Exception ex)
        {
            var settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = Strings.OK;

            await _dialogCoordinator.ShowMessageAsync(this, Strings.Error, ex.Message,
                MessageDialogStyle.Affirmative, settings);
        }
    }

    public void SetFilePathFromDragDrop(string filePath)
    {
        ApplicationFilePath = filePath;

        OnPropertyChanged(nameof(ApplicationFilePath));
    }

    #endregion
}