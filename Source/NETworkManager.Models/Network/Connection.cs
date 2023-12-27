using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

public static class Connection
{
    #region Variables

    [DllImport("iphlpapi.dll", SetLastError = true)]
    public static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwSize, bool bOrder, int ulAf,
        TcpTableClass tableClass, uint reserved);

    #endregion

    [StructLayout(LayoutKind.Sequential)]
    public struct MibTcpRowOwnerPid
    {
        public uint state;
        public uint localAddr;
        public byte localPort1;
        public byte localPort2;
        public byte localPort3;
        public byte localPort4;
        public uint remoteAddr;
        public byte remotePort1;
        public byte remotePort2;
        public byte remotePort3;
        public byte remotePort4;
        public uint owningPid;
    }

    #region Enums

    public enum TcpTableClass
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }

    // Cache for remote host names with some default values
    private static readonly Dictionary<IPAddress, string> _remoteHostNames = new()
    {
        { IPAddress.Parse("127.0.0.1"), "localhost" },
        { IPAddress.Parse("::1"), "localhost" },
        { IPAddress.Parse("0.0.0.0"), "-/-" },
        { IPAddress.Parse("::"), "-/-" }
    };

    #endregion

    #region Methods

    public static Task<List<ConnectionInfo>> GetActiveTcpConnectionsAsync()
    {
        return Task.Run(GetActiveTcpConnections);
    }

    private static List<ConnectionInfo> GetActiveTcpConnections()
    {
        var result = new List<ConnectionInfo>();

        var size = 0;
        // ReSharper disable once RedundantAssignment - size is get by reference
        var dwResult = GetExtendedTcpTable(IntPtr.Zero, ref size, false, 2, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

        var tcpTable = Marshal.AllocHGlobal(size);

        try
        {
            dwResult = GetExtendedTcpTable(tcpTable, ref size, false, 2, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

            if (dwResult != 0)
                throw new Exception("Error while retrieving TCP table");

            var tableRows = Marshal.ReadInt32(tcpTable);
            var rowPtr = tcpTable + 4;

            for (var i = 0; i < tableRows; i++)
            {
                var row = (MibTcpRowOwnerPid)Marshal.PtrToStructure(rowPtr, typeof(MibTcpRowOwnerPid))!;

                var localAddress = new IPAddress(row.localAddr);
                var localPort = BitConverter.ToUInt16(new[] { row.localPort2, row.localPort1 }, 0);
                var remoteAddress = new IPAddress(row.remoteAddr);
                var remotePort = BitConverter.ToUInt16(new[] { row.remotePort2, row.remotePort1 }, 0);
                var state = (TcpState)row.state;

                // Get process info by PID
                var processId = (int)row.owningPid;
                var processName = "-/-";
                var processPath = "-/-";

                try
                {
                    var process = Process.GetProcessById(processId);
                    processName = process.ProcessName;
                    processPath = process.MainModule?.FileName;
                }
                catch (Exception)
                {
                    // ignored - process not found
                }

                // Resolve remote host name if not cached
                if (!_remoteHostNames.ContainsKey(remoteAddress))
                {
                    var dnsResolverTask = DNSClient.GetInstance().ResolvePtrAsync(remoteAddress);

                    dnsResolverTask.Wait();

                    // Cache the result
                    _remoteHostNames.Add(remoteAddress,
                        !dnsResolverTask.Result.HasError ? dnsResolverTask.Result.Value : "-/-");

                    Debug.WriteLine("Cache: Added " + remoteAddress + " -> " +
                                    (!dnsResolverTask.Result.HasError ? dnsResolverTask.Result.Value : "-/-"));
                }

                result.Add(new ConnectionInfo(
                    TransportProtocol.Tcp,
                    localAddress,
                    localPort,
                    remoteAddress,
                    remotePort,
                    _remoteHostNames.GetValueOrDefault(remoteAddress, "-/-"),
                    state,
                    processId,
                    processName,
                    processPath)
                );

                rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(typeof(MibTcpRowOwnerPid)));
            }
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            Marshal.FreeHGlobal(tcpTable);
        }

        return result;
    }

    #endregion
}