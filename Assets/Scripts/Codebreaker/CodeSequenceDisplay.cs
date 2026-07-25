using System;
using TMPro;
using UnityEngine;

public class CodeSequenceDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;

    private int?[] digits = Array.Empty<int?>();

    public int DigitCount => digits.Length;
    public int RevealedDigitCount { get; private set; }
    public bool IsComplete =>
        DigitCount > 0 && RevealedDigitCount == DigitCount;

    public event Action CodeDisplayChanged;

    private void Awake()
    {
        ValidateReferences();
    }

    public bool ValidateReferences()
    {
        if (displayText != null)
        {
            return true;
        }

        Debug.LogError(
            "CodeSequenceDisplay is missing its TMP text reference.",
            this);
        return false;
    }

    public void Initialize(int digitCount)
    {
        if (digitCount < 1)
        {
            Debug.LogError(
                "CodeSequenceDisplay requires at least one digit.",
                this);
            digits = Array.Empty<int?>();
            RevealedDigitCount = 0;
            Render();
            return;
        }

        digits = new int?[digitCount];
        RevealedDigitCount = 0;
        Render();
        CodeDisplayChanged?.Invoke();
    }

    public bool RevealDigit(int index, int digit)
    {
        if (index < 0 || index >= digits.Length)
        {
            return false;
        }

        if (digit < 0 || digit > 9)
        {
            Debug.LogError(
                $"CodeSequenceDisplay rejected invalid digit {digit}.",
                this);
            return false;
        }

        if (digits[index].HasValue)
        {
            return false;
        }

        digits[index] = digit;
        RevealedDigitCount++;
        Render();
        CodeDisplayChanged?.Invoke();
        return true;
    }

    public bool RevealNextDigit(int digit)
    {
        for (int i = 0; i < digits.Length; i++)
        {
            if (!digits[i].HasValue)
            {
                return RevealDigit(i, digit);
            }
        }

        return false;
    }

    public void Clear()
    {
        for (int i = 0; i < digits.Length; i++)
        {
            digits[i] = null;
        }

        RevealedDigitCount = 0;
        Render();
        CodeDisplayChanged?.Invoke();
    }

    public bool TryGetDigit(int index, out int digit)
    {
        digit = 0;

        if (index < 0 ||
            index >= digits.Length ||
            !digits[index].HasValue)
        {
            return false;
        }

        digit = digits[index].Value;
        return true;
    }

    private void Render()
    {
        if (displayText == null)
        {
            return;
        }

        string[] renderedDigits = new string[digits.Length];

        for (int i = 0; i < digits.Length; i++)
        {
            renderedDigits[i] = digits[i]?.ToString() ?? "?";
        }

        displayText.text = string.Join(" ", renderedDigits);
    }
}
