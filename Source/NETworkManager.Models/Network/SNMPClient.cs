using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

public sealed class SNMPClient
{
    #region Events
    public event EventHandler<SNMPReceivedArgs> Received;

    private void OnReceived(SNMPReceivedArgs e)
    {
        Received?.Invoke(this, e);
    }

    public event EventHandler DataUpdated;

    private void OnDataUpdated()
    {
        DataUpdated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler Error;

    private void OnError()
    {
        Error?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler Complete;

    private void OnComplete()
    {
        Complete?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler Canceled;

    private void OnCanceled()
    {
        Canceled?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Methods
    public void GetAsync(IPAddress ipAddress, List<string> oids, SNMPOptions options)
    {
        Task.Run(async () =>
        {
            try
            {
                VersionCode version = options.Version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2;
                IPEndPoint ipEndPoint = new(ipAddress, options.Port);
                OctetString community = new(SecureStringHelper.ConvertToString(options.Community));

                List<Variable> variables = new();

                foreach (var oid in oids)
                    variables.Add(new Variable(new ObjectIdentifier(oid)));

                var results = await Messenger.GetAsync(version, ipEndPoint, community, variables, options.CancellationToken);

                foreach (var result in results)
                    OnReceived(new SNMPReceivedArgs(
                        new SNMPInfo(result.Id, result.Data)));
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

    public void GetAsyncV3(IPAddress ipAddress, List<string> oids, SNMPOptionsV3 options)
    {
        Task.Run(async () =>
        {
            try
            {
                IPEndPoint ipEndpoint = new(ipAddress, options.Port);
                OctetString username = new(options.Username);

                List<Variable> variables = new();

                foreach (var oid in oids)
                    variables.Add(new Variable(new ObjectIdentifier(oid)));

                Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                ReportMessage report = await discovery.GetResponseAsync(ipEndpoint, options.CancellationToken);

                IPrivacyProvider privacy = GetPrivacyProvider(options);

                GetRequestMessage request = new(VersionCode.V3, Messenger.NextMessageId, Messenger.NextMessageId, username, OctetString.Empty, variables, privacy, Messenger.MaxMessageSize, report);
                ISnmpMessage reply = await request.GetResponseAsync(ipEndpoint, options.CancellationToken);

                if (reply.Pdu().ErrorStatus.ToInt32() == 0)
                {
                    var result = reply.Pdu().Variables[0];

                    OnReceived(new SNMPReceivedArgs(
                        new SNMPInfo(result.Id, result.Data)));
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
                    OnReceived(new SNMPReceivedArgs(
                        new SNMPInfo(result.Id, result.Data)));
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
                    OnReceived(new SNMPReceivedArgs(
                        new SNMPInfo(result.Id, result.Data)));
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
        return options.Security switch
        {
            // AuthPriv
            SNMPV3Security.AuthPriv => GetPrivacyProvider(options.AuthProvider, options.Auth, options.PrivProvider,
                options.Priv),
            
            // AuthNoPriv
            SNMPV3Security.AuthNoPriv => GetPrivacyProvider(options.AuthProvider, options.Auth),
            
            // NoAuthNoPriv
            _ => GetPrivacyProvider()
        };
    }

    // noAuthNoPriv
    private static IPrivacyProvider GetPrivacyProvider()
    {
        return new DefaultPrivacyProvider(DefaultAuthenticationProvider.Instance);
    }

    // authNoPriv
    private static IPrivacyProvider GetPrivacyProvider(SNMPV3AuthenticationProvider authProvider, SecureString auth)
    {
        return new DefaultPrivacyProvider(GetAuthenticationProvider(authProvider, auth));
    }

    // authPriv
    private static IPrivacyProvider GetPrivacyProvider(SNMPV3AuthenticationProvider authProvider, SecureString auth, SNMPV3PrivacyProvider privProvider, SecureString priv)
    {
        var privPlain = SecureStringHelper.ConvertToString(priv);
        
        return privProvider switch
        {
#pragma warning disable CS0618 // Allow outdated algorithms. We provide the function also for old devices. The user should use newer algorithms...
            SNMPV3PrivacyProvider.DES => new DESPrivacyProvider(new OctetString(privPlain), GetAuthenticationProvider(authProvider, auth)),
#pragma warning restore CS0618 // Allow outdated algorithms. We provide the function also for old devices. The user should use newer algorithms...
            SNMPV3PrivacyProvider.AES => new AESPrivacyProvider(new OctetString(privPlain), GetAuthenticationProvider(authProvider, auth)),
            SNMPV3PrivacyProvider.AES192 => new AES192PrivacyProvider(new OctetString(privPlain), GetAuthenticationProvider(authProvider, auth)),
            SNMPV3PrivacyProvider.AES256 => new AES256PrivacyProvider(new OctetString(privPlain), GetAuthenticationProvider(authProvider, auth)),
            _ => null,
        };
    }

    private static IAuthenticationProvider GetAuthenticationProvider(SNMPV3AuthenticationProvider authProvider, SecureString auth)
    {
        var authPlain = SecureStringHelper.ConvertToString(auth);
        
        return authProvider switch
        {
#pragma warning disable CS0618 // Allow outdated algorithms. We provide the function also for old devices. The user should use newer algorithms...
            SNMPV3AuthenticationProvider.MD5 => new MD5AuthenticationProvider(new OctetString(authPlain)),
            SNMPV3AuthenticationProvider.SHA1 => new SHA1AuthenticationProvider(new OctetString(authPlain)),
#pragma warning restore CS0618 // Allow outdated algorithms. We provide the function also for old devices. The user should use newer algorithms...
            SNMPV3AuthenticationProvider.SHA256 => new SHA256AuthenticationProvider(new OctetString(authPlain)),
            SNMPV3AuthenticationProvider.SHA384 => new SHA384AuthenticationProvider(new OctetString(authPlain)),
            SNMPV3AuthenticationProvider.SHA512 => new SHA512AuthenticationProvider(new OctetString(authPlain)),
            _ => null,
        };
    }
    #endregion
}
