using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.Views
{
    public static class SettingsApplicationViewManager
    {
        // List of all applications
        public static List<ApplicationViewInfo> List
        {
            get
            {
                if (SettingsManager.Current.DeveloperMode)
                    return new List<ApplicationViewInfo>(ApplicationViewManager.List.Where(x => x.HasSettings == true));
                else
                    return new List<ApplicationViewInfo>(ApplicationViewManager.List.Where(x => x.HasSettings == true && x.IsDev == false));
            }
        }
    }
}
