namespace NETworkManager.Profiles
{
    /// <summary>
    /// Interface for the profile manager. 
    /// Minimal implementation to get the view model.
    /// </summary>
    public interface IProfileManagerMinimal
    {
        /// <summary>
        /// Event is fired when the profile dialog is opened.
        /// </summary>
        void OnProfileDialogOpen();

        /// <summary>
        /// Event is fired when the profile dialog is closed.
        /// </summary>
        void OnProfileDialogClose();
    }
}
