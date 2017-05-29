using System.Reflection;

namespace NETworkManager.Models.Settings
{
    public static class AssemblyManager
    {
        public static AssemblyInfo Current { get; set; }

        public static void Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            AssemblyTitleAttribute title = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute;
            AssemblyCompanyAttribute company = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0] as AssemblyCompanyAttribute;
            AssemblyCopyrightAttribute copyright = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute;
            AssemblyName name = assembly.GetName();

            Current = new AssemblyInfo()
            {
                Title = title.Title,
                Company = company.Company,
                Copyright = copyright.Copyright,
                AssemblyVersion = name.Version
            };
        }
    }
}
