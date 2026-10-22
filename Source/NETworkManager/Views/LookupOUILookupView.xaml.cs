using System;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class LookupOUILookupView
{
    private readonly LookupOUILookupViewModel _viewModel = new();

    public LookupOUILookupView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Lookup(string search, bool start)
    {
        _viewModel.Lookup(search, start);
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }
}