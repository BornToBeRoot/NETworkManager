using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the Port Scanner settings.
/// </summary>
public class PortScannerSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    private readonly IDialogCoordinator _dialogCoordinator;

    /// <summary>
    /// Gets the collection view of port profiles.
    /// </summary>
    public ICollectionView PortProfiles { get; }

    private PortProfileInfo _selectedPortProfile = new();

    /// <summary>
    /// Gets or sets the selected port profile.
    /// </summary>
    public PortProfileInfo SelectedPortProfile
    {
        get => _selectedPortProfile;
        set
        {
            if (value == _selectedPortProfile)
                return;

            _selectedPortProfile = value;
            OnPropertyChanged();
        }
    }

    private bool _showAllResults;

    /// <summary>
    /// Gets or sets a value indicating whether to show all results (open and closed ports).
    /// </summary>
    public bool ShowAllResults
    {
        get => _showAllResults;
        set
        {
            if (value == _showAllResults)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_ShowAllResults = value;

            _showAllResults = value;
            OnPropertyChanged();
        }
    }

    private int _timeout;

    /// <summary>
    /// Gets or sets the timeout for port scanning in milliseconds.
    /// </summary>
    public int Timeout
    {
        get => _timeout;
        set
        {
            if (value == _timeout)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_Timeout = value;

            _timeout = value;
            OnPropertyChanged();
        }
    }

    private bool _resolveHostname;

    /// <summary>
    /// Gets or sets a value indicating whether to resolve hostnames.
    /// </summary>
    public bool ResolveHostname
    {
        get => _resolveHostname;
        set
        {
            if (value == _resolveHostname)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_ResolveHostname = value;

            _resolveHostname = value;
            OnPropertyChanged();
        }
    }

    private int _maxHostThreads;

    /// <summary>
    /// Gets or sets the maximum number of threads for scanning hosts.
    /// </summary>
    public int MaxHostThreads
    {
        get => _maxHostThreads;
        set
        {
            if (value == _maxHostThreads)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_MaxHostThreads = value;

            _maxHostThreads = value;
            OnPropertyChanged();
        }
    }

    private int _maxPortThreads;

    /// <summary>
    /// Gets or sets the maximum number of threads for scanning ports.
    /// </summary>
    public int MaxPortThreads
    {
        get => _maxPortThreads;
        set
        {
            if (value == _maxPortThreads)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_MaxPortThreads = value;

            _maxPortThreads = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="PortScannerSettingsViewModel"/> class.
    /// </summary>
    /// <param name="instance">The dialog coordinator instance.</param>
    public PortScannerSettingsViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        PortProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortProfiles);
        PortProfiles.SortDescriptions.Add(
            new SortDescription(nameof(PortProfileInfo.Name), ListSortDirection.Ascending));

        SelectedPortProfile = PortProfiles.Cast<PortProfileInfo>().FirstOrDefault();

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ShowAllResults = SettingsManager.Current.PortScanner_ShowAllResults;
        Timeout = SettingsManager.Current.PortScanner_Timeout;
        ResolveHostname = SettingsManager.Current.PortScanner_ResolveHostname;
        MaxHostThreads = SettingsManager.Current.PortScanner_MaxHostThreads;
        MaxPortThreads = SettingsManager.Current.PortScanner_MaxPortThreads;
    }

    #endregion

    #region ICommand & Actions

    public ICommand AddPortProfileCommand => new RelayCommand(_ => AddPortProfileAction());

    private void AddPortProfileAction()
    {
        AddPortProfile().ConfigureAwait(false);
    }

    public ICommand EditPortProfileCommand => new RelayCommand(_ => EditPortProfileAction());

    private void EditPortProfileAction()
    {
        EditPortProfile().ConfigureAwait(false);
    }

    public ICommand DeletePortProfileCommand => new RelayCommand(_ => DeletePortProfileAction());

    private void DeletePortProfileAction()
    {
        DeletePortProfile().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private async Task AddPortProfile()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.AddPortProfile
        };

        var viewModel = new PortProfileViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.PortScanner_PortProfiles.Add(new PortProfileInfo(instance.Name, instance.Ports));
        }, async _ => { await _dialogCoordinator.HideMetroDialogAsync(this, customDialog); });

        customDialog.Content = new PortProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    public async Task EditPortProfile()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.EditPortProfile
        };

        var viewModel = new PortProfileViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.PortScanner_PortProfiles.Remove(SelectedPortProfile);
                SettingsManager.Current.PortScanner_PortProfiles.Add(new PortProfileInfo(instance.Name,
                    instance.Ports));
            }, async _ => { await _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, true,
            SelectedPortProfile);

        customDialog.Content = new PortProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private async Task DeletePortProfile()
    {
        var result = await DialogHelper.ShowOKCancelMessageAsync(Application.Current.MainWindow,
            Strings.DeletePortProfile,
            Strings.DeletePortProfileMessage,
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        SettingsManager.Current.PortScanner_PortProfiles.Remove(SelectedPortProfile);

        // Select first item after deletion
        SelectedPortProfile = PortProfiles.Cast<PortProfileInfo>().FirstOrDefault();
    }

    #endregion
}