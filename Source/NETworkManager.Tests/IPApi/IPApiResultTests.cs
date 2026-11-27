using System;
using System.Net;
using System.Net.Http;
using NETworkManager.Models.IPApi;
using Xunit;

namespace NETworkManager.Tests.IPApi;

public class IPApiResultTests
{
    [Fact]
    public void IPGeolocationResultOfAnHttpErrorHasTheStatusCode()
    {
        var error = new HttpRequestException("Service Unavailable", null, HttpStatusCode.ServiceUnavailable);

        var result = new IPGeolocationResult(error);

        Assert.True(result.HasError);
        Assert.Same(error, result.Error);
        Assert.Equal(503, result.ErrorCode);
        Assert.Equal("Service Unavailable", result.ErrorMessage);
    }

    [Fact]
    public void IPGeolocationResultWithoutResponseHasNoStatusCode()
    {
        var error = new TimeoutException();

        var result = new IPGeolocationResult(error);

        Assert.True(result.HasError);
        Assert.Same(error, result.Error);
        Assert.Equal(-1, result.ErrorCode);
    }

    [Fact]
    public void DNSResolverResultOfAnHttpErrorHasTheStatusCode()
    {
        var error = new HttpRequestException("Bad Gateway", null, HttpStatusCode.BadGateway);

        var result = new DNSResolverResult(error);

        Assert.True(result.HasError);
        Assert.Same(error, result.Error);
        Assert.Equal(502, result.ErrorCode);
        Assert.Equal("Bad Gateway", result.ErrorMessage);
    }

    [Fact]
    public void DNSResolverResultWithoutResponseHasNoStatusCode()
    {
        var result = new DNSResolverResult(new HttpRequestException("No such host is known."));

        Assert.True(result.HasError);
        Assert.Equal(0, result.ErrorCode);
        Assert.Null(result.Info);
    }
}
