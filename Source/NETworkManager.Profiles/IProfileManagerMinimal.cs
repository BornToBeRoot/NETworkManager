namespace NETworkManager.Profiles;

/// <summary>
///     Minimal interface for the profile manager, providing lifecycle callbacks
///     for when a <see cref="ProfileManager" /> dialog is opened or closed.
/// </summary>
public interface IProfileManagerMinimal
{
    /// <summary>
    ///     Called when a dialog in the <see cref="ProfileManager" /> is opened.
    /// </summary>
    public void OnProfileManagerDialogOpen()
    {

    }

    /// <summary>
    ///     Called when a dialog in the <see cref="ProfileManager" /> is closed.
    /// </summary>
    public void OnProfileManagerDialogClose()
    {

    }
}
