﻿using System.Net.Sockets;
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
        private int Port { get; }
        private IPAddress Host { get; }
        private IPEndPoint IpEndPoint { get; }
        private Socket Socket { get; set; }

        /// <summary>
        /// Instantiates the instance.
        /// </summary>
        /// <param name="host">The IQFeed host to connect to.</param>
        /// <param name="port">The IQFeed port to connect to.</param>
        public IqfeedClient(string host, int port)
        {
            Host = IPAddress.Parse(host);
            IpEndPoint = new IPEndPoint(Host, Port);
            Port = port;
        }

        /// <summary>
        /// Runs indefinitely. Connects to IQFeed and continuously sends a connect message.
        /// </summary>
        public void Run()
        {
            while (true)
            {
                try
                {
                    var bytes = new byte[256];
                    Socket.Receive(bytes);
                    var message = Encoding.ASCII.GetString(bytes);

                    if (message.Contains("Not Connected"))
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        Socket = new Socket(IpEndPoint.AddressFamily,
                            SocketType.Stream, ProtocolType.Tcp);
                        Socket.Connect(Host, Port);
                        var bytes = Encoding.ASCII.GetBytes("S,CONNECT\r\n");
                        Socket.Send(bytes);
                    }
                    catch (Exception)
                    {
                    }
                }

                Thread.Sleep(3000);
            }
        }
    }
}