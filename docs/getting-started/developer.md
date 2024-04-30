# Getting started as a developer

## Prerequisites

* Windows with admin rights
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) or equivalent
* [.NET SDK 6](https://dotnet.microsoft.com/en-us/download)
* [Visual Studio](https://visualstudio.com), [Jetbrains Rider](https://www.jetbrains.com/rider/), or any IDE you are comfortable with
* [Katai struct compiler](https://kaitai.io/#download)
* A TMP4 compatible client (just google for "TMP4 Client")

1. Setting up the project

    1. Clone the repo

        ```sh
        git clone https://github.com/MeikelLP/quantum-core-x.git
        ```

    2. Run the Katai struct compiler

        ```sh
        # in the repo directory
        kaitai-struct-compiler src/Executables/Game/Types/item_proto.ksy -t csharp --outdir src/Executables/Game/Types/Types/ --dotnet-namespace QuantumCore.Core.Types
        kaitai-struct-compiler src/Executables/Game/Types/mob_proto.ksy -t csharp --outdir src/Executables/Game/Types/Types/ --dotnet-namespace QuantumCore.Core.Types
        # you can ignore any warning
        ```

    3. Generate a `mob_proto` and `item_proto`

        In the client directory there should be a folder called `Eternexus` with an folder `--dump_proto--`. Just execute the `dump_proto.exe`. It should generate you 2 files:

        * `item_proto`
        * `mob_proto`

    4. Compile the project

        ```sh
        dotnet build
        # you should see a "Build succeeded"
        ```

    5. Save the following file as `atlasinfo.txt`

        ```txt
        metin2_map_a1	409600	896000	4	5
        metin2_map_b1	0	102400	4	5
        metin2_map_c1	921600	204800	4	5
        ```

    6. Save the following file as `jobs.toml`

        ```toml
        [[job]]
        id=0
        name="warrior" 
        st=6
        ht=4
        dx=3
        iq=3
        start_hp=600
        start_sp=200
        hp_per_ht=40
        sp_per_iq=20
        hp_per_level=36
        sp_per_level=44
        [[job]]
        id=1
        name="assassin" 
        st=4
        ht=3
        dx=6
        iq=3
        start_hp=650
        start_sp=200
        hp_per_ht=40
        sp_per_iq=20
        hp_per_level=36
        sp_per_level=44
        [[job]]
        id=2
        name="sura" 
        st=5
        ht=3
        dx=3
        iq=5
        start_hp=650
        start_sp=200
        hp_per_ht=40
        sp_per_iq=20
        hp_per_level=36
        sp_per_level=44
        [[job]]
        id=3
        name="shamana" 
        st=3
        ht=4
        dx=3
        iq=6
        start_hp=700
        start_sp=200
        hp_per_ht=40
        sp_per_iq=20
        hp_per_level=36
        sp_per_level=44
        ```

    7. Save the following file as `settings.toml`

        ```toml
        maps = ["metin2_map_c1", "metin2_map_b1", "metin2_map_a1"]
        ```

    8. Copy required data to the execution path

        ```sh
        mkdir Core/bin/Debug/net6.0/data
        cp atlasinfo.txt Core/bin/Debug/net6.0/data/
        cp jobs.toml Core/bin/Debug/net6.0/data/
        cp settings.toml Core/bin/Debug/net6.0/
        cp item_proto Core/bin/Debug/net6.0/data/
        cp mob_proto Core/bin/Debug/net6.0/data/
        ```

2. Setting up required services

    1. Use the following `docker-compose.yml` to setup all required dependencies as docker containers.

        ```yml
        version: '3'
        services:

        # authentication service - may be run manually from source code
        auth:
            image: registry.gitlab.com/quantum-core/core-dotnet:master
            ports:
            - "11002:11002"
            entrypoint: '/app/Core auth --redis-host cache --game-database-host db --game-database-user root --game-database-password supersecure.123 --account-database-host db --account-database-user root --account-database-password supersecure.123'

        # redis holds live data of the game world
        # used as distributed memory between server nodes
        cache:
            image: redis:latest
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

        Simply save the file and execute

        ```sh
        docker-compose up -d
        ```

        in the directory

    2. Migrate the database

        ```sh
        # get the ID of the mysql container
        docker ps
        ```

        Create the required database schemas

        ```sh
        # replace __CONTAINER_ID__ with your ID
        docker exec __CONTAINER_ID__ /bin/mysql -u root -psupersecure.123 --execute="CREATE DATABASE account; CREATE DATABASE game;"
        ```

        > powershell/powershell-core may break this command. Please use cmd instead

    3. Run the db migrator

        ```sh
        docker run --rm --entrypoint /app/Core --network=host registry.gitlab.com/quantum-core/core-dotnet:master migrate --redis-host localhost --game-database-host localhost --game-database-user root --game-database-password supersecure.123 --account-database-host localhost --account-database-user root --account-database-password supersecure.123
        ```

3. Prepare the client

    1. Go to `Eternexus/root` in your client
    2. Edit the `serverinfo.py` file
    3. Replace its content with the following

        ```py
        SERVER_NAME			= "Metin2"
        SERVER_NAME_TEST	= "Test"
        SERVER_IP			= "127.0.0.1"
        SERVER_IP_TEST		= "127.0.0.1"
        CH1_NAME			= "CH1"
        CH2_NAME			= "CH2"
        CH3_NAME			= "CH3"
        CH4_NAME			= "CH4"
        PORT_1				= 13001
        PORT_2				= 13010
        PORT_3				= 13020
        PORT_4				= 13030
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
            1:{"key":11,"name":CH1_NAME,"ip":SERVER_IP,"tcp_port":PORT_1,"udp_port":PORT_1, "state":STATE_NONE,},
        }

        SERVER02_CHANNEL_DICT = {
            1:{"key":21,"name":CH1_NAME,"ip":SERVER_IP_TEST,"tcp_port":PORT_1,"udp_port":PORT_1,    "state":STATE_NONE,},
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

    4. Execute `EterNexus.exe`
    5. Select `File` > `Pack Archive` and select the `root` folder where the `serverinfo.py` is
    6. Copy the created `root.eix` and `root.epk` to your clients `pack` folder and overwrite the existing files

4. Create an account `admin` with the password `admin`

    ```sh
    # replace __CONTAINER_ID__ with your mysql container ID
    docker exec __CONTAINER_ID__ /bin/mysql -u root -psupersecure.123 --execute="INSERT INTO account.accounts (Id, Username, Password, Email, Status, LastLogin, CreatedAt, UpdatedAt, DeleteCode) VALUES ('584C4BC9-559F-47DD-9A7E-49EEB65DD831', 'admin', '$2y$10$dTh8zmAfA742vKZ35Oarzugv3QXJPTOYRhKpk807o9h9SWBsFcys6', 'some@mail.com', DEFAULT, null, DEFAULT, DEFAULT, DEFAULT);"
    ```

    for more infos about account creation look at [Account Creation](../tutorials/account-creation.md)

5. Start the server

    Start the application from source

    ```sh
    dotnet run --launch-profile Game --project Core
    ```

6. Start the client
7. Connect with username `admin` and password `admin`
8. Create a player
9. Join the server

## Closure

There are more things that can be setup (for levels) but that's not required to start the server. From here on you can start coding.
