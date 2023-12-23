namespace NETworkManager.Models.Network;

public class SubnetmaskInfo
{
    public SubnetmaskInfo()
    {
    }

    public SubnetmaskInfo(int cidr, string subnetmask)
    {
        CIDR = cidr;
        Subnetmask = subnetmask;
    }

    public int CIDR { get; set; }
    public string Subnetmask { get; set; }

    public override string ToString()
    {
        return Subnetmask;
    }
}