using System.Diagnostics;

namespace NETworkManager.Utilities
{
    public static class CustomCommand
    {
        public static void Run(CustomCommandInfo info)
        {
            if (string.IsNullOrEmpty(info.Arguments))
                Process.Start(info.FilePath);
            else
                Process.Start(info.FilePath, info.Arguments);
        }
    }
}