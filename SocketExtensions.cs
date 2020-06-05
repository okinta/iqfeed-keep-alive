using Nito.AsyncEx;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Text;

namespace IqfeedKeepAlive
{
    /// <summary>
    /// Extends the Socket class.
    /// </summary>
    internal static class SocketExtensions
    {
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
        public static async Task ConnectAsync(
            this Socket socket, string host, int port, CancellationToken token)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            token.ThrowIfCancellationRequested();
            await socket.ConnectAsync(host, port).WaitAsync(token);
        }

        /// <summary>
        /// Establishes a connection to a remote host. The host is specified by a host name
        /// and a port number. Allows the operation to be cancelled via a
        /// CancellationToken. Times out after <paramref name="millisecondsDelay"/>.
        /// </summary>
        /// <param name="socket">The socket to perform the connect operation on.</param>
        /// <param name="host">The name of the remote host.</param>
        /// <param name="port">The port number of the remote host.</param>
        /// <param name="millisecondsDelay">The number of milliseconds to wait before
        /// timing out the request.</param>
        /// <param name="token">The token to check for cancellation.</param>
        /// <exception cref="OperationCanceledException">If the task is cancelled before
        /// completion.</exception>
        /// <exception cref="SocketException">If the connection times out after
        /// <paramref name="millisecondsDelay"/>.</exception>
        public static async Task ConnectAsync(
            this Socket socket, string host, int port,
            int millisecondsDelay, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeoutToken.CancelAfter(millisecondsDelay);

            try
            {
                await socket.ConnectAsync(host, port, timeoutToken.Token);
            }
            catch (OperationCanceledException) when (timeoutToken.IsCancellationRequested)
            {
                throw new SocketException((int)SocketError.TimedOut);
            }
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
        /// Sends a message via the socket.
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
        /// Gets a message from the socket.
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
    }
}
