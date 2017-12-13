using System.Security;
using System;
using System.Runtime.InteropServices;

namespace NETworkManager.Helpers
{
    public static class SecureStringHelper
    {
        public static string ConvertToString(SecureString secureString)
        {
            IntPtr valuePtr = IntPtr.Zero;
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

        public static SecureString ConvertToSecureString(string string1)
        {
            if (string1 == null)
                throw new ArgumentNullException("string1");

            var securePassword = new SecureString();

            foreach (char c in string1)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }
    }
}