# Player Permissions

By default every player has no permissions to execute commands.
At some point you might want to make a user an admin to execute commands.

## Authorization concept

The authorization in QCX works as follows:

* A `player` may have n memberships to `roles`
* A `role` may have n `permissions`
* A `permission` defines the ability to execute a command

## Add permissions for a player

> This guide uses Docker for executing SQL, however you are free to use any database management tool you desire.

Use the following command to execute sql

```sh
# replace __CONTAINER_ID__ with your mysql container ID
docker exec __CONTAINER_ID__ -it /bin/mysql -u root -psupersecure.123
```

This command will open a mysql terminal session in which you can execute SQL. You can leave with `exit`

Next, execute the following

```sql
SET @PlayerId = 'YOUR_PLAYER_ID_HERE';
SET @GroupId = uuid();
INSERT INTO game.perm_groups (Id, Name) VALUES (@GroupId, 'admins');
INSERT INTO game.perm_auth (`Group`, Command) VALUES (@GroupId, 'spawn');
INSERT INTO game.perm_auth (`Group`, Command) VALUES (@GroupId, 'give');
INSERT INTO game.perm_users (`Group`, Player) VALUES (@GroupId, @PlayerId);
```

This will add the player to the newly created group `admins` and allow him to execute `/spawn` and `/give`.
Modifying these permissions require you to restart the game server

There are even more commands to execute. Try `/help` to see all of them