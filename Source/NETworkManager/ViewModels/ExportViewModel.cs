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
/// </summary>
public class ExportViewModel : ViewModelBase
{
    /// <summary>
    /// </summary>
    private bool _exportAll = true;

    private bool _exportSelected;

    private string _filePath;

    private bool _showCsv;

    /// <summary>
    /// </summary>
    private bool _showExportSelected;


    private bool _showJson;

    private bool _showTxt;

    private bool _showXml;

    private bool _useCsv;

    private bool _useJson;

    private bool _useTxt;

    private bool _useXml;

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
    /// </summary>
    public ICommand ExportCommand { get; }

    /// <summary>
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
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

    public ExportFileType FileType { get; private set; }

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

    public ICommand BrowseFileCommand => new RelayCommand(_ => BrowseFileAction());

    private void BrowseFileAction()
    {
        var saveFileDialog = new SaveFileDialog();

        var fileExtension = ExportManager.GetFileExtensionAsString(FileType);

        saveFileDialog.Filter = $@"{fileExtension}-{Strings.File} | *.{fileExtension.ToLower()}";

        if (saveFileDialog.ShowDialog() == DialogResult.OK) FilePath = saveFileDialog.FileName;
    }

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