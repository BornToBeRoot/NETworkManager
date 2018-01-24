using NETworkManager.Models.Settings;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using NETworkManager.Models.Update;

namespace NETworkManager.ViewModels.Settings
{
    public class AboutViewModel : ViewModelBase
    {
        #region Variables
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title)
                    return;

                _title = value;
                OnPropertyChanged();
            }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                if (value == _version)
                    return;

                _version = value;
                OnPropertyChanged();
            }
        }

        private string _copyrightAndAuthor;
        public string CopyrightAndAuthor
        {
            get { return _copyrightAndAuthor; }
            set
            {
                if (value == _copyrightAndAuthor)
                    return;

                _copyrightAndAuthor = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpdateCheckRunning;
        public bool IsUpdateCheckRunning
        {
            get { return _isUpdateCheckRunning; }
            set
            {
                if (value == _isUpdateCheckRunning)
                    return;

                _isUpdateCheckRunning = value;
                OnPropertyChanged();

            }
        }

        private bool _updateAvailable;
        public bool UpdateAvailable
        {
            get { return _updateAvailable; }
            set
            {
                if (value == _updateAvailable)
                    return;

                _updateAvailable = value;
                OnPropertyChanged();
            }
        }

        private string _updateText;
        public string UpdateText
        {
            get { return _updateText; }
            set
            {
                if (value == _updateText)
                    return;

                _updateText = value;
                OnPropertyChanged();
            }
        }

        private bool _noUpdateAvailable;
        public bool NoUpdateAvailable
        {
            get { return _noUpdateAvailable; }
            set
            {
                if (value == _noUpdateAvailable)
                    return;

                _noUpdateAvailable = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public AboutViewModel()
        {
            Version = string.Format("{0} {1}", Application.Current.Resources["String_Version"] as string, AssemblyManager.Current.Version);
            CopyrightAndAuthor = string.Format("{0} {1}.", AssemblyManager.Current.Copyright, AssemblyManager.Current.Company);
        }
        #endregion

        #region Commands & Actions
        public ICommand CheckForUpdatesCommand
        {
            get { return new RelayCommand(p => CheckForUpdatesAction()); }
        }

        private void CheckForUpdatesAction()
        {
            CheckForUpdates();
        }

        public ICommand OpenWebsiteCommand
        {
            get { return new RelayCommand(p => OpenWebsiteAction(p)); }
        }

        private void OpenWebsiteAction(object url)
        {
            Process.Start((string)url);
        }
        #endregion

        #region Methods
        private void CheckForUpdates()
        {
            UpdateAvailable = false;
            NoUpdateAvailable = false;

            IsUpdateCheckRunning = true;

            Updater updater = new Updater();

            updater.UpdateAvailable += Updater_UpdateAvailable;
            updater.NoUpdateAvailable += Updater_NoUpdateAvailable;

            updater.Check();
        }
        #endregion

        #region Events
        private void Updater_UpdateAvailable(object sender, UpdateAvailableArgs e)
        {
            UpdateText = string.Format(Application.Current.Resources["String_VersionxxAvailable"] as string, e.Version);

            IsUpdateCheckRunning = false;
            UpdateAvailable = true;            
        }

        private void Updater_NoUpdateAvailable(object sender, System.EventArgs e)
        {
            IsUpdateCheckRunning = false;
            NoUpdateAvailable = true;
        }
        #endregion
    }
}