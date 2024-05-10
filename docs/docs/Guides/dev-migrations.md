---
tags: [developer]
---

# Developer: Migrations

As we provide support for multiple database providers we need to have multiple migrations for each individual database. Luckily we are using [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) which can generate these for us. However EFCore can only generate one migration at a time.

The following commands may help you generate these easily.

:::warning

It's always recommended to __not__ write migrations yourself. Let EF generate them for you and validate them. If they do not match your desired result you may misconfigured the entities which will lead to runtime errors

:::

:::tip

To support rolling updates (updates without downtime) you must not create any migration that results in a data loss. Removing and updating columns are destructive changes which may break old app versions. Thus do not create such migrations in the first step. Create a migration which is downward compatible. These destructive changes can then be implemented in the next (major) updates so all running instances should already be updated and not use the old APIs/tables/columns. Read more [here](https://stackoverflow.com/questions/51243156/how-to-implement-rolling-updates-and-a-relational-database).

Don't let this stop you though! We are currently in pre-production so nothing is declared stable anyway :)

:::

The following lines will generate each a migration for a provider in the auth persistence project

```sh
# assuming you are in src/Data/Auth.Persistence

# mysql
dotnet ef migrations add Initial --context MysqlAuthDbContext --output-dir Migrations/Mysql --startup-project ../../Executables/Auth -- --Database:Provider mysql --Database:ConnectionString "just for validation not null"

# sqlite
dotnet ef migrations add Initial --context SqliteAuthDbContext --output-dir Migrations/Sqlite --startup-project ../../Executables/Auth -- --Database:Provider sqlite --Database:ConnectionString "just for validation not null"

# sqlite
dotnet ef migrations add Initial --context PostgresqlAuthDbContext --output-dir Migrations/Postgresql --startup-project ../../Executables/Auth -- --Database:Provider postgresql --Database:ConnectionString "just for validation not null"
```

The following lines will generate each a migration for a provider in the game persistence project

```sh
# assuming you are in src/Data/Game.Persistence

# mysql
dotnet ef migrations add Initial --context MysqlGameDbContext --output-dir Migrations/Mysql --startup-project ../../Executables/Game -- --Database:Provider mysql --Database:ConnectionString "just for validation not null"

# sqlite
dotnet ef migrations add Initial --context SqliteGameDbContext --output-dir Migrations/Sqlite --startup-project ../../Executables/Game -- --Database:Provider sqlite --Database:ConnectionString "just for validation not null"

# sqlite
dotnet ef migrations add Initial --context PostgresqlGameDbContext --output-dir Migrations/Postgresql --startup-project ../../Executables/Game -- --Database:Provider postgresql --Database:ConnectionString "just for validation not null"
```
