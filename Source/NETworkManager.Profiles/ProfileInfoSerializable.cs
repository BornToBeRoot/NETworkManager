namespace NETworkManager.Profiles
{
    public class ProfileInfoSerializable : ProfileInfo
    {
        public string RemoteDesktop_Password { get; set; }

        public ProfileInfoSerializable()
        {

        }

        public ProfileInfoSerializable(ProfileInfo profile) : base(profile)
        {
            
        }
    }
}
