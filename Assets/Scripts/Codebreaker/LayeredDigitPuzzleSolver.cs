using System;
using System.Collections.Generic;

public static class LayeredDigitPuzzleSolver
{
    public static IReadOnlyList<LayeredDigitPuzzleSolution>
        FindValidSolutions(LayeredDigitPuzzleConfig config)
    {
        if (config == null || config.HitBudget < 0)
        {
            return Array.Empty<LayeredDigitPuzzleSolution>();
        }

        LayeredSegmentStackDefinition[] stacks =
            new LayeredSegmentStackDefinition[7];

        for (int i = 0; i < stacks.Length; i++)
        {
            if (!config.TryGetStack(
                    (LayeredSegmentPosition)i,
                    out LayeredSegmentStackDefinition stack) ||
                stack == null ||
                !stack.ValidateConfiguration(out _))
            {
                return Array.Empty<LayeredDigitPuzzleSolution>();
            }

            stacks[i] = stack;
        }

        var solutions = new List<LayeredDigitPuzzleSolution>();
        int[] hitAllocation = new int[7];
        EnumerateAllocations(
            stacks,
            0,
            config.HitBudget,
            hitAllocation,
            solutions);
        return solutions.AsReadOnly();
    }

    private static void EnumerateAllocations(
        IReadOnlyList<LayeredSegmentStackDefinition> stacks,
        int stackIndex,
        int hitsRemaining,
        int[] hitAllocation,
        ICollection<LayeredDigitPuzzleSolution> solutions)
    {
        if (stackIndex == stacks.Count)
        {
            if (hitsRemaining == 0 &&
                TryResolve(
                    stacks,
                    hitAllocation,
                    out LayeredDigitPuzzleSolution solution))
            {
                solutions.Add(solution);
            }

            return;
        }

        int maximumForStack = Math.Min(
            stacks[stackIndex].MaximumHits,
            hitsRemaining);

        for (int hits = 0; hits <= maximumForStack; hits++)
        {
            hitAllocation[stackIndex] = hits;
            EnumerateAllocations(
                stacks,
                stackIndex + 1,
                hitsRemaining - hits,
                hitAllocation,
                solutions);
        }
    }

    private static bool TryResolve(
        IReadOnlyList<LayeredSegmentStackDefinition> stacks,
        IReadOnlyList<int> hitAllocation,
        out LayeredDigitPuzzleSolution solution)
    {
        solution = null;
        var activeSegments = new List<LayeredSegmentPosition>(7);
        var finalStates = new LayeredSegmentColor[7];
        LayeredSegmentColor resolvedColor = LayeredSegmentColor.Gray;

        for (int i = 0; i < stacks.Count; i++)
        {
            LayeredSegmentColor state =
                stacks[i].GetStateAfterHits(hitAllocation[i]);
            finalStates[i] = state;

            if (state == LayeredSegmentColor.Gray)
            {
                continue;
            }

            if (resolvedColor == LayeredSegmentColor.Gray)
            {
                resolvedColor = state;
            }
            else if (resolvedColor != state)
            {
                return false;
            }

            activeSegments.Add((LayeredSegmentPosition)i);
        }

        if (activeSegments.Count == 0 ||
            !SevenSegmentDigitUtility.TryGetDigit(
                activeSegments,
                out int digit))
        {
            return false;
        }

        solution = new LayeredDigitPuzzleSolution(
            digit,
            resolvedColor,
            hitAllocation,
            finalStates);
        return true;
    }
}
