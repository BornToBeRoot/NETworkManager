using NETworkManager.Models.Network;
using NETworkManager.Settings;

namespace NETworkManager.Profiles.Application;

public static class PortScanner
{
    /// <summary>
    ///     Gets the ports to scan for a profile: its own ports (if overridden), otherwise the ports of its group (if
    ///     overridden), otherwise none. Port profiles with their current name (also after a rename); if a port profile
    ///     can't be found (deleted, or a profile file shared from another computer - port profiles are saved in the
    ///     settings), the ports saved with the profile or group.
    /// </summary>
    /// <param name="profile">Profile to get the ports for.</param>
    /// <param name="portProfileNotFound">True if a port profile can't be found.</param>
    /// <returns>Ports to scan, empty if neither the profile nor its group has ports.</returns>
    public static string GetPorts(ProfileInfo profile, out bool portProfileNotFound)
    {
        // Get group info
        var group = ProfileManager.GetGroupByName(profile.Group);

        var (ports, portsInput) = profile.PortScanner_OverridePorts
            ? (profile.PortScanner_Ports, profile.PortScanner_PortsInput)
            : group.PortScanner_OverridePorts
                ? (group.PortScanner_Ports, group.PortScanner_PortsInput)
                : (string.Empty, null);

        return PortProfile.Decode(ports, portsInput, SettingsManager.Current.PortScanner_PortProfiles,
            out portProfileNotFound);
    }
}
