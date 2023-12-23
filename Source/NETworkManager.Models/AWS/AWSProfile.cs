using System.Collections.Generic;

namespace NETworkManager.Models.AWS;

public static class AWSProfile
{
    public static List<AWSProfileInfo> GetDefaultList()
    {
        return new List<AWSProfileInfo>
        {
            new(false, "default", "eu-central-1"),
            new(false, "default", "us-east-1")
        };
    }
}