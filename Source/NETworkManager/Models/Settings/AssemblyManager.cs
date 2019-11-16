using System.Reflection;

namespace NETworkManager.Models.Settings
{
    public static class AssemblyManager
    {
        public static AssemblyInfo Current { get; set; }

        static AssemblyManager()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var name = assembly.GetName();

            Current = new AssemblyInfo
            {
                Version = name.Version,
                Location = assembly.Location,
                Name = name.Name
            };
        }
    }
}
