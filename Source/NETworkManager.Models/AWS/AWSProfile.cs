using System.Collections.Generic;

namespace NETworkManager.Models.AWS
{
    public static class AWSProfile
    {
        public static List<AWSProfileInfo> GetDefaultList()
        {
            return new List<AWSProfileInfo>
            {
                new AWSProfileInfo(false, "default", "eu-central-1"),
                new AWSProfileInfo(false, "default", "us-east-1"),
            };
        }
    }
}
