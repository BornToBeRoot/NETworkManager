using System;
using System.Reflection;

namespace NETworkManager.Models.Settings
{
    public class AssemblyInfo
    {
        public string Title { get; set; }
        public Version Version { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }

        public AssemblyInfo()
        {

        }
    }
}