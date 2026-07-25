using System;
using System.Collections.Generic;

public sealed class LayeredDigitPuzzleSolution
{
    private readonly int[] hitAllocation;
    private readonly LayeredSegmentColor[] finalStates;
    private readonly IReadOnlyList<int> readOnlyHitAllocation;
    private readonly IReadOnlyList<LayeredSegmentColor> readOnlyFinalStates;

    public int Digit { get; }
    public LayeredSegmentColor Color { get; }
    public IReadOnlyList<int> HitAllocation => readOnlyHitAllocation;
    public IReadOnlyList<LayeredSegmentColor> FinalStates =>
        readOnlyFinalStates;

    public LayeredDigitPuzzleSolution(
        int digit,
        LayeredSegmentColor color,
        IReadOnlyList<int> hits,
        IReadOnlyList<LayeredSegmentColor> states)
    {
        if (hits == null || hits.Count != 7)
        {
            throw new ArgumentException(
                "A solution requires seven hit allocations.",
                nameof(hits));
        }

        if (states == null || states.Count != 7)
        {
            throw new ArgumentException(
                "A solution requires seven final states.",
                nameof(states));
        }

        Digit = digit;
        Color = color;
        hitAllocation = new int[7];
        finalStates = new LayeredSegmentColor[7];

        for (int i = 0; i < 7; i++)
        {
            hitAllocation[i] = hits[i];
            finalStates[i] = states[i];
        }

        readOnlyHitAllocation = Array.AsReadOnly(hitAllocation);
        readOnlyFinalStates = Array.AsReadOnly(finalStates);
    }

    public int GetHits(LayeredSegmentPosition position)
    {
        return hitAllocation[(int)position];
    }

    public LayeredSegmentColor GetFinalState(
        LayeredSegmentPosition position)
    {
        return finalStates[(int)position];
    }
}
