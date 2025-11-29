using System;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for a custom command.
/// </summary>
public class CustomCommandViewModel : ViewModelBase
{
    /// <summary>
    /// Backing field for <see cref="ID"/>.
    /// </summary>
    private readonly Guid _id;

    /// <summary>
    /// The original custom command info.
    /// </summary>
    private readonly CustomCommandInfo _info;

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// Backing field for <see cref="Arguments"/>.
    /// </summary>
    private string _arguments;

    /// <summary>
    /// Backing field for <see cref="FilePath"/>.
    /// </summary>
    private string _filePath;

    /// <summary>
    /// Backing field for <see cref="InfoChanged"/>.
    /// </summary>
    private bool _infoChanged;

    /// <summary>
    /// Backing field for <see cref="IsEdited"/>.
    /// </summary>
    private bool _isEdited;

    /// <summary>
    /// Backing field for <see cref="Name"/>.
    /// </summary>
    private string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomCommandViewModel"/> class.
    /// </summary>
    /// <param name="saveCommand">The action to execute when saving.</param>
    /// <param name="cancelHandler">The action to execute when canceling.</param>
    /// <param name="isEdited">Indicates if the command is being edited.</param>
    /// <param name="info">The custom command info.</param>
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

    /// <summary>
    /// Gets the save command.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Gets the cancel command.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets the ID of the custom command.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the name of the custom command.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the file path of the custom command.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the arguments of the custom command.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the info has changed.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the command is being edited.
    /// </summary>
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

    /// <summary>
    /// Checks if the info has changed.
    /// </summary>
    private void CheckInfoChanged()
    {
        InfoChanged = _info.Name != null || _info.FilePath != FilePath || _info.Arguments != Arguments;
    }
}