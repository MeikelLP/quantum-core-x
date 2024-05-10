---
tags: [user]
---

# Configuration

## Settings

:::warning

:construction: This area is work in progress and might not be up to date

:::

QuantumCoreX uses [Microsoft.Extensions.Configuration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) which can have multiple data sources (i.e. json files, command line args, environment variables). Feel free to read into their docs do get a better understanding about overriding and naming.

|Key|Type|Default|Required|Example|
|---|---|---|---|---|
|Database:Provider|`DatabaseProvider` (postgresql, mysql, sqlite)|`null`|true|`mysql`|
|Database:ConnectionString|string|`null`|true|Refer to [ConnectionStrings](https://www.connectionstrings.com)|

By default QC comes with a preexisting `appsettings.json` file next to the executable. You can create a `appsettings.Production.json` to override values without touching the base config file

## Additional config files

### Experience Table

|||
|---|---|
|Location|`data/exp.csv`|
|Recommendation|Recommended|

A CSV file with information about how much experience is required per level.

* The file has only one column.
* All values must be only digits (no delimiter)
* No comments allowed
* Each line represents the exp needed to acquire the next level. The line number represents the current level
* This file implicitly defines the maximum level

#### Example

```csv
300
800
1500
2500
4300
7200
11000
17000
24000
33000
```
