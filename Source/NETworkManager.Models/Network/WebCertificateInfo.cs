using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace NETworkManager.Models.Network;

/// <summary>
///     TLS certificate of a web page, see <see cref="WebPageResolver" />.
/// </summary>
/// <param name="Subject">Name of the subject (common name, e.g. "FRITZ!Box").</param>
/// <param name="Issuer">Name of the issuer (common name, e.g. "Let's Encrypt R11").</param>
/// <param name="NotAfter">End of the validity (UTC).</param>
/// <param name="IsSelfSigned">The certificate is self-signed (subject is the issuer).</param>
public sealed record WebCertificateInfo(string Subject, string Issuer, DateTime NotAfter, bool IsSelfSigned)
{
    /// <summary>
    ///     Indicates whether the certificate is expired.
    /// </summary>
    public bool IsExpired => NotAfter < DateTime.UtcNow;

    /// <summary>
    ///     Creates the information of a certificate (copies the values, the certificate can be disposed afterwards).
    /// </summary>
    /// <param name="certificate">Certificate of the web server.</param>
    /// <returns>Information of the certificate.</returns>
    public static WebCertificateInfo FromCertificate(X509Certificate2 certificate)
    {
        return new WebCertificateInfo(
            WebPageResolver.CleanHeaderValue(certificate.GetNameInfo(X509NameType.SimpleName, false)),
            WebPageResolver.CleanHeaderValue(certificate.GetNameInfo(X509NameType.SimpleName, true)),
            certificate.NotAfter.ToUniversalTime(),
            certificate.SubjectName.RawData.SequenceEqual(certificate.IssuerName.RawData));
    }
}
