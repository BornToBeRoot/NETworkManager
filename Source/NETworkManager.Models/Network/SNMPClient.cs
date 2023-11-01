using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using ABI.Windows.Networking.BackgroundTransfer;

namespace NETworkManager.Models.Network;

public sealed class SNMPClient
{
    #region Variables

    /// <summary>
    /// List of known SNMPv3 error codes with their Object Identifier.
    /// </summary>
    private readonly Dictionary<ObjectIdentifier, SNMPV3ErrorCode> _snmpv3ErrorOIDs = new()
    {
        { new ObjectIdentifier("1.3.6.1.6.3.15.1.1.3.0"), SNMPV3ErrorCode.UnknownUserName },
        { new ObjectIdentifier("1.3.6.1.6.3.15.1.1.5.0"), SNMPV3ErrorCode.AuthenticationFailed }
    };

    #endregion

    #region Events

    /// <summary>
    /// Event that is called when an SNMP message is received (Applies to Get and Walk).
    /// </summary>
    public event EventHandler<SNMPReceivedArgs> Received;

    /// <summary>
    /// Private method to call the <see cref="Received"/> event.
    /// </summary>
    /// <param name="e">SNMP received arguments.</param>
    private void OnReceived(SNMPReceivedArgs e)
    {
        Received?.Invoke(this, e);
    }

    /// <summary>
    /// Event that is called when the data is updated (Applies to Set).
    /// </summary>
    public event EventHandler DataUpdated;

