namespace NETworkManager.Models;

/// <summary>
/// Mode of an add/edit dialog.
/// </summary>
public enum EditMode
{
    /// <summary>
    /// Add a new item with default values.
    /// </summary>
    Add,

    /// <summary>
    /// Edit an existing item.
    /// </summary>
    Edit,

    /// <summary>
    /// Add a new item, pre-filled with the values of an existing item.
    /// </summary>
    Copy
}
