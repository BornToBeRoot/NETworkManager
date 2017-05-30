using NETworkManager.Helpers;
using NETworkManager.Models.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using NETworkManager.Models.Network;
using System.ComponentModel;
using System.Windows.Data;

namespace NETworkManager.ViewModels.Applications
{
    public class WakeOnLANViewModel : ViewModelBase
    {
        private IDialogCoordinator dialogCoordinator;

        MetroDialogSettings dialogSettings = new MetroDialogSettings();

        #region  Variables                
        private string _MACAddress;
        public string MACAddress
        {
            get { return _MACAddress; }
            set
            {
                if (value == _MACAddress)
                    return;

                _MACAddress = value;
                OnPropertyChanged();
            }
        }

        private string _broadcast;
        public string Broadcast
        {
            get { return _broadcast; }
            set
            {
                if (value == _broadcast)
                    return;

                _broadcast = value;
                OnPropertyChanged();
            }
        }

        private string _port;
        public string Port
        {
            get { return _port; }
            set
            {
                if (value == _port)
                    return;

                _port = value;
                OnPropertyChanged();
            }
        }

        ICollectionView _wakeOnLanTemplates;
        public ICollectionView WakeOnLANTemplates
        {
            get { return _wakeOnLanTemplates; }
        }

        private TemplateWakeOnLANInfo _selectedWakeOnLANTemplate = new TemplateWakeOnLANInfo();
        public TemplateWakeOnLANInfo SelectedWakeOnLANTemplate
        {
            get { return _selectedWakeOnLANTemplate; }
            set
            {
                if (value == _selectedWakeOnLANTemplate)
                    return;

                if (value != null)
                {
                    Broadcast = value.Broadcast.ToString();
                    Port = value.Port.ToString();
                }

                _selectedWakeOnLANTemplate = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedWakeOnLANInfos = new ArrayList();
        public IList SelectedWakeOnLANInfos
        {
            get { return _selectedWakeOnLANInfos; }
            set
            {
                _selectedWakeOnLANInfos = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, Load templates
        public WakeOnLANViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            _wakeOnLanTemplates = CollectionViewSource.GetDefaultView(TemplateManager.WakeOnLANTemplates);

            Port = Properties.Resources.WakeOnLAN_DefaultPort;
        }
        #endregion

        #region ICommands
        public ICommand WakeUpCommand
        {
            get { return new RelayCommand(p => WakeUpAction()); }
        }

        public ICommand AddTemplateCommand
        {
            get { return new RelayCommand(p => AddTemplateAction()); }
        }

        public ICommand DeleteSelectedWakeOnLANInfosCommand
        {
            get { return new RelayCommand(p => DeleteSelectedWakeOnLANInfosAction()); }
        }
        #endregion

        #region Methods
        private async void WakeUpAction()
        {
            try
            {
                WakeOnLAN wakeOnLan = new WakeOnLAN();
                wakeOnLan.Completed += WakeOnLAN_Completed;

                WakeOnLANInfo info = new WakeOnLANInfo
                {
                    MagicPacket = MagicPacketHelper.Create(MACAddress),
                    Broadcast = IPAddress.Parse(Broadcast),
                    Port = int.Parse(Port)
                };

                wakeOnLan.Send(info);
            }
            catch (Exception ex)
            {
                dialogSettings.AffirmativeButtonText = Application.Current.Resources["String_Button_OK"] as string;

                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Error"] as string, ex.Message, MessageDialogStyle.Affirmative, dialogSettings);
            }
        }

        private async void AddTemplateAction()
        {
            dialogSettings.AffirmativeButtonText = Application.Current.Resources["String_Button_Add"] as string;
            dialogSettings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            string hostname = await dialogCoordinator.ShowInputAsync(this, Application.Current.Resources["String_AddTemplate"] as string, Application.Current.Resources["String_EnterHostnameForTemplate"] as string, dialogSettings);

            if (string.IsNullOrEmpty(hostname))
                return;

            TemplateWakeOnLANInfo template = new TemplateWakeOnLANInfo
            {
                Hostname = hostname.ToUpper(),
                MACAddress = MACAddressHelper.GetDefaultFormat(MACAddress),
                Broadcast = Broadcast,
                Port = int.Parse(Port)
            };

            TemplateManager.WakeOnLANTemplates.Add(template);
        }

        private async void DeleteSelectedWakeOnLANInfosAction()
        {
            dialogSettings.AffirmativeButtonText = Application.Current.Resources["String_Button_Delete"] as string;
            dialogSettings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            dialogSettings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            MessageDialogResult result = await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_AreYouSure"] as string, Application.Current.Resources["String_DeleteTemplatesMessage"] as string, MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            if (result == MessageDialogResult.Negative)
                return;

            List<TemplateWakeOnLANInfo> list = new List<TemplateWakeOnLANInfo>();

            foreach (TemplateWakeOnLANInfo template in SelectedWakeOnLANInfos)
                list.Add(template);

            foreach (TemplateWakeOnLANInfo info in list)
                TemplateManager.WakeOnLANTemplates.Remove(info);
        }
        #endregion

        #region Events      
        private void WakeOnLAN_Completed(object sender, System.EventArgs e)
        {
            MessageBox.Show("Sended");
        }
        #endregion
    }
}
