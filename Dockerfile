FROM mcr.microsoft.com/dotnet/core/sdk:5.0-alpine

COPY iqfeed-keep-alive.csproj /app/iqfeed-keep-alive.csproj
RUN dotnet restore /app/iqfeed-keep-alive.csproj

COPY . /app
RUN dotnet build -c Release /app/iqfeed-keep-alive.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:5.0-alpine

COPY --from=0 /app/bin/Release/net5.0 /app
RUN ln -s /app/iqfeed-keep-alive /usr/local/bin/iqfeed-keep-alive

CMD ["iqfeed-keep-alive", "--help"]
