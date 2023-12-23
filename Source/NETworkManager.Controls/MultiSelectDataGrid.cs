using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Controls;

public class MultiSelectDataGrid : DataGrid
{
    public static readonly DependencyProperty SelectedItemsListProperty =
        DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(MultiSelectDataGrid),
            new PropertyMetadata(null));

    public MultiSelectDataGrid()
    {
        SelectionChanged += DataGridMultiItemSelect_SelectionChanged;
    }

    public IList SelectedItemsList
    {
        get => (IList)GetValue(SelectedItemsListProperty);
        set => SetValue(SelectedItemsListProperty, value);
    }

    private void DataGridMultiItemSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedItemsList = SelectedItems;
    }
}