using System.Windows;

namespace NETworkManager.Utilities.WPF;

/// <summary>
/// Values for the animations of the styles (e.g. dialogs), used with <c>x:Static</c> in the storyboards (storyboards
/// in templates are frozen, <c>DynamicResource</c> can't be used there).
///
/// Reduced motion: if "Animation effects" is turned off in Windows (Settings &gt; Accessibility &gt; Visual effects,
/// <see cref="SystemParameters.ClientAreaAnimation"/>), elements don't move or scale, they only fade. Read once when
/// the styles are loaded, a change applies after a restart of the application.
/// </summary>
public static class Motion
{
    /// <summary>
    /// <c>true</c> if Windows shows animations ("Animation effects" turned on).
    /// </summary>
    public static bool IsEnabled { get; } = SystemParameters.ClientAreaAnimation;

    /// <summary>
    /// Scale of a dialog when it starts to open (grows to 1).
    /// </summary>
    public static double DialogShowScale { get; } = IsEnabled ? 0.96 : 1;

    /// <summary>
    /// Scale of a dialog when it is closed (shrinks from 1).
    /// </summary>
    public static double DialogHideScale { get; } = IsEnabled ? 0.98 : 1;
}
