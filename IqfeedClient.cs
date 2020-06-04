using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System;

namespace IqfeedKeepAlive
{
    /// <summary>
    /// Defines ability to maintain a connection to IQFeed.
    /// </summary>
    internal class IqfeedClient
    {
        private const int Sleep = 3000;
        private int Port { get; }
        private string Host { get; }
        private Socket Socket { get; set; }

        /// <summary>
        /// Instantiates the instance.
        /// </summary>
        /// <param name="host">The IQFeed host to connect to.</param>
        /// <param name="port">The IQFeed port to connect to.</param>
        public IqfeedClient(string host, int port)
        {
            Host = host;
            Port = port;
        }

        /// <summary>
        /// Runs indefinitely. Connects to IQFeed and continuously sends a connect message.
        /// </summary>
        public void Run()
        {
            var connected = false;
            while (true)
            {
                try
                {
                    if (!connected)
                    {
                        Connect();
                        Console.WriteLine("Connected");
                        connected = true;
                    }

                    var bytes = new byte[256];
                    Socket.Receive(bytes);
                    var message = Encoding.ASCII.GetString(bytes);

                    if (message.Contains("Not Connected"))
                    {
                        Console.WriteLine("Not connected");
                        connected = false;
                    }
                    else
                        Console.WriteLine("Active");
                }
                catch (SocketException e)
                {
                    Console.Error.WriteLine(e.Message);
                }

                Thread.Sleep(Sleep);
            }
        }

        /// <summary>
        /// Connects to IQFeed.
        /// </summary>
        private void Connect()
        {
            IPEndPoint ipEndPoint;
            try
            {
                ipEndPoint = new IPEndPoint(IPAddress.Parse(Host), Port);
            }

            // If we are given a domain, perform a DNS lookup to find the host
            catch (FormatException)
            {
                ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(Host)
                    .OrderBy(x => Guid.NewGuid())
                    .First(), Port);
            }

            Socket = new Socket(ipEndPoint.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            Socket.Connect(Host, Port);
            var bytes = Encoding.ASCII.GetBytes("S,CONNECT\r\n");
            Socket.Send(bytes);
        }
    }
}
