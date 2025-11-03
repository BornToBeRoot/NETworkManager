using System;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// Base class for connect dialog ViewModels that have Connect and Cancel commands.
/// </summary>
/// <typeparam name="T">The type of the derived ViewModel.</typeparam>
public abstract class ConnectDialogViewModelBase<T> : ViewModelBase where T : ConnectDialogViewModelBase<T>
{
    /// <summary>
    /// Command executed when Connect button is clicked.
    /// </summary>
    public ICommand ConnectCommand { get; }

    /// <summary>
    /// Command executed when Cancel button is clicked.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectDialogViewModelBase{T}"/> class.
    /// </summary>
    /// <param name="connectCommand">Action to execute when Connect is clicked.</param>
    /// <param name="cancelHandler">Action to execute when Cancel is clicked.</param>
    protected ConnectDialogViewModelBase(Action<T> connectCommand, Action<T> cancelHandler)
    {
        ConnectCommand = new RelayCommand(_ => connectCommand((T)this));
        CancelCommand = new RelayCommand(_ => cancelHandler((T)this));
    }
}
