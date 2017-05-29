using System;
using System.Windows.Media.Imaging;

namespace NETworkManager.Models.Settings
{
    public class LocalizationInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public BitmapImage Icon { get; set; }
        public string Translator { get; set; }
        public string Code { get; set; }

        public LocalizationInfo()
        {

        }

        public LocalizationInfo(string code)
        {
            Code = code;
        }

        public LocalizationInfo(string name, string path, BitmapImage icon, string translator, string code)
        {
            Name = name;
            Path = path;
            Icon = icon;
            Translator = translator;
            Code = code;
        }

        public LocalizationInfo(string name, string path, Uri iconPath, string translator, string code) : this(name, path, new BitmapImage(iconPath), translator, code)
        {

        }
    }
}
