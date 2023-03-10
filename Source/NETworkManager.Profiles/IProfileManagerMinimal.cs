namespace NETworkManager.Profiles;

/// <summary>
/// Interface for the profile manager. 
/// Minimal implementation to get the view model.
/// </summary>
public interface IProfileManagerMinimal
{
    /// <summary>
    /// Event is fired when a dialog in the <see cref="ProfileManager"/> is opened.
    /// </summary>
    public void OnProfileManagerDialogOpen()
    {

    }

    /// <summary>
    /// Event is fired when a dialog in the <see cref="ProfileManager"/> is closed.
    /// </summary>
    public void OnProfileManagerDialogClose()
    {

    }
}
