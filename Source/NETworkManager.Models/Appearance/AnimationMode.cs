namespace NETworkManager.Models.Appearance;

/// <summary>
///     Animations of the application (e.g. when a dialog is opened or closed).
/// </summary>
public enum AnimationMode
{
    /// <summary>
    ///     Follow the Windows setting "Animation effects" (Settings > Accessibility > Visual effects).
    /// </summary>
    System,

    /// <summary>
    ///     Always show animations.
    /// </summary>
    On,

    /// <summary>
    ///     Never show animations.
    /// </summary>
    Off
}
