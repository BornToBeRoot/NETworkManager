using NETworkManager.Localization.Resources;
using NETworkManager.Models.HostsFileEditor;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for adding or editing a hosts file entry (or bulk editing multiple entries)
/// in the HostsFileEditor dialog.
/// </summary>
public class HostsFileEditorEntryViewModel : ViewModelBase
{
    /// <summary>
    /// Indicates whether the view model is loading (initial values are being set).
    /// </summary>
    private readonly bool _isLoading;

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
        _isLoading = true;

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

        _isLoading = false;
    }

    /// <summary>
    /// Creates a new instance of <see cref="HostsFileEditorEntryViewModel" /> for bulk editing
    /// multiple hosts file entries. Only the comment can be changed in this mode.
    /// </summary>
    /// <param name="okCommand">OK command to save the entries.</param>
    /// <param name="cancelHandler">Cancel command to discard the changes.</param>
    /// <param name="entries">Entries to edit.</param>
    public HostsFileEditorEntryViewModel(Action<HostsFileEditorEntryViewModel> okCommand,
        Action<HostsFileEditorEntryViewModel> cancelHandler, IReadOnlyList<HostsFileEntry> entries)
    {
        _isLoading = true;

        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        Entries = entries;
        IsBulkEdit = true;

        var comments = entries.Select(e => e.Comment ?? string.Empty).Distinct().ToList();

        // Prefill the comment if all entries share the same one, otherwise show a "multiple values" hint
        if (comments.Count == 1)
            Comment = comments[0];
        else
            IsCommentMixed = true;

        _isLoading = false;
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
    /// Gets the hosts file entries (bulk edit).
    /// </summary>
    public IReadOnlyList<HostsFileEntry> Entries { get; } = [];

    /// <summary>
    /// Indicates whether multiple entries are edited at once.
    /// </summary>
    public bool IsBulkEdit { get; }

    /// <summary>
    /// Indicates whether the selected entries have different comments (bulk edit).
    /// </summary>
    public bool IsCommentMixed { get; }

    /// <summary>
    /// Indicates whether the comment was changed by the user.
    /// </summary>
    public bool IsCommentChanged
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CommentWatermark));
        }
    }

    /// <summary>
    /// Watermark of the comment text box. Shows a "multiple values" hint as long as the
    /// selected entries have different comments and the user did not change the comment.
    /// </summary>
    public string CommentWatermark => IsCommentMixed && !IsCommentChanged
        ? Strings.MultipleValues
        : Strings.ExampleHostsFileEntryComment;

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

            if (!_isLoading)
                IsCommentChanged = true;

            OnPropertyChanged();
        }
    }
}
