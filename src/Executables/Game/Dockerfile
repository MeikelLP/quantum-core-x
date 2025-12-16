FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine
COPY . /src
WORKDIR /src/Executables/Game
RUN dotnet publish -c Release -o /src/bin/core -r linux-musl-x64 --no-self-contained -p:PublishSingleFile=true -p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine
RUN apk add --no-cache icu-libs icu-data-full # install korean locale
COPY --from=0 /src/bin/core /app
WORKDIR /app
ENTRYPOINT ["/app/QuantumCore.Game"]
