using NETworkManager.Documentation;
using NETworkManager.Models;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SettingsView
{
    private readonly SettingsViewModel _viewModel;

    public SettingsView()
    {
        InitializeComponent();
        _viewModel = new SettingsViewModel();

        DataContext = _viewModel;
    }

    private void ScrollViewer_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
    {
        e.Handled = true;
    }

    public void ChangeSettingsView(ApplicationName name)
    {
        _viewModel.ChangeSettingsView(name);

        // Scroll into view
        ListBoxSettings.ScrollIntoView(_viewModel.SelectedSettingsView);
    }

    public void OnViewVisible()
    {
        ProfilesView.OnViewVisible();
    }

    public void OnViewHide()
    {
        ProfilesView.OnViewHide();
    }

    public DocumentationIdentifier GetDocumentationIdentifier()
    {
        return _viewModel.GetDocumentationIdentifier();
    }        
}