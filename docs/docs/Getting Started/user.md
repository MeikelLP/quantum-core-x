# User Guide

In this guide a _User_ refers to someone who wants to host/administrate QCX.

## Prerequisites

* Docker (or any OCI compliant alternative)
* A TMP4 compatible client (just google for "TMP4 Client")

## Quickstart

### Requirements

Save the following files in a new directory

* `game.appsettings.json`

  ```json
  {
    "Hosting": {
      "Port": 13001
    },
    "Cache": {
      "Host": "cache",
      "Port": 6379
    },
    "Database": {
      "Provider": "mysql",
      "ConnectionString": "Server=localhost;Database=game;Uid=root;Pwd=supersecure.123;"
    },
    "maps": [
      "metin2_map_a1",
      "metin2_map_b1",
      "metin2_map_c1"
    ]
  }
    ```

* `auth.appsettings.json`

  ```json
  {
    "Hosting": {
      "Port": 11002
    },
    "Cache": {
      "Host": "cache",
      "Port": 6379
    },
    "Database": {
      "Provider": "mysql",
      "ConnectionString": "Server=localhost;Database=auth;Uid=root;Pwd=supersecure.123;"
    }
  }
  ```

* `docker-compose.yml`

  ```yml
  version: '3'
  services:
  
    # authentication service
    auth:
      image: ghcr.io/meikellp/quantum-core-x/auth
      restart: unless-stopped
      ports:
        - "11002:11002"
      volumes:
        - ./auth.appsettings.json:/app/Core/appsettings.Production.json:ro
  
    # game service
    game:
      image: ghcr.io/meikellp/quantum-core-x/game
      restart: unless-stopped
      ports:
        - "13001:13001"
      volumes:
        - ./game.appsettings.json:/app/Core/appsettings.Production.json:ro
        - ./settings.toml:/app/Core/settings.toml:ro
        - ./data:/app/Core/data:ro
  
    # redis holds live data of the game world
    # used as distributed memory between server nodes
    cache:
      image: redis:latest
      restart: unless-stopped
      volumes:
        - cache_data:/data
      ports:
        - "6379:6379"
  
    # persistent storage for game data
    db:
      image: mariadb:latest
      ports:
        - "3306:3306"
      environment:
        - MARIADB_USER=metin2
        - MARIADB_PASSWORD=metin2
        - MARIADB_ROOT_PASSWORD=supersecure.123
      volumes:
        - db_data:/var/lib/mysql
  
  volumes:
    cache_data:
    db_data:
  ```

* `atlasinfo.txt`

  ```tsv
  metin2_map_a1	409600	896000	4	5
  metin2_map_a3	307200	819200	4	4
  metin2_map_b1	0	102400	4	5
  ```

* execute the `Eternexus\--dump_proto--\dump_proto.exe` in the client's directory. This should create two files:
  * `item_proto`
  * `mob_proto`

* Lastly move the files to look like this:
  
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

### Create an admin account

A default user `admin` with password `admin` is created for you by default.

:::warning
If you plan on opening up the server to other people you should change that admins password.
:::

:::tip
For more infos about account creation look at [Account Creation](../Guides/account-creation.md)
:::

### Setup client

 See [Client](client.md) to setup your client

### Startup

Start the db detached first because it is not available instantly but we require a valid database as soon as possible.

```sh
docker-compose up db -d
```

Finally boot up all services. Add `-d` to run them in the background

```sh
docker-compose up
```

## Next steps

* [Add player +permissions](../Guides/player-permission.md)
