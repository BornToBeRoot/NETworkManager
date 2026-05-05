using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using NETworkManager.Utilities;
using SMA = System.Management.Automation;
using log4net;

namespace NETworkManager.Models.Network;

/// <summary>
/// Provides static methods to read and modify the Windows IP neighbor table
/// (IPv4 ARP and IPv6 NDP). Read access uses the <c>GetIpNetTable2</c> Win32 API.
/// Modifying operations (add/delete entries, clear table) run via PowerShell in a
/// shared <see cref="Runspace"/> that is lazily initialized on first use with the
/// required execution policy and the <c>NetTCPIP</c> module imported. A
/// <see cref="SemaphoreSlim"/> serializes access so the runspace is never used
/// concurrently. Modifying operations require the application to run with elevated rights.
/// </summary>
public class NeighborTable
{
    #region Variables

    /// <summary>
    /// The logger for this class.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(NeighborTable));

    // Address family constants for SOCKADDR_INET / GetIpNetTable2.
    private const ushort AF_UNSPEC = 0;
    private const ushort AF_INET = 2;
    private const ushort AF_INET6 = 23;

    // Maximum length of a physical address inside MIB_IPNET_ROW2.
    private const int IF_MAX_PHYS_ADDRESS_LENGTH = 32;

    // Size of SOCKADDR_INET (matches sizeof(SOCKADDR_IN6) = 28).
    private const int SOCKADDR_INET_SIZE = 28;

