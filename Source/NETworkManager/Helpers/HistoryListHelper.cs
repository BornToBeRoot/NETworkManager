using System.Collections.Generic;

namespace NETworkManager.Helpers
{
    public static class HistoryListHelper
    {
        public static List<string> Modify(List<string> list, string entry, int length)
        {
            int index = list.IndexOf(entry);

            if (index != -1)
                list.RemoveAt(index);
            else if (list.Count == length)
                list.RemoveAt(length - 1);

            list.Insert(0, entry);

            return list;
        }
    }
}
