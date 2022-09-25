namespace NETworkManager.Profiles
{
    public class ProfileInfoSerializable : ProfileInfo
    {
        /// <summary>
        /// Override the default remote desktop password to make it serializable.
        /// </summary>
        public new string RemoteDesktop_Password { get; set; }

        public ProfileInfoSerializable()
        {

        }
                
        public ProfileInfoSerializable(ProfileInfo profile) : base(profile)
        {
            
        }
    }
}
