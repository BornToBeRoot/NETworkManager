using System;

namespace NETworkManager.Controls
{
    public class GroupExpandedChangedArgs(string groupName, bool isExpanded) : EventArgs
    {
        public string GroupName { get; } = groupName;
        public bool IsExpanded { get; } = isExpanded;
    }
}
