using NETworkManager.Settings;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Network;
using NETworkManager.Views;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels
{
    public class SNTPLookupSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private readonly IDialogCoordinator _dialogCoordinator;

        public ICollectionView SNTPServers { get; }

        private SNTPServerInfo _selectedSNTPServer = new();
        public SNTPServerInfo SelectedSNTPServer
        {
            get => _selectedSNTPServer;
            set
            {
                if (value == _selectedSNTPServer)
                    return;

                _selectedSNTPServer = value;
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
                    SettingsManager.Current.SNTPLookup_Timeout = value;

                _timeout = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public SNTPLookupSettingsViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            SNTPServers = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNTPLookup_SNTPServers);
            SNTPServers.SortDescriptions.Add(new SortDescription(nameof(SNTPServerInfo.Name), ListSortDirection.Ascending));

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {            
            Timeout = SettingsManager.Current.SNTPLookup_Timeout;
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddSNTPServerCommand => new RelayCommand(p => AddSNTPServerAction());

        private void AddSNTPServerAction()
        {
            AddSNTPServer();
        }

        public ICommand EditSNTPServerCommand => new RelayCommand(p => EditSNTPServerAction());

        private void EditSNTPServerAction()
        {
            EditSNTPServer();
        }

        public ICommand DeleteSNTPServerCommand => new RelayCommand(p => DeleteSNTPServerAction());

        private void DeleteSNTPServerAction()
        {
            DeleteSNTPServer();
        }
        #endregion
                
        #region Methods
        public async Task AddSNTPServer()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.AddSNTPServer
            };

            var viewModel = new SNTPServerViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                //SettingsManager.Current.DNSLookup_DNSServers.Add(new DNSServerInfo(instance.Name, instance.DNSServers.Replace(" ", "").Split(';').ToList(), instance.Port));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new SNTPServerDialog
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async Task EditSNTPServer()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.EditSNTPServer
            };

            var viewModel = new SNTPServerViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                //SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
                //SettingsManager.Current.DNSLookup_DNSServers.Add(new DNSServerInfo(instance.Name, instance.DNSServers.Replace(" ", "").Split(';').ToList(), instance.Port));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, true, SelectedSNTPServer);

            customDialog.Content = new SNTPServerDialog
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async Task DeleteSNTPServer()
        {
            var customDialog = new CustomDialog
            {                
                Title = Localization.Resources.Strings.DeleteSNTPServer
            };

            var viewModel = new ConfirmDeleteViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.SNTPLookup_SNTPServers.Remove(SelectedSNTPServer);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Localization.Resources.Strings.DeleteSNTPServerMessage);

            customDialog.Content = new ConfirmDeleteDialog
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion        
    }
}