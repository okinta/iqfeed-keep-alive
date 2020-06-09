using IqfeedKeepAlive;
using System.Threading.Tasks;

namespace tests
{
    internal class MySocket : ISocket
    {
        public bool Closed { get; private set; }

        public async Task ConnectAsync(string host, int port)
        {
            await Task.Delay(10000);
        }

        public void Close()
        {
            Closed = true;
        }
    }
}
