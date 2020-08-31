using System.Diagnostics;
using System.Reflection;

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
                Location = assembly.Location,
                Name = name.Name
            };

            Debug.WriteLine(Current);
        }
    }
}
