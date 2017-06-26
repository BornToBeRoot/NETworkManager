using MahApps.Metro.IconPacks;
using System.Collections.Generic;
using System.Windows;

namespace NETworkManager.Views
{
    public static class SettingsViewManager
    {
        // List of all applications
        public static List<SettingsViewInfo> List
        {
            get
            {
                return new List<SettingsViewInfo>
                {
                    new SettingsViewInfo(Name.General, new PackIconModern() { Kind = PackIconModernKind.Box }),
                    new SettingsViewInfo(Name.Applications, new PackIconMaterial() {Kind = PackIconMaterialKind.Application }),
                    new SettingsViewInfo(Name.Appearance, new PackIconMaterial() { Kind = PackIconMaterialKind.AutoFix }),
                    new SettingsViewInfo(Name.Language, new PackIconMaterial() { Kind = PackIconMaterialKind.Flag}),
                    new SettingsViewInfo(Name.HotKeys, new PackIconOcticons() { Kind = PackIconOcticonsKind.Keyboard }),
                    new SettingsViewInfo(Name.Autostart, new PackIconMaterial() { Kind = PackIconMaterialKind.Power }),
                    new SettingsViewInfo(Name.Settings, new PackIconOcticons() { Kind = PackIconOcticonsKind.Settings }),
                    new SettingsViewInfo(Name.ImportExport, new PackIconMaterial() { Kind = PackIconMaterialKind.Import}),
                    new SettingsViewInfo(Name.Developer,  new PackIconOcticons() { Kind = PackIconOcticonsKind.Beaker })
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
            Applications, 
            Appearance,
            Language,
            HotKeys,
            Autostart,
            Settings,
            ImportExport,
            Developer
        }
    }
}
