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

        private bool _macAddressHasError;
        public bool MACAddressHasError
        {
            get { return _macAddressHasError; }
            set
            {
                if (value == _macAddressHasError)
                    return;

                _macAddressHasError = value;
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

        private bool _broadcastHasError;
        public bool BroadcastHasError
        {
            get { return _broadcastHasError; }
            set
            {
                if (value == _broadcastHasError)
                    return;

                _broadcastHasError = value;
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

        private bool _portHasError;
        public bool PortHasError
        {
            get { return _portHasError; }
            set
            {
                if (value == _portHasError)
                    return;

                _portHasError = value;
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

        private IList _selectedWakeOnLANTemplates = new ArrayList();
        public IList SelectedWakeOnLANTemplates
        {
            get { return _selectedWakeOnLANTemplates; }
            set
            {
                _selectedWakeOnLANTemplates = value;
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

        #region ICommands & Actions
        public ICommand WakeUpCommand
        {
            get { return new RelayCommand(p => WakeUpAction(), WakeUpAction_CanExecute); }
        }

        private bool WakeUpAction_CanExecute(object parameter)
        {
            return !MACAddressHasError && !BroadcastHasError && !PortHasError;
        }

        private async void WakeUpAction()
        {
            try
            {
                WakeOnLANInfo info = new WakeOnLANInfo
                {
                    MagicPacket = MagicPacketHelper.Create(MACAddress),
                    Broadcast = IPAddress.Parse(Broadcast),
                    Port = int.Parse(Port)
                };

                WakeOnLAN.Send(info);

                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Success"] as string, Application.Current.Resources["String_MagicPacketSuccessfulSended"] as string, MessageDialogStyle.Affirmative, dialogSettings);
            }
            catch (Exception ex)
            {
                MetroDialogSettings settings = dialogSettings;

                settings.AffirmativeButtonText = Application.Current.Resources["String_Button_OK"] as string;

                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Error"] as string, ex.Message, MessageDialogStyle.Affirmative, settings);
            }
        }

        public ICommand AddTemplateCommand
        {
            get { return new RelayCommand(p => AddTemplateAction()); }
        }

        private async void AddTemplateAction()
        {
            MetroDialogSettings settings = dialogSettings;

            settings.AffirmativeButtonText = Application.Current.Resources["String_Button_Add"] as string;
            settings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;
            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            string hostname = await dialogCoordinator.ShowInputAsync(this, Application.Current.Resources["String_AddTemplate"] as string, Application.Current.Resources["String_EnterHostnameForTemplate"] as string, settings);

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

        public ICommand WakeUpSelectedWakeOnLANTemplatesCommand
        {
            get { return new RelayCommand(p => WakeUpSelectedWakeOnLANTemplatesAction()); }
        }

        public async void WakeUpSelectedWakeOnLANTemplatesAction()
        {
            int errorCount = 0;

            foreach (TemplateWakeOnLANInfo template in SelectedWakeOnLANTemplates)
            {
                try
                {
                    WakeOnLANInfo info = new WakeOnLANInfo
                    {
                        MagicPacket = MagicPacketHelper.Create(template.MACAddress),
                        Broadcast = IPAddress.Parse(template.Broadcast),
                        Port = template.Port
                    };

                    WakeOnLAN.Send(info);

                    if (SelectedWakeOnLANTemplates.Count == 1)
                        await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Success"] as string, Application.Current.Resources["String_MagicPacketSuccessfulSended"] as string, MessageDialogStyle.Affirmative, dialogSettings);
                }
                catch (Exception ex)
                {
                    errorCount++;
                    await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Error"] as string, ex.Message, MessageDialogStyle.Affirmative, dialogSettings);
                }
            }

            if (SelectedWakeOnLANTemplates.Count > 1 && SelectedWakeOnLANTemplates.Count != errorCount)
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_Success"] as string, string.Format(Application.Current.Resources["String_MagicPacketSuccessfulSendedToClients"] as string, SelectedWakeOnLANTemplates.Count - errorCount), MessageDialogStyle.Affirmative, dialogSettings);
        }

        public ICommand DeleteSelectedWakeOnLANTemplatesCommand
        {
            get { return new RelayCommand(p => DeleteSelectedWakeOnLANTemplatesAction()); }
        }

        private async void DeleteSelectedWakeOnLANTemplatesAction()
        {
            dialogSettings.AffirmativeButtonText = Application.Current.Resources["String_Button_Delete"] as string;
            dialogSettings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            dialogSettings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            MessageDialogResult result = await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_AreYouSure"] as string, Application.Current.Resources["String_DeleteTemplatesMessage"] as string, MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            if (result == MessageDialogResult.Negative)
                return;

            List<TemplateWakeOnLANInfo> list = new List<TemplateWakeOnLANInfo>();

            foreach (TemplateWakeOnLANInfo template in SelectedWakeOnLANTemplates)
                list.Add(template);

            foreach (TemplateWakeOnLANInfo info in list)
                TemplateManager.WakeOnLANTemplates.Remove(info);
        }
        #endregion
    }
}
