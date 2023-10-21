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
        // Get data context from viewmodel
        var viewModel = (WiFiConnectViewModel)DataContext;

        switch (viewModel.ConnectMode)
        {
            // Pre-shared key
            case Models.Network.WiFiConnectMode.Psk:
                PasswordBoxPreSharedKey.Focus();
                break;
            // EAP
            case Models.Network.WiFiConnectMode.Eap:
                TextBoxUsername.Focus();
                break;
        }

        // Check if WPS is available for the network
        _ = viewModel.CheckWpsAsync();
    }
}
