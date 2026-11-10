using NETworkManager.Controls;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace NETworkManager.Models;

/// <summary>
///     Provides methods to manage applications.
/// </summary>
public static class ApplicationManager
{
    /// <summary>
    ///     Method to return all available applications in.
    /// </summary>
    /// <returns>IEnumerable with <see cref="ApplicationName" />.</returns>
    public static IEnumerable<ApplicationName> GetNames()
    {
        return Enum.GetValues<ApplicationName>();
    }

    /// <summary>
    ///     Method returns a default list with all <see cref="ApplicationInfo" /> and sets
    ///     the default properties for each application.
    /// </summary>
    /// <returns>IEnumerable with <see cref="ApplicationInfo" />.</returns>
    public static IEnumerable<ApplicationInfo> GetDefaultList()
    {
#pragma warning disable CS0618
        return [.. GetNames().Where(x => x != ApplicationName.None && x != ApplicationName.AWSSessionManager && x != ApplicationName.ARPTable).Select(name => new ApplicationInfo(name, true, name == ApplicationName.Dashboard))];
#pragma warning restore CS0618
    }

    /// <summary>
    ///     Gets the host to open in an application (e.g. the hostname of an IP Scanner result or the target of the run
    ///     command): for an application that resolves it via Windows (Remote Desktop, PuTTY, TigerVNC, PowerShell)
    ///     without the trailing dot (see <see cref="DNSClientHelper.SanitizeHostname" />). The other applications resolve
    ///     it via NETworkManager and keep the dot of a single-label name (see <see cref="DNSClientHelper.TrimTrailingDot" />).
    ///     Used by <c>NavigationManager.Open</c> for every request.
    /// </summary>
    /// <param name="name">Application to open.</param>
    /// <param name="host">Hostname or IP address (or another target, e.g. a URL or IP range).</param>
    /// <returns>Host for the application.</returns>
    public static string GetHostForApplication(ApplicationName name, string host)
    {
        // Not the Web Console: its target is a URL, created by WebConsoleUrlHelper (which also removes the dot)
        return name is ApplicationName.RemoteDesktop or ApplicationName.PuTTY or ApplicationName.TigerVNC
            or ApplicationName.PowerShell
            ? DNSClientHelper.SanitizeHostname(host)
            : host;
    }

    /// <summary>
    ///     Method will return the icon based on <see cref="ApplicationName" />.
    /// </summary>
    /// <param name="name">
    ///     <see cref="ApplicationName" />
    /// </param>
    /// <returns>Application icon as <see cref="Canvas" />.</returns>
    public static Canvas GetIcon(ApplicationName name)
    {
        var canvas = new Canvas();

        switch (name)
        {
            case ApplicationName.Dashboard:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.LayoutDashboard });
                break;
            case ApplicationName.NetworkInterface:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.EthernetPort });
                break;
            case ApplicationName.WiFi:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Wifi });
                break;
            case ApplicationName.IPScanner:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Network });
                break;
            case ApplicationName.PortScanner:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.ScanLine });
                break;
            case ApplicationName.PingMonitor:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Radar });
                break;
            case ApplicationName.Traceroute:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Route });
                break;
            case ApplicationName.DNSLookup:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Globe });
                break;
            case ApplicationName.RemoteDesktop:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Monitor });
                break;
            case ApplicationName.PowerShell:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.SquareChevronRight });
                break;
            case ApplicationName.PuTTY:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Terminal });
                break;
            case ApplicationName.TigerVNC:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Eye });
                break;
            case ApplicationName.WebConsole:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.AppWindow });
                break;
            case ApplicationName.SNMP:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Server });
                break;
            case ApplicationName.SNTPLookup:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.ClockCheck });
                break;
            case ApplicationName.HostsFileEditor:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.FilePenLine });
                break;
            case ApplicationName.DiscoveryProtocol:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.ArrowLeftRight });
                break;
            case ApplicationName.WakeOnLAN:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Power });
                break;
            case ApplicationName.RDAP:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.IdCard });
                break;
            case ApplicationName.Whois:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.FileSearch });
                break;
            case ApplicationName.IPGeolocation:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.MapPin });
                break;
            case ApplicationName.SubnetCalculator:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Calculator });
                break;
            case ApplicationName.BitCalculator:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Binary });
                break;
            case ApplicationName.Lookup:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.DatabaseSearch });
                break;
            case ApplicationName.Connections:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.PlugZap });
                break;
            case ApplicationName.Listeners:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.RadioTower });
                break;
            case ApplicationName.NeighborTable:
#pragma warning disable CS0618
            case ApplicationName.ARPTable:
#pragma warning restore CS0618
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Table });
                break;
            case ApplicationName.Firewall:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.BrickWallFire });
                break;
            case ApplicationName.None:
            case ApplicationName.AWSSessionManager:
            default:
                canvas.Children.Add(new LucideIcon { Kind = LucideIconKind.Frown });
                break;
        }

        return canvas;
    }
}