    /// <summary>
    /// Mirror of the native <c>MIB_IPNET_ROW2</c> structure. The first 28 bytes hold
    /// a <c>SOCKADDR_INET</c> union; we read the address family from the first two
    /// bytes and parse the IPv4 / IPv6 address from the union accordingly.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    private struct MIB_IPNET_ROW2
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SOCKADDR_INET_SIZE)]
        public byte[] Address;

        public uint InterfaceIndex;
        public ulong InterfaceLuid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IF_MAX_PHYS_ADDRESS_LENGTH)]
        public byte[] PhysicalAddress;

        public uint PhysicalAddressLength;
        public uint State;
        public uint Flags;
        public uint ReachabilityTime;
    }

    /// <summary>Retrieves all entries from the IPv4/IPv6 neighbor cache. Returns 0 on success.</summary>
    [DllImport("Iphlpapi.dll")]
    private static extern uint GetIpNetTable2(ushort family, out IntPtr table);

    /// <summary>Frees a MIB table buffer allocated by <c>GetIpNetTable2</c>.</summary>
    [DllImport("Iphlpapi.dll")]
    private static extern void FreeMibTable(IntPtr memory);

    /// <summary>
    /// Ensures that only one PowerShell pipeline runs on <see cref="SharedRunspace"/> at a time.
    /// </summary>
    private static readonly SemaphoreSlim RunspaceLock = new(1, 1);

    /// <summary>Protects reads and writes to <see cref="_interfaceAliasCache"/> and <see cref="_interfaceAliasCacheExpiry"/>.</summary>
    private static readonly Lock InterfaceAliasCacheLock = new();

    /// <summary>Cached result of <see cref="BuildInterfaceAliasMap"/>; <see langword="null"/> until first use.</summary>
    private static Dictionary<int, string> _interfaceAliasCache;

    /// <summary>UTC time after which <see cref="_interfaceAliasCache"/> must be rebuilt.</summary>
    private static DateTime _interfaceAliasCacheExpiry = DateTime.MinValue;

    /// <summary>How long the interface alias cache is considered fresh before being rebuilt (5 minutes).</summary>
    private static readonly TimeSpan InterfaceAliasCacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Lazily initialized PowerShell runspace. Created and configured on first access so that
    /// read-only paths (e.g. MAC address lookup in IP Scanner) do not start a PowerShell
    /// process unless a modifying operation is actually performed.
    /// </summary>
    private static readonly Lazy<Runspace> _sharedRunspace = new(() =>
    {
        var runspace = RunspaceFactory.CreateRunspace();
        runspace.Open();

        using var ps = SMA.PowerShell.Create();
        ps.Runspace = runspace;
        ps.AddScript(@"
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
Import-Module NetTCPIP -ErrorAction Stop").Invoke();

        return runspace;
    });

    /// <summary>Returns the shared runspace, initializing it on first access.</summary>
    private static Runspace SharedRunspace => _sharedRunspace.Value;

    #endregion

    #region Methods

    /// <summary>
    /// Returns all entries from the IPv4 and IPv6 neighbor cache asynchronously.
    /// </summary>
    public static Task<List<NeighborInfo>> GetTableAsync()
    {
        return Task.Run(GetTable);
    }

    /// <summary>
    /// Returns a list of available network interfaces as index/name pairs, sorted by name.
    /// Uses the cached alias map; see <see cref="GetCachedInterfaceAliasMap"/>.
    /// </summary>
    public static Task<List<KeyValuePair<int, string>>> GetInterfacesAsync()
    {
        return Task.Run(() => GetCachedInterfaceAliasMap()
            .OrderBy(kv => kv.Value)
            .ToList());
    }

    /// <summary>
    /// Reads the full neighbor table via <c>GetIpNetTable2</c> and returns it as a list
    /// of <see cref="NeighborInfo"/> objects. IPv4 entries with a virtual or broadcast MAC
    /// are suppressed.
    /// </summary>
    private static List<NeighborInfo> GetTable()
    {
        var list = new List<NeighborInfo>();

        var virtualMAC = new PhysicalAddress([0, 0, 0, 0, 0, 0]);
        var broadcastMAC = new PhysicalAddress([255, 255, 255, 255, 255, 255]);

        var aliasMap = GetCachedInterfaceAliasMap();

        var table = IntPtr.Zero;

        try
        {
            var result = GetIpNetTable2(AF_UNSPEC, out table);

            if (result != 0)
                throw new Win32Exception((int)result);

            // First 4 bytes hold NumEntries; the array of MIB_IPNET_ROW2 starts after
            // 4 bytes of padding (the row contains a ULONG64, requiring 8-byte alignment).
            var numEntries = Marshal.ReadInt32(table);
            var rowSize = Marshal.SizeOf<MIB_IPNET_ROW2>();
            var arrayPtr = IntPtr.Add(table, 8);

            for (var i = 0; i < numEntries; i++)
            {
                var row = Marshal.PtrToStructure<MIB_IPNET_ROW2>(IntPtr.Add(arrayPtr, i * rowSize));

                var family = BitConverter.ToUInt16(row.Address, 0);

                IPAddress ipAddress;
                AddressFamily addressFamily;

                switch (family)
                {
                    case AF_INET:
                    {
                        // SOCKADDR_IN: family(2)+port(2)+addr(4)+zero(8) — addr at offset 4
                        var addrBytes = new byte[4];
                        Buffer.BlockCopy(row.Address, 4, addrBytes, 0, 4);
                        ipAddress = new IPAddress(addrBytes);
                        addressFamily = AddressFamily.InterNetwork;
                        break;
                    }
                    case AF_INET6:
                    {
                        // SOCKADDR_IN6: family(2)+port(2)+flowinfo(4)+addr(16)+scope_id(4) — addr at offset 8
                        var addrBytes = new byte[16];
                        Buffer.BlockCopy(row.Address, 8, addrBytes, 0, 16);
                        var scopeId = BitConverter.ToUInt32(row.Address, 24);
                        ipAddress = new IPAddress(addrBytes, scopeId);
                        addressFamily = AddressFamily.InterNetworkV6;
                        break;
                    }
                    default:
                        continue;
                }

                var macLen = (int)row.PhysicalAddressLength;

                if (macLen is < 0 or > IF_MAX_PHYS_ADDRESS_LENGTH)
                    macLen = 0;

                var macBytes = new byte[macLen];

                if (macLen > 0)
                    Buffer.BlockCopy(row.PhysicalAddress, 0, macBytes, 0, macLen);

                var macAddress = new PhysicalAddress(macBytes);

                // Suppress virtual/broadcast MAC for IPv4 to match legacy behavior.
                if (addressFamily == AddressFamily.InterNetwork &&
                    (macAddress.Equals(virtualMAC) || macAddress.Equals(broadcastMAC)))
                    continue;

                aliasMap.TryGetValue((int)row.InterfaceIndex, out var alias);

                list.Add(new NeighborInfo(
                    ipAddress,
                    macAddress,
                    addressFamily == AddressFamily.InterNetworkV6
                        ? ipAddress.IsIPv6Multicast
                        : IPv4Address.IsMulticast(ipAddress),
                    (int)row.InterfaceIndex,
                    alias ?? string.Empty,
                    (NeighborState)row.State,
                    addressFamily));
            }

            return list;
        }
        finally
        {
            if (table != IntPtr.Zero)
                FreeMibTable(table);
        }
    }

    /// <summary>
    /// Returns the interface alias map, rebuilding it via <see cref="BuildInterfaceAliasMap"/>
    /// when the cache has expired or has not yet been populated.
    /// </summary>
    private static Dictionary<int, string> GetCachedInterfaceAliasMap()
    {
        lock (InterfaceAliasCacheLock)
        {
            if (DateTime.UtcNow < _interfaceAliasCacheExpiry && _interfaceAliasCache != null)
                return _interfaceAliasCache;

            _interfaceAliasCache = BuildInterfaceAliasMap();
            _interfaceAliasCacheExpiry = DateTime.UtcNow.Add(InterfaceAliasCacheDuration);
            return _interfaceAliasCache;
        }
    }

    /// <summary>
    /// Builds a dictionary that maps an interface index (IPv4 or IPv6) to the
    /// human-readable interface name (e.g. <c>"Ethernet"</c>, <c>"Wi-Fi"</c>).
    /// </summary>
    private static Dictionary<int, string> BuildInterfaceAliasMap()
    {
        var map = new Dictionary<int, string>();

        foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            try
            {
                var props = ni.GetIPProperties();

                if (ni.Supports(NetworkInterfaceComponent.IPv4))
                    map[props.GetIPv4Properties().Index] = ni.Name;

                if (ni.Supports(NetworkInterfaceComponent.IPv6))
                    map[props.GetIPv6Properties().Index] = ni.Name;
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to read interface properties for '{ni.Name}': {ex.Message}");
            }
        }

        return map;
    }

    /// <summary>
    /// Returns the MAC address for <paramref name="ipAddress"/> by scanning the neighbor
    /// cache, or <see langword="null"/> when no entry exists. Supports both IPv4 and IPv6.
    /// </summary>
    public static string GetMACAddress(IPAddress ipAddress)
    {
        var entry = GetTable().FirstOrDefault(x => x.IPAddress.Equals(ipAddress));
        return entry?.MACAddress.ToString();
    }

    /// <summary>
    /// Adds a permanent neighbor entry for <paramref name="ipAddress"/> with the given
    /// <paramref name="macAddress"/> by running <c>New-NetNeighbor</c> through the shared
    /// PowerShell runspace. Requires the application to run with elevated rights.
    /// </summary>
    /// <param name="ipAddress">The IP address (IPv4 or IPv6) of the entry.</param>
    /// <param name="macAddress">The link-layer address of the entry, separated with <c>-</c>.</param>
    /// <param name="interfaceIndex">The index of the network interface to add the entry on.</param>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static async Task AddEntryAsync(string ipAddress, string macAddress, int interfaceIndex)
    {
        await RunspaceLock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript($@"New-NetNeighbor -InterfaceIndex {interfaceIndex} -IPAddress '{PowerShellHelper.EscapeSingleQuotes(ipAddress)}' -LinkLayerAddress '{PowerShellHelper.EscapeSingleQuotes(macAddress)}' -State Permanent -ErrorAction Stop | Out-Null");
                ps.Invoke();

                ThrowOnError(ps);
            });
        }
        finally
        {
            RunspaceLock.Release();
        }
    }

    /// <summary>
    /// Removes the neighbor entry for <paramref name="ipAddress"/> by running
    /// <c>Remove-NetNeighbor</c> through the shared PowerShell runspace. Requires the
    /// application to run with elevated rights.
    /// </summary>
    /// <param name="ipAddress">The IP address of the entry to remove.</param>
    /// <param name="interfaceIndex">The index of the network interface the entry belongs to.</param>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static async Task DeleteEntryAsync(string ipAddress, int interfaceIndex)
    {
        await RunspaceLock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript($@"Remove-NetNeighbor -InterfaceIndex {interfaceIndex} -IPAddress '{PowerShellHelper.EscapeSingleQuotes(ipAddress)}' -Confirm:$false -ErrorAction Stop | Out-Null");
                ps.Invoke();

                ThrowOnError(ps);
            });
        }
        finally
        {
            RunspaceLock.Release();
        }
    }
    
    /// <summary>
    /// Clears all dynamic neighbor entries (IPv4 + IPv6) by piping
    /// <c>Get-NetNeighbor</c> into <c>Remove-NetNeighbor</c>, excluding entries whose
    /// state is <c>Permanent</c>. Requires the application to run with elevated rights.
    /// </summary>
    /// <exception cref="Exception">
    /// Thrown when the PowerShell pipeline reports one or more errors.
    /// </exception>
    public static async Task DeleteTableAsync()
    {
        await RunspaceLock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var ps = SMA.PowerShell.Create();
                ps.Runspace = SharedRunspace;

                ps.AddScript(@"Get-NetNeighbor -ErrorAction SilentlyContinue | Where-Object { $_.State -ne 'Permanent' } | Remove-NetNeighbor -Confirm:$false -ErrorAction Stop | Out-Null");
                ps.Invoke();

                ThrowOnError(ps);
            });
        }
        finally
        {
            RunspaceLock.Release();
        }
    }
    
    /// <summary>
    /// Throws an <see cref="Exception"/> whose message is the joined PowerShell error
    /// stream when <paramref name="ps"/> reported one or more errors.
    /// </summary>
    private static void ThrowOnError(SMA.PowerShell ps)
    {
        if (ps.Streams.Error.Count == 0)
            return;

        var message = string.Join(Environment.NewLine, ps.Streams.Error);

        Log.Warn($"PowerShell error: {message}");

        throw new Exception(message);
    }

    #endregion
}
