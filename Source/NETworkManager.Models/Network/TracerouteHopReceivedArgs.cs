namespace NETworkManager.Models.Network;

public class TracerouteHopReceivedArgs : System.EventArgs
{
    public TracerouteHopInfo Args { get; }

    public TracerouteHopReceivedArgs(TracerouteHopInfo args)
    {
        Args = args;
    }
}
