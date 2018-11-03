using NETworkManager.Utilities;
using System;
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

        public bool _exportAll;
        public bool ExportAll
        {
            get => _exportAll;
            set
            {
                if(value==_exportAll)
                    return;

                _exportAll = value;
                OnPropertyChanged();
            }
        }

        public bool _exportSelected;
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

        private bool _useCSV;
        public bool UseCSV
        {
            get => _useCSV;
            set
            {
                if (value == _useCSV)
                    return;

                if (value)
                    FileType = ExportManager.ExportFileType.CSV;

                ChangeFilePathExtension(FileType);

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
                    FileType = ExportManager.ExportFileType.XML;

                ChangeFilePathExtension(FileType);

                _useXML = value;
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

            switch (fileType)
            {
                case ExportManager.ExportFileType.CSV:
                    UseCSV = true;
                    break;
                case ExportManager.ExportFileType.XML:
                    UseXML = true;
                    break;
            }

            FilePath = filePath;
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
            if (fileType == ExportManager.ExportFileType.CSV)
            {
               
            }

            if (fileType == ExportManager.ExportFileType.XML)
            {

            }
        }
    }
}
