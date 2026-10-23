using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

/// <summary>
///     Runs socket operations with the <see cref="SocketAsyncEventArgs" /> pattern. Unlike the Task based socket
///     methods (e.g. <see cref="Socket.ConnectAsync(System.Net.EndPoint)" />), these report the result as
///     <see cref="SocketError" /> instead of throwing an exception for every refused connection, timeout or
///     unreachable host. A scan has thousands of these, and every exception is slow with a debugger attached
///     (the whole application lags). Used by <see cref="PortProbe" /> and <see cref="NetBIOSResolver" />.
/// </summary>
internal static class SocketOperation
{
    /// <summary>
    ///     Starts the operation and returns a task that completes with its <see cref="SocketError" />.
    ///     A pending operation is aborted with <see cref="SocketError.OperationAborted" /> by closing the socket
    ///     (or <see cref="Socket.CancelConnectAsync" /> for a connect).
    /// </summary>
    /// <param name="e">Arguments of the operation (remote end point, buffer). Used for one operation only.</param>
    /// <param name="start">
    ///     Method that starts the operation, e.g. <see cref="Socket.ConnectAsync(SocketAsyncEventArgs)" />. Returns
    ///     false if the operation completed synchronously.
    /// </param>
    /// <returns>Result of the operation, <see cref="SocketError.Success" /> if it succeeded.</returns>
    public static Task<SocketError> RunAsync(SocketAsyncEventArgs e, Func<SocketAsyncEventArgs, bool> start)
    {
        var taskCompletionSource =
            new TaskCompletionSource<SocketError>(TaskCreationOptions.RunContinuationsAsynchronously);

        e.Completed += (_, args) => taskCompletionSource.TrySetResult(args.SocketError);

        if (!start(e))
            taskCompletionSource.TrySetResult(e.SocketError);

        return taskCompletionSource.Task;
    }
}
