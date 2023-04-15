using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

public partial class SNMPClient
{
    #region Events
    public event EventHandler<SNMPReceivedArgs> Received;

    protected virtual void OnReceived(SNMPReceivedArgs e)
    {
        Received?.Invoke(this, e);
    }

    public event EventHandler DataUpdated;

    protected virtual void OnDataUpdated()
    {
        DataUpdated?.Invoke(this, EventArgs.Empty);
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

    public event EventHandler Canceled;

    protected virtual void OnCanceled()
    {
        Canceled?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Constructor
    public SNMPClient()
    {

    }
    #endregion

    #region Methods
    public void GetAsync(IPAddress ipAddress, string oid, SNMPOptions options)
    {
        Task.Run(async () =>
        {
            try
            {
                VersionCode version = options.Version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2;
                IPEndPoint ipEndPoint = new(ipAddress, options.Port);
                OctetString community = new(SecureStringHelper.ConvertToString(options.Community));
                List<Variable> variables = new() { new(new ObjectIdentifier(oid)) };

                var results = await Messenger.GetAsync(version, ipEndPoint, community, variables, options.CancellationToken);

                foreach (var result in results)
                    OnReceived(new SNMPReceivedArgs(result.Id, result.Data));
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (ErrorException)
            {
                OnError();
            }
            finally
            {
                OnComplete();
            }
        }, options.CancellationToken);
    }

    public void GetAsyncV3(IPAddress ipAddress, string oid, SNMPOptionsV3 options)
    {
        Task.Run(async () =>
        {
            try
            {
                IPEndPoint ipEndpoint = new(ipAddress, options.Port);
                OctetString username = new(options.Username);
                List<Variable> variables = new() { new Variable(new ObjectIdentifier(oid)) };

                Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                ReportMessage report = await discovery.GetResponseAsync(ipEndpoint, options.CancellationToken);

                IPrivacyProvider privacy = GetPrivacyProvider(options);

                GetRequestMessage request = new(VersionCode.V3, Messenger.NextMessageId, Messenger.NextMessageId, username, OctetString.Empty, variables, privacy, Messenger.MaxMessageSize, report);
                ISnmpMessage reply = await request.GetResponseAsync(ipEndpoint, options.CancellationToken);

                if (reply.Pdu().ErrorStatus.ToInt32() == 0)
                {
                    var result = reply.Pdu().Variables[0];

                    OnReceived(new SNMPReceivedArgs(result.Id, result.Data));
                }
                else
                {
                    // Show error... maybe add detailed message later.
                    OnError();
                }
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (ErrorException)
            {
                OnError();
            }
            finally
            {
                OnComplete();
            }

        }, options.CancellationToken);
    }

    public void WalkAsync(IPAddress ipAddress, string oid, SNMPOptions options)
    {
        Task.Run(async () =>
        {
            try
            {
                VersionCode version = options.Version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2;
                IPEndPoint ipEndPoint = new(ipAddress, options.Port);
                OctetString community = new(SecureStringHelper.ConvertToString(options.Community));
                ObjectIdentifier table = new(oid);

                IList<Variable> results = new List<Variable>();

                await Messenger.WalkAsync(version, ipEndPoint, community, table, results, options.WalkMode, options.CancellationToken);

                foreach (var result in results)
                    OnReceived(new SNMPReceivedArgs(result.Id, result.Data));
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (ErrorException)
            {
                OnError();
            }
            finally
            {
                OnComplete();
            }
        }, options.CancellationToken);
    }

    public void WalkAsyncV3(IPAddress ipAddress, string oid, SNMPOptionsV3 options)
    {
        Task.Run(async () =>
        {
            try
            {
                IPEndPoint ipEndpoint = new(ipAddress, options.Port);
                OctetString username = new(options.Username);
                ObjectIdentifier table = new(oid);

                Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                ReportMessage report = await discovery.GetResponseAsync(ipEndpoint, options.CancellationToken);

                IPrivacyProvider privacy = GetPrivacyProvider(options);

                var results = new List<Variable>();

                await Messenger.BulkWalkAsync(VersionCode.V3, ipEndpoint, username, OctetString.Empty, table, results, 10, options.WalkMode, privacy, report, options.CancellationToken);

                foreach (var result in results)
                    OnReceived(new SNMPReceivedArgs(result.Id, result.Data));
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (ErrorException)
            {
                OnError();
            }
            finally
            {
                OnComplete();
            }
        }, options.CancellationToken);
    }

    public void SetAsync(IPAddress ipAddress, string oid, string data, SNMPOptions options)
    {
        Task.Run(async () =>
        {
            try
            {
                VersionCode version = options.Version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2;
                IPEndPoint ipEndPoint = new(ipAddress, options.Port);
                OctetString community = new(SecureStringHelper.ConvertToString(options.Community));
                List<Variable> variables = new() { new Variable(new ObjectIdentifier(oid), new OctetString(data)) };

                await Messenger.SetAsync(version, ipEndPoint, community, variables, options.CancellationToken);

                OnDataUpdated();
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (ErrorException)
            {
                OnError();
            }
            finally
            {
                OnComplete();
            }
        });
    }

    public void SetAsyncV3(IPAddress ipAddress, string oid, string data, SNMPOptionsV3 options)
    {
        Task.Run(async () =>
        {
            try
            {
                IPEndPoint ipEndpoint = new(ipAddress, options.Port);
                OctetString username = new(options.Username);
                List<Variable> variables = new() { new Variable(new ObjectIdentifier(oid), new OctetString(data)) };

                Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                ReportMessage report = await discovery.GetResponseAsync(ipEndpoint, options.CancellationToken);

                IPrivacyProvider privacy = GetPrivacyProvider(options);

                SetRequestMessage request = new(VersionCode.V3, Messenger.NextMessageId, Messenger.NextMessageId, username, OctetString.Empty, variables, privacy, Messenger.MaxMessageSize, report);
                ISnmpMessage reply = await request.GetResponseAsync(ipEndpoint);

                if (reply.Pdu().ErrorStatus.ToInt32() == 0)
                    OnDataUpdated();
                else
                    OnError();
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (ErrorException)
            {
                OnError();
            }
            finally
            {
                OnComplete();
            }
        }, options.CancellationToken);
    }

    private static IPrivacyProvider GetPrivacyProvider(SNMPOptionsV3 options)
    {
        if (options.Security == SNMPV3Security.AuthPriv)
            return GetPrivacyProvider(options.AuthProvider, SecureStringHelper.ConvertToString(options.Auth), options.PrivProvider, SecureStringHelper.ConvertToString(options.Priv));

        if (options.Security == SNMPV3Security.AuthNoPriv)
            return GetPrivacyProvider(options.AuthProvider, SecureStringHelper.ConvertToString(options.Auth));

        // NoAuthNoPriv
        return GetPrivacyProvider();
    }

    // noAuthNoPriv
    private static IPrivacyProvider GetPrivacyProvider()
    {
        return new DefaultPrivacyProvider(DefaultAuthenticationProvider.Instance);
    }

    // authNoPriv
    private static IPrivacyProvider GetPrivacyProvider(SNMPV3AuthenticationProvider authProvider, string auth)
    {
        return new DefaultPrivacyProvider(GetAuthenticationProvider(authProvider, auth));
    }

    // authPriv
    private static IPrivacyProvider GetPrivacyProvider(SNMPV3AuthenticationProvider authProvider, string auth, SNMPV3PrivacyProvider privProvider, string priv)
    {
        return privProvider switch
        {
            SNMPV3PrivacyProvider.DES => new DESPrivacyProvider(new OctetString(priv), GetAuthenticationProvider(authProvider, auth)),
            SNMPV3PrivacyProvider.AES => new AESPrivacyProvider(new OctetString(priv), GetAuthenticationProvider(authProvider, auth)),
            SNMPV3PrivacyProvider.AES192 => new AES192PrivacyProvider(new OctetString(priv), GetAuthenticationProvider(authProvider, auth)),
            SNMPV3PrivacyProvider.AES256 => new AES256PrivacyProvider(new OctetString(priv), GetAuthenticationProvider(authProvider, auth)),
            _ => null,
        };
    }

    private static IAuthenticationProvider GetAuthenticationProvider(SNMPV3AuthenticationProvider authProvider, string auth)
    {
        return authProvider switch
        {
            SNMPV3AuthenticationProvider.MD5 => new MD5AuthenticationProvider(new OctetString(auth)),
            SNMPV3AuthenticationProvider.SHA1 => new SHA1AuthenticationProvider(new OctetString(auth)),
            SNMPV3AuthenticationProvider.SHA256 => new SHA256AuthenticationProvider(new OctetString(auth)),
            SNMPV3AuthenticationProvider.SHA384 => new SHA384AuthenticationProvider(new OctetString(auth)),
            SNMPV3AuthenticationProvider.SHA512 => new SHA512AuthenticationProvider(new OctetString(auth)),
            _ => null,
        };
    }
    #endregion
}
