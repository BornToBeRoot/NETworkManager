using NETworkManager.Models.Network;
using NETworkManager.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using NETworkManager.Models.Settings;
using System.Net;
using System.Windows.Threading;
using System.Diagnostics;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib;

namespace NETworkManager.ViewModels.Applications
{
    public class SNMPv1ViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

        private string _hostname;
        public string Hostname
        {
            get { return _hostname; }
            set
            {
                if (value == _hostname)
                    return;

                _hostname = value;
                OnPropertyChanged();
            }
        }

        private List<string> _hostnameHistory = new List<string>();
        public List<string> HostnameHistory
        {
            get { return _hostnameHistory; }
            set
            {
                if (value == _hostnameHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_v1_HostnameHistory = value;

                _hostnameHistory = value;
                OnPropertyChanged();
            }
        }

        private string _oid;
        public string OID
        {
            get { return _oid; }
            set
            {
                if (value == _oid)
                    return;

                _oid = value;
                OnPropertyChanged();
            }
        }

        private List<string> _oidHistory = new List<string>();
        public List<string> OIDHistory
        {
            get { return _oidHistory; }
            set
            {
                if (value == _oidHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_v1_OIDHistory = value;

                _oidHistory = value;
                OnPropertyChanged();
            }
        }

        private string _community;
        public string Community
        {
            get { return _community; }
            set
            {
                if (value == _community)
                    return;

                _community = value;
                OnPropertyChanged();
            }
        }

        private bool _isQueryRunning;
        public bool IsQueryRunning
        {
            get { return _isQueryRunning; }
            set
            {
                if (value == _isQueryRunning)
                    return;

                _isQueryRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get { return _displayStatusMessage; }
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get { return _startTime; }
            set
            {
                if (value == _startTime)
                    return;

                _startTime = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                if (value == _duration)
                    return;

                _duration = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _endTime;
        public DateTime? EndTime
        {
            get { return _endTime; }
            set
            {
                if (value == _endTime)
                    return;

                _endTime = value;
                OnPropertyChanged();
            }
        }

        private bool _expandStatistics;
        public bool ExpandStatistics
        {
            get { return _expandStatistics; }
            set
            {
                if (value == _expandStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNMP_v1_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public SNMPv1ViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Current.SNMP_v1_HostnameHistory != null)
                HostnameHistory = new List<string>(SettingsManager.Current.SNMP_v1_HostnameHistory);

            if (SettingsManager.Current.SNMP_v1_OIDHistory != null)
                OIDHistory = new List<string>(SettingsManager.Current.SNMP_v1_OIDHistory);

            ExpandStatistics = SettingsManager.Current.SNMP_v1_ExpandStatistics;
        }
        #endregion

        #region ICommands & Actions
        public ICommand QueryCommand
        {
            get { return new RelayCommand(p => QueryAction()); }
        }

        private void QueryAction()
        {
            Query();
        }
        #endregion

        #region Methods
        private void Query()
        {
            DisplayStatusMessage = false;
            IsQueryRunning = true;

            // Measure time
            StartTime = DateTime.Now;
            stopwatch.Start();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            EndTime = null;

            try
            {
                /* TEST
                IPAddress ip = Dns.GetHostAddresses(Hostname)[0];

                foreach (Variable test in Messenger.Get(VersionCode.V1, new IPEndPoint(ip, 161), new OctetString(Community), new List<Variable> { new Variable(new ObjectIdentifier(OID)) }, 60000))
                    MessageBox.Show(test.Data.ToString());

                */
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }

            HostnameHistory = new List<string>(HistoryListHelper.Modify(HostnameHistory, Hostname, SettingsManager.Current.Application_HistoryListEntries));
            OIDHistory = new List<string>(HistoryListHelper.Modify(OIDHistory, OID, SettingsManager.Current.Application_HistoryListEntries));

            // Stop timer and stopwatch
            stopwatch.Stop();
            dispatcherTimer.Stop();

            Duration = stopwatch.Elapsed;
            EndTime = DateTime.Now;

            stopwatch.Reset();

            IsQueryRunning = false;
        }
        #endregion

        #region Events
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion
    }
}