using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
///     View model of the dialog to add / edit a service type profile of the mDNS / DNS-SD tool.
/// </summary>
public class MDNSServiceTypeProfileViewModel : ViewModelBase
{
    private readonly MDNSServiceTypeProfileInfo _info;
    private readonly bool _isLoading;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MDNSServiceTypeProfileViewModel" /> class.
    /// </summary>
    /// <param name="saveCommand">The action to execute when saving.</param>
    /// <param name="cancelHandler">The action to execute when canceling.</param>
    /// <param name="info">The service type profile to edit, null to add one.</param>
    public MDNSServiceTypeProfileViewModel(Action<MDNSServiceTypeProfileViewModel> saveCommand,
        Action<MDNSServiceTypeProfileViewModel> cancelHandler, MDNSServiceTypeProfileInfo info = null)
    {
        _isLoading = true;

        SaveCommand = new RelayCommand(_ => saveCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        _info = info ?? new MDNSServiceTypeProfileInfo();

        // Names of the other profiles - a name is used in the service type input, it must be unique
        UsedNames = [.. SettingsManager.Current.MDNSDiscovery_ServiceTypeProfiles
            .Where(x => !ReferenceEquals(x, info)).Select(x => x.Name)];

        Name = _info.Name;
        ServiceTypes = _info.ServiceTypes;

        _isLoading = false;
    }

    /// <summary>
    ///     Gets the names of the other service type profiles (the name must be unique).
    /// </summary>
    public List<string> UsedNames { get; }

    /// <summary>
    ///     Gets the command to save the profile.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    ///     Gets the command to cancel the operation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    ///     Gets or sets the name of the profile.
    /// </summary>
    public string Name
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the service types of the profile, separated by ";".
    /// </summary>
    public string ServiceTypes
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            if (!_isLoading)
                Validate();

            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the profile has changed (save is enabled).
    /// </summary>
    public bool InfoChanged
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    private void Validate()
    {
        InfoChanged = _info.Name != Name || _info.ServiceTypes != ServiceTypes;
    }
}
