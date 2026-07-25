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

        for (int i = 0; i < states.Length; i++)
        {
            if (!Enum.IsDefined(typeof(LayeredSegmentColor), states[i]))
            {
                errorMessage =
                    $"Layered segment stack {position} has invalid color " +
                    $"{states[i]} at state {i}.";
                return false;
            }

            if (i > 0 && states[i] == states[i - 1])
            {
                errorMessage =
                    $"Layered segment stack {position} repeats {states[i]} " +
                    $"in adjacent states {i - 1} and {i}.";
                return false;
            }
        }

        errorMessage = string.Empty;
        return true;
    }
}
