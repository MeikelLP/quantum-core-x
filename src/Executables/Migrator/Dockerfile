FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine
COPY . /src
WORKDIR /src/Executables/Migrator
RUN dotnet publish -c Release -o /src/bin/core -r linux-musl-x64 --no-self-contained -p:PublishSingleFile=true -p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
COPY --from=0 /src/bin/core /app
WORKDIR /app
ENTRYPOINT ["/app/QuantumCore.Migrator"]
