using System.Collections.Generic;
using System.Linq;
using NETworkManager.Localization;
using NETworkManager.Models;

namespace NETworkManager;

public static class RunCommandManager
{
    private const string DefaultArguments = "<SERVER-01|10.0.0.10>";

    public static IEnumerable<RunCommandInfo> GetList()
    {
        return GetApplicationList().Concat(GetSettingsList());
    }

    private static IEnumerable<RunCommandInfo> GetSettingsList()
    {
        return new List<RunCommandInfo>
        {
            new()
            {
                Type = RunCommandType.Setting,
                Name = "Settings",
                TranslatedName = Localization.Resources.Strings.Settings,
                Command = "settings",
                CanHandleArguments = false,
                ExampleArguments = string.Empty
            }
        };
    }


    private static IEnumerable<RunCommandInfo> GetApplicationList()
    {
        return ApplicationManager.GetNames().Where(name => name != ApplicationName.None).Select(name =>
            new RunCommandInfo
            {
                Type = RunCommandType.Application,
                Name = name.ToString(),
                TranslatedName = ResourceTranslator.Translate(ResourceIdentifier.ApplicationName, name),
                Command = name switch
                {
                    _ => name.ToString().ToLower()
                },
                CanHandleArguments = name switch
                {
                    ApplicationName.IPScanner => true,
                    ApplicationName.PortScanner => true,
                    ApplicationName.PingMonitor => true,
                    ApplicationName.Traceroute => true,
                    ApplicationName.DNSLookup => true,
                    ApplicationName.RemoteDesktop => true,
                    ApplicationName.PowerShell => true,
                    ApplicationName.PuTTY => true,
                    ApplicationName.AWSSessionManager => true,
                    ApplicationName.TigerVNC => true,
                    ApplicationName.WebConsole => true,
                    ApplicationName.SNMP => true,
                    ApplicationName.Whois => true,
                    _ => false
                },
                ExampleArguments = name switch
                {
                    ApplicationName.IPScanner => "<10.0.0.0/24>",
                    ApplicationName.PortScanner => "<10.0.0.10;10.0.0.20> <1-1024>",
                    ApplicationName.PingMonitor => DefaultArguments,
                    ApplicationName.Traceroute => DefaultArguments,
                    ApplicationName.DNSLookup => "<SERVER-01|10.0.0.10> <A|CNAME>",
                    ApplicationName.RemoteDesktop => DefaultArguments,
                    ApplicationName.PowerShell => DefaultArguments,
                    ApplicationName.PuTTY => DefaultArguments,
                    ApplicationName.AWSSessionManager => "<i-1234567890abcdef0>",
                    ApplicationName.TigerVNC => DefaultArguments,
                    ApplicationName.WebConsole => "<https://borntoberoot.net>",
                    ApplicationName.SNMP => DefaultArguments,
                    ApplicationName.Whois => "<borntoberoot.net>",
                    _ => string.Empty
                }
            }).ToList();
    }
}