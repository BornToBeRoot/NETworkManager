using System.Net;

namespace NETworkManager.Models.Network;

public class WakeOnLANInfo
{
    public IPAddress Broadcast;
    public byte[] MagicPacket;
    public int Port;

    public WakeOnLANInfo()
    {
    }

    public WakeOnLANInfo(byte[] magicPacket, IPAddress broadcast, int port)
    {
        MagicPacket = magicPacket;
        Broadcast = broadcast;
        Port = port;
    }
}