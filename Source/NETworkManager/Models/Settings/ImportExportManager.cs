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

                    zipArchive.GetEntry(fileName).ExtractToFile(Path.Combine(SettingsManager.SettingsLocation, fileName), true);
                }
            }

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
            NetworkInterfaceConfigTemplates,
            WakeOnLanTemplates
        }

        public static ImportExportOptions GetImportExportOption(string fileName)
        {
            if (fileName == SettingsManager.SettingsFileName)
                return ImportExportOptions.ApplicationSettings;

            if (fileName == TemplateManager.NetworkInterfaceConfigTemplatesFileName)
                return ImportExportOptions.NetworkInterfaceConfigTemplates;

            if (fileName == TemplateManager.WakeOnLanTemplatesFileName)
                return ImportExportOptions.WakeOnLanTemplates;

            return ImportExportOptions.None;
        }

        private static string GetImportExportOptionFileName(ImportExportOptions importExportOption)
        {
            switch (importExportOption)
            {
                case ImportExportOptions.ApplicationSettings:
                    return SettingsManager.SettingsFileName;
                case ImportExportOptions.NetworkInterfaceConfigTemplates:
                    return TemplateManager.NetworkInterfaceConfigTemplatesFileName;
                case ImportExportOptions.WakeOnLanTemplates:
                    return TemplateManager.WakeOnLanTemplatesFileName;
            }

            return string.Empty;
        }

        private static string GetImportExportOptionFilePath(ImportExportOptions importExportOption)
        {
            switch (importExportOption)
            {
                case ImportExportOptions.ApplicationSettings:
                    return SettingsManager.SettingsFilePath;
                case ImportExportOptions.NetworkInterfaceConfigTemplates:
                    return TemplateManager.NetworkInterfaceConfigTemplatesFilePath;
                case ImportExportOptions.WakeOnLanTemplates:
                    return TemplateManager.WakeOnLanTemplatesFilePath;
            }

            return string.Empty;
        }
        #endregion
    }
}
