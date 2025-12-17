using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Controls;

public class MultiSelectDataGrid : DataGrid
{
    private bool _isUpdatingSelection;

    public static readonly DependencyProperty SelectedItemsListProperty =
        DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(MultiSelectDataGrid),
            new PropertyMetadata(null, OnSelectedItemsListChanged));

    public MultiSelectDataGrid()
    {
        SelectionChanged += DataGridMultiItemSelect_SelectionChanged;
    }

    public IList SelectedItemsList
    {
        get => (IList)GetValue(SelectedItemsListProperty);
        set => SetValue(SelectedItemsListProperty, value);
    }

    private static void OnSelectedItemsListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiSelectDataGrid dataGrid)
            return;

        dataGrid.UnsubscribeFromCollectionChanged(e.OldValue as IList);
        dataGrid.SubscribeToCollectionChanged(e.NewValue as IList);
        dataGrid.UpdateSelectedItems();
    }

    private void SubscribeToCollectionChanged(IList list)
    {
        if (list is INotifyCollectionChanged observableCollection)
        {
            observableCollection.CollectionChanged += OnSelectedItemsListCollectionChanged;
        }
    }

    private void UnsubscribeFromCollectionChanged(IList list)
    {
        if (list is INotifyCollectionChanged observableCollection)
        {
            observableCollection.CollectionChanged -= OnSelectedItemsListCollectionChanged;
        }
    }

    private void OnSelectedItemsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateSelectedItems();
    }

    private void UpdateSelectedItems()
    {
        if (_isUpdatingSelection)
            return;

        _isUpdatingSelection = true;

        try
        {
            SelectedItems.Clear();

            if (SelectedItemsList != null)
            {
                foreach (var item in SelectedItemsList)
                {
                    SelectedItems.Add(item);
                }
            }
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    private void DataGridMultiItemSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingSelection || SelectedItemsList == null)
            return;

        _isUpdatingSelection = true;

        try
        {
            SelectedItemsList.Clear();

            foreach (var item in SelectedItems)
            {
                SelectedItemsList.Add(item);
            }
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }
}