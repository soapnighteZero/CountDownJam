using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CodebreakerEquationHUD : MonoBehaviour
{
    private const string Instruction =
        "MOVE SEGMENTS BETWEEN THE TWO DISPLAYS AND BUFFER\n" +
        "MAKE THE EQUATION TRUE, THEN PRESS SPACE";

    [Header("Text")]
    [SerializeField] private TMP_Text entryProgressText;
    [SerializeField] private TMP_Text targetEquationText;
    [SerializeField] private TMP_Text currentValuesText;
    [SerializeField] private TMP_Text acceptedDigitsText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text equationOperatorText;
    [SerializeField] private TMP_Text equationReadyText;
    [SerializeField] private TMP_Text bufferFeedbackText;
    [SerializeField] private Color equationReadyColor =
        new Color(0.45f, 1f, 0.62f, 1f);

    private Color normalTargetEquationColor;
    private Color normalEquationOperatorColor;
    private bool normalColorsCaptured;

    private void Awake()
    {
        ConfigureTextObjects();

        if (targetEquationText != null)
        {
            normalTargetEquationColor = targetEquationText.color;
        }

        if (equationOperatorText != null)
        {
            normalEquationOperatorColor = equationOperatorText.color;
        }

        normalColorsCaptured =
            targetEquationText != null &&
            equationOperatorText != null;
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
            targetEquationText.text = $"= {targetDigit}";
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
            $"CURRENT  A {renderedA} + B {renderedB} = {renderedTotal}";
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

    public void SetEquationReady(bool ready)
    {
        if (targetEquationText != null &&
            (ready || normalColorsCaptured))
        {
            targetEquationText.color =
                ready
                    ? equationReadyColor
                    : normalTargetEquationColor;
        }

        if (equationOperatorText != null &&
            (ready || normalColorsCaptured))
        {
            equationOperatorText.color =
                ready
                    ? equationReadyColor
                    : normalEquationOperatorColor;
        }

        if (equationReadyText == null)
        {
            return;
        }

        equationReadyText.text =
            ready ? "VALID - PRESS SPACE" : string.Empty;
        equationReadyText.gameObject.SetActive(ready);
    }

    public void SetBufferFeedback(string message)
    {
        if (bufferFeedbackText == null)
        {
            return;
        }

        string safeMessage = message ?? string.Empty;
        bufferFeedbackText.text = safeMessage;
        bufferFeedbackText.gameObject.SetActive(
            safeMessage.Length > 0);
    }

    public void ClearBufferFeedback()
    {
        SetBufferFeedback(string.Empty);
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
        isValid &= ValidateText(
            equationOperatorText,
            nameof(equationOperatorText));
        isValid &= ValidateText(
            equationReadyText,
            nameof(equationReadyText));
        isValid &= ValidateText(
            bufferFeedbackText,
            nameof(bufferFeedbackText));
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
            instructionText,
            equationOperatorText,
            equationReadyText,
            bufferFeedbackText
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
