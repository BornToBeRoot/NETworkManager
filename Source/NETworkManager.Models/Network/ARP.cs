// Contains code from: https://stackoverflow.com/a/1148861/4986782
// Modified by BornToBeRoot

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Utilities;
using SMA = System.Management.Automation;
using log4net;

namespace NETworkManager.Models.Network;

/// <summary>
/// Provides static methods to read and modify the Windows ARP table.
/// Read access uses the <c>IpHlpApi</c> Win32 API. Modifying operations
/// (add/delete entries, clear table) run via PowerShell in a shared
/// <see cref="Runspace"/> that is initialized once with the required
/// execution policy. A <see cref="SemaphoreSlim"/> serializes access so
/// the runspace is never used concurrently. Modifying operations require
/// the application to run with elevated rights.
/// </summary>
public class ARP
{
    #region Variables

    /// <summary>
    /// The logger for this class.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(ARP));

    // The max number of physical addresses.
    private const int MAXLEN_PHYSADDR = 8;

    // Define the MIB_IPNETROW structure.
    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_IPNETROW
    {
        [MarshalAs(UnmanagedType.U4)] public int dwIndex;
        [MarshalAs(UnmanagedType.U4)] public int dwPhysAddrLen;
        [MarshalAs(UnmanagedType.U1)] public byte mac0;
        [MarshalAs(UnmanagedType.U1)] public byte mac1;
        [MarshalAs(UnmanagedType.U1)] public byte mac2;
        [MarshalAs(UnmanagedType.U1)] public byte mac3;
        [MarshalAs(UnmanagedType.U1)] public byte mac4;
        [MarshalAs(UnmanagedType.U1)] public byte mac5;
        [MarshalAs(UnmanagedType.U1)] public byte mac6;
        [MarshalAs(UnmanagedType.U1)] public byte mac7;
        [MarshalAs(UnmanagedType.U4)] public int dwAddr;
        [MarshalAs(UnmanagedType.U4)] public int dwType;
    }

    // Declare the GetIpNetTable function.
    [DllImport("IpHlpApi.dll")]
    [return: MarshalAs(UnmanagedType.U4)]
    private static extern int GetIpNetTable(IntPtr pIpNetTable, [MarshalAs(UnmanagedType.U4)] ref int pdwSize,
        bool bOrder);

    [DllImport("IpHlpApi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern int FreeMibTable(IntPtr plpNetTable);

    // The insufficient buffer error.
    private const int ERROR_INSUFFICIENT_BUFFER = 122;

    /// <summary>
    /// Ensures that only one PowerShell pipeline runs on <see cref="SharedRunspace"/> at a time.
    /// </summary>
    private static readonly SemaphoreSlim Lock = new(1, 1);

    /// <summary>
    /// Shared PowerShell runspace, initialized once in the static constructor with
    /// <c>Set-ExecutionPolicy Bypass</c> so subsequent operations can run without
    /// repeating the policy change.
    /// </summary>
    private static readonly Runspace SharedRunspace;

    /// <summary>
    /// Opens <see cref="SharedRunspace"/> and runs the one-time initialization script.
    /// </summary>
    static ARP()
    {
        SharedRunspace = RunspaceFactory.CreateRunspace();
        SharedRunspace.Open();

        using var ps = SMA.PowerShell.Create();
        ps.Runspace = SharedRunspace;
        ps.AddScript("Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process").Invoke();
    }

    #endregion

    #region Methods

    public static Task<List<ARPInfo>> GetTableAsync()
    {
        return Task.Run(GetTable);
    }

    private static List<ARPInfo> GetTable()
    {
        var list = new List<ARPInfo>();

        // The number of bytes needed.
        var bytesNeeded = 0;

        // The result from the API call.
        var result = GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false);

        // Call the function, expecting an insufficient buffer.
        if (result != ERROR_INSUFFICIENT_BUFFER)
            // Throw an exception.
            throw new Win32Exception(result);

        // Allocate the memory, do it in a try/finally block, to ensure
        // that it is released.
        var buffer = IntPtr.Zero;

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
                // Throw an exception.
                throw new Win32Exception(result);

            // Now we have the buffer, we have to marshal it. We can read
            // the first 4 bytes to get the length of the buffer.
            var entries = Marshal.ReadInt32(buffer);

            // Increment the memory pointer by the size of the int.
            var currentBuffer = new IntPtr(buffer.ToInt64() +
                                           Marshal.SizeOf(typeof(int)));

            // Allocate an array of entries.
            var table = new MIB_IPNETROW[entries];

            // Cycle through the entries.
            for (var i = 0; i < entries; i++)
                // Call PtrToStructure, getting the structure information.
                table[i] = (MIB_IPNETROW)Marshal.PtrToStructure(new
                    IntPtr(currentBuffer.ToInt64() + i * Marshal.SizeOf(typeof(MIB_IPNETROW))), typeof(MIB_IPNETROW));

            var virtualMAC = new PhysicalAddress(new byte[] { 0, 0, 0, 0, 0, 0 });
            var broadcastMAC = new PhysicalAddress(new byte[] { 255, 255, 255, 255, 255, 255 });

            for (var i = 0; i < entries; i++)
            {
                var row = table[i];

                var ipAddress = new IPAddress(BitConverter.GetBytes(row.dwAddr));
                var macAddress = new PhysicalAddress(new[]
                    { row.mac0, row.mac1, row.mac2, row.mac3, row.mac4, row.mac5 });

                // Filter 0.0.0.0.0.0, 255.255.255.255.255.255
                if (!macAddress.Equals(virtualMAC) && !macAddress.Equals(broadcastMAC))
                    list.Add(new ARPInfo(ipAddress, macAddress,
                        ipAddress.IsIPv6Multicast || IPv4Address.IsMulticast(ipAddress)));
            }

            return list;
        }
        finally
        {
            // Release the memory.
            FreeMibTable(buffer);
        }
    }

