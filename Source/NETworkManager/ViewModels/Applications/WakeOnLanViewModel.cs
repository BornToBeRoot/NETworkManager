using NETworkManager.Utilities.Network;
using NETworkManager.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using NETworkManager.Settings.Templates;
using NETworkManager.Model.Network;

namespace NETworkManager.ViewModels.Applications
{
    class WakeOnLanViewModel : ViewModelBase
    {
        private IDialogCoordinator dialogCoordinator;

        MetroDialogSettings dialogSettings = new MetroDialogSettings();

        #region  Variables                
        private bool _templatesChanged;
        public bool TemplatesChanged
        {
            get { return _templatesChanged; }
            set
            {
                if (value == _templatesChanged)
                    return;

                _templatesChanged = value;
                OnPropertyChanged();
            }
        }

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

        private ObservableCollection<WakeOnLanTemplate> _wakeOnLanTempaltes = new ObservableCollection<WakeOnLanTemplate>();
        public ObservableCollection<WakeOnLanTemplate> WakeOnLanTemplates
        {
            get { return _wakeOnLanTempaltes; }
            set
            {
                if (value == _wakeOnLanTempaltes)
                    return;

                _wakeOnLanTempaltes = value;
                OnPropertyChanged();
            }
        }

        private WakeOnLanTemplate _selectedItemWakeOnLanTemplate = new WakeOnLanTemplate();
        public WakeOnLanTemplate SelectedItemWakeOnLanTemplate
        {
            get { return _selectedItemWakeOnLanTemplate; }
            set
            {
                if (value == _selectedItemWakeOnLanTemplate)
                    return;

                if (value != null)
                {
                    Port = value.Port;
                    Broadcast = value.Broadcast;
                }

                _selectedItemWakeOnLanTemplate = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedWakeOnLanTemplates = new ArrayList();
        public IList SelectedWakeOnLanTemplates
        {
            get { return _selectedWakeOnLanTemplates; }
            set
            {
                _selectedWakeOnLanTemplates = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, Load templates
        public WakeOnLanViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            LoadTemplates();

            WakeOnLanTemplates.CollectionChanged += WakeOnLanTemplates_CollectionChanged;

            Port = NETworkManager.Settings.Properties.Resources.WakeOnLan_DefaultPort;
        }

        private void LoadTemplates()
        {
            foreach (WakeOnLanTemplate template in SettingsManager.GetWakeOnLanTemplates())
            {
                WakeOnLanTemplates.Add(template);
            }
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

        public ICommand DeleteSelectedWakeOnLanTemplatesCommand
        {
            get { return new RelayCommand(p => DeleteSelectedWakeOnLanTemplatesAction()); }
        }
        #endregion

        #region Methods
        private async void WakeUpAction()
        {
            try
            {
                WakeOnLan wakeOnLan = new WakeOnLan();
                wakeOnLan.Completed += WakeOnLan_Completed;

                WakeOnLanInfo info = new WakeOnLanInfo
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

            WakeOnLanTemplate template = new WakeOnLanTemplate
            {
                MAC = MACAddressHelper.GetDefaultFormat(MACAddress),
                Broadcast = Broadcast,
                Hostname = hostname.ToUpper(),
                Port = Port,
            };

            WakeOnLanTemplates.Add(template);
        }

        private async void DeleteSelectedWakeOnLanTemplatesAction()
        {
            dialogSettings.AffirmativeButtonText = Application.Current.Resources["String_Button_Delete"] as string;
            dialogSettings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            dialogSettings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            MessageDialogResult result = await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_AreYouSure"] as string, Application.Current.Resources["String_DeleteTemplatesMessage"] as string, MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            if (result == MessageDialogResult.Negative)
                return;

            List<WakeOnLanTemplate> list = new List<WakeOnLanTemplate>();

            foreach (WakeOnLanTemplate tmpInfo in SelectedWakeOnLanTemplates)
                list.Add(tmpInfo);

            foreach (WakeOnLanTemplate info in list)
                WakeOnLanTemplates.Remove(info);
        }

        public void SaveTemplates()
        {
            SettingsManager.SaveWakeOnLanTemplates(new List<WakeOnLanTemplate>(WakeOnLanTemplates));

            TemplatesChanged = false;
        }

        public void OnShutdown()
        {
            if (TemplatesChanged)
                SaveTemplates();
        }
        #endregion

        #region Events
        private void WakeOnLanTemplates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            TemplatesChanged = true;
        }

        private void WakeOnLan_Completed(object sender, EventArgs e)
        {
            MessageBox.Show("Sended");
        }
        #endregion
    }
}
