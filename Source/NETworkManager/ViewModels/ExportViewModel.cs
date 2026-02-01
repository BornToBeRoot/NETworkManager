using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for exporting data.
/// </summary>
public class ExportViewModel : ViewModelBase
{
    /// <summary>
    /// Backing field for <see cref="ExportAll"/>.
    /// </summary>
    private bool _exportAll = true;

    /// <summary>
    /// Backing field for <see cref="ExportSelected"/>.
    /// </summary>
    private bool _exportSelected;

    /// <summary>
    /// Backing field for <see cref="FilePath"/>.
    /// </summary>
    private string _filePath;

    /// <summary>
    /// Backing field for <see cref="ShowCsv"/>.
    /// </summary>
    private bool _showCsv;

    /// <summary>
    /// Backing field for <see cref="ShowExportSelected"/>.
    /// </summary>
    private bool _showExportSelected;


    /// <summary>
    /// Backing field for <see cref="ShowJson"/>.
    /// </summary>
    private bool _showJson;

    /// <summary>
    /// Backing field for <see cref="ShowTxt"/>.
    /// </summary>
    private bool _showTxt;

    /// <summary>
    /// Backing field for <see cref="ShowXml"/>.
    /// </summary>
    private bool _showXml;

    /// <summary>
    /// Backing field for <see cref="UseCsv"/>.
    /// </summary>
    private bool _useCsv;

    /// <summary>
    /// Backing field for <see cref="UseJson"/>.
    /// </summary>
    private bool _useJson;

    /// <summary>
    /// Backing field for <see cref="UseTxt"/>.
    /// </summary>
    private bool _useTxt;

