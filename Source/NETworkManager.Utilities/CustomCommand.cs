namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class provides static methods to manage custom commands.
    /// </summary>
    public static class CustomCommand
    {
        /// <summary>
        /// Method to execute a <see cref="CustomCommandInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="CustomCommandInfo"/> which is executed.</param>
        public static void Run(CustomCommandInfo info)
        {
            ExternalProcessStarter.RunProcess(info.FilePath, info.Arguments);
        }
    }
}