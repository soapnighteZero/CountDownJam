using UnityEngine;

public class SevenSegmentDisplay : MonoBehaviour
{
    [Header("Segments: A, B, C, D, E, F, G")]
    [SerializeField] private SevenSegmentPiece segmentA;
    [SerializeField] private SevenSegmentPiece segmentB;
    [SerializeField] private SevenSegmentPiece segmentC;
    [SerializeField] private SevenSegmentPiece segmentD;
    [SerializeField] private SevenSegmentPiece segmentE;
    [SerializeField] private SevenSegmentPiece segmentF;
    [SerializeField] private SevenSegmentPiece segmentG;

    private SevenSegmentPiece[] segments;

    // Segment order: A, B, C, D, E, F, G
    private readonly bool[][] digitPatterns =
    {
        // 0
        new[] { true,  true,  true,  true,  true,  true,  false },

        // 1
        new[] { false, true,  true,  false, false, false, false },

        // 2
        new[] { true,  true,  false, true,  true,  false, true  },

        // 3
        new[] { true,  true,  true,  true,  false, false, true  },

        // 4
        new[] { false, true,  true,  false, false, true,  true  },

        // 5
        new[] { true,  false, true,  true,  false, true,  true  },

        // 6
        new[] { true,  false, true,  true,  true,  true,  true  },

        // 7
        new[] { true,  true,  true,  false, false, false, false },

        // 8
        new[] { true,  true,  true,  true,  true,  true,  true  },

        // 9
        new[] { true,  true,  true,  true,  false, true,  true  }
    };

    public int ActiveSegmentCount
    {
        get
        {
            int count = 0;

            if (segments == null)
            {
                InitializeSegments();
            }

            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] != null && segments[i].IsActive)
                {
                    count++;
                }
            }

            return count;
        }
    }

    private void Awake()
    {
        InitializeSegments();
    }

    private void InitializeSegments()
    {
        segments = new[]
        {
            segmentA,
            segmentB,
            segmentC,
            segmentD,
            segmentE,
            segmentF,
            segmentG
        };
    }

    public int GetRequiredSegmentCount(int digit)
    {
        if (digit < 0 || digit >= digitPatterns.Length)
        {
            return -1;
        }

        int count = 0;
        bool[] pattern = digitPatterns[digit];

        for (int i = 0; i < pattern.Length; i++)
        {
            if (pattern[i])
            {
                count++;
            }
        }

        return count;
    }

    public void SetDigit(int digit)
    {
        if (digit < 0 || digit > 9)
        {
            Debug.LogError($"Invalid digit: {digit}", this);
            return;
        }

        bool[] pattern = digitPatterns[digit];

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null)
            {
                Debug.LogError(
                    $"Segment index {i} is not assigned.",
                    this
                );

                continue;
            }

            segments[i].SetActiveState(pattern[i]);
        }
    }

    public bool TryGetCurrentDigit(out int digit)
    {
        for (int candidate = 0; candidate <= 9; candidate++)
        {
            bool[] pattern = digitPatterns[candidate];
            bool matches = true;

            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] == null ||
                    segments[i].IsActive != pattern[i])
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

        digit = -1;
        return false;
    }
}
