using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace NETworkManager.Utilities.WPF;

/// <summary>
/// Attached-property helpers for <see cref="DataGrid"/>.
/// </summary>
public static class DataGridHelper
{
    #region FillRemainingWidth

    /// <summary>
    /// When set to <see langword="true"/>, a column fills the remaining width of the <see cref="DataGrid"/> - like a
    /// star (<c>*</c>) column, but with a pixel width: the visible column marked with <see cref="IsFillColumnProperty"/>,
    /// otherwise the last visible column. With a visible star column the
    /// DataGrid only lets a column grow by taking the width from the columns to its right (down to their
    /// <c>MinWidth</c>), so a column can't be resized wider than the grid (also not with a double-click on the
    /// gripper), there is never a horizontal scrollbar (#3557). Without star columns a column grows freely and the
    /// grid scrolls horizontally.
    /// <para>
    /// The fill column gets max(base width, available width - other visible columns). The base width is its
    /// <c>MinWidth</c>, or the width the user resized it to. It is updated when the size of the grid / its viewport
    /// changes and when another column is resized or shown / hidden. A column that no longer fills (e.g. hidden)
    /// gets its original width back. Don't use star columns in the same grid.
    /// </para>
    /// </summary>
    public static readonly DependencyProperty FillRemainingWidthProperty =
        DependencyProperty.RegisterAttached(
            "FillRemainingWidth",
            typeof(bool),
            typeof(DataGridHelper),
            new PropertyMetadata(false, OnFillRemainingWidthChanged));

    public static void SetFillRemainingWidth(DataGrid element, bool value) =>
        element.SetValue(FillRemainingWidthProperty, value);

    public static bool GetFillRemainingWidth(DataGrid element) =>
        (bool)element.GetValue(FillRemainingWidthProperty);

    /// <summary>
    /// Marks the column that fills the remaining width (see <see cref="FillRemainingWidthProperty"/>), if it isn't the
    /// last column (e.g. a description in the middle of the columns). While it is hidden, the last visible column
    /// fills.
    /// </summary>
    public static readonly DependencyProperty IsFillColumnProperty =
        DependencyProperty.RegisterAttached(
            "IsFillColumn",
            typeof(bool),
            typeof(DataGridHelper),
            new PropertyMetadata(false));

    public static void SetIsFillColumn(DataGridColumn element, bool value) =>
        element.SetValue(IsFillColumnProperty, value);

    public static bool GetIsFillColumn(DataGridColumn element) =>
        (bool)element.GetValue(IsFillColumnProperty);

    // State of a grid with FillRemainingWidth (subscriptions, current fill column)
    private static readonly DependencyProperty FillStateProperty =
        DependencyProperty.RegisterAttached(
            "FillState",
            typeof(FillState),
            typeof(DataGridHelper),
            new PropertyMetadata(null));

