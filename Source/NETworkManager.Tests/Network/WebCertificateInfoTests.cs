using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class WebCertificateInfoTests
{
    private static X509Certificate2 CreateSelfSigned(string subject, DateTimeOffset notAfter, bool isCA = false)
    {
        using var key = RSA.Create(2048);

        var request = new CertificateRequest($"CN={subject}", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Required to issue certificates with it
        if (isCA)
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));

        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), notAfter);
    }

    [Fact]
    public void FromCertificate_SelfSigned()
    {
        var notAfter = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);

        using var certificate = CreateSelfSigned("FRITZ!Box", notAfter);

        var info = WebCertificateInfo.FromCertificate(certificate);

        Assert.Equal("FRITZ!Box", info.Subject);
        Assert.Equal("FRITZ!Box", info.Issuer);
        Assert.True(info.IsSelfSigned);
        Assert.Equal(notAfter.UtcDateTime, info.NotAfter);
        Assert.Equal(DateTimeKind.Utc, info.NotAfter.Kind);
        Assert.False(info.IsExpired);
    }

    [Fact]
    public void FromCertificate_IssuedByCA()
    {
        using var ca = CreateSelfSigned("Example CA", DateTimeOffset.UtcNow.AddYears(5), true);
        using var key = RSA.Create(2048);

        var request = new CertificateRequest("CN=nas.example.com", key, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        using var certificate = request.Create(ca, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30),
            [1, 2, 3, 4]);

        var info = WebCertificateInfo.FromCertificate(certificate);

        Assert.Equal("nas.example.com", info.Subject);
        Assert.Equal("Example CA", info.Issuer);
        Assert.False(info.IsSelfSigned);
    }

    [Fact]
    public void IsExpired()
    {
        Assert.True(new WebCertificateInfo("a", "a", DateTime.UtcNow.AddDays(-1), true).IsExpired);
        Assert.False(new WebCertificateInfo("a", "a", DateTime.UtcNow.AddDays(1), true).IsExpired);
    }
}
