using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Controls
{
    public class MultiSelectDataGrid : DataGrid
    {
        public MultiSelectDataGrid()
        {
            SelectionChanged += DataGridMultiItemSelect_SelectionChanged;
        }
        
        void DataGridMultiItemSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemsList = SelectedItems;
        }

        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsListProperty = DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(MultiSelectDataGrid), new PropertyMetadata(null));
    }
}
