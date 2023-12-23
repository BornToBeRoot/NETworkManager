namespace NETworkManager.Models;

/// <summary>
///     Stores information's about an application.
/// </summary>
public class ApplicationInfo
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationInfo" /> class.
    /// </summary>
    public ApplicationInfo()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationInfo" /> class with parameters.
    /// </summary>
    /// <param name="name">
    ///     <see cref="Name" />
    /// </param>
    public ApplicationInfo(ApplicationName name)
    {
        Name = name;
        IsVisible = true;
    }

    /// <summary>
    ///     Name of the application.
    /// </summary>
    public ApplicationName Name { get; set; }

    /// <summary>
    ///     Indicates that the application is visible to the user.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    ///     Method to check if an object is equal to this object.
    /// </summary>
    /// <param name="info">Object to check.</param>
    /// <returns>Equality as <see cref="bool" />.</returns>
    private bool Equals(ApplicationInfo info)
    {
        if (info == null)
            return false;

        return Name == info.Name;
    }

    /// <summary>
    ///     Method to check if an object is equal to this object.
    /// </summary>
    /// <param name="obj">Object from type <see cref="ApplicationInfo" /> to check.</param>
    /// <returns>Equality as <see cref="bool" />.</returns>
    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals(obj as ApplicationInfo);
    }

    /// <summary>
    ///     Method to return the hash code of this object.
    /// </summary>
    /// <returns>Hashcode as <see cref="int" />.</returns>
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}