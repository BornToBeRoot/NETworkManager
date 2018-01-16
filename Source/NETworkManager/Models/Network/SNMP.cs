using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System;
using System.Collections.Generic;
using System.Net;
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
        public void Getv1v2cAsync(SNMPVersion version, IPAddress ipAddress, string community, string oid, SNMPOptions options)
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (Variable result in Messenger.Get(version == SNMPVersion.v1 ? VersionCode.V1 : VersionCode.V2, new IPEndPoint(ipAddress, options.Port), new OctetString(community), new List<Variable> { new Variable(new ObjectIdentifier(oid)) }, options.Timeout))
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

        public void Walkv1v2cAsync(SNMPVersion version, IPAddress ipAddress, string community, string oid, SNMPOptions options, WalkMode walkMode)
        {
            Task.Run(() =>
            {
                try
                {
                    IList<Variable> results = new List<Variable>();

                    Messenger.Walk(version == SNMPVersion.v1 ? VersionCode.V1 : VersionCode.V2, new IPEndPoint(ipAddress, options.Port), new OctetString(community), new ObjectIdentifier(oid), results, options.Timeout, walkMode);

                    foreach (Variable result in results)
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

        public enum SNMPv3Security
        {
            noAuthNoPriv,
            AuthNoPriv,
            AuthPriv
        }
        #endregion
    }
}
