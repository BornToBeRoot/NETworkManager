using System.Collections.Generic;

namespace NETworkManager.Models.Network;

public static class PortProfile
{
    public static List<PortProfileInfo> GetDefaultList()
    {
        return new List<PortProfileInfo>
        {
            new("DNS (via TCP)", "53"),
            new("NTP (via TCP)", "123"),
            new("Webserver", "80; 443"),
            new("Webserver (Other)", "80; 443; 8080; 8443"),
            new("Remote access", "22; 23; 3389; 5900"),
            new("Mailserver", "25; 110; 143; 465; 587; 993; 995"),
            new("Filetransfer", "20-21; 22; 989-990; 2049"),
            new("Database", "1433-1434; 1521; 1830; 3306; 5432"),
            new("SMB", "139; 445"),
            new("LDAP", "389; 636"),
            new("HTTP proxy", "3128")
        };
    }
}