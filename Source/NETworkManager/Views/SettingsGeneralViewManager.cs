using MahApps.Metro.IconPacks;
using System.Collections.Generic;
using System.Windows;

namespace NETworkManager.Views
{
    public static class SettingsGeneralViewManager
    {
        // List of all applications
        public static List<SettingsGeneralViewInfo> List
        {
            get
            {
                return new List<SettingsGeneralViewInfo>
                {
                    new SettingsGeneralViewInfo(Name.General, new PackIconModern() { Kind = PackIconModernKind.Box }),
                    new SettingsGeneralViewInfo(Name.Window, new PackIconMaterial() {Kind = PackIconMaterialKind.Application }),
                    new SettingsGeneralViewInfo(Name.Appearance, new PackIconMaterial() { Kind = PackIconMaterialKind.AutoFix }),
                    new SettingsGeneralViewInfo(Name.Language, new PackIconMaterial() { Kind = PackIconMaterialKind.Flag}),
                    new SettingsGeneralViewInfo(Name.HotKeys, new PackIconOcticons() { Kind = PackIconOcticonsKind.Keyboard }),
                    new SettingsGeneralViewInfo(Name.Autostart, new PackIconMaterial() { Kind = PackIconMaterialKind.Power }),
                    new SettingsGeneralViewInfo(Name.Settings, new PackIconOcticons() { Kind = PackIconOcticonsKind.Settings }),
                    new SettingsGeneralViewInfo(Name.ImportExport, new PackIconMaterial() { Kind = PackIconMaterialKind.Import}),
                    new SettingsGeneralViewInfo(Name.Developer,  new PackIconOcticons() { Kind = PackIconOcticonsKind.Beaker })
                };
            }
        }

        public static string TranslateName(Name name)
        {
            return Application.Current.Resources["String_SettingsName_" + name] as string;
        }

        public enum Name
        {
            Window,
            General, 
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
