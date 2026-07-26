using TMPro;
using UnityEngine;

public class CodebreakerHUD : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text debugHelpText;

    [Header("Code Progress")]
    [SerializeField] private CodeSequenceDisplay codeSequenceDisplay;

    public CodeSequenceDisplay CodeSequenceDisplay => codeSequenceDisplay;

    public bool ValidateReferences()
    {
        bool isValid = true;
        isValid &= ValidateTextReference(timerText, nameof(timerText));
        isValid &= ValidateTextReference(phaseText, nameof(phaseText));
        isValid &= ValidateTextReference(statusText, nameof(statusText));
        isValid &= ValidateTextReference(resultText, nameof(resultText));
        isValid &=
            ValidateTextReference(debugHelpText, nameof(debugHelpText));

        if (codeSequenceDisplay == null)
        {
            Debug.LogError(
                "CodebreakerHUD is missing codeSequenceDisplay.",
                this);
            isValid = false;
        }

        return isValid;
    }

    public void SetTimer(float seconds)
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        timerText.text =
            $"COUNTDOWN {minutes:00}:{remainingSeconds:00}";
    }

    public void SetPhase(CodebreakerPhase phase)
    {
        if (phaseText == null)
        {
            return;
        }

        switch (phase)
        {
            case CodebreakerPhase.CodeDiscovery:
                phaseText.text = "CODE DISCOVERY";
                break;
            case CodebreakerPhase.EquationEntry:
                phaseText.text = "EQUATION ENTRY";
                break;
            case CodebreakerPhase.Defused:
                phaseText.text = "DEFUSED";
                break;
            case CodebreakerPhase.Exploded:
                phaseText.text = "EXPLODED";
                break;
        }
    }

    public void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message ?? string.Empty;
        }
    }

    public void SetResult(string message)
    {
        if (resultText != null)
        {
            resultText.text = message ?? string.Empty;
        }
    }

    public void SetDebugHelpVisible(bool visible)
    {
        if (debugHelpText != null)
        {
            debugHelpText.gameObject.SetActive(visible);
        }
    }

    private bool ValidateTextReference(TMP_Text reference, string fieldName)
    {
        if (reference != null)
        {
            return true;
        }

        Debug.LogError(
            $"CodebreakerHUD is missing {fieldName}.",
            this);
        return false;
    }
}
