using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using log4net;

namespace NETworkManager.Models.Network;

/// <summary>
///     Native Wi-Fi (<c>wlanapi.dll</c>) interop used to determine the channel bandwidth
///     (20 / 40 / 80 / 160 / 320 MHz) and its center frequency of nearby access points.
///     <para>
///     The Windows Runtime Wi-Fi API (<see cref="Windows.Devices.WiFi.WiFiAvailableNetwork" />) does
///     not expose the channel width, its frequency is the one of the primary 20 MHz channel (where
///     the beacons are sent). The native BSS list (<c>WlanGetNetworkBssList</c>) however returns the
///     raw 802.11 information elements (IEs) of each beacon / probe response. The width and the
///     center of the whole channel are parsed from the HT-Operation (802.11n), VHT-Operation
///     (802.11ac), HE-Operation (802.11ax / 6 GHz) and EHT-Operation (802.11be) elements.
///     </para>
///     All calls are wrapped in try/catch; on any failure an empty result is returned so the caller
///     can fall back to a heuristic (see <see cref="WiFi" />).
/// </summary>
public static class WlanApi
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(WlanApi));

    private const uint WlanApiVersion2 = 0x00000002;
    private const uint ErrorSuccess = 0;

    // WLAN_INTF_OPCODE wlan_intf_opcode_current_connection: WLAN_CONNECTION_ATTRIBUTES of the interface.
    private const uint WlanIntfOpcodeCurrentConnection = 7;

    // Offset of WLAN_ASSOCIATION_ATTRIBUTES.dot11Bssid in WLAN_CONNECTION_ATTRIBUTES: isState (4), wlanConnectionMode
    // (4), strProfileName (WCHAR[256] = 512), dot11Ssid (4 + 32), dot11BssType (4).
    private const int ConnectionAttributesBssidOffset = 4 + 4 + 512 + 36 + 4;

    // 802.11 information element identifiers.
    private const byte EidHtOperation = 61;
    private const byte EidVhtOperation = 192;
    private const byte EidExtension = 255;
    private const byte ExtEidHeOperation = 36;
    private const byte ExtEidEhtOperation = 106;

    /// <summary>
    ///     Channel of an access point parsed from its information elements.
    /// </summary>
    /// <param name="BandwidthMHz">Channel width in MHz (20, 40, 80, 160, 320), 0 if unknown.</param>
    /// <param name="CenterFrequencyMHz">
    ///     Center frequency of the whole channel in MHz (e.g. 5210 for 80 MHz on the channels 36 - 48), 0 if unknown.
    /// </param>
    public readonly record struct WlanBssChannel(int BandwidthMHz, int CenterFrequencyMHz);

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
    private static extern uint WlanQueryInterface(IntPtr hClientHandle, ref Guid pInterfaceGuid, uint opCode,
        IntPtr pReserved, out uint pdwDataSize, out IntPtr ppData, IntPtr pWlanOpcodeValueType);

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
    ///     Reads the BSSID of the connected access point and the channels (width and center) of all access points
    ///     visible to the given Wi-Fi interface, with one client handle.
    /// </summary>
    /// <param name="interfaceGuid">GUID of the Wi-Fi interface (<c>NetworkAdapter.NetworkAdapterId</c>).</param>
    /// <returns>
    ///     The BSSID of the connected access point (lowercase, colon-separated, e.g. <c>aa:bb:cc:dd:ee:ff</c>; null if
    ///     not connected or not read) and a (possibly empty) map of BSSID to its channel. BSSIDs whose width could not
    ///     be parsed are omitted.
    /// </returns>
    public static (string ConnectedBssid, Dictionary<string, WlanBssChannel> Channels) GetBssInfo(Guid interfaceGuid)
    {
        var channels = new Dictionary<string, WlanBssChannel>(StringComparer.OrdinalIgnoreCase);
        string connectedBssid = null;

        var clientHandle = IntPtr.Zero;

        try
        {
            var ret = WlanOpenHandle(WlanApiVersion2, IntPtr.Zero, out _, out clientHandle);

            if (ret != ErrorSuccess)
            {
                Log.Warn($"WlanOpenHandle failed with error code {ret}.");
                return (null, channels);
            }

            connectedBssid = GetConnectedBssid(clientHandle, interfaceGuid);

            // The WinRT NetworkAdapterId normally matches the native WLAN interface GUID. If it does
            // not (driver/OS specifics), fall back to enumerating all WLAN interfaces and merging
            // their BSS lists (matching is done by BSSID, so mixing interfaces is harmless).
            if (!TryQueryBssList(clientHandle, interfaceGuid, channels))
                foreach (var guid in EnumerateInterfaces(clientHandle))
                    TryQueryBssList(clientHandle, guid, channels);
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

        return (connectedBssid, channels);
    }

    /// <summary>
    ///     Returns the BSSID of the access point the given Wi-Fi interface is connected to.
    /// </summary>
    /// <param name="clientHandle">Open client handle.</param>
    /// <param name="interfaceGuid">GUID of the Wi-Fi interface.</param>
    /// <returns>The BSSID, or null if the interface is not connected or it could not be read.</returns>
    private static string GetConnectedBssid(IntPtr clientHandle, Guid interfaceGuid)
    {
        var dataPtr = IntPtr.Zero;

        try
        {
            var guid = interfaceGuid;

            // Fails with ERROR_INVALID_STATE (5023) if the interface is not connected
            var ret = WlanQueryInterface(clientHandle, ref guid, WlanIntfOpcodeCurrentConnection, IntPtr.Zero,
                out var dataSize, out dataPtr, IntPtr.Zero);

            if (ret != ErrorSuccess || dataPtr == IntPtr.Zero || dataSize < ConnectionAttributesBssidOffset + 6)
                return null;

            var bssid = new byte[6];
            Marshal.Copy(IntPtr.Add(dataPtr, ConnectionAttributesBssidOffset), bssid, 0, 6);

            return FormatBssid(bssid);
        }
        catch (Exception ex)
        {
            // Optional (marks the connected network): the BSS list is still read
            Log.Error("Error while reading the connected Wi-Fi network via wlanapi.dll.", ex);

            return null;
        }
        finally
        {
            if (dataPtr != IntPtr.Zero)
                WlanFreeMemory(dataPtr);
        }
    }

    /// <summary>
    ///     Queries the BSS list for a single WLAN interface and adds the parsed channels to
    ///     <paramref name="result" />. Returns <see langword="true" /> if the query succeeded.
    /// </summary>
    private static bool TryQueryBssList(IntPtr clientHandle, Guid interfaceGuid,
        Dictionary<string, WlanBssChannel> result)
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

                var ieSize = (int)entry.ulIeSize; // safe: guarded to <= 2324 above

                var ie = new byte[ieSize];
                Marshal.Copy(IntPtr.Add(bssListPtr, (int)ieStart), ie, 0, ieSize);

                // Frequency of the primary channel (kHz)
                var channel = GetChannelFromInformationElements(ie, (int)(entry.ulChCenterFrequency / 1000));

                if (channel.BandwidthMHz > 0)
                    result[FormatBssid(entry.dot11Bssid)] = channel;
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
    ///     Walks the TLV-encoded 802.11 information elements and returns the widest channel advertised
    ///     by the HT-, VHT-, HE- or EHT-Operation element, with the center frequency of the whole channel.
    ///     Returns <c>default</c> (width 0) if none of these elements are present.
    /// </summary>
    /// <param name="ie">Information elements of a beacon / probe response.</param>
    /// <param name="primaryFrequencyMHz">Frequency of the primary 20 MHz channel in MHz (e.g. 5180).</param>
    public static WlanBssChannel GetChannelFromInformationElements(byte[] ie, int primaryFrequencyMHz)
    {
        WlanBssChannel ht = default;
        byte[] vhtOperation = null;
        int heOffset = -1, heLength = 0;
        int ehtOffset = -1, ehtLength = 0;

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
                    ht = ParseHtChannel(ie[data + 1], primaryFrequencyMHz);
                    break;
                case EidVhtOperation when len >= 3:
                    vhtOperation = [ie[data], ie[data + 1], ie[data + 2]];
                    break;
                case EidExtension when len >= 1 && ie[data] == ExtEidHeOperation:
                    heOffset = data + 1;
                    heLength = len - 1;
                    break;
                case EidExtension when len >= 1 && ie[data] == ExtEidEhtOperation:
                    ehtOffset = data + 1;
                    ehtLength = len - 1;
                    break;
            }

            pos = data + len;
        }

        // VHT and HE defer to HT for 20 / 40 MHz, so they are parsed after the loop (independent of the order)
        var vht = vhtOperation == null
            ? default
            : ParseVhtChannel(vhtOperation[0], vhtOperation[1], vhtOperation[2], ht, primaryFrequencyMHz);

        var he = heOffset < 0 ? default : ParseHeChannel(ie, heOffset, heLength, ht, primaryFrequencyMHz);

        var eht = ehtOffset < 0 ? default : ParseEhtChannel(ie, ehtOffset, ehtLength, primaryFrequencyMHz);

        // The widest channel; on a tie the newer element (its center is the more precise one)
        return Widest(Widest(Widest(ht, vht), he), eht);
    }

    /// <summary>
    ///     Returns <paramref name="candidate" /> if it is at least as wide as <paramref name="current" />.
    /// </summary>
    private static WlanBssChannel Widest(WlanBssChannel current, WlanBssChannel candidate)
    {
        return candidate.BandwidthMHz > 0 && candidate.BandwidthMHz >= current.BandwidthMHz ? candidate : current;
    }

    /// <summary>
    ///     Parses the HT Operation Information byte 0: bits 0-1 secondary channel offset (1 = above,
    ///     3 = below), bit 2 STA channel width (0 = 20 MHz, 1 = any / 40 MHz).
    /// </summary>
    private static WlanBssChannel ParseHtChannel(byte info, int primaryFrequencyMHz)
    {
        var secondaryChannelOffset = info & 0x03;
        var staChannelWidth = (info >> 2) & 0x01;

        if (staChannelWidth == 1 && secondaryChannelOffset == 1)
            return new WlanBssChannel(40, primaryFrequencyMHz + 10);

        if (staChannelWidth == 1 && secondaryChannelOffset == 3)
            return new WlanBssChannel(40, primaryFrequencyMHz - 10);

        return new WlanBssChannel(20, primaryFrequencyMHz);
    }

    /// <summary>
    ///     Resolves the channel from a VHT-Operation channel-width field plus its two channel center
    ///     frequency segments (CCFS, channel numbers). Falls back to <paramref name="ht" /> when the
    ///     VHT field signals "20/40 MHz".
    /// </summary>
    private static WlanBssChannel ParseVhtChannel(int channelWidth, int segment0, int segment1, WlanBssChannel ht,
        int primaryFrequencyMHz)
    {
        switch (channelWidth)
        {
            case 0: // 20/40 MHz -> defer to HT
                return ht.BandwidthMHz > 0 ? ht : new WlanBssChannel(20, primaryFrequencyMHz);
            case 1: // 80 MHz, unless the segments indicate 160 / 80+80 MHz
                if (segment1 == 0)
                    return new WlanBssChannel(80, ChannelToFrequency(segment0, primaryFrequencyMHz));

                var diff = Math.Abs(segment0 - segment1);

                // 160 MHz: segment 1 is the center of the 160 MHz channel
                if (diff == 8)
                    return new WlanBssChannel(160, ChannelToFrequency(segment1, primaryFrequencyMHz));

                // 80+80 MHz (two separate 80 MHz segments) is shown as 160 MHz around the primary segment
                return diff > 16
                    ? new WlanBssChannel(160, ChannelToFrequency(segment0, primaryFrequencyMHz))
                    : new WlanBssChannel(80, ChannelToFrequency(segment0, primaryFrequencyMHz));
            case 2: // 160 MHz (deprecated encoding): segment 0 is the center of the 160 MHz channel
            case 3: // 80+80 MHz (deprecated encoding) - treated as 160 MHz for display
                return new WlanBssChannel(160, ChannelToFrequency(segment0, primaryFrequencyMHz));
            default:
                return new WlanBssChannel(80, ChannelToFrequency(segment0, primaryFrequencyMHz));
        }
    }

    /// <summary>
    ///     Parses the HE-Operation element (802.11ax). Optional sub-fields (VHT Operation
    ///     Information, 6 GHz Operation Information) are only present when the corresponding bit in
    ///     the HE Operation Parameters is set; their offsets therefore depend on the preceding
    ///     fields. This is a best-effort parse: on any ambiguity <c>default</c> (width 0) is returned so
    ///     the caller can fall back to a heuristic.
    /// </summary>
    /// <param name="ie">The full information element buffer.</param>
    /// <param name="offset">Offset of the HE Operation Parameters (after the extension id byte).</param>
    /// <param name="length">Remaining length of the HE-Operation element after the extension id.</param>
    /// <param name="ht">Channel parsed from the HT-Operation element (used as VHT fallback).</param>
    /// <param name="primaryFrequencyMHz">Frequency of the primary 20 MHz channel in MHz.</param>
    private static WlanBssChannel ParseHeChannel(byte[] ie, int offset, int length, WlanBssChannel ht,
        int primaryFrequencyMHz)
    {
        // HE Operation Parameters (3) + BSS Color (1) + Basic HE-MCS And NSS Set (2) = 6 bytes minimum.
        if (length < 6)
            return default;

        var end = offset + length;

        // HE Operation Parameters bit layout: B14 VHT Operation Information Present,
        // B15 Co-Hosted BSS, B16 ER SU Disable, B17 6 GHz Operation Information Present.
        var heOperationParameters = ie[offset] | (ie[offset + 1] << 8) | (ie[offset + 2] << 16);
        var vhtOperationInformationPresent = ((heOperationParameters >> 14) & 1) == 1;
        var coHostedBss = ((heOperationParameters >> 15) & 1) == 1;
        var sixGhzOperationInformationPresent = ((heOperationParameters >> 17) & 1) == 1;

        var cursor = offset + 3 + 1 + 2; // skip parameters, BSS color, basic HE-MCS
        WlanBssChannel channel = default;

        if (vhtOperationInformationPresent)
        {
            if (cursor + 3 <= end)
                channel = ParseVhtChannel(ie[cursor], ie[cursor + 1], ie[cursor + 2], ht, primaryFrequencyMHz);

            cursor += 3;
        }

        if (coHostedBss)
            cursor += 1;

        if (sixGhzOperationInformationPresent && cursor + 2 <= end)
        {
            // 6 GHz Operation Information: Primary Channel (1), Control (1), CCFS0 (1), CCFS1 (1), ...
            // Control byte bits 0-1 = channel width (0 = 20, 1 = 40, 2 = 80, 3 = 160/80+80 MHz).
            var sixGhzWidth = (ie[cursor + 1] & 0x03) switch
            {
                0 => 20,
                1 => 40,
                2 => 80,
                _ => 160
            };

            // CCFS0 is the center of the channel (of the primary 80 MHz channel for 160 MHz), CCFS1 the center of
            // the 160 MHz channel (0 for narrower channels). For 80+80 MHz (segments more than 8 channels apart),
            // CCFS1 is the center of the second segment: shown as 160 MHz around the primary segment, like VHT.
            var segment0 = cursor + 3 <= end ? ie[cursor + 2] : 0;
            var segment1 = cursor + 4 <= end ? ie[cursor + 3] : 0;
            var centerChannel = sixGhzWidth == 160 && segment1 != 0 && Math.Abs(segment1 - segment0) == 8
                ? segment1
                : segment0;

            channel = Widest(channel,
                new WlanBssChannel(sixGhzWidth, ChannelToFrequency(centerChannel, primaryFrequencyMHz)));
        }

        return channel;
    }

    /// <summary>
    ///     Parses the EHT-Operation element (802.11be): EHT Operation Parameters (1, bit 0 = EHT
    ///     Operation Information present) and Basic EHT-MCS And NSS Set (4), followed by the EHT
    ///     Operation Information: Control (1, bits 0-2 = 20 / 40 / 80 / 160 / 320 MHz), CCFS0 (1),
    ///     CCFS1 (1). Only present if the channel differs from the HT / VHT / HE elements (e.g. 320 MHz).
    /// </summary>
    /// <param name="ie">The full information element buffer.</param>
    /// <param name="offset">Offset of the EHT Operation Parameters (after the extension id byte).</param>
    /// <param name="length">Remaining length of the EHT-Operation element after the extension id.</param>
    /// <param name="primaryFrequencyMHz">Frequency of the primary 20 MHz channel in MHz.</param>
    private static WlanBssChannel ParseEhtChannel(byte[] ie, int offset, int length, int primaryFrequencyMHz)
    {
        if (length < 8 || (ie[offset] & 0x01) == 0)
            return default;

        var cursor = offset + 1 + 4; // skip parameters, basic EHT-MCS

        var width = (ie[cursor] & 0x07) switch
        {
            0 => 20,
            1 => 40,
            2 => 80,
            3 => 160,
            4 => 320,
            _ => 0
        };

        if (width == 0)
            return default;

        // CCFS0 is the center of the channel (of the primary 80 / 160 MHz channel for 160 / 320 MHz), CCFS1 the
        // center of the 160 / 320 MHz channel
        var segment0 = ie[cursor + 1];
        var segment1 = ie[cursor + 2];
        var centerChannel = width >= 160 && segment1 != 0 ? segment1 : segment0;

        return new WlanBssChannel(width, ChannelToFrequency(centerChannel, primaryFrequencyMHz));
    }

    /// <summary>
    ///     Converts a channel number of the band of the primary channel to its frequency in MHz.
    /// </summary>
    /// <param name="channel">Channel number (e.g. 42), 0 if not set.</param>
    /// <param name="primaryFrequencyMHz">Frequency of the primary channel, used for the band and as fallback.</param>
    private static int ChannelToFrequency(int channel, int primaryFrequencyMHz)
    {
        if (channel == 0)
            return primaryFrequencyMHz;

        return primaryFrequencyMHz switch
        {
            >= 5925 => channel == 2 ? 5935 : 5950 + channel * 5, // 6 GHz (channel 2 is below channel 1)
            >= 4900 => 5000 + channel * 5, // 5 GHz
            _ => channel == 14 ? 2484 : 2407 + channel * 5 // 2.4 GHz
        };
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
