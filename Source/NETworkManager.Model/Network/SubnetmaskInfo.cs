namespace NETworkManager.Model.Network
{
    public class SubnetmaskInfo
    {
        public int CIDR { get; set; }
        public string Subnetmask { get; set; }

        public SubnetmaskInfo()
        {

        }

        public SubnetmaskInfo(int cidr, string subnetmask)
        {
            CIDR = cidr;
            Subnetmask = subnetmask;
        }

        public override string ToString()
        {
            return Subnetmask;
        }
    }
}
