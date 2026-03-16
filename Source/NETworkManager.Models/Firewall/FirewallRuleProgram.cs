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
    public FileInfo Executable {
        private set;
        get
        {
            if (field is null && Name is not null)
                field = new FileInfo(Name);

            return field;
        }
    }

    /// <summary>
    /// Represents the name associated with the object.
    /// </summary>
    public string Name
    {
        get;
        // Public modifier required for deserialization
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        set
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
    /// Public empty constructor is required for de-/serialization.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public FirewallRuleProgram()
    {
    }

    /// <summary>
    /// Construct program reference for firewall rule.
    /// </summary>
    /// <param name="pathToExe"></param>
    public FirewallRuleProgram(string pathToExe)
    {
        ArgumentNullException.ThrowIfNull(pathToExe);
        var exe = new FileInfo(pathToExe);
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
        try
        {
            return new FirewallRuleProgram(Executable?.FullName);
        }
        catch (ArgumentNullException)
        {
            return new FirewallRuleProgram();
        }
        
    }

    #endregion
}
