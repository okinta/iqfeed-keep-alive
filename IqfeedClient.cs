using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace IqfeedKeepAlive
{
    /// <summary>
    /// Defines ability to maintain a connection to IQFeed.
    /// </summary>
    internal class IqfeedClient : IDisposable
    {
        private CancellationTokenSource Token { get; }
        private const int Sleep = 30000;
        private Task Task { get; }

        /// <summary>
        /// Establishes a connection to IQFeed and continuously maintains it.
        /// </summary>
        /// <param name="host">The IQFeed host to connect to.</param>
        /// <param name="port">The IQFeed port to connect to.</param>
        public IqfeedClient(string host, int port)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException(
                    nameof(host), "host must be provided");

            if (port < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(port), "port must be larger than 1");

            Token = new CancellationTokenSource();

            // Start maintaining the connection to IQFeed in a background thread
            Task = Task.Run(() => Run(host, port, Token.Token));
        }

        /// <summary>
        /// Closes a connection to IQFeed.
        /// </summary>
        public void Dispose()
        {
            Token.Cancel();

            try
            {
                Task.GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
            }
        }

        private static async Task Run(string host, int port, CancellationToken token)
        {
            Socket socket = null;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (socket is null)
                    {
                        socket = Connect(host, port);
                        await ConsoleX.WriteLineAsync("Connected", token);
                    }

                    if (!socket.Connected)
                        throw new SocketException((int)SocketError.NotConnected);

                    var _ = await SendConnect(socket, host, port, token);

                    var message = await GetMessage(socket, token);
                    if (message.Contains("Not Connected"))
                    {
                        await ConsoleX.WriteLineAsync("Not connected", token);
                        socket = null;
                    }
                    else
                        await ConsoleX.WriteLineAsync("Active", token);

                }
                catch (SocketException e)
                {
                    await ConsoleX.WriteErrorLineAsync(e.Message, token);
                    socket = null;
                }

                await Task.Delay(Sleep, token);
            }
        }

        /// <summary>
        /// Connects to IQFeed.
        /// </summary>
        /// <returns>The connected Socket instance.</returns>
        private static Socket Connect(string host, int port)
        {
            IPEndPoint ipEndPoint;
            try
            {
                ipEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            }

            // If we are given a domain, perform a DNS lookup to find the host
            catch (FormatException)
            {
                ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(host)
                    .OrderBy(x => Guid.NewGuid())
                    .First(), port);
            }

            return new Socket(ipEndPoint.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
        }

        private static async Task<ValueTask<int>> SendConnect(
            Socket socket, string host, int port, CancellationToken token)
        {
            await socket.ConnectAsync(host, port);
            var bytes = Encoding.ASCII.GetBytes("S,CONNECT\r\n");
            return socket.SendAsync(bytes, SocketFlags.None, token);
        }

        private static async Task<string> GetMessage(
            Socket socket, CancellationToken token)
        {
            var bytes = new byte[256];
            await socket.ReceiveAsync(bytes, SocketFlags.None, token);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
