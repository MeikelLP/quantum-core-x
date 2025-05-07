using QuantumCore.API;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("stat_reset", "Reset all stats and get back the stat points")]
public class StatResetCommand : ICommandHandler
{
    private readonly IJobManager _jobManager;

    public StatResetCommand(IJobManager jobManager)
    {
        _jobManager = jobManager;
    }

    public Task ExecuteAsync(CommandContext context)
    {
        var job = _jobManager.Get(context.Player.Player.PlayerClass);

        if (job is null)
        {
            throw new ApplicationException(
                $"Job {context.Player.Player.PlayerClass} not found. This should never happen.");
        }

        context.Player.Player.Ht = job.Ht;
        context.Player.Player.Dx = job.Dx;
        context.Player.Player.Iq = job.Iq;
        context.Player.Player.St = job.St;
        context.Player.Player.AvailableStatusPoints = 0;
        context.Player.Player.GivenStatusPoints = 0;
        context.Player.RecalculateStatusPoints();
        context.Player.SendPoints();

        return Task.CompletedTask;
    }
}
