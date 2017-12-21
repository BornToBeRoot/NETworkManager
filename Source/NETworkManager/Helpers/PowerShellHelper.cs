using System.Diagnostics;

namespace NETworkManager.Helpers
{
    public static class PowerShellHelper
    {
        public static void RunPSCommand(string command, bool asAdmin = false, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
        {
            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = string.Format("-NoProfile -NoLogo -Command {0}", command),
                WindowStyle = windowStyle
            };

            if (asAdmin)
                info.Verb = "runas";

            using (Process process = new Process())
            {
                process.StartInfo = info;

                process.Start();
                process.WaitForExit();
            }
        }

        /* Catch error
         try
                {
                 

                }
                catch (Win32Exception win32ex)
                {
                    switch (win32ex.NativeErrorCode)
                    {
                        case 1223:
                            OnUserHasCanceled();
                            break;
                        default:
                            throw;
                    }
                } 
         */
    }
}
