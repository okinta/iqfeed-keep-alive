using System.Net.Sockets;
using System.Threading.Tasks;

namespace IqfeedKeepAlive
{
    /// <summary>
    /// Wraps a Socket class to be usable as an ISocket interface.
    /// </summary>
    internal class SocketExtension : ISocket
    {
        private Socket Socket { get; }

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="socket">The Socket instance to wrap.</param>
        public SocketExtension(Socket socket)
        {
            Socket = socket;
        }

        public Task ConnectAsync(string host, int port)
        {
            return Socket.ConnectAsync(host, port);
        }

        public void Close()
        {
            Socket.Close();
        }
    }
}
