FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine
RUN wget https://github.com/kaitai-io/kaitai_struct_compiler/releases/download/0.9/kaitai-struct-compiler-0.9.zip
RUN unzip kaitai-struct-compiler-0.9.zip
ENV PATH="/kaitai-struct-compiler-0.9/bin/:${PATH}"
RUN apk add --no-cache bash openjdk8-jre
COPY . /src
WORKDIR /src/Executables/Game
RUN sh generate_kaitai.sh
RUN dotnet publish -c Release -o /src/bin/core -r linux-musl-x64 --no-self-contained -p:PublishSingleFile=true -p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
COPY --from=0 /src/bin/core /app
WORKDIR /app
ENTRYPOINT ["/app/QuantumCore.Game"]
