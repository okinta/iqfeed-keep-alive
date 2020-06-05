using CommandLine;
using Nito.AsyncEx;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace IqfeedKeepAlive
{
    /// <summary>
    /// Runs the program.
    /// </summary>
    internal class Program
    {
        private readonly AsyncManualResetEvent _exitEvent = new AsyncManualResetEvent();
        private readonly CancellationTokenSource _cancelToken =
            new CancellationTokenSource();

        /// <summary>
        /// Instantiates the instance. Creates event handler for ctrl+c.
        /// </summary>
        public Program()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
        }

        /// <summary>
        /// Entrypoint to start the program.
        /// </summary>
        /// <param name="args">The arguments passed via the command line.</param>
        public static void Main(string[] args)
        {
            var program = new Program();
            Parser.Default.ParseArguments<Options>(args).WithParsedAsync(program.Run);
        }

        /// <summary>
        /// Maintains a connection to IQFeed continuously in the background. Waits until
        /// a signal is received to exit the program.
        /// </summary>
        /// <param name="opts">The parsed command line options.</param>
        private async Task Run(Options opts)
        {
            using var _ = new IqfeedClient(opts.Host, opts.Port);
            await _exitEvent.WaitAsync(_cancelToken.Token);
        }

        /// <summary>
        /// Called when ctrl+c is pressed. Signal that we should exit the program.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs eventArgs)
        {
            Console.CancelKeyPress -= OnCancelKeyPress;
            eventArgs.Cancel = true;
            _exitEvent.Set();
            _cancelToken.Cancel();
        }
    }
}
