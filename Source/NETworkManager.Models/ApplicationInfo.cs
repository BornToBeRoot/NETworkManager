using NETworkManager.Utilities;

namespace NETworkManager.Models;

/// <summary>
///     Stores information's about an application.
/// </summary>
public class ApplicationInfo : PropertyChangedBase
{
    /// <summary>
    ///     Private field for the <see cref="IsDefault" /> property.
    /// </summary>
    private bool _isDefault;


    /// <summary>
    ///     Private field for the <see cref="IsVisible" /> property.
    /// </summary>
    private bool _isVisible;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationInfo" /> class.
    /// </summary>
    public ApplicationInfo()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationInfo" /> class with parameters.
    /// </summary>
    /// <param name="name">Name of the application.</param>
    /// <param name="isVisible">Indicates that the application is visible to the user.</param>
    /// <param name="isDefault">Indicates that the application is the default application.</param>
    public ApplicationInfo(ApplicationName name, bool isVisible = true, bool isDefault = false)
    {
        Name = name;
        IsVisible = isVisible;
        IsDefault = isDefault;
    }

    /// <summary>
    ///     Name of the application.
    /// </summary>
    public ApplicationName Name { get; set; }

    /// <summary>
    ///     Indicates that the application is visible to the user.
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (value == _isVisible)
                return;

            _isVisible = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Indicates that the application is the default application.
    /// </summary>
    public bool IsDefault
    {
        get => _isDefault;
        set
        {
            if (value == _isDefault)
                return;

            _isDefault = value;
            OnPropertyChanged();
        }
    }

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
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Name.GetHashCode();
    }
}