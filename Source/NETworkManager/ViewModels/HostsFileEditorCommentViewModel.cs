using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for editing the comment of one or more hosts file entries.
/// </summary>
public class HostsFileEditorCommentViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HostsFileEditorCommentViewModel"/> class.
    /// </summary>
    /// <param name="okCommand">Action executed on confirmation.</param>
    /// <param name="cancelHandler">Action executed on cancellation.</param>
    /// <param name="initialComment">Initial comment value.</param>
    public HostsFileEditorCommentViewModel(Action<HostsFileEditorCommentViewModel> okCommand,
        Action<HostsFileEditorCommentViewModel> cancelHandler, string initialComment = "")
    {
        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));
        Comment = initialComment;
    }

    /// <summary>
    /// OK command to confirm the comment change.
    /// </summary>
    public ICommand OKCommand { get; }

    /// <summary>
    /// Cancel command to discard changes.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets or sets the comment.
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
