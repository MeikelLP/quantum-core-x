# Client Setup

Assuming you are using a valid TMP4 client

1. Go to `Eternexus/root` in your client
2. Edit the `serverinfo.py` file
3. Replace its content with the following

    ```py
    SERVER_NAME			= "Metin2"
    SERVER_NAME_TEST	= "Test"
    SERVER_IP			= "127.0.0.1"
    SERVER_IP_TEST		= "127.0.0.1"
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
