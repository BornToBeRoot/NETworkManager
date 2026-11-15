namespace Dragablz
{
    /// <summary>
    /// Defines which part of a <see cref="TabablzControl"/> accepts a tab which is dragged (as a window) over it.
    /// </summary>
    public enum MergeTargetArea
    {
        /// <summary>
        /// Only the area occupied by the header items (the default).
        /// </summary>
        HeaderItems,

        /// <summary>
        /// The whole header panel (<see cref="TabablzControl.HeaderPanelPartName"/>), including the empty area behind the last tab.
        /// </summary>
        HeaderPanel
    }
}
