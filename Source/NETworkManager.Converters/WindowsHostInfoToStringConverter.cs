using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.Converters;

/// <summary>
///     Converts a value of the <see cref="WindowsHostInfo" /> of an IP Scanner host to a <see cref="string" /> for the
///     result and the row details, "-/-" if there is none. The value is selected with the converter parameter:
///     <c>OS</c>, <c>Edition</c>, <c>Version</c>, <c>Role</c>, <c>ComputerName</c>, <c>Domain</c>,
///     <c>Description</c>, <c>TimeOffset</c>, <c>Users</c> (one per line, the error if they could not be read),
///     <c>SMB</c> (dialect), <c>SMBSigning</c> (required / not required), <c>Error</c> (message of the NetApi error).
/// </summary>
public sealed class WindowsHostInfoToStringConverter : IValueConverter
{
    private const string NoValue = "-/-";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var text = value is WindowsHostInfo info ? GetValue(info, parameter as string) : null;

        return string.IsNullOrEmpty(text) ? NoValue : text;
    }

    /// <summary>
    ///     Gets a value of the Windows host information as text.
    /// </summary>
    /// <param name="info">Windows host information.</param>
    /// <param name="name">Name of the value, see <see cref="WindowsHostInfoToStringConverter" />.</param>
    /// <returns>Value, empty if there is none.</returns>
    public static string GetValue(WindowsHostInfo info, string name)
    {
        return name switch
        {
            "OS" => info.OS,
            "Edition" => info.Edition,
            "Version" => info.Version?.ToString(),
            "Role" => RoleToLocalizedString(info.Role),
            "ComputerName" => info.ComputerName,
            "Domain" => info.Domain,
            "Description" => info.Description,
            "TimeOffset" => info.TimeOffset is { } offset ? DateTimeHelper.TimeSpanToOffsetString(offset) : null,
            "Users" => info.Users != null
                ? string.Join(Environment.NewLine, info.Users)
                : GetErrorMessage(info.UsersError),
            "SMB" => info.SMB?.Dialect,
            "SMBSigning" => info.SMB == null
                ? null
                : info.SMB.SigningRequired
                    ? Strings.Required
                    : Strings.NotRequired,
            "Error" => GetErrorMessage(info.Error),
            _ => null
        };
    }

    /// <summary>
    ///     Gets the message of an error of the NetApi requests (of Windows, or the credentials were not tried).
    /// </summary>
    private static string GetErrorMessage(int error)
    {
        return error switch
        {
            WindowsHostInfo.ErrorLogonSkipped => Strings.WindowsLogonSkippedMessage,
            WindowsHostInfo.ErrorLogonNotChecked => Strings.WindowsLogonNotCheckedMessage,
            _ => WindowsHostInfo.GetErrorMessage(error)
        };
    }

    /// <summary>
    ///     Gets the translated role of a Windows host, empty if unknown.
    /// </summary>
    /// <param name="role">Role.</param>
    /// <returns>Translated role.</returns>
    public static string RoleToLocalizedString(WindowsHostRole role)
    {
        return role switch
        {
            WindowsHostRole.Workstation => Strings.Workstation,
            WindowsHostRole.Server => Strings.Server,
            WindowsHostRole.DomainController => Strings.DomainController,
            _ => string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
