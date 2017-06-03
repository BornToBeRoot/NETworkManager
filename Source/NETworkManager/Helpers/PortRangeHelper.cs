using System.Threading.Tasks;
using System.Collections.Generic;

namespace NETworkManager.Helpers
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
            List<int> list = new List<int>();

            foreach (string portOrRange in portRange.Replace(" ", "").Split(';'))
            {
                if (portOrRange.Contains("-"))
                {
                    string[] portRangeSplit = portOrRange.Split('-');

                    for (int i = int.Parse(portRangeSplit[0]); i < int.Parse(portRangeSplit[1]) + 1; i++)
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