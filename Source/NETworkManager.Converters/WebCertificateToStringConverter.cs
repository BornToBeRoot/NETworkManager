using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a <see cref="WebCertificateInfo" /> to a translated <see cref="string" /> for the row details of the
///     IP Scanner (subject, issuer or end of the validity).
/// </summary>
public sealed class WebCertificateToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert a <see cref="WebCertificateInfo" /> to a translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Certificate as <see cref="WebCertificateInfo" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter">
    ///     "Subject", "Issuer" ("Self-signed" for a self-signed certificate) or "NotAfter" (local time, "(expired)" if
    ///     it is expired).
    /// </param>
    /// <param name="culture"></param>
    /// <returns>Translated value, "-/-" without certificate (e.g. http).</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not WebCertificateInfo certificate)
            return "-/-";

        var notAfter = DateTimeHelper.DateTimeToFullDateTimeString(certificate.NotAfter.ToLocalTime());

        return parameter as string switch
        {
            "Issuer" => certificate.IsSelfSigned ? Strings.SelfSigned : certificate.Issuer,
            "NotAfter" => certificate.IsExpired ? string.Format(Strings.XExpired, notAfter) : notAfter,
            _ => certificate.Subject
        };
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