    private static readonly DependencyPropertyDescriptor ColumnActualWidthDescriptor =
        DependencyPropertyDescriptor.FromProperty(DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));

    private static readonly DependencyPropertyDescriptor ColumnVisibilityDescriptor =
        DependencyPropertyDescriptor.FromProperty(DataGridColumn.VisibilityProperty, typeof(DataGridColumn));

    private static void OnFillRemainingWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
            return;

        if (e.NewValue is true)
        {
            var state = new FillState(dataGrid);

            dataGrid.SetValue(FillStateProperty, state);

            dataGrid.Loaded += state.OnLoaded;
            dataGrid.Unloaded += state.OnUnloaded;

            if (dataGrid.IsLoaded)
                state.Attach();
        }
        else if (dataGrid.GetValue(FillStateProperty) is FillState state)
        {
            dataGrid.Loaded -= state.OnLoaded;
            dataGrid.Unloaded -= state.OnUnloaded;

            state.Detach();

            dataGrid.ClearValue(FillStateProperty);
        }
    }

    /// <summary>
    /// Subscriptions and the current fill column of a grid. The column change notifications
    /// (<see cref="DependencyPropertyDescriptor.AddValueChanged"/>) hold strong references, so they are only
    /// subscribed while the grid is loaded.
    /// </summary>
    private sealed class FillState(DataGrid dataGrid)
    {
        private ScrollViewer _scrollViewer;
        private DataGridColumn[] _columns = [];

        // Current fill column, its original width (restored when another column fills) and base width
        private DataGridColumn _fillColumn;
        private DataGridLength _fillColumnOriginalWidth;
        private double _fillColumnBaseWidth;

        // Width set by Update - another width of the fill column was set by the user (resize / double-click)
        private double _lastSetWidth = double.NaN;
        private bool _isUpdating;
        private bool _isUpdateQueued;

        public void OnLoaded(object sender, RoutedEventArgs e) => Attach();

        public void OnUnloaded(object sender, RoutedEventArgs e) => Detach();

        public void Attach()
        {
            Detach();

            _columns = [.. dataGrid.Columns];

            foreach (var column in _columns)
            {
                ColumnActualWidthDescriptor.AddValueChanged(column, OnColumnChanged);
                ColumnVisibilityDescriptor.AddValueChanged(column, OnColumnChanged);
            }

            dataGrid.SizeChanged += OnSizeChanged;

            _scrollViewer = FindScrollViewer(dataGrid);

            if (_scrollViewer != null)
                _scrollViewer.ScrollChanged += OnScrollChanged;

            QueueUpdate();
        }

        public void Detach()
        {
            foreach (var column in _columns)
            {
                ColumnActualWidthDescriptor.RemoveValueChanged(column, OnColumnChanged);
                ColumnVisibilityDescriptor.RemoveValueChanged(column, OnColumnChanged);
            }

            _columns = [];

            dataGrid.SizeChanged -= OnSizeChanged;

            if (_scrollViewer != null)
                _scrollViewer.ScrollChanged -= OnScrollChanged;

            _scrollViewer = null;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) => QueueUpdate();

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Viewport width changes e.g. when the vertical scrollbar appears; extent changes are caused by the
            // columns (handled via OnColumnChanged)
            if (!DoubleUtilIsZero(e.ViewportWidthChange))
                QueueUpdate();
        }

        private void OnColumnChanged(object sender, EventArgs e)
        {
            if (_isUpdating)
                return;

            // The user resized the fill column (or double-clicked its gripper): that is its new base width
            if (ReferenceEquals(sender, _fillColumn) && !double.IsNaN(_lastSetWidth) &&
                Math.Abs(_fillColumn.ActualWidth - _lastSetWidth) > 0.5)
                _fillColumnBaseWidth = _fillColumn.ActualWidth;

            QueueUpdate();
        }

        // Several changes in one layout pass (e.g. all columns while rows are added) -> one update
        private void QueueUpdate()
        {
            if (_isUpdateQueued)
                return;

            _isUpdateQueued = true;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                _isUpdateQueued = false;
                Update();
            }), DispatcherPriority.Loaded);
        }

        private void Update()
        {
            if (!dataGrid.IsLoaded)
                return;

            var visibleColumns = dataGrid.Columns
                .Where(x => x.Visibility == Visibility.Visible)
                .OrderBy(x => x.DisplayIndex)
                .ToList();

            // Marked fill column, otherwise the last visible column
            var fillColumn = visibleColumns.FirstOrDefault(GetIsFillColumn) ?? visibleColumns.LastOrDefault();

            _isUpdating = true;

            try
            {
                if (!ReferenceEquals(fillColumn, _fillColumn))
                {
                    // Give the previous fill column its original width back
                    if (_fillColumn != null && dataGrid.Columns.Contains(_fillColumn))
                        _fillColumn.Width = _fillColumnOriginalWidth;

                    _fillColumn = fillColumn;
                    _lastSetWidth = double.NaN;

                    if (_fillColumn == null)
                        return;

                    _fillColumnOriginalWidth = _fillColumn.Width;
                    _fillColumnBaseWidth = _fillColumn.MinWidth;
                }

                if (_fillColumn == null)
                    return;

                var availableWidth = GetAvailableWidth();

                if (availableWidth <= 0)
                    return;

                var otherColumnsWidth = dataGrid.Columns
                    .Where(x => x.Visibility == Visibility.Visible && !ReferenceEquals(x, _fillColumn))
                    .Sum(x => x.ActualWidth);

                // Floor - a fraction of a pixel too much would show the horizontal scrollbar
                var width = Math.Max(_fillColumnBaseWidth, Math.Floor(availableWidth - otherColumnsWidth));

                width = Math.Min(Math.Max(width, _fillColumn.MinWidth), _fillColumn.MaxWidth);

                if (!double.IsNaN(_lastSetWidth) && Math.Abs(width - _lastSetWidth) < 0.5 &&
                    Math.Abs(_fillColumn.ActualWidth - width) < 0.5)
                    return;

                _lastSetWidth = width;
                _fillColumn.Width = new DataGridLength(width);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>
        /// Gets the width available for the columns: the viewport of the grid's scroll viewer (without the vertical
        /// scrollbar) minus the row headers.
        /// </summary>
        private double GetAvailableWidth()
        {
            var width = _scrollViewer is { ViewportWidth: > 0 } ? _scrollViewer.ViewportWidth : dataGrid.ActualWidth;

            return width - dataGrid.CellsPanelHorizontalOffset;
        }

        private static bool DoubleUtilIsZero(double value) => Math.Abs(value) < 0.01;

        private static ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is ScrollViewer scrollViewer)
                    return scrollViewer;

                var result = FindScrollViewer(child);

                if (result != null)
                    return result;
            }

            return null;
        }
    }

    #endregion
}
