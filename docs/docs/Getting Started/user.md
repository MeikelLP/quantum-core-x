# User Guide

In this guide a _User_ refers to someone who wants to host/administrate QCX.

## Prerequisites

* Docker (or any OCI compliant alternative with docker compose feature)
* A TMP4 compatible client (just google for "TMP4 Client")

## Quickstart

1. Get the files from the sample in the [sample folder](../../samples/full-setup/)

2. execute the `Eternexus\--dump_proto--\dump_proto.exe` in the client's directory. This should create two files:

    * `item_proto`
    * `mob_proto`

3. Your folder should look like this now:
  
    ```txt
    auth.appsettings.json
    docker-compose.yml
    game.appsettings.json
    data/
      atlasinfo.txt
      exp.csv
      item_proto
      jobs.json
      mob_proto
    ```

### Setup client

Replace the contents of the `serverinfo.py` from your client with this:

```py
SERVER_NAME			= "QuantumCoreX"
SERVER_IP			= "localhost"
CH1_NAME			= "CH1"
PORT_1				= 13000
PORT_AUTH			= 11000
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
docker-compose up -d
```

You can now connect to the game & auth server. A default admin user will be created for you:

```txt
username: admin
password: admin
```

> :warning: Be sure to change these credentials as soon as you go production!

## Next steps

* [Add player permissions](../Guides/player-permission.md)
