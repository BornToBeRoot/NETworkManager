using System;
using log4net;
using Windows.Networking.Connectivity;

namespace NETworkManager.Models.Network;

/// <summary>
///     Internet connectivity as reported by Windows (Network Connectivity Status Indicator, NCSI). Windows checks it
///     itself with its own test server, so no request is sent by the application.
/// </summary>
public enum InternetConnectivityLevel
{
    /// <summary>
    ///     Unknown (e.g. the status could not be read).
    /// </summary>
    Unknown,

    /// <summary>
    ///     No connection.
    /// </summary>
    None,

    /// <summary>
    ///     Local network access only.
    /// </summary>
    LocalAccess,

    /// <summary>
    ///     Limited internet access, e.g. a hotspot / captive portal that requires a sign-in.
    /// </summary>
    ConstrainedInternetAccess,

    /// <summary>
    ///     Internet access.
    /// </summary>
    InternetAccess
}

/// <summary>
///     Reads the internet connectivity reported by Windows.
/// </summary>
public static class InternetConnectivity
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(InternetConnectivity));

    /// <summary>
    ///     Gets the internet connectivity of the connection profile that Windows uses for the internet.
    /// </summary>
    /// <returns>Connectivity level, <see cref="InternetConnectivityLevel.Unknown" /> on error.</returns>
    public static InternetConnectivityLevel GetLevel()
    {
        try
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();

            if (profile == null)
                return InternetConnectivityLevel.None;

            return profile.GetNetworkConnectivityLevel() switch
            {
                NetworkConnectivityLevel.None => InternetConnectivityLevel.None,
                NetworkConnectivityLevel.LocalAccess => InternetConnectivityLevel.LocalAccess,
                NetworkConnectivityLevel.ConstrainedInternetAccess => InternetConnectivityLevel.ConstrainedInternetAccess,
                NetworkConnectivityLevel.InternetAccess => InternetConnectivityLevel.InternetAccess,
                _ => InternetConnectivityLevel.Unknown
            };
        }
        catch (Exception ex)
        {
            Log.Warn("Could not get the internet connectivity reported by Windows.", ex);
            return InternetConnectivityLevel.Unknown;
        }
    }
}
