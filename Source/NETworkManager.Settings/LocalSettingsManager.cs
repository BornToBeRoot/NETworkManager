namespace NETworkManager.Settings
{
    public static class LocalSettingsManager
    {
        public static bool UpgradeRequired
        {
            get { return Properties.Settings.Default.UpgradeRequired; }
            set { Properties.Settings.Default.UpgradeRequired = value; }
        }

        public static string Settings_CustomSettingsLocation
        {
            get { return Properties.Settings.Default.Settings_CustomSettingsLocation; }
            set { Properties.Settings.Default.Settings_CustomSettingsLocation = value; }
        }

        public static void Upgrade()
        {
            Properties.Settings.Default.Upgrade();
        }

        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}
