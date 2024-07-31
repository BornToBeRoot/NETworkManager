using Dragablz;

namespace NETworkManager.Controls;

/// <summary>
///     Interface for a user control that is a <see cref="DragablzTabItem.View" /> in a <see cref="DragablzTabItem" /> in a
///     <see cref="TabablzControl" />.
/// </summary>
public interface IDragablzTabItem
{
    /// <summary>
    ///     Method to close the tab.
    /// </summary>
    public void CloseTab()
    {
    }
}