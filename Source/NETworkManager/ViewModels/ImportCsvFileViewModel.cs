using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Input;
using log4net;
using NETworkManager.Localization.Resources;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public sealed class ImportCsvFileViewModel : ViewModelBase
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ImportCsvFileViewModel));

    private readonly Action<IReadOnlyList<ProfileImportCandidate>, ImportCsvFileViewModel> _parseCompleted;

    public ImportCsvFileViewModel(Action<IReadOnlyList<ProfileImportCandidate>, ImportCsvFileViewModel> parseCompleted,
        Action cancelDialog, ImportCsvFileViewModel previousState = null)
    {
        _parseCompleted = parseCompleted;

        FilePath = previousState != null
            ? previousState.FilePath
            : SettingsManager.Current.Profiles_ImportCsvLastFilePath ?? string.Empty;

        BrowseFileCommand = new RelayCommand(_ => BrowseFileAction());
        ParseCommand = new RelayCommand(_ => ImportAction());
        CancelCommand = new RelayCommand(_ => cancelDialog());
    }

    public string FilePath
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsStatusMessageDisplayed
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ICommand BrowseFileCommand { get; }

    public ICommand ParseCommand { get; }

    public ICommand CancelCommand { get; }

    private void BrowseFileAction()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = GlobalStaticConfiguration.CsvFileExtensionFilter
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
            FilePath = openFileDialog.FileName;
    }

    /// <summary>
    ///     Set the <see cref="FilePath" /> from drag and drop.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    public void SetFilePathFromDragDrop(string filePath)
    {
        if (!filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = Strings.OnlyCsvFilesAllowed;
            IsStatusMessageDisplayed = true;
            return;
        }

        IsStatusMessageDisplayed = false;
        FilePath = filePath;

        OnPropertyChanged(nameof(FilePath));
    }

    private void ImportAction()
    {
        IsStatusMessageDisplayed = false;

        IReadOnlyList<ProfileImportCandidate> candidates;

        try
        {
            var importedAt = DateTime.Now.ToString("g", CultureInfo.CurrentUICulture);
            var fallbackDescription = string.Format(Strings.Csv_ImportDescription, importedAt);

            candidates = CsvProfileImportParser.Parse(FilePath.Trim(), fallbackDescription);
        }
        catch (Exception exception)
        {
            Log.Error("CSV import failed.", exception);

            StatusMessage = exception.Message;
            IsStatusMessageDisplayed = true;

            return;
        }

        if (candidates.Count == 0)
        {
            StatusMessage = Strings.CsvNoEntriesFound;
            IsStatusMessageDisplayed = true;

            return;
        }

        SettingsManager.Current.Profiles_ImportCsvLastFilePath = FilePath.Trim();

        _parseCompleted(candidates, this);
    }
}
