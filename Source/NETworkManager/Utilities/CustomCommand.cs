using System;
using System.Diagnostics;
using System.Windows;

namespace NETworkManager.Utilities
{
    public static class CustomCommand
    {
        public static void Run(CustomCommandInfo info)
        {
            try
            {
                if (string.IsNullOrEmpty(info.Arguments))
                    Process.Start(info.FilePath);
                else
                    Process.Start(info.FilePath, info.Arguments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in Utilities.CustomCommand.Run()", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}