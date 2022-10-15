using NETworkManager.Utilities;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using NETworkManager.Models.Export;
using NETworkManager.Localization.Resources;
using System.Linq;

namespace NETworkManager.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ExportViewModel : ViewModelBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ICommand ExportCommand { get; }

        /// <summary>
        ///
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        private bool _exportAll = true;

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        private bool _showExportSelected;

        /// <summary>
        /// 
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

        private bool _exportSelected;
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

        public ExportManager.ExportFileType FileType { get; set; }

        private bool _showCSV;
        public bool ShowCSV
        {
            get => _showCSV;
            set
            {
                if (value == _showCSV)
                    return;

                _showCSV = value;
                OnPropertyChanged();
            }
        }

        private bool _useCSV;
        public bool UseCSV
        {
            get => _useCSV;
            set
            {
                if (value == _useCSV)
                    return;

                if (value)
                {
                    FileType = ExportManager.ExportFileType.CSV;
                    ChangeFilePathExtension(FileType);
                }

                _useCSV = value;
                OnPropertyChanged();
            }
        }

        private bool _showXML;
        public bool ShowXML
        {
            get => _showXML;
            set
            {
                if (value == _showXML)
                    return;

                _showXML = value;
                OnPropertyChanged();
            }
        }

        private bool _useXML;
        public bool UseXML
        {
            get => _useXML;
            set
            {
                if (value == _useXML)
                    return;

                if (value)
                {
                    FileType = ExportManager.ExportFileType.XML;
                    ChangeFilePathExtension(FileType);
                }

                _useXML = value;
                OnPropertyChanged();
            }
        }


        private bool _showJSON;
        public bool ShowJSON
        {
            get => _showJSON;
            set
            {
                if (value == _showJSON)
                    return;

                _showJSON = value;
                OnPropertyChanged();
            }
        }

        private bool _useJSON;
        public bool UseJSON
        {
            get => _useJSON;
            set
            {
                if (value == _useJSON)
                    return;

                if (value)
                {
                    FileType = ExportManager.ExportFileType.JSON;
                    ChangeFilePathExtension(FileType);
                }

                _useJSON = value;
                OnPropertyChanged();
            }
        }

        private bool _showTXT;
        public bool ShowTXT
        {
            get => _showTXT;
            set
            {
                if (value == _showTXT)
                    return;

                _showTXT = value;
                OnPropertyChanged();
            }
        }

        private bool _useTXT;
        public bool UseTXT
        {
            get => _useTXT;
            set
            {
                if (value == _useTXT)
                    return;

                if (value)
                {
                    FileType = ExportManager.ExportFileType.TXT;
                    ChangeFilePathExtension(FileType);
                }

                _useTXT = value;
                OnPropertyChanged();
            }
        }

        private string _filePath;
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

        public ExportViewModel(Action<ExportViewModel> deleteCommand, Action<ExportViewModel> cancelHandler, ExportManager.ExportFileType[] showFilesTypes, bool showExportSelected)
        {
            ExportCommand = new RelayCommand(p => deleteCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            ShowCSV = showFilesTypes.Contains(ExportManager.ExportFileType.CSV);
            ShowXML = showFilesTypes.Contains(ExportManager.ExportFileType.XML);
            ShowJSON = showFilesTypes.Contains(ExportManager.ExportFileType.JSON);
            ShowTXT = showFilesTypes.Contains(ExportManager.ExportFileType.TXT);

            ShowExportSelected = showExportSelected;
        }

        public ExportViewModel(Action<ExportViewModel> deleteCommand, Action<ExportViewModel> cancelHandler, ExportManager.ExportFileType[] showFilesTypes, bool showExportSelected, ExportManager.ExportFileType fileType, string filePath) :
            this(deleteCommand, cancelHandler, showFilesTypes, showExportSelected)
        {
            FilePath = filePath;

            switch (fileType)
            {
                case ExportManager.ExportFileType.CSV:
                    UseCSV = true;
                    break;
                case ExportManager.ExportFileType.XML:
                    UseXML = true;
                    break;
                case ExportManager.ExportFileType.JSON:
                    UseJSON = true;
                    break;
                case ExportManager.ExportFileType.TXT:
                    UseTXT = true;                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }
        }

        public ICommand BrowseFileCommand => new RelayCommand(p => BrowseFileAction());

        private void BrowseFileAction()
        {
            var saveFileDialog = new SaveFileDialog();

            var fileExtension = ExportManager.GetFileExtensionAsString(FileType);

            saveFileDialog.Filter = $@"{fileExtension}-{Strings.File} | *.{fileExtension.ToLower()}";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                FilePath = saveFileDialog.FileName;
            }
        }

        private void ChangeFilePathExtension(ExportManager.ExportFileType fileType)
        {
            if (string.IsNullOrEmpty(FilePath))
                return;

            var extension = Path.GetExtension(FilePath).Replace(".", "");

            var newExtension = ExportManager.GetFileExtensionAsString(fileType);

            if (extension.Equals(newExtension, StringComparison.CurrentCultureIgnoreCase))
                return;

            FilePath = FilePath[..^extension.Length] + newExtension.ToLower();
        }
    }
}
