using System.Security;
using System;
using System.Runtime.InteropServices;

namespace NETworkManager.Utilities
{
    public static class SecureStringHelper
    {
        public static string ConvertToString(SecureString secureString)
        {
            var valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        public static SecureString ConvertToSecureString(string clearText)
        {
            if (clearText == null)
                throw new ArgumentNullException(nameof(clearText));

            var securePassword = new SecureString();

            foreach (var c in clearText)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }
    }
}