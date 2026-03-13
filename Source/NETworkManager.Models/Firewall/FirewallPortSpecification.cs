// ReSharper disable MemberCanBePrivate.Global
// Needed for serialization.
namespace NETworkManager.Models.Firewall;

/// <summary>
/// Represents a specification for defining and validating firewall ports.
/// </summary>
/// <remarks>
/// This class is used to encapsulate rules and configurations for
/// managing firewall port restrictions or allowances. It provides
/// properties and methods to define a range of acceptable ports or
/// individual port specifications.
/// </remarks>
public class FirewallPortSpecification
{
    /// <summary>
    /// Gets or sets the start point or initial value of a process, range, or operation.
    /// </summary>
    /// <remarks>
    /// The <c>Start</c> property typically represents the beginning state or position for sequential
    /// processing or iteration. The exact usage of this property may vary depending on the context of
    /// the class or object it belongs to.
    /// </remarks>
    public int Start { get; set; }

    /// <summary>
    /// Gets or sets the endpoint or final state of a process, range, or operation.
    /// </summary>
    /// <remarks>
    /// This property typically represents the termination position, time, or value
    /// in a sequence, operation, or any bounded context. Its specific meaning may vary
    /// depending on the context in which it is used.
    /// </remarks>
    public int End { get; set; }

    /// <summary>
    /// For serializing.
    /// </summary>
    public FirewallPortSpecification()
    {
        Start = -1;
        End = -1;
    }

    /// <summary>
    /// Represents the specification for a firewall port, detailing its configuration
    /// and rules for inbound or outbound network traffic.
    /// </summary>
    public FirewallPortSpecification(int start, int end = -1)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current instance of the object.</returns>
    public override string ToString()
    {
        if (Start is 0)
            return string.Empty;
        return End is -1 or 0 ? $"{Start}" : $"{Start}-{End}";
    }
}