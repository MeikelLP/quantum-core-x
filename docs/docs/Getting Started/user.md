# User Guide

> :warning: This setup is currently only designed for testing!

In this guide a _User_ refers to someone who wants to host/administrate QCX.

## Prerequisites

* A TMP4 compatible client (just google for "TMP4 Client")
* A Quantum Core X executable:
    * [Build it yourself](./developer.md)
    * [Github Releases](https://github.com/MeikelLP/quantum-core-x/releases) (stable) - available from 0.1 forward
    * Directly from
      the [build pipelines](https://nightly.link/MeikelLP/quantum-core-x/workflows/dotnet-pipeline/main) (unstable).

## Quickstart

1. execute the `Eternexus\--dump_proto--\dump_proto.exe` in the (TMP4) client's directory. This should create two files
   directly next to the `dump_proto.exe`:

    * `item_proto`
    * `mob_proto`

   Put these files into the data folder

2. Your folder should look like this now:
  
    ```txt
    appsettings.json
    QuantumCore.Single.exe
    data/
      item_proto
      mob_proto
    [...some other files]
    ```

### Setup client

> Note that the ports differ in the Single setup than in the docker setup

Replace the contents of the `serverinfo.py` from your client with this:

```py
SERVER_NAME			= "QuantumCoreX"
SERVER_IP			= "localhost"
CH1_NAME			= "CH1"
PORT_1				= 13001
PORT_AUTH			= 11002
PORT_MARK			= 13000

STATE_NONE = "..."

STATE_DICT = {
	0 : "....",
	1 : "NORM",
	2 : "BUSY",
	3 : "FULL"
}

SERVER01_CHANNEL_DICT = {
	1:{"key":11,"name":CH1_NAME,"ip":SERVER_IP,"tcp_port":PORT_1,"udp_port":PORT_1,"state":STATE_NONE,},
}

REGION_NAME_DICT = {
	0 : "",		
}

REGION_AUTH_SERVER_DICT = {
	0 : {
		1 : { "ip":SERVER_IP, "port":PORT_AUTH, },

	}		
}

REGION_DICT = {
	0 : {
		1 : { "name" :SERVER_NAME, "channel" : SERVER01_CHANNEL_DICT, },						
	},
}

MARKADDR_DICT = {
	10 : { "ip" : SERVER_IP, "tcp_port" : PORT_MARK, "mark" : "10.tga", "symbol_path" : "10", },
}
```

For more information see [Client](client.md)

### Startup

```sh
./QuantumCore.Single.exe
```

Once you see

```
Start listening for connections on 127.0.0.1:13001 (game) 
Start listening for connections on 127.0.0.1:11002 (auth) 
Start listening for connections... 
```

you can connect to the game & auth server. A default admin user will be created for you:

```txt
username: admin
password: admin
```

## Next steps

By default, the server will start fine but many configurations may be missing. Please add them to add additional
features / data: [Configurations](../Configuration/index.md)
