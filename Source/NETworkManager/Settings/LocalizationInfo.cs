using System;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace NETworkManager.Settings
{
    [Serializable]
    public class LocalizationInfo
    {
        [XmlIgnore]
        public string Name { get; set; }

        [XmlIgnore]
        public string Path { get; set; }

        [XmlIgnore]
        public BitmapImage Icon { get; set; }

        [XmlIgnore]
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
