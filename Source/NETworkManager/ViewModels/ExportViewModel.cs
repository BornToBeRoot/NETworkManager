using NETworkManager.Utilities;
using System;
using System.Windows.Forms;
using System.Windows.Input;
using NETworkManager.Resources.Localization;

namespace NETworkManager.ViewModels
{
    public class ExportViewModel : ViewModelBase
    {
        public ICommand ExportCommand { get; }

        public ICommand CancelCommand { get; }

        private bool _useCSV;
        public bool UseCSV
        {
            get => _useCSV;
            set
            {
                if(value == _useCSV)
                    return;

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
                if(value== _useXML)
                    return;

                _useXML = value;
                OnPropertyChanged();
            }
        }

        private string _exportFilePath;
        public string ExportFilePath
        {
            get => _exportFilePath;
            set
            {
                if(value == _exportFilePath)
                    return;

                _exportFilePath = value;
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

            if (UseCSV)
                saveFileDialog.Filter = $@"CSV-{Strings.File} | *.csv";

            if(UseXML)
                saveFileDialog.Filter = $@"XML-{Strings.File} | *.xml";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ExportFilePath = saveFileDialog.FileName;
            }

        }
    }
}
