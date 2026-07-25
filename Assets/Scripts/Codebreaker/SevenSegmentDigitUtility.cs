using System;
using System.Collections.Generic;

public static class SevenSegmentDigitUtility
{
    private static readonly LayeredSegmentPosition[][] DigitSegments =
    {
        new[]
        {
            LayeredSegmentPosition.A,
            LayeredSegmentPosition.B,
            LayeredSegmentPosition.C,
            LayeredSegmentPosition.D,
            LayeredSegmentPosition.E,
            LayeredSegmentPosition.F
        },
        new[]
        {
            LayeredSegmentPosition.B,
            LayeredSegmentPosition.C
        },
        new[]
        {
            LayeredSegmentPosition.A,
            LayeredSegmentPosition.B,
            LayeredSegmentPosition.D,
            LayeredSegmentPosition.E,
            LayeredSegmentPosition.G
        },
        new[]
        {
            LayeredSegmentPosition.A,
            LayeredSegmentPosition.B,
            LayeredSegmentPosition.C,
            LayeredSegmentPosition.D,
            LayeredSegmentPosition.G
        },
        new[]
        {
            LayeredSegmentPosition.B,
            LayeredSegmentPosition.C,
            LayeredSegmentPosition.F,
            LayeredSegmentPosition.G
        },
        new[]
        {
            LayeredSegmentPosition.A,
            LayeredSegmentPosition.C,
            LayeredSegmentPosition.D,
            LayeredSegmentPosition.F,
            LayeredSegmentPosition.G
        },
        new[]
        {
            LayeredSegmentPosition.A,
            LayeredSegmentPosition.C,
            LayeredSegmentPosition.D,
            LayeredSegmentPosition.E,
            LayeredSegmentPosition.F,
            LayeredSegmentPosition.G
        },
        new[]
        {
            LayeredSegmentPosition.A,
            LayeredSegmentPosition.B,
            LayeredSegmentPosition.C
        },
        new[]
        {
            LayeredSegmentPosition.A,
            LayeredSegmentPosition.B,
            LayeredSegmentPosition.C,
            LayeredSegmentPosition.D,
            LayeredSegmentPosition.E,
            LayeredSegmentPosition.F,
            LayeredSegmentPosition.G
        },
        new[]
        {
            LayeredSegmentPosition.A,
            LayeredSegmentPosition.B,
            LayeredSegmentPosition.C,
            LayeredSegmentPosition.D,
            LayeredSegmentPosition.F,
            LayeredSegmentPosition.G
        }
    };

    public static bool TryGetDigit(
        IReadOnlyCollection<LayeredSegmentPosition> activeSegments,
        out int digit)
    {
        digit = -1;

        if (activeSegments == null)
        {
            return false;
        }

        bool[] active = new bool[7];
        int uniqueCount = 0;

        foreach (LayeredSegmentPosition position in activeSegments)
        {
            int index = (int)position;

            if (index < 0 || index >= active.Length || active[index])
            {
                continue;
            }

            active[index] = true;
            uniqueCount++;
        }

        for (int candidate = 0; candidate < DigitSegments.Length; candidate++)
        {
            IReadOnlyList<LayeredSegmentPosition> pattern =
                DigitSegments[candidate];

            if (pattern.Count != uniqueCount)
            {
                continue;
            }

            bool matches = true;

            for (int positionIndex = 0;
                positionIndex < active.Length;
                positionIndex++)
            {
                if (active[positionIndex] !=
                    IsSegmentActive(
                        candidate,
                        (LayeredSegmentPosition)positionIndex))
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                digit = candidate;
                return true;
            }
        }

        return false;
    }

    public static bool IsSegmentActive(
        int digit,
        LayeredSegmentPosition position)
    {
        if (digit < 0 ||
            digit >= DigitSegments.Length ||
            !Enum.IsDefined(typeof(LayeredSegmentPosition), position))
        {
            return false;
        }

        IReadOnlyList<LayeredSegmentPosition> pattern =
            DigitSegments[digit];

        for (int i = 0; i < pattern.Count; i++)
        {
            if (pattern[i] == position)
            {
                return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<LayeredSegmentPosition>
        GetActiveSegments(int digit)
    {
        if (digit < 0 || digit >= DigitSegments.Length)
        {
            return Array.Empty<LayeredSegmentPosition>();
        }

        return Array.AsReadOnly(DigitSegments[digit]);
    }
}
