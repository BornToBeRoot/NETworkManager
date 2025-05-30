﻿namespace NETworkManager.Models.AWS;

/// <summary>
///     Class is used to store information about an AWS profile.
/// </summary>
public class AWSProfileInfo
{
    /// <summary>
    ///     Create an empty instance of <see cref="AWSProfileInfo" />.
    /// </summary>
    public AWSProfileInfo()
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="AWSProfileInfo" /> with parameters.
    /// </summary>
    /// <param name="isEnabled"><see cref="IsEnabled" />.</param>
    /// <param name="profile"><see cref="Profile" />.</param>
    /// <param name="region"><see cref="Region" />.</param>
    public AWSProfileInfo(bool isEnabled, string profile, string region)
    {
        IsEnabled = isEnabled;
        Profile = profile;
        Region = region;
    }

    /// <summary>
    ///     Indicates if the AWS profile is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    ///     Name of the profile configured in ~\.aws\credentials.
    /// </summary>
    public string Profile { get; set; }

    /// <summary>
    ///     AWS region.
    /// </summary>
    public string Region { get; set; }
}