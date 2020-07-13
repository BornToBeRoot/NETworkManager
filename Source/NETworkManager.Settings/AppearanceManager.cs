using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace NETworkManager.Settings
{
    public static class AppearanceManager
    {
        public static MetroDialogSettings MetroDialog = new MetroDialogSettings();

        public static void Load()
        {
            ThemeManager.Current.ChangeTheme(System.Windows.Application.Current, "Dark.Teal");

            var Colors = typeof(Colors)
                        .GetProperties()
                        .Where(prop => typeof(Color).IsAssignableFrom(prop.PropertyType))
                        .Select(prop => new KeyValuePair<String, Color>(prop.Name, (Color)prop.GetValue(null)))
                        .ToList();

            var selectedColor = Colors.FirstOrDefault(x => x.Key == "WhiteSmoke").Value;
            
            var theme = ThemeManager.Current.DetectTheme(System.Windows.Application.Current);

            var inverseTheme = ThemeManager.Current.GetInverseTheme(theme);
            ThemeManager.Current.AddTheme(RuntimeThemeGenerator.Current.GenerateRuntimeTheme(inverseTheme.BaseColorScheme, selectedColor));
            ThemeManager.Current.ChangeTheme(System.Windows.Application.Current, ThemeManager.Current.AddTheme(RuntimeThemeGenerator.Current.GenerateRuntimeTheme(theme.BaseColorScheme, selectedColor)));
        }

        public static void ChangeTheme(string name)
        {

        }

        public static void ChangeColor(string name)
        {

        }
    }
}
