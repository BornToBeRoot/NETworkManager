// Source: https://stackoverflow.com/a/1148861/4986782
// Modified by BornToBeRoot

using NETworkManager.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class ARPTable
    {
        #region Variables
        // The max number of physical addresses.
        const int MAXLEN_PHYSADDR = 8;

        // Define the MIB_IPNETROW structure.
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_IPNETROW
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwIndex;
            [MarshalAs(UnmanagedType.U4)]
            public int dwPhysAddrLen;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac0;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac1;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac2;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac3;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac4;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac5;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac6;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac7;
            [MarshalAs(UnmanagedType.U4)]
            public int dwAddr;
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
        }

        // Declare the GetIpNetTable function.
        [DllImport("IpHlpApi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int GetIpNetTable(IntPtr pIpNetTable, [MarshalAs(UnmanagedType.U4)] ref int pdwSize, bool bOrder);

        [DllImport("IpHlpApi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int FreeMibTable(IntPtr plpNetTable);

        // The insufficient buffer error.
        const int ERROR_INSUFFICIENT_BUFFER = 122;
        #endregion

        #region Events
        public event EventHandler UserHasCanceled;

        protected virtual void OnUserHasCanceled()
        {
            UserHasCanceled?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        public static List<ARPTableInfo> GetTable()
        {
            List<ARPTableInfo> list = new List<ARPTableInfo>();

            // The number of bytes needed.
            int bytesNeeded = 0;

            // The result from the API call.
            int result = GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false);

            // Call the function, expecting an insufficient buffer.
            if (result != ERROR_INSUFFICIENT_BUFFER)
            {
                // Throw an exception.
                throw new Win32Exception(result);
            }

            // Allocate the memory, do it in a try/finally block, to ensure
            // that it is released.
            IntPtr buffer = IntPtr.Zero;

            // Try/finally.
            try
            {
                // Allocate the memory.
                buffer = Marshal.AllocCoTaskMem(bytesNeeded);

                // Make the call again. If it did not succeed, then
                // raise an error.
                result = GetIpNetTable(buffer, ref bytesNeeded, false);

                // If the result is not 0 (no error), then throw an exception.
                if (result != 0)
                {
                    // Throw an exception.
                    throw new Win32Exception(result);
                }

                // Now we have the buffer, we have to marshal it. We can read
                // the first 4 bytes to get the length of the buffer.
                int entries = Marshal.ReadInt32(buffer);

                // Increment the memory pointer by the size of the int.
                IntPtr currentBuffer = new IntPtr(buffer.ToInt64() +
                   Marshal.SizeOf(typeof(int)));

                // Allocate an array of entries.
                MIB_IPNETROW[] table = new MIB_IPNETROW[entries];

                // Cycle through the entries.
                for (int i = 0; i < entries; i++)
                {
                    // Call PtrToStructure, getting the structure information.
                    table[i] = (MIB_IPNETROW)Marshal.PtrToStructure(new
                       IntPtr(currentBuffer.ToInt64() + (i * Marshal.SizeOf(typeof(MIB_IPNETROW)))), typeof(MIB_IPNETROW));
                }

                PhysicalAddress virtualMAC = new PhysicalAddress(new byte[] { 0, 0, 0, 0, 0, 0 });
                PhysicalAddress broadcastMAC = new PhysicalAddress(new byte[] { 255, 255, 255, 255, 255, 255 });

                for (int i = 0; i < entries; i++)
                {
                    MIB_IPNETROW row = table[i];

                    IPAddress ipAddress = new IPAddress(BitConverter.GetBytes(row.dwAddr));
                    PhysicalAddress macAddress = new PhysicalAddress(new byte[] { row.mac0, row.mac1, row.mac2, row.mac3, row.mac4, row.mac5 });

                    // Filter 0.0.0.0.0.0, 255.255.255.255.255.255
                    if (!macAddress.Equals(virtualMAC) && !macAddress.Equals(broadcastMAC)) //&& !(ipAddress.IsIPv6Multicast || IPv4AddressHelper.IsMulticast(ipAddress)))
                        list.Add(new ARPTableInfo(ipAddress, macAddress));
                }

                return list;
            }
            finally
            {
                // Release the memory.
                FreeMibTable(buffer);
            }
        }

        public Task DeleteTableAsync()
        {
            return Task.Run(() => DeleteTable());
        }

        public void DeleteTable()
        {
            string command = string.Format("netsh interface ip delete arpcache");

            // Start process with elevated rights...
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                Verb = "runas",
                FileName = "powershell.exe",
                Arguments = string.Format("-NoProfile -NoLogo -Command {0}", command)
            };

            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            using (Process process = new Process())
            {
                process.StartInfo = processStartInfo;

                try
                {
                    process.Start();
                    process.WaitForExit();
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
            }
        }
        #endregion
    }
}