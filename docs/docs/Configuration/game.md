# Game Settings

|Key|Type|Default|Required|Examples|Notes|
|---|---|---|---|---|---|
|`maps`|`string[]`||`true`|`["metin2_map_a1","metin2_map_b1","metin2_map_c1"]`|Maps this core is handling|


## Auth

Section: `Auth`

|Key|Type|Default|Required|Examples|Notes|
|---|---|---|---|---|---|
|`BaseUrl`|`string`|`http://localhost:5000`|`false`|`http://localhost:5000`|Http endpoint base for communicating with the auth server|

## Game

Section: `Game`

|Key|Type|Default|Required|Examples|Notes|
|---|---|---|---|---|---|
|`Game:InGameShop`|`string`|`https://example.com/`|`false`|`https://example.com/`|URL that will be opened when the client clicks on the item shop|

## Commands

Section: `Game:Commands`

| Key          | Type   | Default | Required | Examples | Notes                                                                                                                                   |
|--------------|--------|---------|----------|----------|-----------------------------------------------------------------------------------------------------------------------------------------|
| `StrictMode` | `bool` | `false` | `false`  | `true`   | Will throw exceptions (and maybe disconnect the player) when commands fail. Used for testing. Otherwise sends chat messages in response |

### Skills

Section: `Game:Skills`

|Key|Type|Default|Required|Examples|Notes|
|---|---|---|---|---|---|
|`GenericSkillBookId`|`uint`|`50300`|`false`|`50300`|Skill book id that is used when creating a specific skill book for a skill|
|`SkillBookStartId`|`uint`|`50400`|`false`|`50400`|Identifier for iterating over skill book ids|
|`SkillBookNeededExperience`|`int`|`50400`|`false`|`50400`|Consumed player experience when using a skill book|
|`SkillBookDelayMin`|`int`|`64800`|`false`|`64800`|Minimum delay to wait after using a skill book|
|`SkillBookDelayMax`|`int`|`108000`|`false`|`108000`|Maximum delay to wait after using a skill book|
|`SoulStoneId`|`int`|`50513`|`false`|`50513`|Identifier for the soul stone item|
