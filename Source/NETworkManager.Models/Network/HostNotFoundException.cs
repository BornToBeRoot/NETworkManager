using System;

namespace NETworkManager.Models.Network
{
    public class HostNotFoundException : Exception
    {
        public HostNotFoundException()
        {

        }

        public HostNotFoundException(string message) : base(message)
        {

        }

        public HostNotFoundException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
