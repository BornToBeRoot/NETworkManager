using System.Security;

namespace NETworkManager.Models.RemoteDesktop;

public class RemoteDesktopSessionInfo
{
    public string Hostname { get; set; }
    public bool UseCredentials { get; set; }
    public string Username { get; set; }
    public string Domain { get; set; }
    public SecureString Password { get; set; }
    public int Port { get; set; }
    public bool AdjustScreenAutomatically { get; set; }
    public bool UseCurrentViewSize { get; set; }
    public int DesktopWidth { get; set; }
    public int DesktopHeight { get; set; }
    public int ColorDepth { get; set; }
    public bool EnableCredSspSupport { get; set; }
    public uint AuthenticationLevel { get; set; }
    public bool EnableGatewayServer { get; set; }
    public string GatewayServerHostname { get; set; }
    public bool GatewayServerBypassLocalAddresses { get; set; }
    public GatewayUserSelectedCredsSource GatewayServerLogonMethod { get; set; }
    public bool GatewayServerShareCredentialsWithRemoteComputer { get; set; }
    public bool UseGatewayServerCredentials { get; set; }
    public string GatewayServerUsername { get; set; }
    public string GatewayServerDomain { get; set; }
    public SecureString GatewayServerPassword { get; set; }
    public AudioRedirectionMode AudioRedirectionMode { get; set; }
    public AudioCaptureRedirectionMode AudioCaptureRedirectionMode { get; set; }
    public KeyboardHookMode KeyboardHookMode { get; set; }
    public bool RedirectClipboard { get; set; }
    public bool RedirectDevices { get; set; }
    public bool RedirectDrives { get; set; }
    public bool RedirectPorts { get; set; }
    public bool RedirectSmartCards { get; set; }
    public bool RedirectPrinters { get; set; }
    public bool PersistentBitmapCaching { get; set; }
    public bool ReconnectIfTheConnectionIsDropped { get; set; }
    public NetworkConnectionType NetworkConnectionType { get; set; }
    public bool DesktopBackground { get; set; }
    public bool FontSmoothing { get; set; }
    public bool DesktopComposition { get; set; }
    public bool ShowWindowContentsWhileDragging { get; set; }
    public bool MenuAndWindowAnimation { get; set; }
    public bool VisualStyles { get; set; }
}