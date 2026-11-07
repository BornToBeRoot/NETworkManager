using System;
using System.Windows;
using System.Windows.Threading;
using NETworkManager.Models.Network;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WiFiConnectChildWindow
{
    public WiFiConnectChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not WiFiConnectViewModel viewModel)
            return;

        // Focus the first input: SSID of a hidden network, otherwise the pre-shared key or the username (EAP)
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            if (viewModel.IsSsidRequired)
                TextBoxSsid.Focus();
            else if (viewModel.ConnectMode == WiFiConnectMode.Psk)
                PasswordBoxPreSharedKey.Focus();
            else if (viewModel.ConnectMode == WiFiConnectMode.Eap)
                TextBoxUsername.Focus();
        }));

        // Shows the WPS button if WPS is available for the network
        _ = viewModel.CheckWpsAsync();
    }
}