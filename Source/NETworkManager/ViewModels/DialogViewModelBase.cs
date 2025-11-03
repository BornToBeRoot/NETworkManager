using System;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// Base class for dialog ViewModels that have OK and Cancel commands.
/// </summary>
/// <typeparam name="T">The type of the derived ViewModel.</typeparam>
public abstract class DialogViewModelBase<T> : ViewModelBase where T : DialogViewModelBase<T>
{
    /// <summary>
    /// Command executed when OK button is clicked.
    /// </summary>
    public ICommand OKCommand { get; }

    /// <summary>
    /// Command executed when Cancel button is clicked.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogViewModelBase{T}"/> class.
    /// </summary>
    /// <param name="okCommand">Action to execute when OK is clicked.</param>
    /// <param name="cancelHandler">Action to execute when Cancel is clicked.</param>
    protected DialogViewModelBase(Action<T> okCommand, Action<T> cancelHandler)
    {
        OKCommand = new RelayCommand(_ => okCommand((T)this));
        CancelCommand = new RelayCommand(_ => cancelHandler((T)this));
    }
}