    public static string GetMACAddress(IPAddress ipAddress)
    {
        var arpInfo = GetTable().FirstOrDefault(x => x.IPAddress.Equals(ipAddress));

        return arpInfo?.MACAddress.ToString();
    }

    /// <summary>
    /// Adds a static ARP entry by running <c>arp -s</c> through the shared PowerShell
    /// runspace. Requires the application to run with elevated rights.
    /// </summary>
    /// <param name="ipAddress">The IP address of the entry.</param>
    /// <param name="macAddress">The MAC address of the entry, separated with <c>-</c>.</param>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static async Task AddEntryAsync(string ipAddress, string macAddress)
    {
        await InvokeAsync($"arp -s '{EscapePs(ipAddress)}' '{EscapePs(macAddress)}' 2>&1 | Out-String");
    }

    /// <summary>
    /// Removes a single ARP entry by running <c>arp -d</c> through the shared PowerShell
    /// runspace. Requires the application to run with elevated rights.
    /// </summary>
    /// <param name="ipAddress">The IP address of the entry to remove.</param>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static async Task DeleteEntryAsync(string ipAddress)
    {
        await InvokeAsync($"arp -d '{EscapePs(ipAddress)}' 2>&1 | Out-String");
    }

    /// <summary>
    /// Clears the entire ARP cache by running <c>netsh interface ip delete arpcache</c>
    /// through the shared PowerShell runspace. Requires the application to run with
    /// elevated rights.
    /// </summary>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static async Task DeleteTableAsync()
    {
        await InvokeAsync("netsh interface ip delete arpcache 2>&1 | Out-String");
    }

    /// <summary>
    /// Runs <paramref name="script"/> on the shared runspace and throws when the
    /// command exits with a non-zero exit code or writes to the PowerShell error stream.
    /// </summary>
    /// <param name="script">The PowerShell script to execute.</param>
    private static async Task InvokeAsync(string script)
    {
        await Lock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript(script + @"
if ($LASTEXITCODE -ne 0) { Write-Error ""Exit code: $LASTEXITCODE"" }");
                var results = ps.Invoke();

                if (ps.Streams.Error.Count > 0)
                {
                    var output = string.Join(Environment.NewLine,
                        results.Select(r => r?.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)));
                    var errors = string.Join(Environment.NewLine, ps.Streams.Error);

                    var message = string.IsNullOrWhiteSpace(output)
                        ? errors
                        : $"{output.Trim()}{Environment.NewLine}{errors}";

                    Log.Warn($"PowerShell error: {message}");
                    throw new Exception(message);
                }
            });
        }
        finally
        {
            Lock.Release();
        }
    }

    /// <summary>
    /// Escapes a string for embedding inside a PowerShell single-quoted string by
    /// doubling any single-quote characters.
    /// </summary>
    /// <param name="value">The raw string value to escape.</param>
    private static string EscapePs(string value) => value.Replace("'", "''");

    #endregion
}