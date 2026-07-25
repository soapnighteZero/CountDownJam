using UnityEngine;

public class CodeModuleController : MonoBehaviour
{
    [Header("Code Displays")]
    [SerializeField]
    private SevenSegmentDisplay[] codeDisplays;

    [Header("Target Code")]
    [SerializeField]
    private int[] targetDigits = { 1, 1, 1 };

    public int DigitCount =>
        codeDisplays == null ? 0 : codeDisplays.Length;

    public bool IsComplete
    {
        get
        {
            if (!IsConfigurationValid(false))
            {
                return false;
            }

            for (int i = 0; i < DigitCount; i++)
            {
                if (!IsDigitCorrect(i))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public int GetTargetDigit(int index)
    {
        return targetDigits != null &&
            index >= 0 &&
            index < targetDigits.Length
                ? targetDigits[index]
                : -1;
    }

    public bool TryGetCurrentDigit(int index, out int digit)
    {
        digit = -1;

        if (codeDisplays == null ||
            index < 0 ||
            index >= codeDisplays.Length ||
            codeDisplays[index] == null)
        {
            return false;
        }

        return codeDisplays[index].TryGetCurrentDigit(out digit);
    }

    public bool IsDigitCorrect(int index)
    {
        return TryGetCurrentDigit(index, out int digit) &&
            digit == GetTargetDigit(index);
    }

    public void ClearCode()
    {
        if (codeDisplays == null)
        {
            return;
        }

        for (int i = 0; i < codeDisplays.Length; i++)
        {
            SevenSegmentDisplay display = codeDisplays[i];

            if (display == null)
            {
                continue;
            }

            SevenSegmentPiece[] pieces =
                display.GetComponentsInChildren<SevenSegmentPiece>(true);

            for (int pieceIndex = 0;
                pieceIndex < pieces.Length;
                pieceIndex++)
            {
                pieces[pieceIndex].SetActiveState(false);
            }
        }
    }

    public bool ValidateConfiguration()
    {
        return IsConfigurationValid(true);
    }

    public bool ContainsDisplay(SevenSegmentDisplay display)
    {
        if (display == null || codeDisplays == null)
        {
            return false;
        }

        for (int i = 0; i < codeDisplays.Length; i++)
        {
            if (codeDisplays[i] == display)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsConfigurationValid(bool logErrors)
    {
        if (codeDisplays == null || codeDisplays.Length == 0)
        {
            return ReportError(
                "Code module has no code displays.",
                logErrors
            );
        }

        if (targetDigits == null)
        {
            return ReportError(
                "Code module target digits are missing.",
                logErrors
            );
        }

        if (codeDisplays.Length != targetDigits.Length)
        {
            return ReportError(
                "Code display and target digit counts do not match.",
                logErrors
            );
        }

        for (int i = 0; i < codeDisplays.Length; i++)
        {
            if (codeDisplays[i] == null)
            {
                return ReportError(
                    $"Code display at index {i} is missing.",
                    logErrors
                );
            }

            for (int previous = 0; previous < i; previous++)
            {
                if (codeDisplays[previous] == codeDisplays[i])
                {
                    return ReportError(
                        $"Code display at index {i} is duplicated.",
                        logErrors
                    );
                }
            }

            if (targetDigits[i] < 0 || targetDigits[i] > 9)
            {
                return ReportError(
                    $"Target digit at index {i} is invalid: " +
                    $"{targetDigits[i]}.",
                    logErrors
                );
            }
        }

        return true;
    }

    private bool ReportError(string message, bool logError)
    {
        if (logError)
        {
            Debug.LogError(message, this);
        }

        return false;
    }
}
