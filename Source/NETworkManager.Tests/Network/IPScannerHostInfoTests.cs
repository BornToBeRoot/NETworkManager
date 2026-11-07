using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class IPScannerHostInfoTests
{
    /// <summary>
    ///     Every value is copied (<see cref="IPScannerHostInfo.Clone" /> uses <see cref="IPScannerHostInfo.CopyFrom" />),
    ///     also a value added later.
    /// </summary>
    [Fact]
    public void Clone_CopiesAllValues()
    {
        var ipAddress = IPAddress.Parse("10.0.0.1");

        var host = new IPScannerHostInfo(ipAddress, true, new PingInfo(DateTime.Now, ipAddress, IPStatus.Success),
            true, [], new NetBIOSInfo(ipAddress));

        // Lookups ended (e.g. canceled scan), the pending flags below are set again
        host.EndPendingLookups();

        // Every property that has a setter (the setters are internal) gets a value that is not the default
        foreach (var property in typeof(IPScannerHostInfo).GetProperties().Where(x => x.SetMethod != null))
        {
            object value = property.PropertyType switch
            {
                var type when type == typeof(string) => property.Name,
                var type when type == typeof(bool) => true,
                var type when type == typeof(int) => 42,
                var type when type.IsEnum => Enum.GetValues(type).Cast<object>().Last(),
                var type when type == typeof(WebCertificateInfo) =>
                    new WebCertificateInfo("CN", "Issuer", DateTime.UtcNow, false),
                var type when type == typeof(WindowsHostInfo) =>
                    new WindowsHostInfo(null, WindowsNetApiInfo.FromError(5)),
                // Objects of the constructor (ping, ports, NetBIOS)
                _ => null
            };

            if (value != null)
                property.SetValue(host, value);
        }

        var clone = host.Clone();

        // Also the computed values (e.g. IsHostnamePending depends on the ended lookups)
        foreach (var property in typeof(IPScannerHostInfo).GetProperties())
            Assert.Equal(property.GetValue(host), property.GetValue(clone));
    }
}
