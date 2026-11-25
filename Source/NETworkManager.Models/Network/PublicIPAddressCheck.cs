using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Status of the request of the public IP address from an API (e.g. api.ipify.org).
/// </summary>
public enum PublicIPAddressCheckStatus
{
    /// <summary>
    ///     The API returned a public IP address.
    /// </summary>
    Success,

    /// <summary>
    ///     The API returned a private, link-local or loopback IP address (e.g. the page of a captive portal or a proxy).
    /// </summary>
    PrivateIPAddress,

    /// <summary>
    ///     The API answered, but not with a success status code or without an IP address of the address family.
    /// </summary>
    InvalidResponse,

    /// <summary>
    ///     The host name of the API could not be resolved (DNS).
    /// </summary>
    NameResolutionFailed,

    /// <summary>
    ///     The request failed (e.g. no connection, timeout).
    /// </summary>
    RequestFailed
}

/// <summary>
///     Result of the request of the public IP address.
/// </summary>
/// <param name="Status">Status of the request.</param>
/// <param name="IPAddress">IP address returned by the API (also if private), otherwise <c>null</c>.</param>
public sealed record PublicIPAddressCheckResult(PublicIPAddressCheckStatus Status, IPAddress IPAddress = null);

/// <summary>
///     Evaluates the answer of a public IP address API.
/// </summary>
public static class PublicIPAddressCheck
{
    /// <summary>
    ///     Parses the answer of the API. The answer should be the IP address as plain text; for custom APIs the first IP
    ///     address of the address family in the text is used (e.g. in a JSON object).
    /// </summary>
    /// <param name="isSuccessStatusCode">The HTTP status code is 2xx.</param>
    /// <param name="content">Content of the answer.</param>
    /// <param name="addressFamily">IPv4 or IPv6.</param>
    /// <returns>Result of the request.</returns>
    public static PublicIPAddressCheckResult ParseResponse(bool isSuccessStatusCode, string content,
        AddressFamily addressFamily)
    {
        if (!isSuccessStatusCode)
            return new PublicIPAddressCheckResult(PublicIPAddressCheckStatus.InvalidResponse);

        var text = content?.Trim() ?? string.Empty;

        if (!TryParseIPAddress(text, addressFamily, out var ipAddress))
        {
            var match = addressFamily == AddressFamily.InterNetwork
                ? RegexHelper.IPv4AddressExtractRegex().Match(text)
                : Regex.Match(text, RegexHelper.IPv6AddressRegex);

            if (!match.Success || !TryParseIPAddress(match.Value, addressFamily, out ipAddress))
                return new PublicIPAddressCheckResult(PublicIPAddressCheckStatus.InvalidResponse);
        }

        return GetStatus(ipAddress) switch
        {
            PublicIPAddressCheckStatus.InvalidResponse => new PublicIPAddressCheckResult(
                PublicIPAddressCheckStatus.InvalidResponse),
            var status => new PublicIPAddressCheckResult(status, ipAddress)
        };
    }

    /// <summary>
    ///     Parses an IP address in its normal notation only: IPv4 as four decimal numbers (<see cref="IPAddress.TryParse(string, out IPAddress)" />
    ///     also accepts e.g. "429" as 0.0.1.173 - an HTTP status code in the answer), IPv6 without a scope id.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <param name="addressFamily">IPv4 or IPv6.</param>
    /// <param name="ipAddress">Parsed IP address.</param>
    /// <returns><see langword="true" /> if the text is an IP address of the address family.</returns>
    private static bool TryParseIPAddress(string text, AddressFamily addressFamily, out IPAddress ipAddress)
    {
        ipAddress = null;

        var isValidNotation = addressFamily == AddressFamily.InterNetwork
            ? RegexHelper.IPv4AddressRegex().IsMatch(text)
            : text.Contains(':') && !text.Contains('%');

        return isValidNotation && IPAddress.TryParse(text, out ipAddress) && ipAddress.AddressFamily == addressFamily;
    }

    /// <summary>
    ///     Status of an IP address returned by the API: a private, shared (carrier-grade NAT), link-local or loopback
    ///     address is not the public IP address (e.g. the answer of a captive portal or a proxy); an unspecified,
    ///     multicast or reserved address is no valid answer.
    /// </summary>
    /// <param name="ipAddress">IP address returned by the API.</param>
    /// <returns><see cref="PublicIPAddressCheckStatus.Success" />, <see cref="PublicIPAddressCheckStatus.PrivateIPAddress" />
    ///     or <see cref="PublicIPAddressCheckStatus.InvalidResponse" />.</returns>
    public static PublicIPAddressCheckStatus GetStatus(IPAddress ipAddress)
    {
        var bytes = ipAddress.GetAddressBytes();

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            // 0.0.0.0/8 (this network), 224.0.0.0/4 (multicast), 240.0.0.0/4 (reserved, broadcast)
            if (bytes[0] == 0 || bytes[0] >= 224)
                return PublicIPAddressCheckStatus.InvalidResponse;

            // 100.64.0.0/10 (shared address space, carrier-grade NAT)
            if (bytes[0] == 100 && bytes[1] is >= 64 and <= 127)
                return PublicIPAddressCheckStatus.PrivateIPAddress;
        }
        else if (ipAddress.Equals(IPAddress.IPv6Any) || ipAddress.IsIPv6Multicast)
        {
            return PublicIPAddressCheckStatus.InvalidResponse;
        }

        return IPAddressHelper.IsPrivateIPAddress(ipAddress) || NetworkInterface.IsLinkLocal(ipAddress) ||
               IPAddress.IsLoopback(ipAddress)
            ? PublicIPAddressCheckStatus.PrivateIPAddress
            : PublicIPAddressCheckStatus.Success;
    }

    /// <summary>
    ///     Result of a failed request: a DNS error (the host name of the API could not be resolved) or another error.
    /// </summary>
    /// <param name="exception">Exception of the request.</param>
    /// <returns>Result of the request.</returns>
    public static PublicIPAddressCheckResult FromException(Exception exception)
    {
        var isNameResolutionError = exception is HttpRequestException
        {
            InnerException: SocketException
            {
                SocketErrorCode: SocketError.HostNotFound or SocketError.NoData or SocketError.TryAgain
            }
        };

        return new PublicIPAddressCheckResult(isNameResolutionError
            ? PublicIPAddressCheckStatus.NameResolutionFailed
            : PublicIPAddressCheckStatus.RequestFailed);
    }
}
