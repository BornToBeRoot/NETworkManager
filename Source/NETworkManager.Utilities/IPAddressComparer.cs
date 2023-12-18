using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace NETworkManager.Utilities
{
    public class IPAddressComparer : IComparer<IPAddress>
    {
        public int Compare(IPAddress first, IPAddress second)
        {
            return IPAddressHelper.CompareIPAddresses(first, second);
        }
    }
}
