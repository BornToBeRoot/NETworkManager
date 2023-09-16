namespace NETworkManager.Models.Network;

/// <summary>
/// Contains the information of a received SNMP message in a <see cref="SNMPClient"/>.
/// </summary>
public class SNMPReceivedArgs : System.EventArgs
{
    /// <summary>
    /// SNMP information.
    /// </summary>
    public SNMPInfo Args { get; }

    /// <summary>
    /// Creates a new instance of <see cref="SNMPReceivedArgs"/> with the given <see cref="SNMPInfo"/>.
    /// </summary>
    /// <param name="args">SNMP information.</param>
    public SNMPReceivedArgs(SNMPInfo args)
    {
        Args = args;
    }
}