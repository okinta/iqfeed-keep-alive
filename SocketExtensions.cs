using Nito.AsyncEx;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;

[assembly: InternalsVisibleTo("tests")]

namespace IqfeedKeepAlive
{
    /// <summary>
    /// Extends the Socket class.
    /// </summary>
    internal static class SocketExtensions
    {
        private delegate Task ConnectAsyncDelegate(string host, int port);

        /// <summary>
        /// Establishes a connection to a remote host. The host is specified by a host name
        /// and a port number. Allows the operation to be cancelled via a
        /// CancellationToken.
        /// </summary>
        /// <param name="socket">The socket to perform the connect operation on.</param>
        /// <param name="host">The name of the remote host.</param>
        /// <param name="port">The port number of the remote host.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <exception cref="OperationCanceledException">If the task is cancelled before
        /// completion.</exception>
        public static Task ConnectAsync(
            this ISocket socket, string host, int port, CancellationToken token)
        {
            return ConnectAsync(socket.ConnectAsync, host, port, token);
        }

        /// <summary>
        /// Establishes a connection to a remote host. The host is specified by a host name
        /// and a port number. Allows the operation to be cancelled via a
        /// CancellationToken.
        /// </summary>
        /// <param name="socket">The socket to perform the connect operation on.</param>
        /// <param name="host">The name of the remote host.</param>
        /// <param name="port">The port number of the remote host.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <exception cref="OperationCanceledException">If the task is cancelled before
        /// completion.</exception>
        public static Task ConnectAsync(
            this Socket socket, string host, int port, CancellationToken token)
        {
            return ConnectAsync(socket.ConnectAsync, host, port, token);
        }

        /// <summary>
        /// Establishes a connection to a remote host. The host is specified by a host name
        /// and a port number. Allows the operation to be cancelled via a
        /// CancellationToken. Times out after <paramref name="millisecondsTimeout"/>.
        /// </summary>
        /// <param name="socket">The socket to perform the connect operation on.</param>
        /// <param name="host">The name of the remote host.</param>
        /// <param name="port">The port number of the remote host.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait before
        /// timing out the request.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <exception cref="OperationCanceledException">If the task is cancelled before
        /// completion.</exception>
        /// <exception cref="SocketException">If the connection times out after
        /// <paramref name="millisecondsTimeout"/>.</exception>
        public static Task ConnectAsync(
            this ISocket socket, string host, int port,
            int millisecondsTimeout, CancellationToken token = default)
        {
            return TimeoutAsync(
                t => socket.ConnectAsync(host, port, t),
                millisecondsTimeout, token);
        }

        /// <summary>
        /// Establishes a connection to a remote host. The host is specified by a host name
        /// and a port number. Allows the operation to be cancelled via a
        /// CancellationToken. Times out after <paramref name="millisecondsTimeout"/>.
        /// </summary>
        /// <param name="socket">The socket to perform the connect operation on.</param>
        /// <param name="host">The name of the remote host.</param>
        /// <param name="port">The port number of the remote host.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait before
        /// timing out the request.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <exception cref="OperationCanceledException">If the task is cancelled before
        /// completion.</exception>
        /// <exception cref="SocketException">If the connection times out after
        /// <paramref name="millisecondsTimeout"/>.</exception>
        public static Task ConnectAsync(
            this Socket socket, string host, int port,
            int millisecondsTimeout, CancellationToken token = default)
        {
            return TimeoutAsync(
                t => socket.ConnectAsync(host, port, t),
                millisecondsTimeout, token);
        }

        /// <summary>
        /// Asynchronously closes the Socket connection and releases all associated
        /// resources.
        /// </summary>
        /// <param name="socket">The Socket to close.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <exception cref="OperationCanceledException">If the task is cancelled before
        /// completion.</exception>
        public static async Task CloseAsync(
            this Socket socket, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var wait = new AsyncManualResetEvent();
            var args = new SocketAsyncEventArgs();
            args.Completed += (sender, eventArgs) => wait.Set();
            if (socket.DisconnectAsync(args))
                await wait.WaitAsync(token);

            token.ThrowIfCancellationRequested();
            socket.Close();
        }

