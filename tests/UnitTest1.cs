using IqfeedKeepAlive;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace tests
{
    public class UnitTest1
    {
        private const int Port = 9000;
        private const string Host = "127.0.0.1";

        [Fact]
        public async Task TestConnect()
        {
            using var _ = new SocketServer(Host, Port);
            var socket = new Socket(
                IPAddress.Parse(Host).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(Host, Port, 5000);
            await socket.CloseAsync();
        }
    }
}
