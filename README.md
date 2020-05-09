# IQFeed Keep Alive

Keeps IQFeed running by opening and maintaining a continuous connection.

Simply run the program then start IQFeed. Run `IQFeedKeepAlive.exe --help` for
more options.

## Development

Building can be conducted via Visual Studio or via a container. To build the
container, run:

    docker build -t okinta/iqfeed-keep-alive .
