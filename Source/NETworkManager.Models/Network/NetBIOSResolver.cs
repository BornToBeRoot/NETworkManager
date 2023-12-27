using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NETworkManager.Models.Lookup;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     NetBIOS resolver written in C# based on the following sources:
///     https://web.archive.org/web/20100409111218/http://msdn.microsoft.com/en-us/library/system.net.sockets.socket.aspx
///     https://github.com/angryip/ipscan (GPLv2)
/// </summary>
public static class NetBIOSResolver
{
    private const int NetBIOSUdpPort = 137;

    private const int ResponseTypePos = 47;
    private const byte ResponseTypeNbstat = 33;

    private const int ResponseBaseLen = 57;
    private const int ResponseNameLen = 15;
    private const int ResponseBlockLen = 18;

    private const int GroupNameFlag = 128;

    private const int NameTypeDomain = 0x00;
    private const int NameTypeMessenger = 0x03;

    private static readonly byte[] RequestData =
    {
        0x80, 0x94, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x20, 0x43, 0x4b, 0x41,
        0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
        0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
        0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
        0x41, 0x41, 0x41, 0x41, 0x41, 0x00, 0x00, 0x21,
        0x00, 0x01
    };

    public static async Task<NetBIOSInfo> ResolveAsync(IPAddress ipAddress, int timeout,
        CancellationToken cancellationToken)
    {
        var udpClient = new UdpClient();
        udpClient.Client.ReceiveTimeout = timeout;

        var remoteEndPoint = new IPEndPoint(ipAddress, NetBIOSUdpPort);

        try
        {
            await udpClient.SendAsync(RequestData, RequestData.Length, remoteEndPoint);

            // ReSharper disable once MethodSupportsCancellation - cancellation is handled below by Task.WhenAny
            var receiveTask = udpClient.ReceiveAsync();

            if (!receiveTask.Wait(timeout, cancellationToken))
                return new NetBIOSInfo();

            var response = receiveTask.Result;

            if (response.Buffer.Length < ResponseBaseLen || response.Buffer[ResponseTypePos] != ResponseTypeNbstat)
                return new NetBIOSInfo(); // response was too short

            var count = response.Buffer[ResponseBaseLen - 1] & 0xFF;

            if (response.Buffer.Length < ResponseBaseLen + ResponseBlockLen * count)
                return new NetBIOSInfo(); // data was truncated or something is wrong

            var result = ExtractNames(response.Buffer, count);

            var vendor = string.Empty;

            // ReSharper disable once InvertIf - readability
            if (!string.IsNullOrEmpty(result.MacAddress))
            {
                // ReSharper disable once MethodHasAsyncOverload - Parent method is async
                var info = OUILookup.LookupByMacAddress(result.MacAddress).FirstOrDefault();

                if (info != null)
                    vendor = info.Vendor;
            }

            return new NetBIOSInfo(
                ipAddress,
                result.ComputerName,
                result.UserName,
                result.GroupName,
                MACAddressHelper.GetDefaultFormat(result.MacAddress),
                vendor
            );
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            udpClient.Close();
        }
    }

    private static (string ComputerName, string UserName, string GroupName, string MacAddress) ExtractNames(
        byte[] response, int count)
    {
        // Computer name
        var computerName = count > 0 ? Decode(response, 0) : string.Empty;

        // Group or domain name
        var groupName = string.Empty;

        for (var i = 1; i < count; i++)
        {
            if (GetType(response, i) != NameTypeDomain || (GetFlag(response, i) & GroupNameFlag) <= 0)
                continue;

            groupName = Decode(response, i);

            break;
        }

        // User name
        var userName = string.Empty;

        for (var i = count - 1; i > 0; i--)
        {
            if (GetType(response, i) != NameTypeMessenger)
                continue;

            userName = Decode(response, i);

            break;
        }

        // MAC address
        var macAddress =
            $"{GetByte(response, count, 0):X2}{GetByte(response, count, 1):X2}" +
            $"{GetByte(response, count, 2):X2}{GetByte(response, count, 3):X2}" +
            $"{GetByte(response, count, 4):X2}{GetByte(response, count, 5):X2}";

        return (computerName, userName, groupName, macAddress);
    }

    private static string Decode(byte[] response, int i)
    {
        return Encoding.ASCII.GetString(response, ResponseBaseLen + ResponseBlockLen * i, ResponseNameLen).Trim();
    }

    private static int GetByte(IReadOnlyList<byte> response, int i, int n)
    {
        return response[ResponseBaseLen + ResponseBlockLen * i + n] & 0xFF;
    }

    private static int GetFlag(IReadOnlyList<byte> response, int i)
    {
        return response[ResponseBaseLen + ResponseBlockLen * i + ResponseNameLen + 1] & (0xFF +
            (response[ResponseBaseLen + ResponseBlockLen * i + ResponseNameLen + 2] & 0xFF) * 0xFF);
    }

    private static int GetType(IReadOnlyList<byte> response, int i)
    {
        return response[ResponseBaseLen + ResponseBlockLen * i + ResponseNameLen] & 0xFF;
    }
}