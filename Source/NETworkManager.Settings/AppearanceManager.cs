using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace NETworkManager.Settings
{
    public static class AppearanceManager
    {
        
        
        private static readonly string ThemesFilePath = Path.Combine(ConfigurationManager.Current.ExecutionPath, "Themes");

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