    /// <summary>
    /// Backing field for <see cref="UseXml"/>.
    /// </summary>
    private bool _useXml;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportViewModel"/> class.
    /// </summary>
    /// <param name="deleteCommand">The action to execute when export is confirmed.</param>
    /// <param name="cancelHandler">The action to execute when cancel is clicked.</param>
    /// <param name="showFilesTypes">The array of file types to show.</param>
    /// <param name="showExportSelected">Indicates whether to show the "Export selected" option.</param>
    private ExportViewModel(Action<ExportViewModel> deleteCommand, Action<ExportViewModel> cancelHandler,
        ExportFileType[] showFilesTypes, bool showExportSelected)
    {
        ExportCommand = new RelayCommand(_ => deleteCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        ShowCsv = showFilesTypes.Contains(ExportFileType.Csv);
        ShowXml = showFilesTypes.Contains(ExportFileType.Xml);
        ShowJson = showFilesTypes.Contains(ExportFileType.Json);
        ShowTxt = showFilesTypes.Contains(ExportFileType.Txt);

        ShowExportSelected = showExportSelected;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportViewModel"/> class.
    /// </summary>
    /// <param name="deleteCommand">The action to execute when export is confirmed.</param>
    /// <param name="cancelHandler">The action to execute when cancel is clicked.</param>
    /// <param name="showFilesTypes">The array of file types to show.</param>
    /// <param name="showExportSelected">Indicates whether to show the "Export selected" option.</param>
    /// <param name="fileType">The initial file type.</param>
    /// <param name="filePath">The initial file path.</param>
    public ExportViewModel(Action<ExportViewModel> deleteCommand, Action<ExportViewModel> cancelHandler,
        ExportFileType[] showFilesTypes, bool showExportSelected, ExportFileType fileType, string filePath) :
        this(deleteCommand, cancelHandler, showFilesTypes, showExportSelected)
    {
        FilePath = filePath;

        switch (fileType)
        {
            case ExportFileType.Csv:
                UseCsv = true;
                break;
            case ExportFileType.Xml:
                UseXml = true;
                break;
            case ExportFileType.Json:
                UseJson = true;
                break;
            case ExportFileType.Txt:
                UseTxt = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
        }
    }

    /// <summary>
    /// Gets the command to export data.
    /// </summary>
    public ICommand ExportCommand { get; }

    /// <summary>
    /// Gets the command to cancel the operation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to export all data.
    /// </summary>
    public bool ExportAll
    {
        get => _exportAll;
        set
        {
            if (value == _exportAll)
                return;

            _exportAll = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the "Export selected" option.
    /// </summary>
    public bool ShowExportSelected
    {
        get => _showExportSelected;
        set
        {
            if (value == _showExportSelected)
                return;

            _showExportSelected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to export only selected data.
    /// </summary>
    public bool ExportSelected
    {
        get => _exportSelected;
        set
        {
            if (value == _exportSelected)
                return;

            _exportSelected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the selected file type for export.
    /// </summary>
    public ExportFileType FileType { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show the CSV option.
    /// </summary>
    public bool ShowCsv
    {
        get => _showCsv;
        set
        {
            if (value == _showCsv)
                return;

            _showCsv = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether CSV format is selected.
    /// </summary>
    public bool UseCsv
    {
        get => _useCsv;
        set
        {
            if (value == _useCsv)
                return;

            if (value)
            {
                FileType = ExportFileType.Csv;
                ChangeFilePathExtension(FileType);
            }

            _useCsv = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the XML option.
    /// </summary>
    public bool ShowXml
    {
        get => _showXml;
        set
        {
            if (value == _showXml)
                return;

            _showXml = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether XML format is selected.
    /// </summary>
    public bool UseXml
    {
        get => _useXml;
        set
        {
            if (value == _useXml)
                return;

            if (value)
            {
                FileType = ExportFileType.Xml;
                ChangeFilePathExtension(FileType);
            }

            _useXml = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the JSON option.
    /// </summary>
    public bool ShowJson
    {
        get => _showJson;
        set
        {
            if (value == _showJson)
                return;

            _showJson = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether JSON format is selected.
    /// </summary>
    public bool UseJson
    {
        get => _useJson;
        set
        {
            if (value == _useJson)
                return;

            if (value)
            {
                FileType = ExportFileType.Json;
                ChangeFilePathExtension(FileType);
            }

            _useJson = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the TXT option.
    /// </summary>
    public bool ShowTxt
    {
        get => _showTxt;
        set
        {
            if (value == _showTxt)
                return;

            _showTxt = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether TXT format is selected.
    /// </summary>
    public bool UseTxt
    {
        get => _useTxt;
        set
        {
            if (value == _useTxt)
                return;

            if (value)
            {
                FileType = ExportFileType.Txt;
                ChangeFilePathExtension(FileType);
            }

            _useTxt = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the file path for export.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set
        {
            if (value == _filePath)
                return;

            _filePath = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the command to browse for a file.
    /// </summary>
    public ICommand BrowseFileCommand => new RelayCommand(_ => BrowseFileAction());

    /// <summary>
    /// Action to browse for a file.
    /// </summary>
    private void BrowseFileAction()
    {
        var saveFileDialog = new SaveFileDialog();

        var fileExtension = ExportManager.GetFileExtensionAsString(FileType);

        saveFileDialog.Filter = $@"{fileExtension}-{Strings.File} | *.{fileExtension.ToLower()}";

        if (saveFileDialog.ShowDialog() == DialogResult.OK) FilePath = saveFileDialog.FileName;
    }

    /// <summary>
    /// Changes the file path extension based on the selected file type.
    /// </summary>
    /// <param name="fileType">The new file type.</param>
    private void ChangeFilePathExtension(ExportFileType fileType)
    {
        if (string.IsNullOrEmpty(FilePath))
            return;

        var extension = Path.GetExtension(FilePath).Replace(".", "");

        var newExtension = ExportManager.GetFileExtensionAsString(fileType);

        if (newExtension == null)
            return;

        if (extension.Equals(newExtension, StringComparison.OrdinalIgnoreCase))
            return;

        FilePath = FilePath[..^extension.Length] + newExtension.ToLower();
    }
}