FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine
RUN wget https://github.com/kaitai-io/kaitai_struct_compiler/releases/download/0.9/kaitai-struct-compiler-0.9.zip
RUN unzip kaitai-struct-compiler-0.9.zip
ENV PATH="/kaitai-struct-compiler-0.9/bin/:${PATH}"
RUN apk add --no-cache bash openjdk8-jre
COPY . /app
WORKDIR /app
RUN ./generate_kaitai.sh
RUN dotnet publish Core -c Release -o bin/core

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine
COPY --from=0 /app/bin/core /app
RUN mkdir /core
WORKDIR /core
ENTRYPOINT ["/app/Core"]
