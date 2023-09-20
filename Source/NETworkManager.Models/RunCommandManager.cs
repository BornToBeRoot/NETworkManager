﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.Models;

public static class RunCommandManager
{
    public static IEnumerable<RunCommandInfo> GetList()
    {
        return ApplicationManager.GetNames().Where(name => name != ApplicationName.None).Select(name =>
            new RunCommandInfo
            {
                Name = name,
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
                    ApplicationName.IPScanner => "10.0.0.0/24",
                    ApplicationName.PortScanner => "10.0.0.10;10.0.0.20 1-1024",
                    ApplicationName.DNSLookup => "SERVER-01|10.0.0.10 A|CNAME",
                    ApplicationName.AWSSessionManager => "i-1234567890abcdef0",
                    ApplicationName.Whois => "borntoberoot.net",
                    _ => "SERVER-01|10.0.0.10"
                }
            }).ToList();
    }
}