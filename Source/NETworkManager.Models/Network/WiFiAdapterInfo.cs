using NETworkManager.Utilities;
using Windows.Devices.WiFi;

namespace NETworkManager.Models.Network;

public class WiFiAdapterInfo : PropertyChangedBase
{
    public NetworkInterfaceInfo NetworkInterfaceInfo { get; set; }
    public WiFiAdapter WiFiAdapter { get; set; }

    /// <summary>
    ///     Indicates if the adapter is up (connected to a network). Updated after each scan (see
    ///     <see cref="WiFi.UpdateAdapterStates" />), <see cref="NetworkInterfaceInfo" /> is the state when the adapters
    ///     were loaded.
    /// </summary>
    public bool IsOperational
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }
}