using System.Security;
using System.Threading;
using Lextm.SharpSnmpLib.Messaging;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class representing the SNMP options for version 1 and 2c.
/// </summary>
public class SNMPOptions : SNMPOptionsBase
{
    /// <summary>
    ///     Create an instance of <see cref="SNMPOptions" /> with parameters.
    /// </summary>
    /// <param name="community">Community as <see cref="SecureString" /> for SNMPv1 or v2c.</param>
    /// <param name="version">SNMP version to use.</param>
    /// <param name="port">Port to use for SNMP requests.</param>
    /// <param name="walkMode">Walk mode.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public SNMPOptions(SecureString community, SNMPVersion version, int port, WalkMode walkMode,
        CancellationToken cancellationToken) : base(version, port, walkMode, cancellationToken)
    {
        Community = community;
    }

    /// <summary>
    ///     Community as <see cref="SecureString" /> for SNMPv1 or v2c.
    /// </summary>
    public SecureString Community { get; set; }
}