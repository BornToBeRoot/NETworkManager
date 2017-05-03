using MahApps.Metro.IconPacks;
using System.Collections.Generic;
using System.Windows;

namespace NETworkManager.ViewModel.Settings
{
    public static class SettingsView
    {
        // List of all applications
        public static List<SettingsViewInfo> List
        {
            get
            {
                return new List<SettingsViewInfo>
                {
                    new SettingsViewInfo(Name.General, new PackIconModern() { Kind = PackIconModernKind.Box }),
                    new SettingsViewInfo(Name.Appearance, new PackIconMaterial() { Kind = PackIconMaterialKind.AutoFix }),
                    new SettingsViewInfo(Name.Language, new PackIconMaterial() { Kind = PackIconMaterialKind.Flag}),
                    new SettingsViewInfo(Name.HotKeys, new PackIconMaterial() { Kind = PackIconMaterialKind.KeyboardVariant }),
                    new SettingsViewInfo(Name.Autostart, new PackIconMaterial() { Kind = PackIconMaterialKind.TrendingUp }),
                    new SettingsViewInfo(Name.Settings, new PackIconEntypo() { Kind = PackIconEntypoKind.Save }),
                    new SettingsViewInfo(Name.Experimental,  new PackIconOcticons() { Kind = PackIconOcticonsKind.Beaker })
                };
            }
        }

        public static string TranslateName(Name name)
        {
            return Application.Current.Resources["String_SettingsName_" + name] as string;
        }

        public enum Name
        {
            General,
            Appearance,
            Language,
            HotKeys,
            Autostart,
            Settings,
            Experimental
        }
    }
}
