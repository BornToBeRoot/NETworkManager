using System.Threading.Tasks;
using System.Collections.Generic;

namespace NETworkManager.Utilities
{
    public static class PortRangeHelper
    {
        public static Task<int[]> ConvertPortRangeToIntArrayAsync(string portRange)
        {
            return Task.Run(() => ConvertPortRangeToIntArray(portRange));
        }

        // Generate from a string like "9; 11-13; 15" an list with "9,11,12,13,15"
        public static int[] ConvertPortRangeToIntArray(string portRange)
        {
            var list = new List<int>();

            foreach (var portOrRange in portRange.Replace(" ", "").Split(';'))
            {
                if (portOrRange.Contains("-"))
                {
                    var portRangeSplit = portOrRange.Split('-');

                    for (var i = int.Parse(portRangeSplit[0]); i < int.Parse(portRangeSplit[1]) + 1; i++)
                    {
                        list.Add(i);
                    }
                }
                else
                {
                    list.Add(int.Parse(portOrRange));
                }
            }

            return list.ToArray();
        }
    }
}