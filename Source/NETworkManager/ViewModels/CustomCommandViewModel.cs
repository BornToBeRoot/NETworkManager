using System;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class CustomCommandViewModel : ViewModelBase
{
    private readonly Guid _id;

    private readonly CustomCommandInfo _info;
    private readonly bool _isLoading;

    private string _arguments;

    private string _filePath;

    private bool _infoChanged;

    private bool _isEdited;

    private string _name;

    public CustomCommandViewModel(Action<CustomCommandViewModel> saveCommand,
        Action<CustomCommandViewModel> cancelHandler, bool isEdited = false, CustomCommandInfo info = null)
    {
        _isLoading = true;

        SaveCommand = new RelayCommand(_ => saveCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        _isEdited = isEdited;

        // Create new --> GUID
        _info = info ?? new CustomCommandInfo();

        ID = _info.ID;
        Name = _info.Name;
        FilePath = _info.FilePath;
        Arguments = _info.Arguments;

        _isLoading = false;
    }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public Guid ID
    {
        get => _id;
        private init
        {
            if (_id == value)
                return;

            _id = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;

            _name = value;

            if (!_isLoading)
                CheckInfoChanged();

            OnPropertyChanged();
        }
    }

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath == value)
                return;

            _filePath = value;

            if (!_isLoading)
                CheckInfoChanged();

            OnPropertyChanged();
        }
    }

    public string Arguments
    {
        get => _arguments;
        set
        {
            if (_arguments == value)
                return;

            _arguments = value;

            if (!_isLoading)
                CheckInfoChanged();

            OnPropertyChanged();
        }
    }

    public bool InfoChanged
    {
        get => _infoChanged;
        set
        {
            if (value == _infoChanged)
                return;

            _infoChanged = value;
            OnPropertyChanged();
        }
    }

    public bool IsEdited
    {
        get => _isEdited;
        set
        {
            if (value == _isEdited)
                return;

            _isEdited = value;
            OnPropertyChanged();
        }
    }

    private void CheckInfoChanged()
    {
        InfoChanged = _info.Name != null || _info.FilePath != FilePath || _info.Arguments != Arguments;
    }
}