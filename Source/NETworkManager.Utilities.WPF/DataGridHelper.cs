using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NETworkManager.Utilities.WPF;

/// <summary>
/// Attached-property helpers for <see cref="DataGrid"/>.
/// </summary>
public static class DataGridHelper
{
    /// <summary>
    /// When set to <see langword="true"/>, forces star-sized columns to recompute their width after
    /// the first row is realized. Without this, an initially empty <see cref="DataGrid"/> sizes star
    /// columns to <c>MinWidth</c> because the inner <see cref="System.Windows.Controls.ScrollViewer"/>
    /// measures with infinite width; only a window resize would otherwise trigger a correct re-measure.
    /// The handler unsubscribes itself after the first row so there is no ongoing overhead.
    /// </summary>
    public static readonly DependencyProperty RefreshStarColumnsOnFirstRowProperty =
        DependencyProperty.RegisterAttached(
            "RefreshStarColumnsOnFirstRow",
            typeof(bool),
            typeof(DataGridHelper),
            new PropertyMetadata(false, OnRefreshStarColumnsOnFirstRowChanged));

    public static void SetRefreshStarColumnsOnFirstRow(DataGrid element, bool value) =>
        element.SetValue(RefreshStarColumnsOnFirstRowProperty, value);

    public static bool GetRefreshStarColumnsOnFirstRow(DataGrid element) =>
        (bool)element.GetValue(RefreshStarColumnsOnFirstRowProperty);

    private static void OnRefreshStarColumnsOnFirstRowChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid || e.NewValue is not bool enabled || !enabled)
            return;

        dataGrid.LoadingRow += Handler;
        return;

        void Handler(object sender, DataGridRowEventArgs args)
        {
            dataGrid.LoadingRow -= Handler;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var column in dataGrid.Columns)
                {
                    if (!column.Width.IsStar)
                        continue;

                    var width = column.Width;
                    column.Width = 0;
                    column.Width = width;
                }
            }), DispatcherPriority.ContextIdle);
        }
    }
}
