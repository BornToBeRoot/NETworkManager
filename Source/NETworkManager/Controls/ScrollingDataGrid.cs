using System.Collections.Specialized;
using System.Windows.Controls;

namespace NETworkManager.Controls
{
    public class ScrollingDataGrid : DataGrid
    {
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;

            var newItemCount = e.NewItems.Count;

            if (newItemCount > 0)
                ScrollIntoView(e.NewItems[newItemCount - 1]);

            base.OnItemsChanged(e);
        }
    }
}