using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;
using System.Xml.Serialization;

namespace NETworkManager.Profiles
{
    /// <summary>
    /// Class represents a group.
    /// </summary>
    public class GroupInfo
    {
        /// <summary>
        /// Name of the group.
        /// </summary>
        public string Name { get; set; }

        [XmlIgnore]
        public new ObservableCollection<ProfileInfo> Profiles { get; set; }

        public string RemoteDesktop_Username { get; set; }

        [XmlIgnore]
        public SecureString RemoteDesktop_Password { get; set; }


        /// <summary>
        /// Initializes a new instance of the<see cref="GroupInfo"/> class.
        /// </summary>
        public GroupInfo()
        {
            Profiles = new ObservableCollection<ProfileInfo>();
        }

        /// <summary>
        /// Initializes a new instance of the<see cref="GroupInfo"/> class with name.
        /// </summary>
        public GroupInfo(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the<see cref="GroupInfo"/> class with properties.
        /// </summary>
        public GroupInfo(GroupInfo profileGroup) : this(profileGroup.Name)
        {
            Profiles = profileGroup.Profiles;            

            RemoteDesktop_Username = profileGroup.RemoteDesktop_Username;
            RemoteDesktop_Password = profileGroup.RemoteDesktop_Password;
        }
    }
}
