namespace NETworkManager.Models.RemoteDesktop;

/// <summary>
///     Represents keystrokes which can be send into the remote session.
/// </summary>
public enum Keystroke
{
    /// <summary>
    ///     Ctrl + Alt + Del keystroke.
    /// </summary>
    CtrlAltDel,

    /// <summary>
    ///     Ctrl + Shift + Esc keystroke (opens Task Manager).
    /// </summary>
    TaskManager,

    /// <summary>
    ///     Win + L keystroke (locks the session).
    /// </summary>
    Lock,

    /// <summary>
    ///     Win + D keystroke (shows the desktop / minimizes all windows).
    /// </summary>
    ShowDesktop,

    /// <summary>
    ///     Win + E keystroke (opens File Explorer).
    /// </summary>
    Explorer,

    /// <summary>
    ///     Win + R keystroke (opens the Run dialog).
    /// </summary>
    RunDialog
}