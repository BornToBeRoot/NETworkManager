using System;

namespace NETworkManager.Profiles;

public class ProfileFileInfoArgs : EventArgs
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ProfileFileInfoArgs" /> class with the specified parameters.
    /// </summary>
    /// <param name="profileFileInfo">Profile file info which is loaded.</param>
    public ProfileFileInfoArgs(ProfileFileInfo profileFileInfo)
    {
        ProfileFileInfo = profileFileInfo;
        ProfileFileUpdating = false;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ProfileFileInfoArgs" /> class with the specified parameters.
    /// </summary>
    /// <param name="profileFileInfo">Profile file info which should be loaded.</param>
    /// <param name="profileFileUpdating">
    ///     If true, the profile will be updated in the UI, but does not trigger a profile load
    ///     event (e.g. to enter the password for decryption).
    /// </param>
    public ProfileFileInfoArgs(ProfileFileInfo profileFileInfo, bool profileFileUpdating)
    {
        ProfileFileInfo = profileFileInfo;
        ProfileFileUpdating = profileFileUpdating;
    }

    /// <summary>
    ///     Profile file info which is loaded or should be loaded.
    /// </summary>
    public ProfileFileInfo ProfileFileInfo { get; set; }

    /// <summary>
    ///     If true, the profile will be updated in the UI, but does not trigger a
    ///     profile load event (e.g. to enter the password for decryption).
    /// </summary>
    public bool ProfileFileUpdating { get; set; }
}