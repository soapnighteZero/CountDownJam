using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LayeredDigitPuzzle",
    menuName = "CountDownJam/Layered Digit Puzzle")]
public class LayeredDigitPuzzleConfig : ScriptableObject
{
    [SerializeField] private string puzzleId = string.Empty;
    [SerializeField] private int targetCodeIndex;
    [SerializeField] private int hitBudget = 1;
    [SerializeField] private int expectedDigit;
    [SerializeField] private LayeredSegmentColor expectedColor =
        LayeredSegmentColor.Yellow;
    [SerializeField] private float failureTimePenaltySeconds;
    [SerializeField] private float failureResetDelaySeconds = 1f;
    [SerializeField] private bool requireUniqueSolution = true;
    [SerializeField] private LayeredSegmentStackDefinition[] segmentStacks =
        Array.Empty<LayeredSegmentStackDefinition>();

    public string PuzzleId => puzzleId;
    public int TargetCodeIndex => targetCodeIndex;
    public int HitBudget => hitBudget;
    public int ExpectedDigit => expectedDigit;
    public LayeredSegmentColor ExpectedColor => expectedColor;
    public float FailureTimePenaltySeconds => failureTimePenaltySeconds;
    public float FailureResetDelaySeconds => failureResetDelaySeconds;
    public bool RequireUniqueSolution => requireUniqueSolution;
    public IReadOnlyList<LayeredSegmentStackDefinition> SegmentStacks =>
        Array.AsReadOnly(
            segmentStacks ??
            Array.Empty<LayeredSegmentStackDefinition>());

    public bool TryGetStack(
        LayeredSegmentPosition position,
        out LayeredSegmentStackDefinition stack)
    {
        stack = null;

        if (segmentStacks == null)
        {
            return false;
        }

        for (int i = 0; i < segmentStacks.Length; i++)
        {
            LayeredSegmentStackDefinition candidate = segmentStacks[i];

            if (candidate != null && candidate.Position == position)
            {
                stack = candidate;
                return true;
            }
        }

        return false;
    }

    public bool ValidateConfiguration(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(puzzleId))
        {
            errorMessage = $"{name} has a blank puzzleId.";
            return false;
        }

        if (targetCodeIndex < 0)
        {
            errorMessage = $"{name} has a negative targetCodeIndex.";
            return false;
        }

        if (hitBudget <= 0)
        {
            errorMessage = $"{name} requires a positive hitBudget.";
            return false;
        }

        if (expectedDigit < 0 || expectedDigit > 9)
        {
            errorMessage =
                $"{name} expected digit must be between 0 and 9.";
            return false;
        }

        if (!Enum.IsDefined(
                typeof(LayeredSegmentColor),
                expectedColor) ||
            expectedColor == LayeredSegmentColor.Gray)
        {
            errorMessage =
                $"{name} expected color must be an active non-gray color.";
            return false;
        }

        if (!IsFinite(failureTimePenaltySeconds) ||
            failureTimePenaltySeconds < 0f)
        {
            errorMessage =
                $"{name} failure time penalty must be finite and " +
                "non-negative.";
            return false;
        }

        if (!IsFinite(failureResetDelaySeconds) ||
            failureResetDelaySeconds < 0f)
        {
            errorMessage =
                $"{name} failure reset delay must be finite and " +
                "non-negative.";
            return false;
        }

        if (segmentStacks == null || segmentStacks.Length != 7)
        {
            errorMessage =
                $"{name} must contain exactly seven segment stacks.";
            return false;
        }

        bool[] positionsSeen = new bool[7];
        int totalMaximumHits = 0;

        for (int i = 0; i < segmentStacks.Length; i++)
        {
            LayeredSegmentStackDefinition stack = segmentStacks[i];

            if (stack == null)
            {
                errorMessage =
                    $"{name} has a null segment stack at index {i}.";
                return false;
            }

            if (!stack.ValidateConfiguration(out errorMessage))
            {
                errorMessage = $"{name}: {errorMessage}";
                return false;
            }

            int positionIndex = (int)stack.Position;

            if (positionsSeen[positionIndex])
            {
                errorMessage =
                    $"{name} has duplicate segment position " +
                    $"{stack.Position}.";
                return false;
            }

            positionsSeen[positionIndex] = true;
            totalMaximumHits += stack.MaximumHits;
        }

        for (int i = 0; i < positionsSeen.Length; i++)
        {
            if (!positionsSeen[i])
            {
                errorMessage =
                    $"{name} has no stack for position " +
                    $"{(LayeredSegmentPosition)i}.";
                return false;
            }
        }

        if (hitBudget > totalMaximumHits)
        {
            errorMessage =
                $"{name} hit budget {hitBudget} exceeds the total " +
                $"available depth {totalMaximumHits}.";
            return false;
        }

        IReadOnlyList<LayeredDigitPuzzleSolution> solutions =
            LayeredDigitPuzzleSolver.FindValidSolutions(this);
        bool expectedReachable = false;

        for (int i = 0; i < solutions.Count; i++)
        {
            if (solutions[i].Digit == expectedDigit &&
                solutions[i].Color == expectedColor)
            {
                expectedReachable = true;
                break;
            }
        }

        if (solutions.Count == 0)
        {
            errorMessage = $"{name}: No valid solution exists.";
            return false;
        }

        if (!expectedReachable)
        {
            errorMessage =
                $"{name}: Expected {expectedColor.ToString().ToLowerInvariant()} " +
                $"digit {expectedDigit} is not reachable.";
            return false;
        }

        if (requireUniqueSolution && solutions.Count != 1)
        {
            errorMessage =
                $"{name}: Expected one unique solution but found " +
                $"{solutions.Count}.";
            return false;
        }

        if (requireUniqueSolution &&
            (solutions[0].Digit != expectedDigit ||
             solutions[0].Color != expectedColor))
        {
            errorMessage =
                $"{name}: Unique result is " +
                $"{solutions[0].Color.ToString().ToLowerInvariant()} digit " +
                $"{solutions[0].Digit} instead of " +
                $"{expectedColor.ToString().ToLowerInvariant()} digit " +
                $"{expectedDigit}.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private void OnValidate()
    {
        targetCodeIndex = Mathf.Max(0, targetCodeIndex);
        hitBudget = Mathf.Max(1, hitBudget);
        expectedDigit = Mathf.Clamp(expectedDigit, 0, 9);

        if (!IsFinite(failureTimePenaltySeconds))
        {
            failureTimePenaltySeconds = 0f;
        }
        else
        {
            failureTimePenaltySeconds =
                Mathf.Max(0f, failureTimePenaltySeconds);
        }

        if (!IsFinite(failureResetDelaySeconds))
        {
            failureResetDelaySeconds = 1f;
        }
        else
        {
            failureResetDelaySeconds =
                Mathf.Max(0f, failureResetDelaySeconds);
        }
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
