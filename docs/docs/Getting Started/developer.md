# Developer Guide

## Prerequisites

* Windows for the client. This tutorial can be run on any OS but the client has to connect from Windows
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) or equivalent
* [.NET SDK 8](https://dotnet.microsoft.com/en-us/download)
* [Visual Studio](https://visualstudio.com), [Jetbrains Rider](https://www.jetbrains.com/rider/), or any IDE you are comfortable with
* [Katai struct compiler](https://kaitai.io/#download)
* A TMP4 compatible client (just google for "TMP4 Client")

## Setting up the project

### 1. Clone the repo

```sh
git clone https://github.com/MeikelLP/quantum-core-x.git
```

### 2. Generate required files

Run the Kaitai struct compiler (use the `.ps1` when on windows)

```sh
# in the repo directory
kaitai-struct-compiler src/Executables/Game/Types/item_proto.ksy -t csharp --outdir src/Executables/Game/Types/Types/ --dotnet-namespace QuantumCore.Core.Types
kaitai-struct-compiler src/Executables/Game/Types/mob_proto.ksy -t csharp --outdir src/Executables/Game/Types/Types/ --dotnet-namespace QuantumCore.Core.Types
# you can ignore any warning
```

### 3. Generate a `mob_proto` and `item_proto`

In the client directory there should be a folder called `Eternexus` with an folder `--dump_proto--`. Just execute the `dump_proto.exe`. It should generate you 2 files:

* `item_proto`
* `mob_proto`

### 4. Compile the project

```sh
dotnet build
# you should see "0 Error(s)"
```

### 5. Create the data folder

Create the directory `data` in `./Executables/Game/bin/Debug/net7.0/`

### 6. Copying settings

Copy the following files into that folder

* In your clients `Eternexus` folder there should be an `atlasinfo.txt`
* `item_proto` & `mob_proto` from step 3

### 7. Setup required services

Use the `docker-compose.yml` in `src/` to boot up all services.

```sh
docker-compose up -d
```

This starts:

* `auth` server for account authentication
* `db` for persistant storage
* `cache` for distributed live data

### 8. Setup client

See [Client](client.md) to setup your client

### 9. Credentials

By default the user `admin` with the password `admin` is created. For obvious reasons it's recommended to change this once you plan on opening it up for others.

:::tip
For more infos about account creation refer to [Account Creation](../Guides/account-creation.md)
:::

### 10. Start the server

Start the application from source

```sh
dotnet run --project Executables/Game
```

### 11. Connecting

1. Start the client
2. Connect with username `admin` and password `admin`
3. Create a player
4. Join the server

## Closure

There are more things that can be setup (for levels) but that's not required to start the server. From here on you can start coding.

## Further Reading

* [Developer VM](../Guides/dev-vm.md)
