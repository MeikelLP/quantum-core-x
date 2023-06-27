FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine
COPY . /src
WORKDIR /src/Executables/Auth
RUN dotnet publish -c Release -o /src/bin/core -r linux-musl-x64 --no-self-contained -p:PublishSingleFile=true -p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine
COPY --from=0 /src/bin/core /app
WORKDIR /app
ENTRYPOINT ["/app/QuantumCore.Auth"]
