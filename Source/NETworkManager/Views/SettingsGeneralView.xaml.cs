﻿using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views;

public partial class SettingsGeneralView
{
    private readonly SettingsGeneralViewModel _viewModel = new();

    public SettingsGeneralView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }
}