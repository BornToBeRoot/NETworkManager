namespace NETworkManager.Models.Network;

public class TracerouteErrorArgs : System.EventArgs
{
    public string ErrorMessage { get; set; }

    public TracerouteErrorArgs()
    {

    }

    public TracerouteErrorArgs(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
