using UnityEngine;
using UnityEngine.InputSystem;
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

    // Segment order:
    // A, B, C, D, E, F, G
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

    private void Awake()
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

    private void Start()
    {
        SetDigit(8);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        // 键盘顶部数字键和小键盘都支持
        if (keyboard.digit0Key.wasPressedThisFrame ||
            keyboard.numpad0Key.wasPressedThisFrame)
        {
            SetDigit(0);
        }

        if (keyboard.digit1Key.wasPressedThisFrame ||
            keyboard.numpad1Key.wasPressedThisFrame)
        {
            SetDigit(1);
        }

        if (keyboard.digit2Key.wasPressedThisFrame ||
            keyboard.numpad2Key.wasPressedThisFrame)
        {
            SetDigit(2);
        }

        if (keyboard.digit3Key.wasPressedThisFrame ||
            keyboard.numpad3Key.wasPressedThisFrame)
        {
            SetDigit(3);
        }

        if (keyboard.digit4Key.wasPressedThisFrame ||
            keyboard.numpad4Key.wasPressedThisFrame)
        {
            SetDigit(4);
        }

        if (keyboard.digit5Key.wasPressedThisFrame ||
            keyboard.numpad5Key.wasPressedThisFrame)
        {
            SetDigit(5);
        }

        if (keyboard.digit6Key.wasPressedThisFrame ||
            keyboard.numpad6Key.wasPressedThisFrame)
        {
            SetDigit(6);
        }

        if (keyboard.digit7Key.wasPressedThisFrame ||
            keyboard.numpad7Key.wasPressedThisFrame)
        {
            SetDigit(7);
        }

        if (keyboard.digit8Key.wasPressedThisFrame ||
            keyboard.numpad8Key.wasPressedThisFrame)
        {
            SetDigit(8);
        }

        if (keyboard.digit9Key.wasPressedThisFrame ||
            keyboard.numpad9Key.wasPressedThisFrame)
        {
            SetDigit(9);
        }
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
}