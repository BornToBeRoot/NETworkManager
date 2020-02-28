namespace NETworkManager.Localization
{
    /// <summary>
    /// Interface to translate strings.
    /// </summary>
    interface ILocalizationStringTranslator
    {
        /// <summary>
        /// Method to translate strings.
        /// </summary>
        /// <param name="value">Original string to translate.</param>
        /// <returns>Translated string.</returns>
        string Translate(string value);
    }
}
