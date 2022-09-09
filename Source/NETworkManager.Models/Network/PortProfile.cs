using System.Collections.Generic;

namespace NETworkManager.Models.Network
{
    public static class PortProfile
    {
        public static List<PortProfileInfo> DefaultList()
        {
            return new List<PortProfileInfo>
            {
                new PortProfileInfo("Webserver", "80; 443"),
                new PortProfileInfo("Remote access", "22; 23; 3389"),
                new PortProfileInfo("Mailserver", "25; 110; 143; 465; 587; 993; 995"),
                new PortProfileInfo("Filetransfer", "20-21; 22; 989-990; 2049"),                
                new PortProfileInfo("Database", "1433-1434; 1521; 1830; 3306; 5432"),                
                new PortProfileInfo("SMB", "139; 445"),
                new PortProfileInfo("LDAP", "389; 636"),
                new PortProfileInfo("HTTP proxy", "3128")
            };
        }
    }
}
