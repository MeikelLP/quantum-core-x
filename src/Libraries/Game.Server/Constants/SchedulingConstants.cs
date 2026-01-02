namespace QuantumCore.Game.Constants;

public static class SchedulingConstants
{
    public const int LOGOUT_WAIT_SECONDS = 3;
    public const int LOGOUT_COMBAT_WAIT_SECONDS = 10;
    public const int LOGOUT_COMBAT_GRACE_PERIOD_SECONDS = 10;

    public static readonly TimeSpan GroundItemOwnershipLock = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan GroundItemLifetime = TimeSpan.FromSeconds(300);

    public static readonly TimeSpan PlayerAutoRespawnDelay = TimeSpan.FromSeconds(180);
    public static readonly TimeSpan RestartHereMinWait = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan RestartTownMinWait = TimeSpan.FromSeconds(7);

    public static readonly TimeSpan KnockoutToDeathDelay = TimeSpan.FromSeconds(3);
    public static readonly TimeSpan MonsterDespawnAfterDeath = TimeSpan.FromSeconds(5);

    public static readonly TimeSpan PlayerAutosaveInterval = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan PlayerManaRegenInterval = TimeSpan.FromSeconds(3);
    public static readonly TimeSpan PlayerHealthRegenInterval = TimeSpan.FromSeconds(3);
}
