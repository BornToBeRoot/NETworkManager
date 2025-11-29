using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for managing a port profile.
/// </summary>
public class PortProfileViewModel : ViewModelBase
{
    private readonly PortProfileInfo _info;
    private readonly bool _isLoading;

    private readonly string _previousPortAsString;

    private bool _infoChanged;

    private bool _isEdited;

    private string _name;

    private string _ports;

    /// <summary>
    /// Initializes a new instance of the <see cref="PortProfileViewModel"/> class.
    /// </summary>
    /// <param name="saveCommand">The action to execute when saving.</param>
    /// <param name="cancelHandler">The action to execute when cancelling.</param>
    /// <param name="isEdited">Indicates whether the profile is being edited.</param>
    /// <param name="info">The port profile information.</param>
    public PortProfileViewModel(Action<PortProfileViewModel> saveCommand, Action<PortProfileViewModel> cancelHandler,
        bool isEdited = false, PortProfileInfo info = null)
    {
        _isLoading = true;

        SaveCommand = new RelayCommand(_ => saveCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        IsEdited = isEdited;

        _info = info ?? new PortProfileInfo();

        Name = _info.Name;

        // List to string
        if (_info.Ports != null)
            Ports = _info.Ports;

        _previousPortAsString = Ports;

        _isLoading = false;
    }

    /// <summary>
    /// Gets the command to save the profile.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Gets the command to cancel the operation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets or sets the name of the profile.
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
                Validate();

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the ports associated with the profile.
    /// </summary>
    public string Ports
    {
        get => _ports;
        set
        {
            if (_ports == value)
                return;

            _ports = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the profile information has changed.
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
    /// Gets or sets a value indicating whether the profile is being edited.
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

    private void Validate()
    {
        InfoChanged = _info.Name != Name || _previousPortAsString != Ports;
    }
}