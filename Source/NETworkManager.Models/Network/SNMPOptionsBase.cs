using System.Threading;
using Lextm.SharpSnmpLib.Messaging;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class representing the SNMP options for all versions.
/// </summary>
public abstract class SNMPOptionsBase
{
    /// <summary>
    ///     Create an instance of <see cref="SNMPOptionsBase" /> with parameters.
    /// </summary>
    /// <param name="version">SNMP version to use.</param>
    /// <param name="port">Port to use for SNMP requests.</param>
    /// <param name="walkMode">Walk mode.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public SNMPOptionsBase(SNMPVersion version, int port, WalkMode walkMode, CancellationToken cancellationToken)
    {
        Version = version;
        Port = port;
        WalkMode = walkMode;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    ///     SNMP version to use.
    /// </summary>
    public SNMPVersion Version { get; set; }

    /// <summary>
    ///     Port to use for SNMP requests.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///     Walk mode.
    /// </summary>
    public WalkMode WalkMode { get; set; }

    /// <summary>
    ///     Cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
}