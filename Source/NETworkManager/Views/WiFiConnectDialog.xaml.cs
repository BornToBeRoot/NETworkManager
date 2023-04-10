using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WiFiConnectDialog
{
    public WiFiConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Get datacontext from viewmodel
        var viewModel = (WiFiConnectViewModel)DataContext;
                
        // Pre-shared key
        if (viewModel.ConnectMode == Models.Network.WiFiConnectMode.Psk)
            PasswordBoxPreSharedKey.Focus();

        // EAP
        if (viewModel.ConnectMode == Models.Network.WiFiConnectMode.Eap)
            TextBoxUsername.Focus();

        // Check if WPS is availble for the network
        _ = viewModel.CheckWpsAsync();
    }
}
