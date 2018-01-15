using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using NETworkManager.Helpers;
using NETworkManager.Models.Lookup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
namespace NETworkManager.Models.Network
{
    public class SNMP
    {
        #region Variables

        #endregion

        #region Events
        public event EventHandler<SNMPReceivedArgs> Received;

        protected virtual void OnReceived(SNMPReceivedArgs e)
        {
            Received?.Invoke(this, e);
        }

        public event EventHandler Timeout;

        protected virtual void OnTimeout()
        {
            Timeout?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Error;

        protected virtual void OnError()
        {
            Error?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Complete;

        protected virtual void OnComplete()
        {
            Complete?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler UserHasCanceled;

        protected virtual void OnUserHasCanceled()
        {
            UserHasCanceled?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        public void QueryAsync(VersionCode version, IPAddress ipAddress, string community, string oid, SNMPOptions options, CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, options.Port);

                IList<Variable> list = new List<Variable>();

                try
                {
                    if (options.Walk)
                        Messenger.Walk(version, ipEndpoint, new OctetString(community), new ObjectIdentifier(oid), list, options.Timeout, options.WalkMode);
                    else
                        list = Messenger.Get(version, ipEndpoint, new OctetString(community), new List<Variable> { new Variable(new ObjectIdentifier(oid)) }, options.Timeout);

                    foreach (Variable result in list)
                    {
                        OnReceived(new SNMPReceivedArgs(result.Id.ToString(), result.Data.ToString()));
                    }

                    OnComplete();
                }
                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    OnTimeout();
                }
                catch (ErrorException)
                {
                    OnError();
                }
            });
        }
        #endregion
    }
}
