using System;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class LookupPortLookupView
{
    private readonly LookupPortLookupViewModel _viewModel = new();

    public LookupPortLookupView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Lookup(string search, bool start)
    {
        _ = _viewModel.Lookup(search, start);
    }

}