# Common Settings

## Cache

Section: `Cache`

|Key|Type|Default|Required|Examples|Notes|
|---|---|---|---|---|---|
|`Host`|`string`|`127.0.0.1`|`false`|`localhost`||
|`Port`|`ushort`|`6379`|`false`|`6379`|default value 13001 (game) and 11002 (auth)|

## Database

Section: `Database`

|Key|Type|Default|Required|Examples|Notes|
|---|---|---|---|---|---|
|`Provider`|`DatabaseProvider`|`null`|`true`|`postgresql`, `mysql`, `sqlite`||
|`ConnectionString`|`string`|`null`|`true`|`Server=localhost;Database=metin2;Uid=metin2;Pwd=metin2;`|Refer to [ConnectionStrings](https://www.connectionstrings.com)|

## Hosting

Section: `Hosting`

|Key|Type|Default|Required|Examples|Notes|
|---|---|---|---|---|---|
|`Hosting:IpAddress`|`string`|`0.0.0.0`|`false`|`127.0.0.1`|only IP addresses no host names|
|`Hosting:Port`|`ushort`||`false`|`13001`|default value 13001 (game) and 11002 (auth)|

## Serilog

Section: `Serilog`

|Key|Type|Default|Required|Examples|Notes|
|---|---|---|---|---|---|
|`Serilog:*`|||||Refer to [Serilog](https://github.com/serilog/serilog-settings-configuration)|
