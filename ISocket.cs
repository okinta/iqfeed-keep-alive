using System.Threading.Tasks;

namespace IqfeedKeepAlive
{
    /// <summary>
    /// An interface for describing methods to communicate via a socket.
    /// </summary>
    internal interface ISocket
    {
        public Task ConnectAsync(string host, int port);
        public void Close();
    }
}
