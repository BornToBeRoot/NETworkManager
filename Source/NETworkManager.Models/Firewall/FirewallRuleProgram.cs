using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace NETworkManager.Models.Firewall;

/// <summary>
/// Represents a program associated with a firewall rule.
/// </summary>
public class FirewallRuleProgram : ICloneable
{
    #region Variables
    /// <summary>
    /// Program to apply rule to.
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    public FileInfo Executable { private init; get; }

    /// <summary>
    /// Represents the name associated with the object.
    /// </summary>
    public string Name
    {
        get;
        private init
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            Executable = new FileInfo(value);
            field = value;
        }
    }
    #endregion

    #region Constructor
    /// <summary>
    /// Required for serialization.
    /// </summary>
    public FirewallRuleProgram()
    {
        
    }

    /// <summary>
    /// Construct program reference for firewall rule.
    /// </summary>
    /// <param name="pathToExe"></param>
    public FirewallRuleProgram(string pathToExe)
    {
        var exe = new FileInfo(pathToExe ?? string.Empty);
        Executable = exe;
        Name = exe.FullName;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Convert the full file path to string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Executable?.FullName;
    }

    /// <summary>
    /// Clone instance.
    /// </summary>
    /// <returns>An instance clone.</returns>
    public object Clone()
    {
        return new FirewallRuleProgram(Executable?.FullName);
    }

    #endregion
}
