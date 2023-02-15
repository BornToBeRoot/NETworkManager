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

        private (string Server, int Port) _serverInfoDialogNewItemOptions = ("time.example.com", 123);

        private ICollectionView _servers;
        public ICollectionView Servers
        {
            get => _servers;
            set
            {
                if(value == _servers) 
                    return;

                _servers = value;
                OnPropertyChanged();
            }
        }

        private ServerInfoProfile _selectedServer = new();
        public ServerInfoProfile SelectedServer
        {
            get => _selectedServer;
            set
            {
                if (value == _selectedServer)
                    return;

                _selectedServer = value;
                OnPropertyChanged();
            }
        }

        private List<string> ServerInfoProfileNames => SettingsManager.Current.SNTPLookup_SNTPServers.Select(x => x.Name).ToList();

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

            Servers = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNTPLookup_SNTPServers);
            Servers.SortDescriptions.Add(new SortDescription(nameof(ServerInfoProfile.Name), ListSortDirection.Ascending));

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Timeout = SettingsManager.Current.SNTPLookup_Timeout;
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddServerCommand => new RelayCommand(p => AddServerAction());

        private void AddServerAction()
        {
            AddServer();
        }

        public ICommand EditServerCommand => new RelayCommand(p => EditServerAction());

        private void EditServerAction()
        {
            EditServer();
        }

        public ICommand DeleteServerCommand => new RelayCommand(p => DeleteServerAction(), DeleteServer_CanExecute);

        private bool DeleteServer_CanExecute(object obj)
        {
            return Servers.Cast<ServerInfoProfile>().Count() > 1;
        }

        private void DeleteServerAction()
        {
            DeleteServer();
        }
        #endregion

        #region Methods
        public async Task AddServer()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.AddSNTPServer
            };

            var viewModel = new ServerInfoProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.SNTPLookup_SNTPServers.Add(new ServerInfoProfile(instance.Name, instance.Servers));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, (ServerInfoProfileNames, false));

            customDialog.Content = new ServerInfoProfileDialog(_serverInfoDialogNewItemOptions)
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async Task EditServer()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.EditSNTPServer
            };

            var viewModel = new ServerInfoProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.SNTPLookup_SNTPServers.Remove(SelectedServer);
                SettingsManager.Current.SNTPLookup_SNTPServers.Add(new ServerInfoProfile(instance.Name, instance.Servers));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, (ServerInfoProfileNames, true), SelectedServer);

            customDialog.Content = new ServerInfoProfileDialog(_serverInfoDialogNewItemOptions)
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async Task DeleteServer()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.DeleteSNTPServer
            };

            var viewModel = new ConfirmDeleteViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.SNTPLookup_SNTPServers.Remove(SelectedServer);
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