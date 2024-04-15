# Getting started as a developer

## Prerequisites

* Windows with admin rights (for the client - the server can be developed on any platform)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) or equivalent
* [.NET SDK 7](https://dotnet.microsoft.com/en-us/download)
* [Visual Studio](https://visualstudio.com), [Jetbrains Rider](https://www.jetbrains.com/rider/), or any IDE you are comfortable with
* [Katai struct compiler](https://kaitai.io/#download)
* A TMP4 compatible client (just google for "TMP4 Client")

1. Setting up the project

    1. Clone the repo

        ```sh
        git clone https://github.com/MeikelLP/quantum-core-x.git
        ```

    2. Run the Kaitai struct compiler (use the `.ps1` when on windows)

        ```sh
        # in the repo directory
        ./Executables/Game/generate_kaitai.sh
        # you can ignore any warning
        ```

    3. Generate a `mob_proto` and `item_proto`

        In the client directory there should be a folder called `Eternexus` with an folder `--dump_proto--`. Just execute the `dump_proto.exe`. It should generate you 2 files:

        * `item_proto`
        * `mob_proto`

    4. Compile the project

        ```sh
        dotnet build
        # you should see "0 Error(s)"
        ```

    5. Create the folder `data` in `./Executables/Game/bin/Debug/net7.0/`
    6. Copy the following files into that folder:
        * In your clients `Eternexus` folder there should be an `atlasinfo.txt`
        * `item_proto` & `mob_proto` from step 3
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

2. Setting up required services

    1. Use the `docker-compose.yml` in `src/` to boot up all services.

        ```sh
        docker-compose up -d
        ```
       
        This starts:
       * `auth` server for account authentication
       * `db` for persistant storage
       * `cache` for distributed live data

    2. Run the db migrator

        ```sh
         dotnet run --project Executables/Migrator --launch-profile Migrator
        ```

3. Setup client

    See [Client](client.md) to setup your client

4. Create an account `admin` with the password `admin`

    ```sh
    # /bin/sh
    # replace __CONTAINER_ID__ with your mysql container ID
    docker exec __CONTAINER_ID__ /bin/mysql -u root -psupersecure.123 --execute="INSERT INTO account.accounts (Id, Username, Password, Email, Status, LastLogin, CreatedAt, UpdatedAt, DeleteCode) VALUES ('584C4BC9-559F-47DD-9A7E-49EEB65DD831', 'admin', '\$2y\$10\$dTh8zmAfA742vKZ35Oarzugv3QXJPTOYRhKpk807o9h9SWBsFcys6', 'some@mail.com', DEFAULT, null, DEFAULT, DEFAULT, DEFAULT);"
    ```
   
    ```ps1
    # for powershell / windows
    # replace __CONTAINER_ID__ with your mysql container ID
    docker exec __CONTAINER_ID__ /bin/mysql -u root '-psupersecure.123' --execute="INSERT INTO account.accounts (Id, Username, Password, Email, Status, LastLogin, CreatedAt, UpdatedAt, DeleteCode) VALUES ('584C4BC9-559F-47DD-9A7E-49EEB65DD831', 'admin', '`$2y`$10`$dTh8zmAfA742vKZ35Oarzugv3QXJPTOYRhKpk807o9h9SWBsFcys6', 'some@mail.com', DEFAULT, null, DEFAULT, DEFAULT, DEFAULT);"
    ```

    for more infos about account creation look at [Account Creation](../tutorials/account-creation.md)

5. Start the server

    Start the application from source

    ```sh
    dotnet run --project Executables/Game
    ```

6. Start the client
7. Connect with username `admin` and password `admin`
8. Create a player
9. Join the server

## Closure

There are more things that can be setup (for levels) but that's not required to start the server. From here on you can start coding.

## Further Reading

* [Developer VM](../tutorials/dev-vm.md)
