using NETworkManager.Models.Lookup;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class representing a port info.
/// </summary>
public class PortInfo : PropertyChangedBase
{
    /// <summary>
    ///     Create an instance of <see cref="PortInfo" /> with parameters.
    /// </summary>
    /// <param name="port"></param>
    /// <param name="lookupInfo"></param>
    /// <param name="state"></param>
    public PortInfo(int port, PortLookupInfo lookupInfo, PortState state)
    {
        Port = port;
        LookupInfo = lookupInfo;
        State = state;
    }

    /// <summary>
    ///     Port number.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///     Port lookup info like service and description.
    /// </summary>
    public PortLookupInfo LookupInfo { get; set; }

    /// <summary>
    ///     State of the port. Observable, a rescan of the port updates it in place (Port Scanner).
    /// </summary>
    public PortState State
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