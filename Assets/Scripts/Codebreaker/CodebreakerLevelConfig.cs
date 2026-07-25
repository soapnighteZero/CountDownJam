using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CodebreakerLevel",
    menuName = "CountDownJam/Codebreaker Level")]
public class CodebreakerLevelConfig : ScriptableObject
{
    [Header("Identification")]
    [SerializeField] private string levelId = string.Empty;
    [SerializeField] private string displayName = string.Empty;

    [Header("Code")]
    [SerializeField] private int[] expectedCodeDigits = Array.Empty<int>();

    [Header("Difficulty Estimation")]
    [SerializeField] private CodebreakerDifficulty difficulty =
        CodebreakerDifficulty.Normal;
    [SerializeField, Min(0)] private int totalDiscoveryHitBudget;
    [SerializeField, Min(0)] private int estimatedEquationMoveCount;
    [SerializeField] private float extraAuthoredSeconds;

    [Header("Manual Override")]
    [SerializeField] private bool useManualTimeOverride;
    [SerializeField, Min(0.01f)] private float manualTimeSeconds = 60f;

    public string LevelId => levelId;
    public string DisplayName => displayName;
    public IReadOnlyList<int> ExpectedCodeDigits =>
        expectedCodeDigits ?? Array.Empty<int>();
    public int CodeDigitCount => expectedCodeDigits?.Length ?? 0;
    public CodebreakerDifficulty Difficulty => difficulty;
    public int TotalDiscoveryHitBudget => totalDiscoveryHitBudget;
    public int EstimatedEquationMoveCount => estimatedEquationMoveCount;
    public float ExtraAuthoredSeconds => extraAuthoredSeconds;
    public bool UseManualTimeOverride => useManualTimeOverride;
    public float ManualTimeSeconds => manualTimeSeconds;

    public float GetStartingTimeSeconds()
    {
        return CodebreakerTimeCalculator.CalculateStartingTimeSeconds(this);
    }

    public bool ValidateConfiguration(out string errorMessage)
    {
        if (expectedCodeDigits == null || expectedCodeDigits.Length == 0)
        {
            errorMessage =
                $"{name} must contain at least one expected code digit.";
            return false;
        }

        for (int i = 0; i < expectedCodeDigits.Length; i++)
        {
            int digit = expectedCodeDigits[i];

            if (digit < 0 || digit > 9)
            {
                errorMessage =
                    $"{name} contains digit {digit} at index {i}; " +
                    "digits must be between 0 and 9.";
                return false;
            }
        }

        if (!Enum.IsDefined(typeof(CodebreakerDifficulty), difficulty))
        {
            errorMessage =
                $"{name} has an unsupported difficulty value: {difficulty}.";
            return false;
        }

        if (totalDiscoveryHitBudget < 0)
        {
            errorMessage =
                $"{name} has a negative total discovery hit budget.";
            return false;
        }

        if (estimatedEquationMoveCount < 0)
        {
            errorMessage =
                $"{name} has a negative estimated equation move count.";
            return false;
        }

        if (!IsFinite(extraAuthoredSeconds))
        {
            errorMessage =
                $"{name} has a non-finite authored time adjustment.";
            return false;
        }

        if (!IsFinite(manualTimeSeconds) || manualTimeSeconds <= 0f)
        {
            errorMessage =
                $"{name} manual time must be a positive, finite value.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private void OnValidate()
    {
        totalDiscoveryHitBudget = Mathf.Max(0, totalDiscoveryHitBudget);
        estimatedEquationMoveCount =
            Mathf.Max(0, estimatedEquationMoveCount);

        if (!IsFinite(extraAuthoredSeconds))
        {
            extraAuthoredSeconds = 0f;
        }
        else
        {
            extraAuthoredSeconds = Mathf.Clamp(
                extraAuthoredSeconds,
                -600f,
                600f);
        }

        if (!IsFinite(manualTimeSeconds))
        {
            manualTimeSeconds = 60f;
        }
        else
        {
            manualTimeSeconds = Mathf.Clamp(
                manualTimeSeconds,
                CodebreakerTimeCalculator.MinimumManualTimeSeconds,
                CodebreakerTimeCalculator.MaximumManualTimeSeconds);
        }
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
