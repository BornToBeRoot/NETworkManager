using NETworkManager.Models.RunCommand;
using Xunit;

namespace NETworkManager.Tests.RunCommand;

public class RunCommandTargetTests
{
    [Theory]
    [InlineData("10.0.0.1", RunCommandTargetKind.Host)]
    [InlineData("2001:db8::1", RunCommandTargetKind.Host)]
    [InlineData("server-01.borntoberoot.net", RunCommandTargetKind.Host)]
    [InlineData("borntoberoot.net", RunCommandTargetKind.Host)]
    [InlineData("10.0.0.1;10.0.0.2", RunCommandTargetKind.Hosts)]
    // A list with a range or subnet is a range (for the scanners, not for a DNS lookup).
    [InlineData("borntoberoot.net;10.0.0.0/24", RunCommandTargetKind.IPRange)]
    [InlineData("10.0.0.0/24;10.0.0.1", RunCommandTargetKind.IPRange)]
    [InlineData("10.0.0.1-10.0.0.100", RunCommandTargetKind.IPRange)]
    [InlineData("10.0.0.1-100", RunCommandTargetKind.IPRange)]
    [InlineData("10.0.[0-1].1", RunCommandTargetKind.IPRange)]
    // Exclusions (e.g. "!10.0.0.1") make a list a range for the scanners
    [InlineData("10.0.0.0/24;!10.0.0.1", RunCommandTargetKind.IPRange)]
    [InlineData("10.0.0.1;10.0.0.2;!10.0.0.2", RunCommandTargetKind.IPRange)]
    [InlineData("server.borntoberoot.net;!10.0.0.1", RunCommandTargetKind.IPRange)]
    [InlineData("10.0.0.0/24", RunCommandTargetKind.Subnet)]
    [InlineData("10.0.0.0/255.255.255.0", RunCommandTargetKind.Subnet)]
    [InlineData("2001:db8::/32", RunCommandTargetKind.Subnet)]
    [InlineData("00:11:22:33:44:55", RunCommandTargetKind.MACAddress)]
    [InlineData("00-11-22-33-44-55", RunCommandTargetKind.MACAddress)]
    [InlineData("001122334455", RunCommandTargetKind.MACAddress)]
    [InlineData("0011.2233.4455", RunCommandTargetKind.MACAddress)]
    [InlineData("00:11:22", RunCommandTargetKind.OUI)]
    [InlineData("22", RunCommandTargetKind.Ports)]
    [InlineData("22;80;443", RunCommandTargetKind.Ports)]
    [InlineData("80-90/tcp", RunCommandTargetKind.Ports)]
    [InlineData("AS13335", RunCommandTargetKind.ASN)]
    [InlineData("https://borntoberoot.net", RunCommandTargetKind.URL)]
    public void Kind(string target, RunCommandTargetKind kind)
    {
        Assert.Equal(kind, RunCommandTarget.GetKind(target));
    }

    // A host name without a domain (e.g. in a company network), which can also be a mistyped command (e.g. "pnig")
    [Theory]
    [InlineData("server01")]
    [InlineData("server-01")]
    [InlineData("pnig")]
    [InlineData("web")]
    [InlineData("facade")]
    public void Hostname(string target)
    {
        Assert.Equal(RunCommandTargetKind.Hostname, RunCommandTarget.GetKind(target));
    }

    [Theory]
    [InlineData("10.0.0.1;server01")]
    [InlineData("server01;server02.borntoberoot.net")]
    public void HostsWithHostnames(string target)
    {
        Assert.Equal(RunCommandTargetKind.Hosts, RunCommandTarget.GetKind(target));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("10.0.0.1 10.0.0.2")]
    [InlineData("server/24")]
    [InlineData("123456")]
    [InlineData("-server")]
    [InlineData("server_01")]
    [InlineData("@server")]
    [InlineData("10.0.0.1;server_01")]
    // Only host names without a domain can be any words (e.g. copied text)
    [InlineData("server01;server02")]
    [InlineData("foo;")]
    [InlineData("foo;!bar")]
    // Only exclusions, an invalid exclusion
    [InlineData("!10.0.0.1")]
    [InlineData("!10.0.0.1;!10.0.0.2")]
    [InlineData("10.0.0.0/24;!server_01")]
    // Ports only from 1 to 65535
    [InlineData("0")]
    [InlineData("99999")]
    [InlineData("22;99999")]
    // A range from a lower to a higher port, like the port lookup
    [InlineData("90-80")]
    [InlineData("80-80")]
    [InlineData("22;90-80/tcp")]
    // A shortened or partly typed IPv4 address
    [InlineData("10.1")]
    [InlineData("192.168.1")]
    [InlineData("192.168.1.")]
    public void NotATarget(string target)
    {
        Assert.Equal(RunCommandTargetKind.None, RunCommandTarget.GetKind(target));
    }
}
