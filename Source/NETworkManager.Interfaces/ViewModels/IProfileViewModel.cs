namespace NETworkManager.Interfaces.ViewModels;

// ReSharper disable InconsistentNaming
public interface IProfileViewModel
{
    #region General
    public string Name { get; }
    #endregion

    #region Firewall
    public bool Firewall_Enabled { get; set; }

    public IFirewallViewModel Firewall_IViewModel { get; set; }
    #endregion
}