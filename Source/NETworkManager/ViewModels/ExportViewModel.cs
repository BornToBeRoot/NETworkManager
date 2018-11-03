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

        public ExportManager.ExportFileType FileType;

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

        public ExportViewModel(Action<ExportViewModel> deleteCommand, Action<ExportViewModel> cancelHandler)
        {
            ExportCommand = new RelayCommand(p => deleteCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            // Default
            UseCSV = true;
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
    }
}
