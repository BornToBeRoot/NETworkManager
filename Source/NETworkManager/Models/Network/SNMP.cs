using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class SNMP
    {
        #region Variables
        public int Port = 161;
        public int Timeout = 10000;
        #endregion

        #region Events
        public event EventHandler<SNMPReceivedArgs> Received;

        protected virtual void OnReceived(SNMPReceivedArgs e)
        {
            Received?.Invoke(this, e);
        }

        public event EventHandler TimeoutReached;

        protected virtual void OnTimeoutReached()
        {
            TimeoutReached?.Invoke(this, EventArgs.Empty);
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
        public void GetV1V2CAsync(SNMPVersion version, IPAddress ipAddress, string community, string oid)
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (var result in Messenger.Get(version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2, new IPEndPoint(ipAddress, Port), new OctetString(community), new List<Variable> { new Variable(new ObjectIdentifier(oid)) }, Timeout))
                        OnReceived(new SNMPReceivedArgs(result.Id, result.Data));

                    OnComplete();
                }
                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    OnTimeoutReached();
                }
                catch (ErrorException)
                {
                    OnError();
                }
            });
        }

        public void WalkV1V2CAsync(SNMPVersion version, IPAddress ipAddress, string community, string oid, WalkMode walkMode)
        {
            Task.Run(() =>
            {
                try
                {
                    IList<Variable> results = new List<Variable>();

                    Messenger.Walk(version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2, new IPEndPoint(ipAddress, Port), new OctetString(community), new ObjectIdentifier(oid), results, Timeout, walkMode);

                    foreach (var result in results)
                        OnReceived(new SNMPReceivedArgs(result.Id, result.Data));

                    OnComplete();
                }
                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    OnTimeoutReached();
                }
                catch (ErrorException)
                {
                    OnError();
                }
            });
        }

        public void SetV1V2CAsync(SNMPVersion version, IPAddress ipAddress, string communtiy, string oid, string data)
        {
            Task.Run(() =>
            {
                try
                {
                    Messenger.Set(version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2, new IPEndPoint(ipAddress, Port), new OctetString(communtiy), new List<Variable> { new Variable(new ObjectIdentifier(oid), new OctetString(data)) }, Timeout);

                    OnComplete();
                }
                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    OnTimeoutReached();
                }
                catch (ErrorException)
                {
                    OnError();
                }
            });
        }

        public void Getv3Async(IPAddress ipAddress, string oid, SNMPV3Security security, string username, SNMPV3AuthenticationProvider authProvider, string auth, SNMPV3PrivacyProvider privProvider, string priv)
        {
            Task.Run(() =>
            {
                try
                {
                    var ipEndpoint = new IPEndPoint(ipAddress, Port);

                    // Discovery
                    var discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                    var report = discovery.GetResponse(Timeout, ipEndpoint);

                    IPrivacyProvider privacy;

                    switch (security)
                    {
                        case SNMPV3Security.AuthPriv:
                            privacy = GetPrivacy(authProvider, auth, privProvider, priv);
                            break;
                        // noAuthNoPriv
                        case SNMPV3Security.AuthNoPriv:
                            privacy = GetPrivacy(authProvider, auth);
                            break;
                        default:
                            privacy = GetPrivacy();
                            break;
                    }

                    var request = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(username), new List<Variable> { new Variable(new ObjectIdentifier(oid)) }, privacy, Messenger.MaxMessageSize, report);
                    var reply = request.GetResponse(Timeout, ipEndpoint);

                    var result = reply.Pdu().Variables[0];

                    OnReceived(new SNMPReceivedArgs(result.Id, result.Data));

                    OnComplete();
                }
                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    OnTimeoutReached();
                }
                catch (ErrorException)
                {
                    OnError();
                }
            });
        }

        public void WalkV3Async(IPAddress ipAddress, string oid, SNMPV3Security security, string username, SNMPV3AuthenticationProvider authProvider, string auth, SNMPV3PrivacyProvider privProvider, string priv, WalkMode walkMode)
        {
            Task.Run(() =>
            {
                try
                {
                    var ipEndpoint = new IPEndPoint(ipAddress, Port);

                    // Discovery
                    var discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                    var report = discovery.GetResponse(Timeout, ipEndpoint);

                    IPrivacyProvider privacy;

                    switch (security)
                    {
                        case SNMPV3Security.AuthPriv:
                            privacy = GetPrivacy(authProvider, auth, privProvider, priv);
                            break;
                        // noAuthNoPriv
                        case SNMPV3Security.AuthNoPriv:
                            privacy = GetPrivacy(authProvider, auth);
                            break;
                        default:
                            privacy = GetPrivacy();
                            break;
                    }

                    var results = new List<Variable>();

                    Messenger.BulkWalk(VersionCode.V3, ipEndpoint, new OctetString(username), OctetString.Empty, new ObjectIdentifier(oid), results, Timeout, 10, walkMode, privacy, report);

                    foreach (var result in results)
                        OnReceived(new SNMPReceivedArgs(result.Id, result.Data));

                    OnComplete();
                }
                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    OnTimeoutReached();
                }
                catch (ErrorException)
                {
                    OnError();
                }
            });
        }

        public void SetV3Async(IPAddress ipAddress, string oid, SNMPV3Security security, string username, SNMPV3AuthenticationProvider authProvider, string auth, SNMPV3PrivacyProvider privProvider, string priv, string data)
        {
            Task.Run(() =>
            {
                try
                {
                    var ipEndpoint = new IPEndPoint(ipAddress, Port);

                    // Discovery
                    var discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                    var report = discovery.GetResponse(Timeout, ipEndpoint);

                    IPrivacyProvider privacy;

                    switch (security)
                    {
                        case SNMPV3Security.AuthPriv:
                            privacy = GetPrivacy(authProvider, auth, privProvider, priv);
                            break;
                        // noAuthNoPriv
                        case SNMPV3Security.AuthNoPriv:
                            privacy = GetPrivacy(authProvider, auth);
                            break;
                        default:
                            privacy = GetPrivacy();
                            break;
                    }

                    var request = new SetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(username), OctetString.Empty, new List<Variable> { new Variable(new ObjectIdentifier(oid), new OctetString(data)) }, privacy, Messenger.MaxMessageSize, report);
                    var reply = request.GetResponse(Timeout, ipEndpoint);

                    OnComplete();
                }
                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    OnTimeoutReached();
                }
                catch (ErrorException)
                {
                    OnError();
                }
            });
        }

        // noAuthNoPriv
        private static IPrivacyProvider GetPrivacy()
        {
            return new DefaultPrivacyProvider(DefaultAuthenticationProvider.Instance);
        }

        // authNoPriv
        private static IPrivacyProvider GetPrivacy(SNMPV3AuthenticationProvider authProvider, string auth)
        {
            IAuthenticationProvider authenticationProvider;

            if (authProvider == SNMPV3AuthenticationProvider.MD5)
                authenticationProvider = new MD5AuthenticationProvider(new OctetString(auth));
            else
                authenticationProvider = new SHA1AuthenticationProvider(new OctetString(auth));

            return new DefaultPrivacyProvider(authenticationProvider);
        }

        // authPriv
        private static IPrivacyProvider GetPrivacy(SNMPV3AuthenticationProvider authProvider, string auth, SNMPV3PrivacyProvider privProvider, string priv)
        {
            IAuthenticationProvider authenticationProvider;

            if (authProvider == SNMPV3AuthenticationProvider.MD5)
                authenticationProvider = new MD5AuthenticationProvider(new OctetString(auth));
            else
                authenticationProvider = new SHA1AuthenticationProvider(new OctetString(auth));

            if (privProvider == SNMPV3PrivacyProvider.DES)
                return new DESPrivacyProvider(new OctetString(priv), authenticationProvider);

            return new AESPrivacyProvider(new OctetString(priv), authenticationProvider);
        }
        #endregion

        #region Enum
        public enum SNMPVersion
        {
            V1,
            V2C,
            V3
        }

        // Trap and Inform not implemented
        public enum SNMPMode
        {
            Get,
            Walk,
            Set,
            Trap,
            Inform
        }

        public enum SNMPV3Security
        {
            NoAuthNoPriv,
            AuthNoPriv,
            AuthPriv
        }

        public enum SNMPV3AuthenticationProvider
        {
            MD5,
            SHA1
        }

        public enum SNMPV3PrivacyProvider
        {
            DES,
            AES
        }
        #endregion
    }
}
