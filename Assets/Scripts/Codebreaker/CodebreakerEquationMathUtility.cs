using System;
using System.Collections.Generic;
using UnityEngine;

public static class CodebreakerEquationMathUtility
{
    public static int GetRequiredSegmentCount(int digit)
    {
        if (digit < 0 || digit > 9)
        {
            return -1;
        }

        return SevenSegmentDigitUtility.GetActiveSegments(digit).Count;
    }

    public static int GetRequiredSegmentCount(int digitA, int digitB)
    {
        int countA = GetRequiredSegmentCount(digitA);
        int countB = GetRequiredSegmentCount(digitB);

        if (countA < 0 || countB < 0)
        {
            return -1;
        }

        return countA + countB;
    }

    public static bool IsValidEquation(
        int digitA,
        int digitB,
        int targetDigit)
    {
        return IsDigit(digitA) &&
            IsDigit(digitB) &&
            IsDigit(targetDigit) &&
            digitA + digitB == targetDigit;
    }

    public static bool HasSolution(
        int targetDigit,
        int availableSegmentCount)
    {
        return FindSolutions(targetDigit, availableSegmentCount).Count > 0;
    }

    public static bool HasSolution(
        int targetDigit,
        int availableSegmentCount,
        int trayCapacity)
    {
        return FindSolutions(
            targetDigit,
            availableSegmentCount,
            trayCapacity).Count > 0;
    }

    public static IReadOnlyList<Vector2Int> FindSolutions(
        int targetDigit,
        int availableSegmentCount)
    {
        return FindSolutions(
            targetDigit,
            availableSegmentCount,
            availableSegmentCount);
    }

    public static IReadOnlyList<Vector2Int> FindSolutions(
        int targetDigit,
        int availableSegmentCount,
        int trayCapacity)
    {
        if (!IsDigit(targetDigit) ||
            availableSegmentCount < 0 ||
            trayCapacity < 0)
        {
            return Array.Empty<Vector2Int>();
        }

        List<Vector2Int> solutions = new List<Vector2Int>();

        for (int digitA = 0; digitA <= 9; digitA++)
        {
            for (int digitB = 0; digitB <= 9; digitB++)
            {
                if (!IsValidEquation(digitA, digitB, targetDigit))
                {
                    continue;
                }

                int displaySegmentCount =
                    GetRequiredSegmentCount(digitA, digitB);

                if (displaySegmentCount > availableSegmentCount)
                {
                    continue;
                }

                int traySegmentCount =
                    availableSegmentCount - displaySegmentCount;

                if (traySegmentCount < 0 ||
                    traySegmentCount > trayCapacity)
                {
                    continue;
                }

                solutions.Add(new Vector2Int(digitA, digitB));
            }
        }

        return solutions.AsReadOnly();
    }

    private static bool IsDigit(int value)
    {
        return value >= 0 && value <= 9;
    }
}
