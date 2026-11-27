using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model of the dialog to add or edit a target of the latency widget (Settings > Dashboard > Latency).
/// </summary>
public class LatencyTargetDialogViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LatencyTargetDialogViewModel" /> class.
    /// </summary>
    /// <param name="saveCommand">Saves the target.</param>
    /// <param name="cancelHandler">Closes the dialog.</param>
    /// <param name="target">Target to edit, <c>null</c> to add one.</param>
    public LatencyTargetDialogViewModel(Action<LatencyTargetDialogViewModel> saveCommand,
        Action<LatencyTargetDialogViewModel> cancelHandler, LatencyTargetInfo target = null)
    {
        SaveCommand = new RelayCommand(_ => saveCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        Name = target?.Name;
        Host = target?.Host;
        IsGateway = target?.IsGateway ?? false;
    }

    /// <summary>
    /// Gets the command to save the target.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Gets the command to close the dialog.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets or sets the name shown in the widget (empty: the host).
    /// </summary>
    public string Name
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the IP address or host name (not used for the gateway).
    /// </summary>
    public string Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the default gateway is pinged instead of a host.
    /// </summary>
    public bool IsGateway
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Target of the dialog (the host is not saved for the gateway).
    /// </summary>
    public LatencyTargetInfo ToTarget()
    {
        return new LatencyTargetInfo(Name?.Trim(), IsGateway ? null : Host?.Trim(), IsGateway);
    }
}
