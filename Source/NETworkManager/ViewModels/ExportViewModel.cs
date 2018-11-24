using NETworkManager.Utilities;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using NETworkManager.Models.Export;
using NETworkManager.Resources.Localization;

namespace NETworkManager.ViewModels
{
    public class ExportViewModel : ViewModelBase
    {
        public ICommand ExportCommand { get; }

        public ICommand CancelCommand { get; }

        private bool _exportAll;
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

        private bool _textOnly;
        public bool TextOnly
        {
            get => _textOnly;
            set
            {
                if (value == _textOnly)
                    return;

                _textOnly = value;
                OnPropertyChanged();
            }
        }

        public ExportManager.ExportFileType FileType { get; set; }

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

        public ExportViewModel(Action<ExportViewModel> deleteCommand, Action<ExportViewModel> cancelHandler, ExportManager.ExportFileType fileType = ExportManager.ExportFileType.CSV, string filePath = "")
        {
            ExportCommand = new RelayCommand(p => deleteCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            // Default
            ExportAll = true;

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
                    TextOnly = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }
        }
       
        public ICommand BrowseFileCommand
        {
            get { return new RelayCommand(p => BrowseFileAction()); }
        }

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

            if (extension.Equals(ExportManager.GetFileExtensionAsString(fileType), StringComparison.CurrentCultureIgnoreCase))
                return;

            FilePath = FilePath.Substring(0, FilePath.Length - extension.Length);

            FilePath += ExportManager.GetFileExtensionAsString(fileType).ToLower();
        }
    }
}
