namespace NETworkManager;

/// <summary>
///     How the run command dialog runs a command.
/// </summary>
public enum RunCommandRunMode
{
    /// <summary>
    ///     Run the command and close the dialog (Enter).
    /// </summary>
    Run,

    /// <summary>
    ///     Run the command and keep the dialog open to run another one (Ctrl+Enter), unless the command opens a dialog.
    /// </summary>
    RunAndKeepOpen,

    /// <summary>
    ///     Open the application with the input filled in, without starting it (Shift+Enter).
    /// </summary>
    FillIn
}
