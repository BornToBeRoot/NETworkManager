using System.Collections.ObjectModel;
using NETworkManager.Controls;
using NETworkManager.Views.Applications;
using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Views.Dialogs;
using System.Windows;
using NETworkManager.ViewModels.Network;

namespace NETworkManager.ViewModels.Applications
{
    public class RemoteDesktopViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzTabContent> TabContents { get; private set; }

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

                TabContents.Add(new DragablzTabContent(instance.Hostname, new TracerouteView()));
                SelectedTabIndex = TabContents.Count - 1;
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new RemoteDesktopSessionDialog
            {
                DataContext = remoteDesktopSessionViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion
    }
}