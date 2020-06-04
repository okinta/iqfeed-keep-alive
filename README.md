# IQFeed Keep Alive

Keeps IQFeed running by opening and maintaining a continuous connection.

Simply run the program then start IQFeed. Run `iqfeed-keep-alive.exe --help`
for more options.

## Development

Building can be conducted via Visual Studio or via a container. To build the
container, run:

    docker build -t okinta/iqfeed-keep-alive .

## Container Usage

To run via a container, you will first need to establish port forwarding in
Windows. At a command line, run:

    netsh interface portproxy add v4tov4 listenport=9300 listenaddress=[IP] connectaddress=127.0.0.1

Replace `[IP]` with the network address used by docker (can be found by running
`ipconfig` and looking for the adapter associated with either Docker or WSL).

Once the port forwarding is in place, run:

    docker run okinta/iqfeed-keep-alive iqfeed-keep-alive -h $(docker run alpine getent hosts host.docker.internal | cut -d' ' -f1)
