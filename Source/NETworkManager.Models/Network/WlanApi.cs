using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using log4net;

namespace NETworkManager.Models.Network;

/// <summary>
///     Native Wi-Fi (<c>wlanapi.dll</c>) interop used to determine the channel bandwidth
///     (20 / 40 / 80 / 160 MHz) of nearby access points.
///     <para>
///     The Windows Runtime Wi-Fi API (<see cref="Windows.Devices.WiFi.WiFiAvailableNetwork" />) does
///     not expose the channel width. The native BSS list (<c>WlanGetNetworkBssList</c>) however
///     returns the raw 802.11 information elements (IEs) of each beacon / probe response. The width
///     is parsed from the HT-Operation (802.11n), VHT-Operation (802.11ac) and HE-Operation
///     (802.11ax / 6 GHz) elements.
///     </para>
///     All calls are wrapped in try/catch; on any failure an empty result is returned so the caller
///     can fall back to a heuristic (see <see cref="WiFi" />).
/// </summary>
public static class WlanApi
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(WlanApi));

    private const uint WlanApiVersion2 = 0x00000002;
    private const uint ErrorSuccess = 0;

    // 802.11 information element identifiers.
    private const byte EidHtOperation = 61;
    private const byte EidVhtOperation = 192;
    private const byte EidExtension = 255;
    private const byte ExtEidHeOperation = 36;

    private enum DOT11_BSS_TYPE
    {
        // ReSharper disable UnusedMember.Local
        Infrastructure = 1,
        Independent = 2,
        Any = 3
        // ReSharper restore UnusedMember.Local
    }

    [DllImport("wlanapi.dll")]
    private static extern uint WlanOpenHandle(uint dwClientVersion, IntPtr pReserved,
        out uint pdwNegotiatedVersion, out IntPtr phClientHandle);

    [DllImport("wlanapi.dll")]
    private static extern uint WlanCloseHandle(IntPtr hClientHandle, IntPtr pReserved);

    [DllImport("wlanapi.dll")]
    private static extern void WlanFreeMemory(IntPtr pMemory);

    [DllImport("wlanapi.dll")]
    private static extern uint WlanGetNetworkBssList(IntPtr hClientHandle, ref Guid pInterfaceGuid,
        IntPtr pDot11Ssid, DOT11_BSS_TYPE dot11BssType, [MarshalAs(UnmanagedType.Bool)] bool bSecurityEnabled,
        IntPtr pReserved, out IntPtr ppWlanBssList);

    [DllImport("wlanapi.dll")]
    private static extern uint WlanEnumInterfaces(IntPtr hClientHandle, IntPtr pReserved,
        out IntPtr ppInterfaceList);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WLAN_INTERFACE_INFO
    {
        public Guid InterfaceGuid;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string strInterfaceDescription;

        public uint isState;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DOT11_SSID
    {
        public uint uSSIDLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] ucSSID;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WLAN_RATE_SET
    {
        public uint uRateSetLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 126)]
        public ushort[] usRateSet;
    }

    /// <summary>
    ///     Mirror of the native <c>WLAN_BSS_ENTRY</c> structure. Only <see cref="dot11Bssid" />,
    ///     <see cref="ulIeOffset" /> and <see cref="ulIeSize" /> are used; the remaining fields are
    ///     required so the layout (and therefore <see cref="Marshal.SizeOf{T}()" />) matches the
    ///     native struct exactly.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct WLAN_BSS_ENTRY
    {
        public DOT11_SSID dot11Ssid;
        public uint uPhyId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] dot11Bssid;

        public uint dot11BssType;
        public uint dot11BssPhyType;
        public int lRssi;
        public uint uLinkQuality;

        // Native type is BOOLEAN (1 byte), not BOOL (4 bytes).
        [MarshalAs(UnmanagedType.U1)]
        public bool bInRegDomain;

        public ushort usBeaconPeriod;
        public ulong ullTimestamp;
        public ulong ullHostTimestamp;
        public ushort usCapabilityInformation;
        public uint ulChCenterFrequency;
        public WLAN_RATE_SET wlanRateSet;
        public uint ulIeOffset;
        public uint ulIeSize;
    }

    /// <summary>
    ///     Returns a map of BSSID (lowercase, colon-separated, e.g. <c>aa:bb:cc:dd:ee:ff</c>) to the
    ///     channel bandwidth in MHz for all access points visible to the given Wi-Fi interface.
    ///     BSSIDs whose width could not be parsed are omitted from the result.
    /// </summary>
    /// <param name="interfaceGuid">GUID of the Wi-Fi interface (<c>NetworkAdapter.NetworkAdapterId</c>).</param>
    /// <returns>A (possibly empty) map of BSSID to channel bandwidth in MHz.</returns>
    public static Dictionary<string, int> GetBssChannelWidths(Guid interfaceGuid)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var clientHandle = IntPtr.Zero;

        try
        {
            var ret = WlanOpenHandle(WlanApiVersion2, IntPtr.Zero, out _, out clientHandle);

            if (ret != ErrorSuccess)
            {
                Log.Warn($"WlanOpenHandle failed with error code {ret}.");
                return result;
            }

            // The WinRT NetworkAdapterId normally matches the native WLAN interface GUID. If it does
            // not (driver/OS specifics), fall back to enumerating all WLAN interfaces and merging
            // their BSS lists (matching is done by BSSID, so mixing interfaces is harmless).
            if (!TryQueryBssList(clientHandle, interfaceGuid, result))
                foreach (var guid in EnumerateInterfaces(clientHandle))
                    TryQueryBssList(clientHandle, guid, result);
        }
        catch (Exception ex)
        {
            Log.Error("Error while reading the Wi-Fi BSS list via wlanapi.dll.", ex);
        }
        finally
        {
            if (clientHandle != IntPtr.Zero)
                WlanCloseHandle(clientHandle, IntPtr.Zero);
        }

        return result;
    }

    /// <summary>
    ///     Queries the BSS list for a single WLAN interface and adds the parsed channel widths to
    ///     <paramref name="result" />. Returns <see langword="true" /> if the query succeeded.
    /// </summary>
    private static bool TryQueryBssList(IntPtr clientHandle, Guid interfaceGuid, Dictionary<string, int> result)
    {
        var bssListPtr = IntPtr.Zero;

        try
        {
            var guid = interfaceGuid;

            var ret = WlanGetNetworkBssList(clientHandle, ref guid, IntPtr.Zero, DOT11_BSS_TYPE.Any, false,
                IntPtr.Zero, out bssListPtr);

            if (ret != ErrorSuccess || bssListPtr == IntPtr.Zero)
            {
                Log.Warn($"WlanGetNetworkBssList failed with error code {ret}.");
                return false;
            }

            // WLAN_BSS_LIST: DWORD dwTotalSize; DWORD dwNumberOfItems; WLAN_BSS_ENTRY[] entries.
            // The entry array starts after the two DWORDs (8-byte aligned).
            var totalSize = (long)(uint)Marshal.ReadInt32(bssListPtr, 0);
            var numberOfItems = Marshal.ReadInt32(bssListPtr, 4);
            var entrySize = Marshal.SizeOf<WLAN_BSS_ENTRY>();

            for (var i = 0; i < numberOfItems; i++)
            {
                var entryByteOffset = 8 + i * entrySize;
                var entry = Marshal.PtrToStructure<WLAN_BSS_ENTRY>(IntPtr.Add(bssListPtr, entryByteOffset));

                // Skip invalid entries and guard against reading outside the allocated buffer
                // (defensive: a struct layout mismatch would otherwise risk an invalid read).
                // The documented maximum IE data blob size is 2,324 bytes.
                if (entry.dot11Bssid is not { Length: 6 } || entry.ulIeSize is 0 or > 2324 || entry.ulIeOffset == 0)
                    continue;

                var ieStart = entryByteOffset + (long)entry.ulIeOffset;

                if (ieStart + entry.ulIeSize > totalSize)
                    continue;

                var ie = new byte[entry.ulIeSize];
                Marshal.Copy(IntPtr.Add(bssListPtr, (int)ieStart), ie, 0, (int)entry.ulIeSize);

                var width = GetBandwidthFromInformationElements(ie);

                if (width > 0)
                    result[FormatBssid(entry.dot11Bssid)] = width;
            }

            return true;
        }
        finally
        {
            if (bssListPtr != IntPtr.Zero)
                WlanFreeMemory(bssListPtr);
        }
    }

    /// <summary>
    ///     Returns the GUIDs of all WLAN interfaces known to the native Wi-Fi service.
    /// </summary>
    private static List<Guid> EnumerateInterfaces(IntPtr clientHandle)
    {
        var guids = new List<Guid>();

        var listPtr = IntPtr.Zero;

        try
        {
            if (WlanEnumInterfaces(clientHandle, IntPtr.Zero, out listPtr) != ErrorSuccess || listPtr == IntPtr.Zero)
                return guids;

            // WLAN_INTERFACE_INFO_LIST: DWORD dwNumberOfItems; DWORD dwIndex; WLAN_INTERFACE_INFO[] entries.
            var numberOfItems = Marshal.ReadInt32(listPtr, 0);
            var infoSize = Marshal.SizeOf<WLAN_INTERFACE_INFO>();
            var arrayPtr = IntPtr.Add(listPtr, 8);

            for (var i = 0; i < numberOfItems; i++)
            {
                var info = Marshal.PtrToStructure<WLAN_INTERFACE_INFO>(IntPtr.Add(arrayPtr, i * infoSize));
                guids.Add(info.InterfaceGuid);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error while enumerating Wi-Fi interfaces via wlanapi.dll.", ex);
        }
        finally
        {
            if (listPtr != IntPtr.Zero)
                WlanFreeMemory(listPtr);
        }

        return guids;
    }

    /// <summary>
    ///     Walks the TLV-encoded 802.11 information elements and returns the widest channel
    ///     bandwidth (MHz) advertised by the HT-, VHT- or HE-Operation element. Returns 0 if none
    ///     of these elements are present.
    /// </summary>
    private static int GetBandwidthFromInformationElements(byte[] ie)
    {
        int htWidth = 0, vhtWidth = 0, heWidth = 0;

        var pos = 0;

        while (pos + 2 <= ie.Length)
        {
            var id = ie[pos];
            int len = ie[pos + 1];
            var data = pos + 2;

            if (data + len > ie.Length)
                break;

            switch (id)
            {
                case EidHtOperation when len >= 2:
                {
                    // HT Operation Information byte 0: bits 0-1 secondary channel offset,
                    // bit 2 STA channel width (0 = 20 MHz, 1 = any/40 MHz).
                    var info = ie[data + 1];
                    var secondaryChannelOffset = info & 0x03;
                    var staChannelWidth = (info >> 2) & 0x01;
                    htWidth = staChannelWidth == 1 && secondaryChannelOffset != 0 ? 40 : 20;
                    break;
                }
                case EidVhtOperation when len >= 3:
                    vhtWidth = ParseVhtWidth(ie[data], ie[data + 1], ie[data + 2], htWidth);
                    break;
                case EidExtension when len >= 1 && ie[data] == ExtEidHeOperation:
                    heWidth = ParseHeWidth(ie, data + 1, len - 1, htWidth);
                    break;
            }

            pos = data + len;
        }

        return Math.Max(htWidth, Math.Max(vhtWidth, heWidth));
    }

    /// <summary>
    ///     Resolves the channel width from a VHT-Operation channel-width field plus its two center
    ///     frequency segments. Falls back to <paramref name="htWidth" /> when the VHT field signals
    ///     "20/40 MHz".
    /// </summary>
    private static int ParseVhtWidth(int channelWidth, int segment0, int segment1, int htWidth)
    {
        switch (channelWidth)
        {
            case 0: // 20/40 MHz -> defer to HT
                return htWidth > 0 ? htWidth : 20;
            case 1: // 80 MHz, unless the segments indicate 160 / 80+80 MHz
                if (segment1 == 0)
                    return 80;
                var diff = Math.Abs(segment0 - segment1);
                return diff is 8 or > 16 ? 160 : 80;
            case 2: // 160 MHz (deprecated encoding)
            case 3: // 80+80 MHz (deprecated encoding) - treated as 160 MHz for display
                return 160;
            default:
                return 80;
        }
    }

    /// <summary>
    ///     Parses the HE-Operation element (802.11ax). Optional sub-fields (VHT Operation
    ///     Information, 6 GHz Operation Information) are only present when the corresponding bit in
    ///     the HE Operation Parameters is set; their offsets therefore depend on the preceding
    ///     fields. This is a best-effort parse: on any ambiguity 0 is returned so the caller can
    ///     fall back to a heuristic.
    /// </summary>
    /// <param name="ie">The full information element buffer.</param>
    /// <param name="offset">Offset of the HE Operation Parameters (after the extension id byte).</param>
    /// <param name="length">Remaining length of the HE-Operation element after the extension id.</param>
    /// <param name="htWidth">Width parsed from the HT-Operation element (used as VHT fallback).</param>
    private static int ParseHeWidth(byte[] ie, int offset, int length, int htWidth)
    {
        // HE Operation Parameters (3) + BSS Color (1) + Basic HE-MCS And NSS Set (2) = 6 bytes minimum.
        if (length < 6)
            return 0;

        var end = offset + length;

        // HE Operation Parameters bit layout: B14 VHT Operation Information Present,
        // B15 Co-Hosted BSS, B16 ER SU Disable, B17 6 GHz Operation Information Present.
        var heOperationParameters = ie[offset] | (ie[offset + 1] << 8) | (ie[offset + 2] << 16);
        var vhtOperationInformationPresent = ((heOperationParameters >> 14) & 1) == 1;
        var coHostedBss = ((heOperationParameters >> 15) & 1) == 1;
        var sixGhzOperationInformationPresent = ((heOperationParameters >> 17) & 1) == 1;

        var cursor = offset + 3 + 1 + 2; // skip parameters, BSS color, basic HE-MCS
        var width = 0;

        if (vhtOperationInformationPresent)
        {
            if (cursor + 3 <= end)
                width = Math.Max(width, ParseVhtWidth(ie[cursor], ie[cursor + 1], ie[cursor + 2], htWidth));

            cursor += 3;
        }

        if (coHostedBss)
            cursor += 1;

        if (sixGhzOperationInformationPresent && cursor + 2 <= end)
        {
            // 6 GHz Operation Information: Primary Channel (1), Control (1), ...
            // Control byte bits 0-1 = channel width (0 = 20, 1 = 40, 2 = 80, 3 = 160/80+80 MHz).
            var channelWidth = ie[cursor + 1] & 0x03;
            var sixGhzWidth = channelWidth switch
            {
                0 => 20,
                1 => 40,
                2 => 80,
                3 => 160,
                _ => 0
            };
            width = Math.Max(width, sixGhzWidth);
        }

        return width;
    }

    /// <summary>
    ///     Formats a 6-byte BSSID into the lowercase, colon-separated representation used by the
    ///     Windows Runtime API (e.g. <c>aa:bb:cc:dd:ee:ff</c>).
    /// </summary>
    private static string FormatBssid(byte[] mac)
    {
        return $"{mac[0]:x2}:{mac[1]:x2}:{mac[2]:x2}:{mac[3]:x2}:{mac[4]:x2}:{mac[5]:x2}";
    }
}
