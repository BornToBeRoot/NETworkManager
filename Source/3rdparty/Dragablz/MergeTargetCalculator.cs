using System;
using System.Windows;
using System.Windows.Controls;

namespace Dragablz
{
    /// <summary>
    /// Pure geometry used to decide where a dragged tab can be merged into a <see cref="TabablzControl"/>.
    /// </summary>
    internal static class MergeTargetCalculator
    {
        /// <summary>
        /// Calculates the rectangle which accepts a dragged tab.
        /// </summary>
        /// <param name="itemsBounds">Bounds of the header items control.</param>
        /// <param name="panelBounds">Bounds of the header panel, only used for <see cref="MergeTargetArea.HeaderPanel"/>.</param>
        /// <param name="area">Requested area.</param>
        /// <param name="orientation">Horizontal for top/bottom tab strips, vertical for left/right ones.</param>
        /// <param name="fixedItemsEnd">
        /// End of the last fixed item along the main axis, relative to the start of the items control (0 if there is none).
        /// </param>
        public static Rect GetBounds(Rect itemsBounds, Rect? panelBounds, MergeTargetArea area, Orientation orientation,
            double fixedItemsEnd)
        {
            var bounds = area == MergeTargetArea.HeaderPanel && panelBounds.HasValue
                ? panelBounds.Value
                : itemsBounds;

            if (fixedItemsEnd <= 0) return bounds;

            if (orientation == Orientation.Horizontal)
            {
                var left = Math.Max(bounds.Left, itemsBounds.Left + fixedItemsEnd);
                return new Rect(left, bounds.Top, Math.Max(0, bounds.Right - left), bounds.Height);
            }

            var top = Math.Max(bounds.Top, itemsBounds.Top + fixedItemsEnd);
            return new Rect(bounds.Left, top, bounds.Width, Math.Max(0, bounds.Bottom - top));
        }

        /// <summary>
        /// Grows a rectangle on all sides, e.g. to detect a cursor which is near a merge target.
        /// </summary>
        public static Rect Inflate(Rect bounds, double amount)
        {
            if (bounds.IsEmpty) return bounds;
            amount = Math.Max(0, amount);
            return new Rect(bounds.Left - amount, bounds.Top - amount, bounds.Width + 2 * amount,
                bounds.Height + 2 * amount);
        }

        /// <summary>
        /// Keeps the position of an inserted tab between the end of the fixed items and the end of the last item,
        /// so a tab dropped on the empty part of the header ends up behind the last tab.
        /// </summary>
        /// <param name="desiredPosition">Position along the main axis, relative to the items control.</param>
        /// <param name="fixedItemsEnd">End of the last fixed item (0 if there is none).</param>
        /// <param name="lastItemEnd">End of the last item (0 if there are no items).</param>
        public static double ClampInsertPosition(double desiredPosition, double fixedItemsEnd, double lastItemEnd)
        {
            var min = Math.Max(0, fixedItemsEnd);
            var max = Math.Max(min, lastItemEnd);
            return Math.Min(Math.Max(desiredPosition, min), max);
        }
    }
}
