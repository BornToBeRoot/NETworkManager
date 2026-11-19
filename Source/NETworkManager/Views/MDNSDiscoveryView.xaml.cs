using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class MDNSDiscoveryView : IApplicationRequestHandler
{
    private readonly MDNSDiscoveryViewModel _viewModel = new();

    public MDNSDiscoveryView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void OnViewVisible() => _viewModel.OnViewVisible();
    public void OnViewHide() => _viewModel.OnViewHide();
    public void Open(ApplicationRequest request) => _ = _viewModel.OpenAsync(request);

    /// <summary>
    ///     Expands / collapses the details of the row of the button (like the IP Scanner).
    /// </summary>
    private void ExpandRowDetails_OnClick(object sender, RoutedEventArgs e)
    {
        for (var visual = sender as Visual; visual != null; visual = VisualTreeHelper.GetParent(visual) as Visual)
        {
            if (visual is not DataGridRow row)
                continue;

            row.DetailsVisibility =
                row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            break;
        }
    }
}
