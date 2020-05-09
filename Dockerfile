FROM mcr.microsoft.com/dotnet/core/sdk:5.0-alpine

COPY . /app

RUN dotnet build -c Release /app/IQFeedKeepAlive.csproj

FROM mcr.microsoft.com/dotnet/core/runtime:5.0-alpine

COPY --from=0 /app/bin/Release/net5.0 /app
RUN ln -s /app/IQFeedKeepAlive /usr/local/bin/IQFeedKeepAlive

CMD ["IQFeedKeepAlive", "--help"]
