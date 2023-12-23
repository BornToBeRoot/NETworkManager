using System.IO;
using System.Reflection;

namespace NETworkManager.Settings;

/// <summary>
///     Class contains information about the current executing assembly.
/// </summary>
public static class AssemblyManager
{
    /// <summary>
    ///     Creates a new instance of the <see cref="AssemblyManager" /> class and
    ///     sets the <see cref="Current" /> property on the first call.
    /// </summary>
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

    /// <summary>
    ///     Current executing assembly.
    /// </summary>
    public static AssemblyInfo Current { get; set; }
}