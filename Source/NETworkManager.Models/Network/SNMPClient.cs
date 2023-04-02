using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Web.Services.Description;
using Newtonsoft.Json.Bson;

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

    public event EventHandler UserHasCanceled;

    protected virtual void OnUserHasCanceled()
    {
        UserHasCanceled?.Invoke(this, EventArgs.Empty);
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
                List<Variable> oids = new() { new(new ObjectIdentifier(oid)) };

                var results = await Messenger.GetAsync(version, ipEndPoint, community, oids, options.CancellationToken);

                foreach (var result in results)
                    OnReceived(new SNMPReceivedArgs(result.Id, result.Data));
            }
            catch (OperationCanceledException)
            {
                OnUserHasCanceled();
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
                OnUserHasCanceled();
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
            }
            catch (OperationCanceledException)
            {
                OnUserHasCanceled();
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

    /*
    public void GetAsyncV3(IPAddress ipAddress, string oid, SNMPOptionsV3 options)
    {
        Task.Run(() =>
        {
            try
            {
                IPEndPoint ipEndpoint = new(ipAddress, options.Port);


                // Discovery
                var discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                var report = discovery.GetResponse(Timeout, ipEndpoint);

                IPrivacyProvider privacy = GetPrivacyProvider(options);





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
        }, options.CancellationToken);
    }
    */

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
        IAuthenticationProvider authenticationProvider;

        if (privProvider == SNMPV3PrivacyProvider.DES)
            return new DESPrivacyProvider(new OctetString(priv), GetAuthenticationProvider(authProvider, auth));

        return new AESPrivacyProvider(new OctetString(priv), GetAuthenticationProvider(authProvider, auth));
    }

    private static IAuthenticationProvider GetAuthenticationProvider(SNMPV3AuthenticationProvider authProvider, string auth)
    {
        if (authProvider == SNMPV3AuthenticationProvider.MD5)
            return new MD5AuthenticationProvider(new OctetString(auth));

        return new SHA1AuthenticationProvider(new OctetString(auth));
    }
    #endregion
}
