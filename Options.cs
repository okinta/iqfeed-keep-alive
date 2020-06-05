using CommandLine;

namespace IqfeedKeepAlive
{
    /// <summary>
    /// Command line options.
    /// </summary>
    internal class Options
    {
        [Option('h', "host", Default = "127.0.0.1")]
        public string Host { get; set; }

        [Option('x', "port", Default = 9300)]
        public int Port { get; set; }
    }
}
