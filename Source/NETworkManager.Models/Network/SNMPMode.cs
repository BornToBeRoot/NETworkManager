namespace NETworkManager.Models.Network;

/// <summary>
/// Enum SNMP Mode.
/// 
/// Trap and Inform are not implemented yet.
/// </summary>
public enum SNMPMode
{
    /// <summary>
    /// SNMP Get.
    /// </summary>
    Get,

    /// <summary>
    /// SNMP Walk.
    /// </summary>
    Walk,

    /// <summary>
    /// SNMP Set.
    /// </summary>
    Set,

    /// <summary>
    /// SNMP Trap.
    /// </summary>
    Trap,

    /// <summary>
    /// SNMP Inform.
    /// </summary>
    Inform
}
