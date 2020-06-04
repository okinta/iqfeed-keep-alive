using CommandLine;

namespace IqfeedKeepAlive
{
    /// <summary>
    /// Runs the program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Command line options.
        /// </summary>
        public class Options
        {
            [Option('h', "host", Default = "127.0.0.1")]
            public string Host { get; set; }

            [Option('x', "port", Default = 9300)]
            public int Port { get; set; }
        }

        /// <summary>
        /// Entrypoint to start the program.
        /// </summary>
        /// <param name="args">The arguments passed via the command line.</param>
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
        }

        private static void Run(Options opts)
        {
            var client = new IqfeedClient(opts.Host, opts.Port);
            client.Run();
        }
    }
}
