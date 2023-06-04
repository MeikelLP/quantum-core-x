using System;
using System.Threading.Tasks;
using CommandLine;
using FluentMigrator.Runner.BatchParser;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Extensions;

namespace QuantumCore.Game.Commands
{
    [Command("dice", "Returns a random number between 2 values")]
    public class DiceCommand : ICommandHandler<DiceCommandOptions>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IWorld _world;

        public DiceCommand(IItemManager itemManager, ICacheManager cacheManager, IWorld world)
        {
            _cacheManager = cacheManager;
            _world = world;
        }

        public async Task ExecuteAsync(CommandContext<DiceCommandOptions> context)
        {

            var Start = context.Arguments.Start;
            var End = context.Arguments.End;

            if(Start == 0 && End == 0){
                Start = 1;
                End = 100;
            }

            if(Start != 0 && End == 0)
            {
                End = Start;
                Start = 1;
            }

            Start = Math.Min(End, Start);
            End = Math.Max(Start, End);

            Random random = new Random();
            int n = random.Next((int) Start, (int) End + 1);

            /*
                TODO: if player has group, send chat info to each group members
            */
            await context.Player.SendChatInfo($"Dice {n} ({Start} - {End})");
           
        }
    }

    public class DiceCommandOptions
    {

        [Value(0)]
        public uint Start { get; set; }

        [Value(1)]
        public uint End { get; set; }

    }
}