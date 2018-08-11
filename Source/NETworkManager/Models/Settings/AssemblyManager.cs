using System.Reflection;

namespace NETworkManager.Models.Settings
{
    public static class AssemblyManager
    {
        public static AssemblyInfo Current { get; set; }

        public static void Load()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var title = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute;
            var name = assembly.GetName();

            Current = new AssemblyInfo
            {
                Title = title?.Title,
                Version = name.Version
            };
        }
    }
}
