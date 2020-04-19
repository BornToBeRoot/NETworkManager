using System;

namespace NETworkManager.Localization
{
    /// <summary>
    /// Class to hold all informations about a localization.
    /// </summary>
    public class LocalizationInfo
    {
        /// <summary>
        /// Name of the Language.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Native name of the language.
        /// </summary>
        public string NativeName { get; set; }

        /// <summary>
        /// Uri of the country flag.
        /// </summary>
        public Uri FlagUri { get; set; }

        /// <summary>
        /// Translator(s), who have contributed to the translation. Multiple names are separated with ",".
        /// </summary>
        public string Translator { get; set; }

        /// <summary>
        /// Culture code of the language.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Percentage of how many strings are translated.
        /// </summary>
        public double PercentTranslated { get; set; }

        /// <summary>
        /// Indicates whether the language has been translated by the maintainer or the community 
        /// </summary>
        public bool IsOfficial { get; set; }
                
        /// <summary>
        /// Create an empty instance of <see cref="LocalizationInfo"/>.
        /// </summary>
        public LocalizationInfo()
        {

        }

        /// <summary>
        /// Create an instance of <see cref="LocalizationInfo"/> with a culture code.
        /// </summary>
        /// <param name="code">Culture code (like "en-US")</param>
        public LocalizationInfo(string code)
        {
            Code = code;
        }

        /// <summary>
        /// Create an instance of <see cref="LocalizationInfo"/> with all parameters.
        /// </summary>
        /// <param name="name"><see cref="Name"/>.</param>
        /// <param name="nativeName"><see cref="NativeName"/>.</param>
        /// <param name="flagUri"><see cref="FlagUri"/>.</param>
        /// <param name="translator"><see cref="Translator"/>.</param>
        /// <param name="code"><see cref="Code"/>.</param>
        /// <param name="percentTranslated"><see cref="PercentTranslated"/>.</param>
        /// <param name="isOfficial"><see cref="IsOfficial"/>.</param>
        public LocalizationInfo(string name, string nativeName, Uri flagUri, string translator, string code, double percentTranslated, bool isOfficial)
        {
            Name = name;
            NativeName = nativeName;
            FlagUri = flagUri;
            Translator = translator;
            Code = code;
            PercentTranslated = percentTranslated;
            IsOfficial = isOfficial;
        }
    }
}
