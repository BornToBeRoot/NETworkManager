using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace NETworkManager.Models.Network
{
    public class BandwidthMeter
    {
        #region Variables
        public double UpdateInterval = 1000;

        private DispatcherTimer _timer;
        private readonly System.Net.NetworkInformation.NetworkInterface _networkInterface;
        private long _previousBytesSent;
        private long _previousBytesReceived;
        private bool _canUpdate;          // Collect initial data for correct calculation
        #endregion

        #region Events
        public event EventHandler<BandwidthMeterSpeedArgs> UpdateSpeed;

        protected virtual void OnUpdateSpeed(BandwidthMeterSpeedArgs e)
        {
            UpdateSpeed?.Invoke(this, e);
        }
        #endregion

        #region Constructor
        public BandwidthMeter(string id)
        {
            _networkInterface = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(x => x.Id == id);
        }
        #endregion

        #region Methods
        public void Start()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(UpdateInterval)
            };

            _timer.Tick += Timer_Tick;

            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();

            // Reset
            _canUpdate = false;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            var stats = _networkInterface.GetIPv4Statistics();

            var totalBytesSent = stats.BytesSent;
            var totalBytesReceived = stats.BytesReceived;

            var byteSentSpeed = (totalBytesSent - _previousBytesSent);
            var byteReceivedSpeed = (totalBytesReceived - _previousBytesReceived);

            _previousBytesSent = stats.BytesSent;
            _previousBytesReceived = stats.BytesReceived;

            // Need to collect initial data for correct calculation...
            if (!_canUpdate)
            {
                _canUpdate = true;

                return;
            }

            OnUpdateSpeed(new BandwidthMeterSpeedArgs(DateTime.Now, totalBytesReceived, totalBytesSent, byteReceivedSpeed, byteSentSpeed));
        }
        #endregion
    }
}
