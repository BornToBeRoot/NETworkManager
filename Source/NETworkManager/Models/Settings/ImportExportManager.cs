using System.Collections.Generic;
using System.IO.Compression;
using System.IO;

namespace NETworkManager.Models.Settings
{
    public class ImportExportManager
    {
        #region Variables
        public static bool ForceRestart { get; set; }

        public const string ImportExportFileExtension = ".zip";
        #endregion

        #region Methods 
        public static List<ImportExportOptions> ValidateImportFile(string filePath)
        {
            if (!Path.GetExtension(filePath).Equals(ImportExportFileExtension))
                throw new ImportFileNotValidException();

            List<ImportExportOptions> importOptions = new List<ImportExportOptions>();

            using (ZipArchive zipArchive = ZipFile.OpenRead(filePath))
            {
                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    ImportExportOptions importOption = GetImportExportOption(zipArchiveEntry.Name);

                    if (importOption != ImportExportOptions.None)
                        importOptions.Add(importOption);
                }
            }

            if (importOptions.Count == 0)
                throw new ImportFileNotValidException();
            else
                return importOptions;
        }

        public static void Import(string filePath, List<ImportExportOptions> importOptions)
        {
            using (ZipArchive zipArchive = ZipFile.OpenRead(filePath))
            {
                foreach (ImportExportOptions importOption in importOptions)
                {
                    string fileName = GetImportExportOptionFileName(importOption);

                    zipArchive.GetEntry(fileName).ExtractToFile(Path.Combine(SettingsManager.GetSettingsLocation(), fileName), true);
                }
            }

            // Restart, when application settings are changed
            if (importOptions.Contains(ImportExportOptions.ApplicationSettings))
                ForceRestart = true;
        }

        public static void Export(List<ImportExportOptions> exportOptions, string filePath)
        {
            List<string> filesToExport = new List<string>();

            foreach (ImportExportOptions exportOption in exportOptions)
            {
                filesToExport.Add(GetImportExportOptionFilePath(exportOption));
            }

            // Delete existing file (the user is ask in the SaveFileDialog)
            File.Delete(filePath);

            // Create the archiv
            using (ZipArchive zipArchive = ZipFile.Open(filePath, ZipArchiveMode.Create))
            {
                // Add the files
                foreach (string file in filesToExport)
                    zipArchive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            }
        }
        #endregion

        #region enums (incl. helper)
        public enum ImportExportOptions
        {
            None,
            ApplicationSettings,
            NetworkInterfaceProfiles,
            WakeOnLANClients
        }

        public static ImportExportOptions GetImportExportOption(string fileName)
        {
            if (fileName == SettingsManager.GetSettingsFileName())
                return ImportExportOptions.ApplicationSettings;

            if (fileName == NetworkInterfaceProfileManager.ProfilesFileName)
                return ImportExportOptions.NetworkInterfaceProfiles;

            if (fileName == WakeOnLANClientManager.ClientsFileName)
                return ImportExportOptions.WakeOnLANClients;

            return ImportExportOptions.None;
        }

        private static string GetImportExportOptionFileName(ImportExportOptions importExportOption)
        {
            switch (importExportOption)
            {
                case ImportExportOptions.ApplicationSettings:
                    return SettingsManager.GetSettingsFileName();
                case ImportExportOptions.NetworkInterfaceProfiles:
                    return NetworkInterfaceProfileManager.ProfilesFileName;
                case ImportExportOptions.WakeOnLANClients:
                    return WakeOnLANClientManager.ClientsFileName;
            }

            return string.Empty;
        }

        private static string GetImportExportOptionFilePath(ImportExportOptions importExportOption)
        {
            switch (importExportOption)
            {
                case ImportExportOptions.ApplicationSettings:
                    return SettingsManager.GetSettingsFilePath();
                case ImportExportOptions.NetworkInterfaceProfiles:
                    return NetworkInterfaceProfileManager.GetProfilesFilePath();
                case ImportExportOptions.WakeOnLANClients:
                    return WakeOnLANClientManager.GetClientsFilePath();
            }

            return string.Empty;
        }
        #endregion
    }
}
