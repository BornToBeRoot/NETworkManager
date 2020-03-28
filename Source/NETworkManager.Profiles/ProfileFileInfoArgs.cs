using System;

namespace NETworkManager.Profiles
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
