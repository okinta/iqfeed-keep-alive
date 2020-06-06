using IqfeedKeepAlive;
using System.Threading.Tasks;

namespace tests
{
    internal class MySocket : ISocket
    {
        public async Task ConnectAsync(string host, int port)
        {
            await Task.Delay(10000);
        }

        public void Close()
        {
        }
    }
}
