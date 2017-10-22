using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using NETworkManager.Views.Dialogs;
using System.Windows;
using NETworkManager.ViewModels.Network;
using NETworkManager.Models.RemoteDesktop;

namespace NETworkManager.ViewModels.Applications
{
    public class RemoteDesktopViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzTabContent> TabContents { get; private set; }

        private bool _hideWindowsFormsHost;
        public bool HideWindowsFormsHost
        {
            get { return _hideWindowsFormsHost; }
            set
            {
                if (value == _hideWindowsFormsHost)
                    return;

                _hideWindowsFormsHost = value;
                OnPropertyChanged();
            }
        }

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
        #endregion

        #region Constructor
        public RemoteDesktopViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            InterTabClient = new DragablzMainInterTabClient();
            TabContents = new ObservableCollection<DragablzTabContent>();
        }
        #endregion

        #region ICommand & Actions
        public ICommand NewRDPSessionCommand
        {
            get { return new RelayCommand(p => NewRDPSessionActionAsync()); }
        }

        private async void NewRDPSessionActionAsync()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_NewRemoteDesktopConnection"] as string
            };

            RemoteDesktopSessionViewModel remoteDesktopSessionViewModel = new RemoteDesktopSessionViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                HideWindowsFormsHost = false;

                RemoteDesktopSessionInfo remoteDesktopSessionInfo = new RemoteDesktopSessionInfo
                {
                    Hostname = instance.Hostname,
                    Domain = instance.Domain,
                    Username = instance.Username,
                    Password = instance.Password
                };

                TabContents.Add(new DragablzTabContent(instance.Hostname, new RemoteDesktopControl(remoteDesktopSessionInfo)));
                SelectedTabIndex = TabContents.Count - 1;                                
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                HideWindowsFormsHost = false;
            });

            customDialog.Content = new RemoteDesktopSessionDialog
            {
                DataContext = remoteDesktopSessionViewModel
            };

            // This will fix airpace problem in winform-wpf
            HideWindowsFormsHost = true;

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion
    }
}