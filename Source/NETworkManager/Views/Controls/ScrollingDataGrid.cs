using System.Collections.Specialized;
using System.Windows.Controls;

namespace NETworkManager.Views.Controls
{
    public class ScrollingDataGrid : DataGrid
    {
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;

            int newItemCount = e.NewItems.Count;

            if (newItemCount > 0)
                ScrollIntoView(e.NewItems[newItemCount - 1]);

            base.OnItemsChanged(e);
        }
    }
}
