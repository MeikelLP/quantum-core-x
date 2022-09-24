using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using QuantumCore.Game.Packets;
using QuantumCore.API.Game;
using System.Threading.Tasks;
using QuantumCore.Database;
using Dapper;
using Microsoft.Extensions.Logging;
using QuantumCore.Core.Cache;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    public class CommandManager : ICommandManager
    {
        private readonly ILogger<CommandManager> _logger;
        private readonly IDatabaseManager _databaseManager;
        private readonly ICacheManager _cacheManager;
        public Dictionary<string, CommandCache> Commands { get; } = new ();
        public Dictionary<Guid, PermissionGroup> Groups { get; } = new ();

        public readonly Guid Operator_Group = Guid.Parse("45bff707-1836-42b7-956d-00b9b69e0ee0");

        public CommandManager(ILogger<CommandManager> logger, IDatabaseManager databaseManager, ICacheManager cacheManager)
        {
            _logger = logger;
            _databaseManager = databaseManager;
            _cacheManager = cacheManager;
        }
        
        public void Register(string ns, Assembly assembly = null)
        {
            _logger.LogDebug("Registring commands from namespace {Namespace}", ns);
            if (assembly == null) assembly = Assembly.GetAssembly(typeof(CommandManager));

            var types = assembly.GetTypes().Where(t => string.Equals(t.Namespace, ns, StringComparison.Ordinal))
                .Where(t => t.GetCustomAttribute<CommandAttribute>() != null).ToArray();

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<CommandAttribute>();
                _logger.LogDebug("Registring command {CommandName} from {TypeName}", attr.Name, type.Name);
                var bypass = type.GetCustomAttribute<CommandNoPermissionAttribute>();

                Commands[attr.Name] = new CommandCache(attr, type, bypass != null);
            }
        }

        private async Task ParseGroup(Guid id, string name)
        {
            using var db = _databaseManager.GetGameDatabase();

            var p = new PermissionGroup
            {
                Id = id,
                Name = name,
                Users = new List<Guid>(),
                Permissions = new List<string>(),
            };

            if (id != Operator_Group)
            {
                var authq = await db.QueryAsync("SELECT Command FROM perm_auth WHERE `Group` = @Group", new { Group = id });

                foreach (var auth in authq)
                {
                    p.Permissions.Add(auth.Command);
                }
            }

            var pq = await db.QueryAsync("SELECT Player FROM perm_users WHERE `Group` = @Group", new { Group = id });

            foreach (var user in pq)
            {
                p.Users.Add(Guid.Parse(user.Player));

                var key = "perm:" + user.Player;
                var redisList = _cacheManager.CreateList<Guid>(key);

                await redisList.Push(id);
            }

            Groups.Add(p.Id, p);
        }

        public async Task LoadAsync(CancellationToken token = default)
        {
            _logger.LogInformation("Initialize permissions");
            var permission_keys = await _cacheManager.Keys("perm:*");

            foreach (var p in permission_keys)
            {
                await _cacheManager.Del(p);
            }
            using var db = _databaseManager.GetGameDatabase();

            var groups = await db.QueryAsync("SELECT * FROM perm_groups");

            foreach (var group in groups)
            {
                await ParseGroup(Guid.Parse(group.Id), group.Name);
            }

            await ParseGroup(Operator_Group, "Operator");
        }

        public bool HavePerm(Guid group, string cmd)
        {
            if (!Groups.ContainsKey(group))
            {
                return false;
            }

            var g = Groups[group];

            foreach (var p in g.Permissions)
            {
                if (p == cmd)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanUseCommand(PlayerEntity player, string cmd)
        {
            if (Commands[cmd].BypassPerm)
            {
                return true;
            }

            foreach (var group in player.Groups)
            {
                if (group == Operator_Group)
                {
                    return true;
                }
                else if (HavePerm(group, cmd))
                {
                    return true;
                }

            }

            return false;
        }

        public async Task Handle(GameConnection connection, string chatline)
        {
            var args = chatline.Split(" "); // todo implement quotation marks for strings
            var command = args[0].Substring(1);

            if (Commands.ContainsKey(command))
            {
                var cmd = Commands[command];

                if (!CanUseCommand(connection.Player, command))
                {
                    await connection.Send(new ChatOutcoming()
                    {
                        MessageType = ChatMessageTypes.Info,
                        Vid = 0,
                        Empire = 0,
                        Message = $"You don't have enough permission to use this command"
                    });
                    return;
                }

                var objects = new object[args.Length];
                objects[0] = connection.Player;

                for (var i = 1; i < args.Length; i++)
                {
                    var str = args[i];

                    if (str.Contains('.') || str.Contains(','))
                    {
                        var numStr = str.Replace(".", ",");

                        if (float.TryParse(numStr, out var f))
                        {
                            objects[i] = f;
                        }
                        else
                        {
                            objects[i] = str;
                        }
                    }
                    else
                    {
                        if (int.TryParse(str, out var n))
                        {
                            objects[i] = n;
                        }
                        else
                        {
                            objects[i] = str;
                        }
                    }
                }

                await cmd.Run(objects);
            }
            else
            {
                await connection.Send(new ChatOutcoming()
                {
                    MessageType = ChatMessageTypes.Info,
                    Vid = 0,
                    Empire = 0,
                    Message = $"Unknown command {command}"
                });
            }
        }
    }
}
