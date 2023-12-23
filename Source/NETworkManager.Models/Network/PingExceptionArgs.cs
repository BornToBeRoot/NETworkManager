using System;

namespace NETworkManager.Models.Network;

public class PingExceptionArgs : EventArgs
{
    public PingExceptionArgs(string message, Exception innerException)
    {
        Message = message;
        InnerException = innerException;
    }

    public string Message { get; set; }
    public Exception InnerException { get; set; }
}