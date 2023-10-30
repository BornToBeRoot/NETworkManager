using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using ErrorCode = log4net.Core.ErrorCode;

namespace NETworkManager.Models.Network;

public sealed class SNMPClient
{
    #region Variables
    private readonly Dictionary<ObjectIdentifier, SNMPV3ErrorCode> _snmpv3ErrorOiDs = new()
    {
        { new ObjectIdentifier("1.3.6.1.6.3.15.1.1.3.0"), SNMPV3ErrorCode.UnknownUserName},
        { new ObjectIdentifier("1.3.6.1.6.3.15.1.1.5.0"), SNMPV3ErrorCode.AuthenticationFailed}
    };
    #endregion
    
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

    public event EventHandler<SNMPErrorArgs> Error;

    private void OnError(SNMPErrorArgs e)
    {
        Error?.Invoke(this, e);
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
                var version = options.Version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2;
                var ipEndPoint = new IPEndPoint(ipAddress, options.Port);
                var community = new OctetString(SecureStringHelper.ConvertToString(options.Community));

                var variables = oids.Select(oid => new Variable(new ObjectIdentifier(oid))).ToList();

                var message = new GetRequestMessage(Messenger.NextMessageId, version, community, variables);
                var response = await message.GetResponseAsync(ipEndPoint, options.CancellationToken);

                var pdu = response.Pdu();

                if (pdu.ErrorStatus.ToInt32() != 0)
                {
                    OnError(new SNMPErrorArgs($"Pdu error status {pdu.ErrorStatus}, Pdu error index {pdu.ErrorIndex}"));
                    
                    return;
                }

                foreach (var variable in pdu.Variables)
                    OnReceived(new SNMPReceivedArgs(new SNMPInfo(variable.Id, variable.Data)));
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (Exception ex)
            {
                OnError(new SNMPErrorArgs(ex.Message));
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
                var ipEndpoint = new IPEndPoint(ipAddress, options.Port);
                var username = new OctetString(options.Username);

                var variables = oids.Select(oid => new Variable(new ObjectIdentifier(oid))).ToList();

                var discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                var report = await discovery.GetResponseAsync(ipEndpoint, options.CancellationToken);

                var privacy = GetPrivacyProvider(options);
                
                var message = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextMessageId,
                    username, OctetString.Empty, variables, privacy, Messenger.MaxMessageSize, report);
                var response = await message.GetResponseAsync(ipEndpoint, options.CancellationToken);

                var pdu = response.Pdu();

                if (pdu.ErrorStatus.ToInt32() != 0)
                {
                    OnError(new SNMPErrorArgs($"Pdu error status {pdu.ErrorStatus}, Pdu error index {pdu.ErrorIndex}"));

                    return;
                }
                
                foreach (var variable in pdu.Variables)
                {
                    // Check if the variable is an SNMPv3 error code
                    if (_snmpv3ErrorOiDs.TryGetValue(variable.Id, out var errorCode))
                    {
                        OnError(new SNMPErrorArgs(errorCode));
                        
                        return;
                    }
                    
                    OnReceived(new SNMPReceivedArgs(new SNMPInfo(variable.Id, variable.Data)));
                }
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (Exception ex)
            {
                OnError(new SNMPErrorArgs(ex.Message));
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
                var version = options.Version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2;
                IPEndPoint ipEndPoint = new(ipAddress, options.Port);
                OctetString community = new(SecureStringHelper.ConvertToString(options.Community));
                ObjectIdentifier table = new(oid);

                IList<Variable> results = new List<Variable>();

                await Messenger.WalkAsync(version, ipEndPoint, community, table, results, options.WalkMode,
                    options.CancellationToken);

                foreach (var result in results)
                    OnReceived(new SNMPReceivedArgs(
                        new SNMPInfo(result.Id, result.Data)));
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (Exception ex)
            {
                OnError(new SNMPErrorArgs(ex.Message));
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

                var discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                var report = await discovery.GetResponseAsync(ipEndpoint, options.CancellationToken);

                var privacy = GetPrivacyProvider(options);

                var results = new List<Variable>();

                await Messenger.BulkWalkAsync(VersionCode.V3, ipEndpoint, username, OctetString.Empty, table, results,
                    10, options.WalkMode, privacy, report, options.CancellationToken);

                foreach (var result in results)
                    OnReceived(new SNMPReceivedArgs(
                        new SNMPInfo(result.Id, result.Data)));
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (Exception ex)
            {
                OnError(new SNMPErrorArgs(ex.Message));
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
            catch (Exception ex)
            {
                OnError(new SNMPErrorArgs(ex.Message));
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

                SetRequestMessage request = new(VersionCode.V3, Messenger.NextMessageId, Messenger.NextMessageId,
                    username, OctetString.Empty, variables, privacy, Messenger.MaxMessageSize, report);
                ISnmpMessage reply = await request.GetResponseAsync(ipEndpoint);

                var pdu = reply.Pdu();

                if (pdu.ErrorStatus.ToInt32() != 0)
                {
                    OnError(new SNMPErrorArgs($"Pdu error status {pdu.ErrorStatus}, Pdu error index {pdu.ErrorIndex}"));
                    
                    return;
                }

                OnDataUpdated();
                
            }
            catch (OperationCanceledException)
            {
                OnCanceled();
            }
            catch (Exception ex)
            {
                OnError(new SNMPErrorArgs(ex.Message));
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
    private static IPrivacyProvider GetPrivacyProvider(SNMPV3AuthenticationProvider authProvider, SecureString auth,
        SNMPV3PrivacyProvider privProvider, SecureString priv)
    {
        var privPlain = SecureStringHelper.ConvertToString(priv);

        return privProvider switch
        {
#pragma warning disable CS0618 // Allow outdated algorithms. We provide the function also for old devices. The user should use newer algorithms...
            SNMPV3PrivacyProvider.DES => new DESPrivacyProvider(new OctetString(privPlain),
                GetAuthenticationProvider(authProvider, auth)),
#pragma warning restore CS0618 // Allow outdated algorithms. We provide the function also for old devices. The user should use newer algorithms...
            SNMPV3PrivacyProvider.AES => new AESPrivacyProvider(new OctetString(privPlain),
                GetAuthenticationProvider(authProvider, auth)),
            SNMPV3PrivacyProvider.AES192 => new AES192PrivacyProvider(new OctetString(privPlain),
                GetAuthenticationProvider(authProvider, auth)),
            SNMPV3PrivacyProvider.AES256 => new AES256PrivacyProvider(new OctetString(privPlain),
                GetAuthenticationProvider(authProvider, auth)),
            _ => null,
        };
    }

    private static IAuthenticationProvider GetAuthenticationProvider(SNMPV3AuthenticationProvider authProvider,
        SecureString auth)
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