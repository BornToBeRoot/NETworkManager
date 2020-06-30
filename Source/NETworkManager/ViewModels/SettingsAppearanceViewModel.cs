using ControlzEx.Theming;
using MahApps.Metro;
using NETworkManager.Settings;
using System.ComponentModel;
using System.Windows.Media;
using System.Linq;
using System.Windows.Data;
using System.Collections.Generic;
using Windows.UI.Xaml.Input;

namespace NETworkManager.ViewModels
{
    public class SettingsAppearanceViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        public ICollectionView BaseColorSchemes { get; private set; }

        public ICollectionView Themes { get; private set; }
        
        private Theme _selectedTheme;
        public Theme SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (value == _selectedTheme)
                    return;

                if (!_isLoading)
                {
                    AppearanceManager.ChangeTheme(value.Name);
                    SettingsManager.Current.Appearance_AppTheme = value.Name;
                }

                _selectedTheme = value;
                OnPropertyChanged();
            }
        }

        //private Accent _accentSelectedItem;
        //public Accent AccentSelectedItem
        //{
        //    get => _accentSelectedItem;
        //    set
        //    {
        //        if (value == _accentSelectedItem)
        //            return;

        //        if (!_isLoading)
        //        {
        //            AppearanceManager.ChangeAccent(value.Name);
        //            SettingsManager.Current.Appearance_Accent = value.Name;
        //        }

        //        _accentSelectedItem = value;
        //        OnPropertyChanged();
        //    }
        //}
        #endregion        

        #region Constructor, LoadSettings
        public SettingsAppearanceViewModel()
        {
            _isLoading = true;

            BaseColorSchemes = new CollectionViewSource { Source = ThemeManager.Current.Themes.GroupBy(x => x.BaseColorScheme) }.View;

            Themes = new CollectionViewSource { Source = ThemeManager.Current.Themes.Where(x => x.BaseColorScheme == "Light")}.View;
            //Themes.GroupDescriptions.Add(new PropertyGroupDescription("BaseColorScheme"));
            
            //Colors = new CollectionViewSource { Source = typeof(Colors).GetProperties().Where(prop => typeof(Color).IsAssignableFrom(prop.PropertyType)).Select(prop => new KeyValuePair<string, Color>(prop.Name, (Color)prop.GetValue(null))).ToList() }.View;



            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            //AppThemeSelectedItem = ThemeManager.DetectAppStyle().Item1;
            //AccentSelectedItem = ThemeManager.DetectAppStyle().Item2;
        }
        #endregion
    }
}