    /// <summary>
    /// Private method to call the <see cref="DataUpdated"/> event.
    /// </summary>
    private void OnDataUpdated()
    {
        DataUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event that is called when an error occurs.
    /// </summary>
    public event EventHandler<SNMPErrorArgs> Error;

    /// <summary>
    /// Private method to call the <see cref="Error"/> event.
    /// </summary>
    /// <param name="e">SNMP error arguments.</param>
    private void OnError(SNMPErrorArgs e)
    {
        Error?.Invoke(this, e);
    }

    /// <summary>
    /// Event that is called when the operation is complete.
    /// </summary>
    public event EventHandler Complete;

    /// <summary>
    /// Private method to call the <see cref="Complete"/> event.
    /// </summary>
    private void OnComplete()
    {
        Complete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event that is called when the operation is canceled.
    /// </summary>
    public event EventHandler Canceled;

    /// <summary>
    /// Private method to call the <see cref="Canceled"/> event.
    /// </summary>
    private void OnCanceled()
    {
        Canceled?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get asynchronously the SNMP information of the given IP address (Applies to v1 and v2c).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="oids">List of Object Identifiers.</param>
    /// <param name="options">SNMP options.</param>
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

                // Check for errors
                var errorCode = pdu.ErrorStatus.ToInt32();

                if (errorCode != 0)
                {
                    OnError(new SNMPErrorArgs(pdu.ErrorStatus.ToErrorCode()));

                    return;
                }

                // Return the SNMP information
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

    /// <summary>
    /// Get asynchronously the SNMP information of the given IP address (Applies to v3).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="oids">List of Object Identifiers.</param>
    /// <param name="options">SNMP options.</param>
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

                // Check for errors
                var errorCode = pdu.ErrorStatus.ToInt32();

                if (errorCode != 0)
                {
                    OnError(new SNMPErrorArgs(pdu.ErrorStatus.ToErrorCode()));

                    return;
                }

                // Check if the response is a report message
                if (response is ReportMessage)
                {
                    // Check for SNMPv3 error codes
                    if (pdu.Variables.Count > 0 &&
                        _snmpv3ErrorOIDs.TryGetValue(pdu.Variables[0].Id, out var errorCodeV3))
                    {
                        OnError(new SNMPErrorArgs(errorCodeV3));

                        return;
                    }
                }

                // Return the SNMP information
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

    /// <summary>
    /// Walk asynchronously the SNMP information of the given IP address (Applies to v1 and v2c).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="oid">Object Identifier to use for the walk.</param>
    /// <param name="options">SNMP options.</param>
    public void WalkAsync(IPAddress ipAddress, string oid, SNMPOptions options)
    {
        Task.Run(async () =>
        {
            try
            {
                var version = options.Version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2;
                var ipEndPoint = new IPEndPoint(ipAddress, options.Port);
                var community = new OctetString(SecureStringHelper.ConvertToString(options.Community));
                var table = new ObjectIdentifier(oid);

                var results = new List<Variable>();

                var seed = new Variable(table);
                var subTreeMask = string.Format(CultureInfo.InvariantCulture, "{0}.", table);

                do
                {
                    var variables = new List<Variable>
                    {
                        new(seed.Id)
                    };
                    
                    var message = new GetNextRequestMessage(Messenger.NextRequestId, version, community, variables);

                    var response = await message.GetResponseAsync(ipEndPoint, options.CancellationToken)
                        .ConfigureAwait(false);
                    
                    var pdu = response.Pdu();
                    
                    // No more objects
                    if(pdu.ErrorStatus.ToErrorCode() == ErrorCode.NoSuchName)
                        break;
                    
                    // Check for errors
                    var errorCode = pdu.ErrorStatus.ToInt32();
                    
                    if (errorCode != 0)
                    {
                        OnError(new SNMPErrorArgs(pdu.ErrorStatus.ToErrorCode()));
                        
                        return;
                    }

                    // Skip the first entry
                    if (pdu.Variables[0].Id == table)
                        continue;

                    // Not in subtree
                    if (options.WalkMode == WalkMode.WithinSubtree && !pdu.Variables[0].Id.ToString().StartsWith(subTreeMask, StringComparison.Ordinal))
                        break;

                    results.Add(pdu.Variables[0]);

                    // Next seed
                    seed = pdu.Variables[0];

                } while (true);

                foreach (var result in results)
                    OnReceived(new SNMPReceivedArgs(new SNMPInfo(result.Id, result.Data)));
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

    /// <summary>
    /// Walk asynchronously the SNMP information of the given IP address (Applies to v3).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="oid">Object Identifier to use for the walk.</param>
    /// <param name="options">SNMP options.</param>
    public void WalkAsyncV3(IPAddress ipAddress, string oid, SNMPOptionsV3 options)
    {
        Task.Run(async () =>
        {
            try
            {
                var ipEndpoint = new IPEndPoint(ipAddress, options.Port);
                var username = new OctetString(options.Username);
                var table = new ObjectIdentifier(oid);

                var discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                var report = await discovery.GetResponseAsync(ipEndpoint, options.CancellationToken);

                var privacy = GetPrivacyProvider(options);

                var results = new List<Variable>();

                await BulkWalkAsync(VersionCode.V3, ipEndpoint, username, OctetString.Empty, table, results,
                    10, options.WalkMode, privacy, report, options.CancellationToken);

                foreach (var result in results)
                    OnReceived(new SNMPReceivedArgs(new SNMPInfo(result.Id, result.Data)));
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

    private static async Task BulkWalkAsync(VersionCode version, IPEndPoint endpoint, OctetString community,
        OctetString contextName, ObjectIdentifier table, IList<Variable> list, int maxRepetitions, WalkMode mode,
        IPrivacyProvider privacy, ISnmpMessage report, CancellationToken token)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        var tableV = new Variable(table);
        var seed = tableV;
        var message = report;
        var data = await BulkHasNextAsync(version, endpoint, community, contextName, seed, maxRepetitions, privacy,
            message, token).ConfigureAwait(false);
        var next = data.Item2;
        message = data.Item3;

        while (data.Item1)
        {
            var subTreeMask = string.Format(CultureInfo.InvariantCulture, "{0}.", table);
            var rowMask = string.Format(CultureInfo.InvariantCulture, "{0}.1.1.", table);
            foreach (var v in next)
            {
                var id = v.Id.ToString();
                if (v.Data.TypeCode == SnmpType.EndOfMibView)
                {
                    goto end;
                }

                if (mode == WalkMode.WithinSubtree && !id.StartsWith(subTreeMask, StringComparison.Ordinal))
                {
                    // not in sub tree
                    goto end;
                }

                list.Add(v);
            }

            seed = next[next.Count - 1];
            data = await BulkHasNextAsync(version, endpoint, community, contextName, seed, maxRepetitions, privacy,
                message, token).ConfigureAwait(false);
            next = data.Item2;
            message = data.Item3;
        }

        end: ;
    }


    private static async Task<Tuple<bool, IList<Variable>, ISnmpMessage>> BulkHasNextAsync(VersionCode version,
        IPEndPoint receiver, OctetString community, OctetString contextName, Variable seed, int maxRepetitions,
        IPrivacyProvider privacy, ISnmpMessage report, CancellationToken token)
    {
        // TODO: report should be updated with latest message from agent.
        if (version == VersionCode.V1)
        {
            throw new NotSupportedException("SNMP v1 is not supported");
        }

        var variables = new List<Variable> { new(seed.Id) };
        var request = version == VersionCode.V3
            ? new GetBulkRequestMessage(
                version,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                community,
                contextName,
                0,
                maxRepetitions,
                variables,
                privacy,
                Messenger.MaxMessageSize,
                report)
            : new GetBulkRequestMessage(
                Messenger.NextRequestId,
                version,
                community,
                0,
                maxRepetitions,
                variables);
        var reply = await request.GetResponseAsync(receiver, token).ConfigureAwait(false);

        if (reply is ReportMessage)
        {
            if (reply.Pdu().Variables.Count == 0)
            {
                // TODO: whether it is good to return?
                return new Tuple<bool, IList<Variable>, ISnmpMessage>(false, new List<Variable>(0), report);
            }

            var id = reply.Pdu().Variables[0].Id;
            if (id != Messenger.NotInTimeWindow)
            {
                return new Tuple<bool, IList<Variable>, ISnmpMessage>(false, new List<Variable>(0), report);
            }

            // according to RFC 3414, send a second request to sync time.
            request = new GetBulkRequestMessage(
                version,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                community,
                contextName,
                0,
                maxRepetitions,
                variables,
                privacy,
                Messenger.MaxMessageSize,
                reply);

            reply = await request.GetResponseAsync(receiver, token).ConfigureAwait(false);
        }
        else if (reply.Pdu().ErrorStatus.ToInt32() != 0)
        {
            throw ErrorException.Create(
                "error in response",
                receiver.Address,
                reply);
        }

        var next = reply.Pdu().Variables;
        return new Tuple<bool, IList<Variable>, ISnmpMessage>(next.Count != 0, next, request);
    }

    /// <summary>
    /// Set asynchronously the SNMP information of the given IP address (Applies to v1 and v2c).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="oid">Object Identifier to use for the set.</param>
    /// <param name="data">Data to set.</param>
    /// <param name="options">SNMP options.</param>
    public void SetAsync(IPAddress ipAddress, string oid, string data, SNMPOptions options)
    {
        Task.Run(async () =>
        {
            try
            {
                var version = options.Version == SNMPVersion.V1 ? VersionCode.V1 : VersionCode.V2;
                var ipEndPoint = new IPEndPoint(ipAddress, options.Port);
                var community = new OctetString(SecureStringHelper.ConvertToString(options.Community));
                var variables = new List<Variable>() { new(new ObjectIdentifier(oid), new OctetString(data)) };

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

    /// <summary>
    /// Set asynchronously the SNMP information of the given IP address (Applies to v3).
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="oid">Object Identifier to use for the set.</param>
    /// <param name="data">Data to set.</param>
    /// <param name="options">SNMP options.</param>
    public void SetAsyncV3(IPAddress ipAddress, string oid, string data, SNMPOptionsV3 options)
    {
        Task.Run(async () =>
        {
            try
            {
                var ipEndpoint = new IPEndPoint(ipAddress, options.Port);
                var username = new OctetString(options.Username);
                var variables = new List<Variable>() { new(new ObjectIdentifier(oid), new OctetString(data)) };

                var discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
                var report = await discovery.GetResponseAsync(ipEndpoint, options.CancellationToken);

                var privacy = GetPrivacyProvider(options);

                var request = new SetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextMessageId,
                    username, OctetString.Empty, variables, privacy, Messenger.MaxMessageSize, report);

                var reply = await request.GetResponseAsync(ipEndpoint);

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

    /// <summary>
    /// Create the privacy provider based on the given information's.
    /// </summary>
    /// <param name="options">SNMP v3 options.</param>
    /// <returns>Privacy provider.</returns>
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

    /// <summary>
    /// Create the privacy provider with default values.
    /// </summary>
    /// <returns>Privacy provider.</returns>
    private static IPrivacyProvider GetPrivacyProvider()
    {
        return new DefaultPrivacyProvider(DefaultAuthenticationProvider.Instance);
    }

    /// <summary>
    /// Create the privacy provider based on the given information's.
    /// </summary>
    /// <param name="authProvider">Authentication provider to use.</param>
    /// <param name="auth">Authentication password.</param>
    /// <returns>Privacy provider.</returns>
    private static IPrivacyProvider GetPrivacyProvider(SNMPV3AuthenticationProvider authProvider, SecureString auth)
    {
        return new DefaultPrivacyProvider(GetAuthenticationProvider(authProvider, auth));
    }

    /// <summary>
    /// Create the privacy provider based on the given information's.
    /// </summary>
    /// <param name="authProvider">Authentication provider to use.</param>
    /// <param name="auth">Authentication password.</param>
    /// <param name="privProvider">Privacy provider to use.</param>
    /// <param name="priv">Privacy password.</param>
    /// <returns>Privacy provider.</returns>
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

    /// <summary>
    /// Create the authentication provider based on the given information's.
    /// </summary>
    /// <param name="authProvider">Authentication provider to use.</param>
    /// <param name="auth">Authentication password.</param>
    /// <returns>Authentication provider.</returns>
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