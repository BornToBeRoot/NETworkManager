using System;

namespace NETworkManager.Models.Profile
{
    public class ProfileFileInfoArgs : EventArgs
    {
        public ProfileFileInfo ProfileFileInfo { get; set; }


        public ProfileFileInfoArgs(ProfileFileInfo profileFileInfo)
        {
            ProfileFileInfo = profileFileInfo;
        }
    }
}
