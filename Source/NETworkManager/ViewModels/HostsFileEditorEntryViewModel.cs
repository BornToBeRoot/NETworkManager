using NETworkManager.Models.HostsFileEditor;
using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for adding or editing a hosts file entry in the HostsFileEditor dialog.
/// </summary>
public class HostsFileEditorEntryViewModel : ViewModelBase
{
    /// <summary>
    /// Creates a new instance of <see cref="HostsFileEditorEntryViewModel" /> for adding or
    /// editing a hosts file entry.
    /// </summary>
    /// <param name="okCommand">OK command to save the entry.</param>
    /// <param name="cancelHandler">Cancel command to discard the entry.</param>
    /// <param name="entry">Entry to edit, if null a new entry will be created.</param>
    public HostsFileEditorEntryViewModel(Action<HostsFileEditorEntryViewModel> okCommand,
        Action<HostsFileEditorEntryViewModel> cancelHandler, HostsFileEntry entry = null)
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        Entry = entry;

        // Add entry
        if (Entry == null)
        {
            IsEnabled = true;
        }
        // Edit entry
        else
        {
            IsEnabled = entry.IsEnabled;
            IPAddress = entry.IPAddress;
            Hostname = entry.Hostname;
            Comment = entry.Comment;
        }
    }

    /// <summary>
    /// OK command to save the entry.
    /// </summary>
    public ICommand OKCommand { get; }

    /// <summary>
    /// Cancel command to discard the entry.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets the hosts file entry.
    /// </summary>
    public HostsFileEntry Entry { get; } = null;

    /// <summary>
    /// Indicates whether the entry is enabled or not.
    /// </summary>
    public bool IsEnabled
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
    /// IP address of the host.
    /// </summary>
    public string IPAddress
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
    /// Host name(s) of the host. Multiple host names are separated by a
    /// space (equal to the hosts file format).
    /// </summary>
    public string Hostname
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
    /// Comment of the host.
    /// </summary>
    public string Comment
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
}