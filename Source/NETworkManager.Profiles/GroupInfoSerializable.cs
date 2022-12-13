using System.Collections.Generic;

namespace NETworkManager.Profiles
{
    public class GroupInfoSerializable : GroupInfo
    {
        /// <summary>
        /// Override the default profiles to make it serializable.
        /// </summary>
        public new List<ProfileInfoSerializable> Profiles { get; set; }

        /// <summary>
        /// Override the default remote desktop password to make it serializable.
        /// </summary>
        public new string RemoteDesktop_Password { get; set; }

        public GroupInfoSerializable()
        {

        }

        public GroupInfoSerializable(GroupInfo profileGroup) : base(profileGroup)
        {

        }
    }
}
