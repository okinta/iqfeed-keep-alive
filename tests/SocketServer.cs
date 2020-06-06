using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System;
using Nito.AsyncEx;

namespace tests
{
    internal class SocketServer : IDisposable
    {
        private CancellationToken Token => TokenSource.Token;
        private CancellationTokenSource TokenSource { get; } =
            new CancellationTokenSource();
        private IList<TcpClient> Clients { get; } = new List<TcpClient>();
        private Task Task { get; }
        private TcpListener Listener { get; }

        public SocketServer(string host, int port)
        {
            Listener = new TcpListener(IPAddress.Parse(host), port);
            Task = Task.Run(Run);
        }

        private async Task Run()
        {
            Listener.Start();

            while (!Token.IsCancellationRequested)
            {
                try
                {
                    var client = await Listener.AcceptTcpClientAsync().WaitAsync(Token);
                    Clients.Add(client);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
        }

        public void Dispose()
        {
            TokenSource.Cancel();
            foreach (var client in Clients)
                client.Close();

            Listener.Stop();

            try
            {
                Task.GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
