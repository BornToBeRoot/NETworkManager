using Lextm.SharpSnmpLib;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class containing SNMP information.
/// </summary>
public class SNMPInfo
{
    /// <summary>
    /// Object identifier of the SNMP message.
    /// </summary>
    public string Oid { get; set; }
    
    /// <summary>
    /// Data of the SNMP message.
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="SNMPInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="oid">Object identifier of the SNMP message.</param>
    /// <param name="data">Data of the SNMP message.</param>
    public SNMPInfo(string oid, string data)
    {
        Oid = oid;
        Data = data;
    }
    
    /// <summary>
    /// Creates a new instance of <see cref="SNMPInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="oid">Object identifier of the SNMP message.</param>
    /// <param name="data">Data of the SNMP message.</param>
    public SNMPInfo(ObjectIdentifier oid, ISnmpData data) : this(oid.ToString(), data.ToString())
    {
        
    }
    
}
