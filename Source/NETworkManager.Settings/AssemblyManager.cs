using System.Reflection;
using System.IO;

namespace NETworkManager.Settings
{
    public static class AssemblyManager
    {
        public static AssemblyInfo Current { get; set; }

        static AssemblyManager()
        {
            var assembly = Assembly.GetEntryAssembly();

            var name = assembly.GetName();

            Current = new AssemblyInfo
            {
                Version = name.Version,
                Location = Path.GetDirectoryName(assembly.Location),
                Name = name.Name
            };
        }
    }
}
