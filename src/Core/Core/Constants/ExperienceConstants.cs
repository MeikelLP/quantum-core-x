namespace QuantumCore.Core.Constants;

public static class ExperienceConstants
{
    public static float GetExperiencePercentageByLevelDifference(uint playerLevel, uint entityLevel)
    {
        var diff = (int)entityLevel - playerLevel;

        return diff switch {
            -15 or < -15 => .01f,
            -14 => .03f,
            -13 => .05f,
            -12 => .07f,
            -11 => .15f,
            -10 => .30f,
            -9 => .60f,
            -8 => .90f,
            -7 => .91f,
            -6 => .92f,
            -5 => .93f,
            -4 => .94f,
            -3 => .95f,
            -2 => .97f,
            -1 => .99f,
            0 => 1.00f,
            1 => 1.05f,
            2 => 1.10f,
            3 => 1.15f,
            4 => 1.20f,
            5 => 1.25f,
            6 => 1.30f,
            7 => 1.35f,
            8 => 1.40f,
            9 => 1.45f,
            10 => 1.50f,
            11 => 1.55f,
            12 => 1.60f,
            13 => 1.65f,
            14 => 1.70f,
            15 or > 15 => 1.80f
        };
    }
}