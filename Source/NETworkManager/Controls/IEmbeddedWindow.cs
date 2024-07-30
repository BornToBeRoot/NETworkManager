namespace NETworkManager.Controls;

/// <summary>
///     Interface for a user control that contains an embedded window like a <see cref="PowerShellControl" /> or
///     <see cref="PuTTYControl" />.
/// </summary>
public interface IEmbeddedWindow
{
    /// <summary>
    ///     Method to focus the embedded window.
    /// </summary>
    public void FocusEmbeddedWindow()
    {
    }
}