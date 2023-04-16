using Lextm.SharpSnmpLib.Messaging;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class SNMPSettingsViewModel : ViewModelBase
{
    #region Variables
    private readonly bool _isLoading;

    private readonly IDialogCoordinator _dialogCoordinator;

    public ICollectionView OIDProfiles { get; }

    private SNMPOIDProfileInfo _selectedÓIDProfile = new();
    public SNMPOIDProfileInfo SelectedOIDProfile
    {
        get => _selectedÓIDProfile;
        set
        {
            if (value == _selectedÓIDProfile)
                return;

            _selectedÓIDProfile = value;
            OnPropertyChanged();
        }
    }

    public List<WalkMode> WalkModes { get; set; }

    private WalkMode _walkMode;
    public WalkMode WalkMode
    {
        get => _walkMode;
        set
        {
            if (value == _walkMode)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_WalkMode = value;

            _walkMode = value;
            OnPropertyChanged();
        }
    }

    private int _timeout;
    public int Timeout
    {
        get => _timeout;
        set
        {
            if (value == _timeout)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_Timeout = value;

            _timeout = value;
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

            if (!_isLoading)
                SettingsManager.Current.SNMP_Port = value;

            _port = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Contructor, load settings
    public SNMPSettingsViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        OIDProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_OIDProfiles);
        OIDProfiles.SortDescriptions.Add(new SortDescription(nameof(SNMPOIDProfileInfo.Name), ListSortDirection.Ascending));

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        WalkModes = System.Enum.GetValues(typeof(WalkMode)).Cast<WalkMode>().OrderBy(x => x.ToString()).ToList();
        WalkMode = WalkModes.First(x => x == SettingsManager.Current.SNMP_WalkMode);
        Timeout = SettingsManager.Current.SNMP_Timeout;
        Port = SettingsManager.Current.SNMP_Port;
    }
    #endregion

    #region ICommand & Actions
    public ICommand AddMIBProfileCommand => new RelayCommand(p => AddMIBProfileAction());

    private void AddMIBProfileAction()
    {
        AddMIBProfile();
    }

    public ICommand EditMIBProfileCommand => new RelayCommand(p => EditMIBProfileAction());

    private void EditMIBProfileAction()
    {
        EditMIBProfile();
    }

    public ICommand DeleteMIBProfileCommand => new RelayCommand(p => DeleteMIBProfileAction());

    private void DeleteMIBProfileAction()
    {
        DeleteMIBProfile();
    }
    #endregion

    #region Methods       
    public async Task AddMIBProfile()
    {
        /*
        var customDialog = new CustomDialog
        {
            Title = Localization.Resources.Strings.AddPortProfile
        };

        var viewModel = new PortProfileViewModel(instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.PortScanner_PortProfiles.Add(new PortProfileInfo(instance.Name, instance.Ports));
        }, instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
        });

        customDialog.Content = new PortProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        */
    }

    public async Task EditMIBProfile()
    {
        /*
        var customDialog = new CustomDialog
        {
            Title = Localization.Resources.Strings.EditPortProfile
        };

        var viewModel = new PortProfileViewModel(instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.PortScanner_PortProfiles.Remove(SelectedPortProfile);
            SettingsManager.Current.PortScanner_PortProfiles.Add(new PortProfileInfo(instance.Name, instance.Ports));
        }, instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
        }, true, SelectedPortProfile);

        customDialog.Content = new PortProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        */
    }

    public async Task DeleteMIBProfile()
    {
        /*
        var customDialog = new CustomDialog
        {
            Title = Localization.Resources.Strings.DeletePortProfile
        };

        var confirmDeleteViewModel = new ConfirmDeleteViewModel(instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.PortScanner_PortProfiles.Remove(SelectedPortProfile);
        }, instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
        }, Localization.Resources.Strings.DeletePortProfileMessage);

        customDialog.Content = new ConfirmDeleteDialog
        {
            DataContext = confirmDeleteViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        */
    }
    #endregion
}