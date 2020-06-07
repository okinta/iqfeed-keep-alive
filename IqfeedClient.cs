using DnsClient.Protocol;
using DnsClient;
using System.Linq;
using System.Net.Sockets;
using System.Net;
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
        private const int ConnectionTimeout = 5000;
        private const int Sleep = 15000;
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
                    socket ??= await Connect(host, port, token);

                    if (!socket.Connected)
                        throw new SocketException((int)SocketError.NotConnected);

                    var message = await socket.GetMessage(ConnectionTimeout, token);
                    if (string.IsNullOrEmpty(message))
                        throw new SocketException((int)SocketError.NotConnected);

                    await ConsoleX.WriteLineAsync("Active", token);
                }
                catch (SocketException e)
                {
                    await ConsoleX.WriteErrorLineAsync(e.Message, token);

                    if (socket != null)
                    {
                        await socket.CloseAsync(token);
                        socket = null;
                    }
                }

                await Task.Delay(Sleep, token);
            }
        }

        /// <summary>
        /// Connects to IQFeed.
        /// </summary>
        /// <returns>The connected Socket instance.</returns>
        private static async Task<Socket> Connect(
            string host, int port, CancellationToken token)
        {
            IPEndPoint ipEndPoint;
            try
            {
                ipEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            }

            // If we are given a domain, perform a DNS lookup to find the host
            catch (FormatException)
            {
                var ip = await GetIpAddress(host, token);
                ipEndPoint = new IPEndPoint(ip, port);
            }

            var socket = new Socket(
                ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(host, port, ConnectionTimeout, token);
            await socket.SendAsync("S,CONNECT\r\n", ConnectionTimeout, token);
            return socket;
        }

        /// <summary>
        /// Performs a DNS query to resolve the given host to an IPAddress.
        /// </summary>
        /// <param name="host">The host to resolve.</param>
        /// <param name="token">The token to check for cancellation requests.</param>
        /// <returns>The resolved IPAddress of the <paramref name="host"/>.</returns>
        private static async Task<IPAddress> GetIpAddress(
            string host, CancellationToken token)
        {
            var lookupClient = new LookupClient();
            var result = await lookupClient.QueryAsync(
                new DnsQuestion(host, QueryType.A), token);

            // Pick a random record
            var record = result.AllRecords
                .OfType<AddressRecord>()
                .OrderBy(x => Guid.NewGuid())
                .First();

            return record.Address;
        }
    }
}
