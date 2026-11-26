using NETworkManager.Models.Cloudflare;
using Newtonsoft.Json;
using Xunit;

namespace NETworkManager.Tests.Cloudflare;

public class SpeedTestMetaInfoTests
{
    [Fact]
    public void MetaWithAllValues()
    {
        var meta = JsonConvert.DeserializeObject<SpeedTestMetaInfo>(
            """{"clientIp":"203.0.113.1","asn":64496,"asOrganization":"Example ISP","country":"DE","city":"Bochum","colo":{"iata":"FRA","city":"Frankfurt-am-Main","cca2":"DE"}}""");

        Assert.Equal(64496, meta.Asn);
        Assert.Equal("Example ISP", meta.AsOrganization);
        Assert.Equal("FRA", meta.Colo.Iata);
    }

    [Fact]
    public void MetaWithoutAsnAndServer()
    {
        // No ASN (e.g. unknown network) must not fail the speed test
        var meta = JsonConvert.DeserializeObject<SpeedTestMetaInfo>("""{"asn":null,"asOrganization":"Example ISP"}""");

        Assert.Null(meta.Asn);
        Assert.Null(meta.Colo);
        Assert.Equal("Example ISP", meta.AsOrganization);
    }
}
