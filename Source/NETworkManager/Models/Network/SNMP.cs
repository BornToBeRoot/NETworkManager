using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System;
using System.Collections.Generic;
using System.Net;
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
        public void Queryv1v2cAsync(SNMPVersion version, IPAddress ipAddress, string community, string oid, SNMPOptions options, CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                // Version
                VersionCode versionCode = version == SNMPVersion.v1 ? VersionCode.V1 : VersionCode.V2;

                // host with ip:port
                IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, options.Port);

                // List for results - snmp walk
                IList<Variable> list = new List<Variable>();

                try
                {
                    if (options.Walk)
                        Messenger.Walk(versionCode, ipEndpoint, new OctetString(community), new ObjectIdentifier(oid), list, options.Timeout, options.WalkMode);
                    else
                        list = Messenger.Get(versionCode, ipEndpoint, new OctetString(community), new List<Variable> { new Variable(new ObjectIdentifier(oid)) }, options.Timeout);

                    foreach (Variable result in list)
                        OnReceived(new SNMPReceivedArgs(result.Id.ToString(), result.Data.ToString()));

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

        #region Enum
        public enum SNMPVersion
        {
            v1,
            v2c,
            v3
        }
        #endregion
    }
}
