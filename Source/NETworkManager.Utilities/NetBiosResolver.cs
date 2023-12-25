using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NETworkManager.Utilities;

/// <summary>
/// NetBIOS resolver written in C# based on the following sources:
/// https://web.archive.org/web/20100409111218/http://msdn.microsoft.com/en-us/library/system.net.sockets.socket.aspx
/// https://github.com/angryip/ipscan (GPLv2) 
/// </summary>
 public static class NetBiosResolver
    {
        private const int NetBiosUdpPort = 137;
        
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

        private const int ResponseTypePos = 47;
        private const byte ResponseTypeNbstat = 33;
        
        private const int ResponseBaseLen = 57;
        private const int ResponseNameLen = 15;
        private const int ResponseBlockLen = 18;

        private const int GroupNameFlag = 128;
        
        private const int NameTypeDomain = 0x00;
        private const int NameTypeMessenger = 0x03;

        public static (string ComputerName, string UserName, string GroupName, string MacAddress)? Resolve(IPAddress ipAddress, int timeout)
        {
            var udpClient = new UdpClient();
            udpClient.Client.ReceiveTimeout = timeout;

            var remoteEndPoint = new IPEndPoint(ipAddress, NetBiosUdpPort);
            var localEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                udpClient.Send(RequestData, RequestData.Length, remoteEndPoint);

                var response = udpClient.Receive(ref localEndPoint);

                if (response.Length < ResponseBaseLen || response[ResponseTypePos] != ResponseTypeNbstat)
                    return null; // response was too short

                var count = response[ResponseBaseLen - 1] & 0xFF;

                if (response.Length < ResponseBaseLen + ResponseBlockLen * count)
                    return null; // data was truncated or something is wrong

                return ExtractNames(response, count);
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

        private static (string ComputerName, string UserName, string GroupName, string MacAddress) ExtractNames(byte[] response, int count)
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
                $"{GetByte(response, count, 0):X2}-{GetByte(response, count, 1):X2}-" +
                $"{GetByte(response, count, 2):X2}-{GetByte(response, count, 3):X2}-" +
                $"{GetByte(response, count, 4):X2}-{GetByte(response, count, 5):X2}";
            
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
            return response[ResponseBaseLen + ResponseBlockLen * i + ResponseNameLen + 1] & 0xFF +
                (response[ResponseBaseLen + ResponseBlockLen * i + ResponseNameLen + 2] & 0xFF) * 0xFF;
        }

        private static int GetType(IReadOnlyList<byte> response, int i)
        {
            return response[ResponseBaseLen + ResponseBlockLen * i + ResponseNameLen] & 0xFF;
        }
    }