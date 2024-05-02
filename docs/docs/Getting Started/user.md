# User guide

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
        "Host": "db",
        "User": "root",
        "Password": "supersecure.123",
        "Database": "game"
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
        "Host": "db",
        "User": "root",
        "Password": "supersecure.123",
        "Database": "account"
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
          - ./auth.appsettings.json:/app/Core/appsettings.json:ro
    
      # game service
      game:
        image: ghcr.io/meikellp/quantum-core-x/game
        restart: unless-stopped
        ports:
          - "13001:13001"
        volumes:
          - ./game.appsettings.json:/app/Core/appsettings.json:ro
          - ./settings.toml:/app/Core/settings.toml:ro
          - ./data:/app/Core/data:ro
    
      # migrate service - required once
      migrate:
        image: ghcr.io/meikellp/quantum-core-x/migrator
        restart: no
        network_mode: host
        command: '--host localhost --port 3306 --user root --password supersecure.123'
    
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
* `jobs.json`
    ```json
    {
      "job": [
        {
          "id": 0,
          "name": "warrior",
          "st": 6,
          "ht": 4,
          "dx": 3,
          "iq": 3,
          "startHp": 600,
          "startSp": 200,
          "hpPerHt": 40,
          "spPerIq": 20,
          "hpPerLevel": 36,
          "spPerLevel": 44
        },
        {
          "id": 1,
          "name": "assassin",
          "st": 4,
          "ht": 3,
          "dx": 6,
          "iq": 3,
          "startHp": 650,
          "startSp": 200,
          "hpPerHt": 40,
          "spPerIq": 20,
          "hpPerLevel": 36,
          "spPerLevel": 44
        },
        {
          "id": 2,
          "name": "sura",
          "st": 5,
          "ht": 3,
          "dx": 3,
          "iq": 5,
          "startHp": 650,
          "startSp": 200,
          "hpPerHt": 40,
          "spPerIq": 20,
          "hpPerLevel": 36,
          "spPerLevel": 44
        },
        {
          "id": 3,
          "name": "shamana",
          "st": 3,
          "ht": 4,
          "dx": 3,
          "iq": 6,
          "startHp": 700,
          "startSp": 200,
          "hpPerHt": 40,
          "spPerIq": 20,
          "hpPerLevel": 36,
          "spPerLevel": 44
        },
        {
          "id": 4,
          "name": "warrior",
          "st": 6,
          "ht": 4,
          "dx": 3,
          "iq": 3,
          "startHp": 600,
          "startSp": 200,
          "hpPerHt": 40,
          "spPerIq": 20,
          "hpPerLevel": 36,
          "spPerLevel": 44
        },
        {
          "id": 5,
          "name": "assassin",
          "st": 4,
          "ht": 3,
          "dx": 6,
          "iq": 3,
          "startHp": 650,
          "startSp": 200,
          "hpPerHt": 40,
          "spPerIq": 20,
          "hpPerLevel": 36,
          "spPerLevel": 44
        },
        {
          "id": 6,
          "name": "sura",
          "st": 5,
          "ht": 3,
          "dx": 3,
          "iq": 5,
          "startHp": 650,
          "startSp": 200,
          "hpPerHt": 40,
          "spPerIq": 20,
          "hpPerLevel": 36,
          "spPerLevel": 44
        },
        {
          "id": 7,
          "name": "shamana",
          "st": 3,
          "ht": 4,
          "dx": 3,
          "iq": 6,
          "startHp": 700,
          "startSp": 200,
          "hpPerHt": 40,
          "spPerIq": 20,
          "hpPerLevel": 36,
          "spPerLevel": 44
        }
      ]
    }
    ```

* execute the `Eternexus\--dump_proto--\dump_proto.exe` in the client's directory. This should create two files:
  * `item_proto`
  * `mob_proto`

* Lastly move the files to look like this:
```
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

This command will create an `admin` account with `admin` as password.
To get the container ID of your mysql container use `docker ps`

```sh
# replace __CONTAINER_ID__ with your mysql container ID
docker exec __CONTAINER_ID__ /bin/mysql -u root -psupersecure.123 --execute="INSERT INTO account.accounts (Id, Username, Password, Email, Status, LastLogin, CreatedAt, UpdatedAt, DeleteCode) VALUES ('584C4BC9-559F-47DD-9A7E-49EEB65DD831', 'admin', '$2y$10$dTh8zmAfA742vKZ35Oarzugv3QXJPTOYRhKpk807o9h9SWBsFcys6', 'some@mail.com', DEFAULT, null, DEFAULT, DEFAULT, DEFAULT);"
```

for more infos about account creation look at [Account Creation](../Tutorials/account-creation.md)

### Setup client

 See [Client](client.md) to setup your client

### Startup

Start the db detached first

```sh
docker-compose up db -d
```

Next migrate the db

```sh
docker-compose up migrate
```

Finally boot up all services. Add `-d` to run them in the background

```sh
docker-compose up
```

## Next steps

* [Add player +permissions](../Tutorials/player-permission.md)
