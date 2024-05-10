---
tags: [user]
---

# Configuration

:::warning

:construction: This area is work in progress and might not be up to date

:::

QuantumCoreX uses [Microsoft.Extensions.Configuration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) which can have multiple data sources (i.e. json files, command line args, environment variables). Feel free to read into their docs do get a better understanding about overriding and naming.

|Key|Type|Default|Required|Example|
|---|---|---|---|---|
|Database:Provider|`DatabaseProvider` (postgresql, mysql, sqlite)|`null`|true|`mysql`|
|Database:ConnectionString|string|`null`|true|Refer to [ConnectionStrings](https://www.connectionstrings.com)|
