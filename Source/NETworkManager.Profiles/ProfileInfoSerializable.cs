using System.Xml.Serialization;

namespace NETworkManager.Profiles
{
    [XmlType("ProfileInfo")] // XML --> Has to mapped because of #378   
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
