using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Controls
{
    public class MultiSelectScrollingDataGrid : DataGrid
    {
        public MultiSelectScrollingDataGrid()
        {
            SelectionChanged += DataGridMultiItemSelect_SelectionChanged;
        }

        private void DataGridMultiItemSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemsList = SelectedItems;
        }

        public IList SelectedItemsList
        {
            get => (IList)GetValue(SelectedItemsListProperty);
            set => SetValue(SelectedItemsListProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsListProperty = DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(MultiSelectScrollingDataGrid), new PropertyMetadata(null));

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