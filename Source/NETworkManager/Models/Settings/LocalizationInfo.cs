using System;
using System.Windows.Media.Imaging;

namespace NETworkManager.Models.Settings
{
    public class LocalizationInfo
    {
        public string Name { get; set; }
        public string NativeName { get; set; }
        public string Path { get; set; }
        public BitmapImage Flag { get; set; }
        public string Translator { get; set; }
        public string Code { get; set; }

        public LocalizationInfo()
        {

        }

        public LocalizationInfo(string code)
        {
            Code = code;
        }

        public LocalizationInfo(string name, string nativeName, string path, BitmapImage flag, string translator, string code)
        {
            Name = name;
            NativeName = nativeName;
            Path = path;
            Flag = flag;
            Translator = translator;
            Code = code;
        }

        public LocalizationInfo(string name, string nativeName, string path, Uri flagPath, string translator, string code) : this(name, nativeName, path, new BitmapImage(flagPath), translator, code)
        {

        }
    }
}
