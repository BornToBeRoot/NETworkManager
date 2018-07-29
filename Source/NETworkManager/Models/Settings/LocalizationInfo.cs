using System;
using System.Windows.Media.Imaging;

namespace NETworkManager.Models.Settings
{
    public class LocalizationInfo
    {
        public string Name { get; set; }
        public string NativeName { get; set; }
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

        public LocalizationInfo(string name, string nativeName, BitmapImage flag, string translator, string code)
        {
            Name = name;
            NativeName = nativeName;
            Flag = flag;
            Translator = translator;
            Code = code;
        }

        public LocalizationInfo(string name, string nativeName, Uri flagPath, string translator, string code) : this(name, nativeName, new BitmapImage(flagPath), translator, code)
        {

        }
    }
}
