# Developer Guide

* _Server_ refers to Quantum Core X
* _Client_ refers to the TMP4 client

## Prerequisites

> [!note]
> The client has been tested to run well on Linux using Proton. However, that setup is out of scope of this guide.
> The guide assumes you are using Windows

* [.NET SDK 10](https://dotnet.microsoft.com/en-us/download)
* A Docker compatible container runtime (Docker, Podman, wslc, ...). This guide assume you are using Docker
* A 40250 compatible client (just google for "TMP4 40250 Client")

## Setting up the project

### 1. Clone the repo

```sh
git clone https://github.com/MeikelLP/quantum-core-x.git
```

### 2. Ensure Docker is running

```
docker ps
```

### 2. Start the project

```sh
# navigate to the project
cd quantum-core-x/src/AppHost

# build the project, start all dependencies and watch for changes (some changes require a restart)
dotnet watch
```

### 3. Setup client

See [Client](client.md) to setup your client

### 4. Connecting

1. Start the client
2. Connect with username `admin` and password `admin`
3. Join the server

## Closure

There are more things that can be setup (for levels) but that's not required to start the server. From here on you can start coding.

## Further Reading

* [Account Creation](../Guides/account-creation.md)
