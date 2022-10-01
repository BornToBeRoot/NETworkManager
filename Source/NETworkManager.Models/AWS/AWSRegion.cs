using Amazon;
using NETworkManager.Utilities;
using System.Collections.Generic;

namespace AWS
{
    public class AWSRegion : SingletonBase<AWSRegion>
    {
        private readonly HashSet<string> _regions = new();

        public AWSRegion()
        {
            foreach(var region in RegionEndpoint.EnumerableAllRegions)
               _regions.Add(region.SystemName);
        }

        public bool RegionExists(string region)
        {
            return _regions.Contains(region);
        }
    }
}
