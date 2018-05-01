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
                        OnReceived(new SNMPReceivedArgs(result.Id, result.Data));

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

        public void Walkv1v2cAsync(SNMPVersion version, IPAddress ipAddress, string community, string oid, WalkMode walkMode, SNMPOptions options)
        {
            Task.Run(() =>
            {
                try
                {
                    IList<Variable> results = new List<Variable>();

                    Messenger.Walk(version == SNMPVersion.v1 ? VersionCode.V1 : VersionCode.V2, new IPEndPoint(ipAddress, options.Port), new OctetString(community), new ObjectIdentifier(oid), results, options.Timeout, walkMode);

                    foreach (Variable result in results)
                        OnReceived(new SNMPReceivedArgs(result.Id, result.Data));

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

        public void Setv1v2cAsync(SNMPVersion version, IPAddress ipAddress, string communtiy, string oid, string data, SNMPOptions options)
        {
            Task.Run(() =>
            {
                try
                {
                    Messenger.Set(version == SNMPVersion.v1 ? VersionCode.V1 : VersionCode.V2, new IPEndPoint(ipAddress, options.Port), new OctetString(communtiy), new List<Variable> { new Variable(new ObjectIdentifier(oid), new OctetString(data)) }, options.Timeout);

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

        public void Getv3Async(IPAddress ipAddress, string oid, SNMPv3Security security, string username, SNMPv3AuthenticationProvider authProvider, string auth, SNMPv3PrivacyProvider privProvider, string priv, SNMPOptions options)
        {
            Task.Run(() =>
            {
                try
                {
                    IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, options.Port);

                    // Discovery
                    Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                    ReportMessage report = discovery.GetResponse(options.Timeout, ipEndpoint);

                    IPrivacyProvider privacy;

                    if (security == SNMPv3Security.authPriv)
                        privacy = GetPrivacy(authProvider, auth, privProvider, priv);
                    else if (security == SNMPv3Security.authNoPriv)
                        privacy = GetPrivacy(authProvider, auth);
                    else // noAuthNoPriv
                        privacy = GetPrivacy();

                    GetRequestMessage request = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(username), new List<Variable> { new Variable(new ObjectIdentifier(oid)) }, privacy, Messenger.MaxMessageSize, report);
                    ISnmpMessage reply = request.GetResponse(options.Timeout, ipEndpoint);

                    Variable result = reply.Pdu().Variables[0];

                    OnReceived(new SNMPReceivedArgs(result.Id, result.Data));

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

        public void Walkv3Async(IPAddress ipAddress, string oid, SNMPv3Security security, string username, SNMPv3AuthenticationProvider authProvider, string auth, SNMPv3PrivacyProvider privProvider, string priv, WalkMode walkMode, SNMPOptions options)
        {
            Task.Run(() =>
            {
                try
                {
                    IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, options.Port);

                    // Discovery
                    Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                    ReportMessage report = discovery.GetResponse(options.Timeout, ipEndpoint);

                    IPrivacyProvider privacy;

                    if (security == SNMPv3Security.authPriv)
                        privacy = GetPrivacy(authProvider, auth, privProvider, priv);
                    else if (security == SNMPv3Security.authNoPriv)
                        privacy = GetPrivacy(authProvider, auth);
                    else // noAuthNoPriv
                        privacy = GetPrivacy();

                    List<Variable> results = new List<Variable>();

                    Messenger.BulkWalk(VersionCode.V3, ipEndpoint, new OctetString(username), OctetString.Empty, new ObjectIdentifier(oid), results, options.Timeout, 10, walkMode, privacy, report);

                    foreach (Variable result in results)
                        OnReceived(new SNMPReceivedArgs(result.Id, result.Data));

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

        public void Setv3Async(IPAddress ipAddress, string oid, SNMPv3Security security, string username, SNMPv3AuthenticationProvider authProvider, string auth, SNMPv3PrivacyProvider privProvider, string priv, string data, SNMPOptions options)
        {
            Task.Run(() =>
            {
                try
                {
                    IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, options.Port);

                    // Discovery
                    Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                    ReportMessage report = discovery.GetResponse(options.Timeout, ipEndpoint);

                    IPrivacyProvider privacy;

                    if (security == SNMPv3Security.authPriv)
                        privacy = GetPrivacy(authProvider, auth, privProvider, priv);
                    else if (security == SNMPv3Security.authNoPriv)
                        privacy = GetPrivacy(authProvider, auth);
                    else // noAuthNoPriv
                        privacy = GetPrivacy();

                    SetRequestMessage request = new SetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString(username), OctetString.Empty, new List<Variable> { new Variable(new ObjectIdentifier(oid), new OctetString(data)) }, privacy, Messenger.MaxMessageSize, report);
                    ISnmpMessage reply = request.GetResponse(options.Timeout, ipEndpoint);

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

        // noAuthNoPriv
        private IPrivacyProvider GetPrivacy()
        {
            return new DefaultPrivacyProvider(DefaultAuthenticationProvider.Instance);
        }

        // authNoPriv
        private IPrivacyProvider GetPrivacy(SNMPv3AuthenticationProvider authProvider, string auth)
        {
            IAuthenticationProvider authenticationProvider;

            if (authProvider == SNMPv3AuthenticationProvider.MD5)
                authenticationProvider = new MD5AuthenticationProvider(new OctetString(auth));
            else
                authenticationProvider = new SHA1AuthenticationProvider(new OctetString(auth));

            return new DefaultPrivacyProvider(authenticationProvider);
        }

        // authPriv
        private IPrivacyProvider GetPrivacy(SNMPv3AuthenticationProvider authProvider, string auth, SNMPv3PrivacyProvider privProvider, string priv)
        {
            IAuthenticationProvider authenticationProvider;

            if (authProvider == SNMPv3AuthenticationProvider.MD5)
                authenticationProvider = new MD5AuthenticationProvider(new OctetString(auth));
            else
                authenticationProvider = new SHA1AuthenticationProvider(new OctetString(auth));

            if (privProvider == SNMPv3PrivacyProvider.DES)
                return new DESPrivacyProvider(new OctetString(priv), authenticationProvider);
            else
                return new AESPrivacyProvider(new OctetString(priv), authenticationProvider);
        }
        #endregion

        #region Enum
        public enum SNMPVersion
        {
            v1,
            v2c,
            v3
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

        public enum SNMPv3Security
        {
            noAuthNoPriv,
            authNoPriv,
            authPriv
        }

        public enum SNMPv3AuthenticationProvider
        {
            MD5,
            SHA1
        }

        public enum SNMPv3PrivacyProvider
        {
            DES,
            AES
        }
        #endregion
    }
}
