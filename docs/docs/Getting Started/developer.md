# Developer Guide

* _Server_ refers to Quantum Core X
* _Client_ refers to the TMP4 client

## Prerequisites

* Windows for the client. This tutorial can be run on any OS but the client has to connect from Windows
* [.NET SDK 9](https://dotnet.microsoft.com/en-us/download)
* A TMP4 compatible client (just google for "TMP4 Client")

## Setting up the project

### 1. Clone the repo

```sh
git clone https://github.com/MeikelLP/quantum-core-x.git
```

### 2. Compile the project

```sh
# navigate to the project
cd quantum-core-x/src

# build the project
dotnet build

# you should see "0 Error(s)"
```

### 3. Create the data folder

Create the directory `data` in `./Executables/Single`

### 4. Generate a `mob_proto` and `item_proto`

In the (TMP4) client directory there should be a folder called `Eternexus` with an folder `--dump_proto--`. 
Just execute the `dump_proto.exe`. It should generate you 2 files:

* `item_proto`
* `mob_proto`

### 5. Copying settings

Copy the following files into the servers `data` folder

* `item_proto`
* `mob_proto`

### 7. Setup client

See [Client](client.md) to setup your client

### 8. Credentials

By default, the user `admin` with the password `admin` is created. For obvious reasons it's recommended to change this once you plan on opening it up for others.

:::tip
For more infos about account creation refer to [Account Creation](../Guides/account-creation.md)
:::

### 9. Start the server

Start the application from source

```sh
# from the quantum-core-x repository root
dotnet run --project src/Executables/Single
```

### 10. Connecting

1. Start the client
2. Connect with username `admin` and password `admin`
3. Create a player
4. Join the server

## Closure

There are more things that can be setup (for levels) but that's not required to start the server. From here on you can start coding.

## Further Reading

* [Developer VM](../Guides/dev-vm.md)
