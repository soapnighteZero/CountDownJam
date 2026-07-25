using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CodebreakerEquationHUD : MonoBehaviour
{
    private const string Instruction =
        "DRAG SEGMENTS BETWEEN A, B, AND THE TRAY\n" +
        "PRESS SPACE TO SUBMIT";

    [Header("Text")]
    [SerializeField] private TMP_Text entryProgressText;
    [SerializeField] private TMP_Text targetEquationText;
    [SerializeField] private TMP_Text currentValuesText;
    [SerializeField] private TMP_Text acceptedDigitsText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text instructionText;

    private void Awake()
    {
        ConfigureTextObjects();
    }

    public void SetEntryProgress(int currentIndex, int totalDigits)
    {
        if (entryProgressText == null)
        {
            return;
        }

        int safeTotal = Mathf.Max(1, totalDigits);
        int displayedIndex = Mathf.Clamp(currentIndex + 1, 1, safeTotal);
        entryProgressText.text =
            $"ENTRY DIGIT {displayedIndex} OF {safeTotal}";
    }

    public void SetTargetDigit(int targetDigit)
    {
        if (targetEquationText != null)
        {
            targetEquationText.text = $"A + B = {targetDigit}";
        }
    }

    public void SetCurrentValues(
        bool validA,
        int valueA,
        bool validB,
        int valueB,
        int targetDigit)
    {
        if (currentValuesText == null)
        {
            return;
        }

        string renderedA = validA ? valueA.ToString() : "?";
        string renderedB = validB ? valueB.ToString() : "?";
        string renderedTotal =
            validA && validB ? (valueA + valueB).ToString() : "?";
        currentValuesText.text =
            $"A {renderedA} + B {renderedB} = {renderedTotal}";
    }

    public void SetAcceptedDigits(
        IReadOnlyList<int> acceptedDigits,
        int totalDigits)
    {
        if (acceptedDigitsText == null)
        {
            return;
        }

        int safeTotal = Mathf.Max(0, totalDigits);
        string[] renderedDigits = new string[safeTotal];

        for (int i = 0; i < safeTotal; i++)
        {
            renderedDigits[i] =
                acceptedDigits != null && i < acceptedDigits.Count
                    ? acceptedDigits[i].ToString()
                    : "_";
        }

        acceptedDigitsText.text =
            $"ENTERED  {string.Join(" ", renderedDigits)}";
    }

    public void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message ?? string.Empty;
        }
    }

    public void ClearFeedback()
    {
        SetFeedback(string.Empty);
    }

    public bool ValidateReferences()
    {
        bool isValid = true;
        isValid &= ValidateText(entryProgressText, nameof(entryProgressText));
        isValid &= ValidateText(targetEquationText, nameof(targetEquationText));
        isValid &= ValidateText(currentValuesText, nameof(currentValuesText));
        isValid &= ValidateText(acceptedDigitsText, nameof(acceptedDigitsText));
        isValid &= ValidateText(feedbackText, nameof(feedbackText));
        isValid &= ValidateText(instructionText, nameof(instructionText));
        ConfigureTextObjects();
        return isValid;
    }

    private void ConfigureTextObjects()
    {
        TMP_Text[] textObjects =
        {
            entryProgressText,
            targetEquationText,
            currentValuesText,
            acceptedDigitsText,
            feedbackText,
            instructionText
        };

        for (int i = 0; i < textObjects.Length; i++)
        {
            if (textObjects[i] != null)
            {
                textObjects[i].raycastTarget = false;
            }
        }

        if (instructionText != null)
        {
            instructionText.text = Instruction;
        }
    }

    private bool ValidateText(TMP_Text textObject, string fieldName)
    {
        if (textObject != null)
        {
            return true;
        }

        Debug.LogError(
            $"CodebreakerEquationHUD is missing {fieldName}.",
            this);
        return false;
    }
}
