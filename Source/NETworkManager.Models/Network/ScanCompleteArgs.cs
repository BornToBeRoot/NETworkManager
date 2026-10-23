using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Event arguments of the <c>ScanComplete</c> event of <see cref="IPScanner" /> and <see cref="PortScanner" />.
///     The event is raised exactly once per scan, also when the scan was canceled or failed.
/// </summary>
/// <param name="isCanceled">True if the scan was canceled by the user.</param>
/// <param name="exception">The exception that stopped the scan, or <see langword="null" />.</param>
public class ScanCompleteArgs(bool isCanceled, Exception exception = null) : EventArgs
{
    /// <summary>
    ///     Gets a value indicating whether the scan was canceled by the user.
    /// </summary>
    public bool IsCanceled { get; } = isCanceled;

    /// <summary>
    ///     Gets the exception that stopped the scan, or <see langword="null" /> if the scan completed or was canceled.
    /// </summary>
    public Exception Exception { get; } = exception;
}
