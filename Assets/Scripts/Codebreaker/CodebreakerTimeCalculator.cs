using System;

public readonly struct CodebreakerTimeBreakdown
{
    public float BaseSeconds { get; }
    public float DiscoverySeconds { get; }
    public float EquationMovementSeconds { get; }
    public float CodePlanningSeconds { get; }
    public float PhaseTransitionSeconds { get; }
    public float AuthoredAdjustmentSeconds { get; }
    public float RawSeconds { get; }
    public float DifficultyMultiplier { get; }
    public float ScaledSeconds { get; }
    public float FinalSeconds { get; }
    public bool UsedManualOverride { get; }

    internal CodebreakerTimeBreakdown(
        float baseSeconds,
        float discoverySeconds,
        float equationMovementSeconds,
        float codePlanningSeconds,
        float phaseTransitionSeconds,
        float authoredAdjustmentSeconds,
        float rawSeconds,
        float difficultyMultiplier,
        float scaledSeconds,
        float finalSeconds,
        bool usedManualOverride)
    {
        BaseSeconds = baseSeconds;
        DiscoverySeconds = discoverySeconds;
        EquationMovementSeconds = equationMovementSeconds;
        CodePlanningSeconds = codePlanningSeconds;
        PhaseTransitionSeconds = phaseTransitionSeconds;
        AuthoredAdjustmentSeconds = authoredAdjustmentSeconds;
        RawSeconds = rawSeconds;
        DifficultyMultiplier = difficultyMultiplier;
        ScaledSeconds = scaledSeconds;
        FinalSeconds = finalSeconds;
        UsedManualOverride = usedManualOverride;
    }
}

public static class CodebreakerTimeCalculator
{
    public const float BaseOverheadSeconds = 15f;
    public const float SecondsPerDiscoveryHit = 1.5f;
    public const float SecondsPerEquationMove = 2.5f;
    public const float SecondsPerCodeDigit = 12f;
    public const float PhaseTransitionSeconds = 8f;

    public const float MinimumCalculatedTimeSeconds = 45f;
    public const float MaximumCalculatedTimeSeconds = 240f;
    public const float CalculatedRoundingIncrementSeconds = 5f;

    public const float MinimumManualTimeSeconds = 10f;
    public const float MaximumManualTimeSeconds = 600f;

    public static float CalculateStartingTimeSeconds(
        CodebreakerLevelConfig config)
    {
        return CalculateBreakdown(config).FinalSeconds;
    }

    public static CodebreakerTimeBreakdown CalculateBreakdown(
        CodebreakerLevelConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (!config.ValidateConfiguration(out string errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(config));
        }

        float baseSeconds = BaseOverheadSeconds;
        float discoverySeconds =
            config.TotalDiscoveryHitBudget * SecondsPerDiscoveryHit;
        float equationSeconds =
            config.EstimatedEquationMoveCount * SecondsPerEquationMove;
        float planningSeconds =
            config.CodeDigitCount * SecondsPerCodeDigit;
        float transitionSeconds = PhaseTransitionSeconds;
        float authoredSeconds = config.ExtraAuthoredSeconds;
        float rawSeconds =
            baseSeconds +
            discoverySeconds +
            equationSeconds +
            planningSeconds +
            transitionSeconds +
            authoredSeconds;
        float multiplier = GetDifficultyMultiplier(config.Difficulty);
        float scaledSeconds = rawSeconds * multiplier;

        if (config.UseManualTimeOverride)
        {
            float manualSeconds = Clamp(
                config.ManualTimeSeconds,
                MinimumManualTimeSeconds,
                MaximumManualTimeSeconds);

            return new CodebreakerTimeBreakdown(
                baseSeconds,
                discoverySeconds,
                equationSeconds,
                planningSeconds,
                transitionSeconds,
                authoredSeconds,
                rawSeconds,
                multiplier,
                scaledSeconds,
                manualSeconds,
                true);
        }

        float clampedSeconds = Clamp(
            scaledSeconds,
            MinimumCalculatedTimeSeconds,
            MaximumCalculatedTimeSeconds);
        float roundedSeconds = (float)(
            Math.Round(
                clampedSeconds / CalculatedRoundingIncrementSeconds,
                MidpointRounding.AwayFromZero) *
            CalculatedRoundingIncrementSeconds);

        return new CodebreakerTimeBreakdown(
            baseSeconds,
            discoverySeconds,
            equationSeconds,
            planningSeconds,
            transitionSeconds,
            authoredSeconds,
            rawSeconds,
            multiplier,
            scaledSeconds,
            roundedSeconds,
            false);
    }

    public static float GetDifficultyMultiplier(
        CodebreakerDifficulty difficulty)
    {
        switch (difficulty)
        {
            case CodebreakerDifficulty.Tutorial:
                return 1.35f;
            case CodebreakerDifficulty.Easy:
                return 1.15f;
            case CodebreakerDifficulty.Normal:
                return 1f;
            case CodebreakerDifficulty.Hard:
                return 0.85f;
            case CodebreakerDifficulty.Expert:
                return 0.72f;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(difficulty),
                    difficulty,
                    "Unsupported Codebreaker difficulty.");
        }
    }

    private static float Clamp(float value, float minimum, float maximum)
    {
        return Math.Min(Math.Max(value, minimum), maximum);
    }
}
