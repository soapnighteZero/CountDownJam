using System;
using System.Collections.Generic;

public static class LayeredSegmentColorProgression
{
    public static int GetRemainingDepth(LayeredSegmentColor color)
    {
        switch (color)
        {
            case LayeredSegmentColor.Red:
                return 3;
            case LayeredSegmentColor.Yellow:
                return 2;
            case LayeredSegmentColor.Green:
                return 1;
            case LayeredSegmentColor.Gray:
                return 0;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(color),
                    color,
                    "Unsupported layered segment color.");
        }
    }

    public static bool TryGetNextColor(
        LayeredSegmentColor current,
        out LayeredSegmentColor next)
    {
        switch (current)
        {
            case LayeredSegmentColor.Red:
                next = LayeredSegmentColor.Yellow;
                return true;
            case LayeredSegmentColor.Yellow:
                next = LayeredSegmentColor.Green;
                return true;
            case LayeredSegmentColor.Green:
                next = LayeredSegmentColor.Gray;
                return true;
            default:
                next = default;
                return false;
        }
    }

    public static bool IsCanonicalSequence(
        IReadOnlyList<LayeredSegmentColor> states,
        out string errorMessage)
    {
        if (states == null)
        {
            errorMessage = "Sequence has a null states array.";
            return false;
        }

        if (states.Count == 0)
        {
            errorMessage = "Sequence requires at least one state.";
            return false;
        }

        if (states.Count > 4)
        {
            errorMessage =
                $"Sequence has {states.Count} states; the canonical " +
                "progression supports at most four.";
            return false;
        }

        for (int i = 0; i < states.Count; i++)
        {
            LayeredSegmentColor color = states[i];

            if (!Enum.IsDefined(typeof(LayeredSegmentColor), color))
            {
                errorMessage =
                    $"Sequence has invalid color {color} at state {i}.";
                return false;
            }

            if (i > 0 && color == states[i - 1])
            {
                errorMessage = $"Sequence repeats {color}.";
                return false;
            }

            if (i < states.Count - 1 &&
                color == LayeredSegmentColor.Gray)
            {
                errorMessage = "Sequence continues after Gray.";
                return false;
            }
        }

        LayeredSegmentColor finalColor = states[states.Count - 1];

        if (finalColor != LayeredSegmentColor.Gray)
        {
            errorMessage = "Sequence must terminate in Gray.";
            return false;
        }

        for (int i = 0; i < states.Count - 1; i++)
        {
            LayeredSegmentColor current = states[i];
            LayeredSegmentColor actualNext = states[i + 1];

            if (!TryGetNextColor(current, out LayeredSegmentColor expectedNext))
            {
                errorMessage = $"Sequence continues after {current}.";
                return false;
            }

            if (actualNext == expectedNext)
            {
                continue;
            }

            int currentDepth = GetRemainingDepth(current);
            int actualDepth = GetRemainingDepth(actualNext);

            if (actualDepth < currentDepth - 1)
            {
                errorMessage =
                    $"Sequence skips {expectedNext} between {current} " +
                    $"and {actualNext}.";
            }
            else
            {
                errorMessage =
                    $"Sequence reverses from {current} to {actualNext}.";
            }

            return false;
        }

        int expectedStateCount =
            GetRemainingDepth(states[0]) + 1;

        if (states.Count != expectedStateCount)
        {
            errorMessage =
                $"Sequence beginning with {states[0]} must contain " +
                $"{expectedStateCount} states and terminate in Gray.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
