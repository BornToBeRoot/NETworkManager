using System.Collections.Generic;

namespace NETworkManager.Models.Network;

public static class PortProfile
{
    public static List<PortProfileInfo> GetDefaultList()
    {
        return new List<PortProfileInfo>
        {
            new PortProfileInfo("DNS (via TCP)", "53"),
            new PortProfileInfo("NTP (via TCP)", "123"),                
            new PortProfileInfo("Webserver", "80; 443"),
            new PortProfileInfo("Webserver (Other)", "80; 443; 8080; 8443"),
            new PortProfileInfo("Remote access", "22; 23; 3389; 5900"),
            new PortProfileInfo("Mailserver", "25; 110; 143; 465; 587; 993; 995"),
            new PortProfileInfo("Filetransfer", "20-21; 22; 989-990; 2049"),                
            new PortProfileInfo("Database", "1433-1434; 1521; 1830; 3306; 5432"),                
            new PortProfileInfo("SMB", "139; 445"),
            new PortProfileInfo("LDAP", "389; 636"),
            new PortProfileInfo("HTTP proxy", "3128")
        };
    }
}
