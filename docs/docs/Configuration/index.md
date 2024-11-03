---
tags: [user]
---
import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# Configuration

QuantumCoreX uses [Microsoft.Extensions.Configuration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) which can have multiple data sources (i.e. json files, command line args, environment variables). Feel free to read into their docs do get a better understanding about overriding and naming.

The following configuration providers are supported by default

* _hardcoded configs in the app (see default value)_
* `appsettings.json`: `{"Section":{"SomeKey": "SOME_VALUE"}}`, `{"Section:SomeKey": "SomeValue"}`
* Environment: `SECTION__SOME_KEY=SOME_VALUE`
* Command Line: `--SECTION:SOME_KEY SOME_VALUE`, `--SECTION:SOME_KEY=SOME_VALUE`

The order is important. The providers are applied on after another. The last is the most important overriding all previous providers. It is not possible to remove configuration from previous providers as of now.

The following examples are equivalent

<Tabs>
  <TabItem value="cli" label="Command Line" default>
    ```sh
    --database:provider sqlite
    ```
  </TabItem>
  <TabItem value="enve" label="Environment">
    ```env
    DATABASE__PROVIDER=sqlite
    ```
  </TabItem>
  <TabItem value="json" label="appsettings.json">
    ```json
    {
      "Database": {
        "Provider": "sqlite"
      }
    }
    ```
  </TabItem>
</Tabs>


## Settings

:::warning

:construction: This area is work in progress and might not be up to date

:::

Depending on your application you have different settings:

* [Auth](auth.md)
* [Common](common.md) (equal in all apps)
* [Game](game.md)

:::tip

By default QC comes with a preexisting `appsettings.json` file next to the executable. You can create a `appsettings.Production.json` to override values without touching the base config file

:::

## Additional config files

The following files are location in `data/` next to the executable

* `936skilltable.txt`
* [atlasinfo.txt](atlasinfo.md)
* [exp.csv](exp.md)
* [exp_guild.csv](exp_guild.md)
* `group.txt`
* `group_group.txt`
* `item_proto`
* `mob_proto`
* `skilltable.txt`
* `maps/*/boss.txt`
* `maps/*/npx.txt`
* `maps/*/regen.txt`
* `maps/*/stone.txt`
* `maps/*/Town.txt`
