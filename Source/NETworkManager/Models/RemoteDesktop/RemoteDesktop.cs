using Microsoft.Win32;

namespace NETworkManager.Models.RemoteDesktop
{
    public static class RemoteDesktop
    {
        private const string CLSID_MsRdpClient9NotSafeForScripting = @"8B918B82-7985-4C24-89DF-C33AD2BBFBCD";

        public static bool IsRDP8dot1Available()
        {
            RegistryKey msRdpClient9NotSafeForScriptingKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\CLSID\{" + CLSID_MsRdpClient9NotSafeForScripting + "}", false);

            return (msRdpClient9NotSafeForScriptingKey != null);
        }
    }
}
