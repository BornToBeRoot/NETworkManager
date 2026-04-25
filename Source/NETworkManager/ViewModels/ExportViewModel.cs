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
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show the "Export selected" option.
    /// </summary>
    public bool ShowExportSelected
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

    /// <summary>
    /// Gets or sets a value indicating whether to export only selected data.
    /// </summary>
    public bool ExportSelected
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

    /// <summary>
    /// Gets the selected file type for export.
    /// </summary>
    public ExportFileType FileType { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show the CSV option.
    /// </summary>
    public bool ShowCsv
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

    /// <summary>
    /// Gets or sets a value indicating whether CSV format is selected.
    /// </summary>
    public bool UseCsv
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                FileType = ExportFileType.Csv;
                ChangeFilePathExtension(FileType);
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the XML option.
    /// </summary>
    public bool ShowXml
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

    /// <summary>
    /// Gets or sets a value indicating whether XML format is selected.
    /// </summary>
    public bool UseXml
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                FileType = ExportFileType.Xml;
                ChangeFilePathExtension(FileType);
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the JSON option.
    /// </summary>
    public bool ShowJson
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

    /// <summary>
    /// Gets or sets a value indicating whether JSON format is selected.
    /// </summary>
    public bool UseJson
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                FileType = ExportFileType.Json;
                ChangeFilePathExtension(FileType);
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the TXT option.
    /// </summary>
    public bool ShowTxt
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

    /// <summary>
    /// Gets or sets a value indicating whether TXT format is selected.
    /// </summary>
    public bool UseTxt
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                FileType = ExportFileType.Txt;
                ChangeFilePathExtension(FileType);
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the file path for export.
    /// </summary>
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