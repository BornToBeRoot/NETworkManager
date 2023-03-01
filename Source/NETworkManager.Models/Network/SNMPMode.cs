namespace NETworkManager.Models.Network;

/// <summary>
/// Enum SNMP Mode.
/// 
/// Trap and Inform are not implemented yet.
/// </summary>
public enum SNMPMode
{
    Get,
    Walk,
    Set,
    Trap,
    Inform
}
