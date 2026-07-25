using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LayeredSegmentStackDefinition
{
    [SerializeField] private LayeredSegmentPosition position;
    [SerializeField] private LayeredSegmentColor[] states =
        Array.Empty<LayeredSegmentColor>();

    public LayeredSegmentPosition Position => position;
    public IReadOnlyList<LayeredSegmentColor> States =>
        Array.AsReadOnly(
            states ?? Array.Empty<LayeredSegmentColor>());
    public int MaximumHits => Mathf.Max(0, (states?.Length ?? 0) - 1);

    public LayeredSegmentColor GetStateAfterHits(int hitCount)
    {
        if (hitCount < 0 || hitCount > MaximumHits)
        {
            throw new ArgumentOutOfRangeException(
                nameof(hitCount),
                hitCount,
                $"Hit count must be between 0 and {MaximumHits}.");
        }

        return states[hitCount];
    }

    public bool ValidateConfiguration(out string errorMessage)
    {
        if (!Enum.IsDefined(typeof(LayeredSegmentPosition), position))
        {
            errorMessage =
                $"Layered segment stack has invalid position {position}.";
            return false;
        }

        if (states == null)
        {
            errorMessage =
                $"Layered segment stack {position} has a null states array.";
            return false;
        }

        if (states.Length == 0)
        {
            errorMessage =
                $"Layered segment stack {position} requires at least one state.";
            return false;
        }

        if (states.Length > 4)
        {
            errorMessage =
                $"Stack {position} has {states.Length} states; the canonical " +
                "progression supports at most four.";
            return false;
        }

        for (int i = 0; i < states.Length; i++)
        {
            if (!Enum.IsDefined(typeof(LayeredSegmentColor), states[i]))
            {
                errorMessage =
                    $"Layered segment stack {position} has invalid color " +
                    $"{states[i]} at state {i}.";
                return false;
            }
        }

        if (!LayeredSegmentColorProgression.IsCanonicalSequence(
                states,
                out string progressionError))
        {
            errorMessage = ReplaceSequenceSubject(progressionError);
            return false;
        }

        int canonicalMaximumHits =
            LayeredSegmentColorProgression.GetRemainingDepth(states[0]);

        if (MaximumHits != canonicalMaximumHits)
        {
            errorMessage =
                $"Stack {position} has MaximumHits {MaximumHits}, but " +
                $"{states[0]} requires {canonicalMaximumHits}.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private string ReplaceSequenceSubject(string progressionError)
    {
        const string sequenceSubject = "Sequence";
        string stackSubject = $"Stack {position}";

        if (progressionError.StartsWith(
                sequenceSubject,
                StringComparison.Ordinal))
        {
            return stackSubject +
                progressionError.Substring(sequenceSubject.Length);
        }

        return $"{stackSubject}: {progressionError}";
    }
}