        /// <summary>
        /// Sends a message via the socket. Allows the operation to be cancelled via a
        /// CancellationToken.
        /// </summary>
        /// <param name="socket">The Socket instance to send a message with.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <returns>An asynchronous task that completes with number of bytes sent to
        /// the socket if the operation was successful. Otherwise, the task will
        /// complete with an invalid socket error.</returns>
        public static ValueTask<int> SendAsync(
            this Socket socket, string message, CancellationToken token = default)
        {
            var bytes = Encoding.ASCII.GetBytes(message);
            return socket.SendAsync(bytes, SocketFlags.None, token);
        }

        /// <summary>
        /// Sends a message via the socket. Allows the operation to be cancelled via a
        /// CancellationToken. Times out after <paramref name="millisecondsTimeout"/>.
        /// </summary>
        /// <param name="socket">The Socket instance to send a message with.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait before
        /// timing out the request.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <returns>An asynchronous task that completes with number of bytes sent to
        /// the socket if the operation was successful. Otherwise, the task will
        /// complete with an invalid socket error.</returns>
        public static async Task<int> SendAsync(
            this Socket socket, string message, int millisecondsTimeout,
            CancellationToken token = default)
        {
            return await TimeoutAsync(
                async t => await socket.SendAsync(message, t),
                millisecondsTimeout, token);
        }

        /// <summary>
        /// Gets a message from the socket. Allows the operation to be cancelled via a
        /// CancellationToken.
        /// </summary>
        /// <param name="socket">The Socket instance to retrieve a message from.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <returns>The retrieved message from the socket.</returns>
        public static async Task<string> GetMessage(
            this Socket socket, CancellationToken token)
        {
            var bytes = new byte[256];
            await socket.ReceiveAsync(bytes, SocketFlags.None, token);
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Gets a message from the socket. Allows the operation to be cancelled via a
        /// CancellationToken. Times out after <paramref name="millisecondsTimeout"/>.
        /// </summary>
        /// <param name="socket">The Socket instance to retrieve a message from.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait before
        /// timing out the request.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <returns>The retrieved message from the socket.</returns>
        public static async Task<string> GetMessage(
            this Socket socket, int millisecondsTimeout, CancellationToken token)
        {
            return await TimeoutAsync(socket.GetMessage, millisecondsTimeout, token);
        }

        private static Task ConnectAsync(
            this ConnectAsyncDelegate connectAsync, string host, int port,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return connectAsync(host, port).WaitAsync(token);
        }

        /// <summary>
        /// Runs a function asyncronously, and times it out after
        /// <paramref name="millisecondsTimeout"/>.
        /// </summary>
        /// <param name="action">The function to call.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait before
        /// timing out the function.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <returns>The result from the function.</returns>
        private static async Task TimeoutAsync(
            Func<CancellationToken, Task> action, int millisecondsTimeout,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeoutToken.CancelAfter(millisecondsTimeout);

            try
            {
                await action(timeoutToken.Token);
            }
            catch (OperationCanceledException) when (timeoutToken.IsCancellationRequested)
            {
                throw new SocketException((int)SocketError.TimedOut);
            }
        }

        /// <summary>
        /// Runs a function asyncronously, and times it out after
        /// <paramref name="millisecondsTimeout"/>.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="action">The function to call.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait before
        /// timing out the function.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <returns>The result from the function.</returns>
        private static async Task<T> TimeoutAsync<T>(
            Func<CancellationToken, Task<T>> action, int millisecondsTimeout,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeoutToken.CancelAfter(millisecondsTimeout);

            try
            {
                return await action(timeoutToken.Token);
            }
            catch (OperationCanceledException) when (timeoutToken.IsCancellationRequested)
            {
                throw new SocketException((int) SocketError.TimedOut);
            }
        }
    }
}
