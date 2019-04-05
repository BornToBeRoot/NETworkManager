using System;
using System.Collections;
using System.Windows.Input;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Models.Lookup;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Export;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels
{
    public class LookupPortLookupViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private string _portOrService;
        public string PortOrService
        {
            get => _portOrService;
            set
            {
                if (value == _portOrService)
                    return;

                _portOrService = value;
                OnPropertyChanged();
            }
        }

        private bool _portOrServiceHasError;
        public bool PortOrServiceHasError
        {
            get => _portOrServiceHasError;
            set
            {
                if (value == _portOrServiceHasError)
                    return;
                _portOrServiceHasError = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView PortsOrServicesHistoryView { get; }

        private bool _isLookupRunning;
        public bool IsLookupRunning
        {
            get => _isLookupRunning;
            set
            {
                if (value == _isLookupRunning)
                    return;

                _isLookupRunning = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PortLookupInfo> _portLookupResults = new ObservableCollection<PortLookupInfo>();
        public ObservableCollection<PortLookupInfo> PortLookupResults
        {
            get => _portLookupResults;
            set
            {
                if (value != null && value == _portLookupResults)
                    return;

                _portLookupResults = value;
            }
        }

        public ICollectionView PortLookupResultsView { get; }

        private PortLookupInfo _selectedPortLookupResult;
        public PortLookupInfo SelectedPortLookupResult
        {
            get => _selectedPortLookupResult;
            set
            {
                if (value == _selectedPortLookupResult)
                    return;

                _selectedPortLookupResult = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedPortLookupResults = new ArrayList();
        public IList SelectedPortLookupResults
        {
            get => _selectedPortLookupResults;
            set
            {
                if (Equals(value, _selectedPortLookupResults))
                    return;

                _selectedPortLookupResults = value;
                OnPropertyChanged();
            }
        }


        private bool _noPortsFound;
        public bool NoPortsFound
        {
            get => _noPortsFound;
            set
            {
                if (value == _noPortsFound)
                    return;

                _noPortsFound = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, Load settings
        public LookupPortLookupViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;

            PortsOrServicesHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Lookup_Port_PortsHistory);
            PortLookupResultsView = CollectionViewSource.GetDefaultView(PortLookupResults);
        }
        #endregion

        #region ICommands & Actions
        public ICommand PortLookupCommand => new RelayCommand(p => PortLookupAction(), PortLookup_CanExecute);

        private bool PortLookup_CanExecute(object parameter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen && !PortOrServiceHasError;

        private async void PortLookupAction()
        {
            IsLookupRunning = true;

            PortLookupResults.Clear();

            var portsByService = new List<string>();

            foreach (var portOrService in PortOrService.Split(';'))
            {
                var portOrService1 = portOrService.Trim();

                if (portOrService1.Contains("-"))
                {
                    var portRange = portOrService1.Split('-');

                    if (int.TryParse(portRange[0], out var startPort) && int.TryParse(portRange[1], out var endPort))
                    {
                        if ((startPort > 0) && (startPort < 65536) && (endPort > 0) && (endPort < 65536) && (startPort < endPort))
                        {
                            for (var i = startPort; i < endPort + 1; i++)
                            {
                                foreach (var info in await PortLookup.LookupAsync(i))
                                {
                                    PortLookupResults.Add(info);
                                }
                            }
                        }
                        else
                        {
                            portsByService.Add(portOrService1);
                        }

                    }
                    else
                    {
                        portsByService.Add(portOrService1);
                    }
                }
                else
                {
                    if (int.TryParse(portOrService1, out int port))
                    {
                        if (port > 0 && port < 65536)
                        {
                            foreach (var info in await PortLookup.LookupAsync(port))
                            {
                                PortLookupResults.Add(info);
                            }
                        }
                        else
                        {
                            portsByService.Add(portOrService1);
                        }
                    }
                    else
                    {
                        portsByService.Add(portOrService1);
                    }
                }
            }

            foreach (var info in await PortLookup.LookupByServiceAsync(portsByService))
            {
                PortLookupResults.Add(info);
            }

            if (PortLookupResults.Count == 0)
            {
                NoPortsFound = true;
            }
            else
            {
                AddPortOrServiceToHistory(PortOrService);
                NoPortsFound = false;
            }

            IsLookupRunning = false;
        }

        public ICommand CopySelectedPortCommand => new RelayCommand(p => CopySelectedPortAction());

        private void CopySelectedPortAction()
        {
            CommonMethods.SetClipboard(SelectedPortLookupResult.Number.ToString());
        }

        public ICommand CopySelectedProtocolCommand => new RelayCommand(p => CopySelectedProtocolAction());

        private void CopySelectedProtocolAction()
        {
            CommonMethods.SetClipboard(SelectedPortLookupResult.Protocol.ToString());
        }

        public ICommand CopySelectedServiceCommand => new RelayCommand(p => CopySelectedServiceAction());

        private void CopySelectedServiceAction()
        {
            CommonMethods.SetClipboard(SelectedPortLookupResult.Service);
        }

        public ICommand CopySelectedDescriptionCommand => new RelayCommand(p => CopySelectedDescriptionAction());

        private void CopySelectedDescriptionAction()
        {
            CommonMethods.SetClipboard(SelectedPortLookupResult.Description);
        }

        public ICommand ExportCommand => new RelayCommand(p => ExportAction());

        private async void ExportAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.Export
            };

            var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? PortLookupResults : new ObservableCollection<PortLookupInfo>(SelectedPortLookupResults.Cast<PortLookupInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Resources.Localization.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, Resources.Localization.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.Lookup_Port_ExportFileType = instance.FileType;
                SettingsManager.Current.Lookup_Port_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, SettingsManager.Current.Lookup_Port_ExportFileType, SettingsManager.Current.Lookup_Port_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region  Methods
        private void AddPortOrServiceToHistory(string portOrService)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.Lookup_Port_PortsHistory.ToList(), portOrService, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.Lookup_Port_PortsHistory.Clear();
            OnPropertyChanged(nameof(PortOrService)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.Lookup_Port_PortsHistory.Add(x));
        }
        #endregion
    }
}