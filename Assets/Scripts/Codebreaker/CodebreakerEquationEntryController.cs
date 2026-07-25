using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CodebreakerEquationEntryController : MonoBehaviour
{
    private const string ConfigurationError =
        "EQUATION MODULE CONFIGURATION ERROR";
    private const string ConservationError =
        "SEGMENT CONSERVATION ERROR";

    [Header("Scene References")]
    [SerializeField] private CodebreakerGameController gameController;
    [SerializeField] private CodeSequenceDisplay codeSequenceDisplay;
    [SerializeField] private SevenSegmentDisplay displayA;
    [SerializeField] private SevenSegmentDisplay displayB;
    [SerializeField] private SharedSegmentInventory sharedInventory;
    [SerializeField]
    private CodebreakerEquationInteractionController interactionController;
    [SerializeField] private CodebreakerEquationHUD equationHUD;
    [SerializeField] private GameObject equationWorldRoot;

    [Header("Settings")]
    [SerializeField] private int startingDigitA = 3;
    [SerializeField] private int startingDigitB = 8;
    [SerializeField] private int totalPhysicalSegments = 12;
    [SerializeField] private float successAdvanceDelaySeconds = 0.6f;

    private readonly List<int> targetDigits = new List<int>();
    private readonly List<int> acceptedDigits = new List<int>();
    private Coroutine advanceCoroutine;
    private bool eventsSubscribed;
    private bool configurationValid;
    private bool configurationErrorShown;
    private bool debugFallbackWarningShown;

    public int CurrentEntryIndex { get; private set; }
    public int CurrentTargetDigit =>
        CurrentEntryIndex >= 0 && CurrentEntryIndex < targetDigits.Count
            ? targetDigits[CurrentEntryIndex]
            : -1;
    public int TotalPhysicalSegments => totalPhysicalSegments;
    public bool IsEntryActive { get; private set; }
    public bool IsTransitioning { get; private set; }
    public bool IsComplete { get; private set; }

    public event Action<int, int, int> EquationDigitAccepted;

    private void OnEnable()
    {
        SubscribeEvents();
}
    private void Start()
    {
        configurationValid = ValidateReferences();

        if (!configurationValid)
        {
            ShowConfigurationError();
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null ||
            !keyboard.spaceKey.wasPressedThisFrame ||
            !CanAttemptSubmission())
        {
            return;
        }

        AttemptSubmit();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        CancelAdvance();

        if (interactionController != null)
        {
            interactionController.SetInteractionEnabled(false);
            interactionController.CancelCurrentDrag();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    public void BeginEquationEntry()
    {
        CancelAdvance();
        configurationValid = ValidateReferences();

        if (!configurationValid || !BuildTargetSequence())
        {
            IsEntryActive = false;
            equationWorldRoot?.SetActive(false);
            interactionController?.SetInteractionEnabled(false);
            ShowConfigurationError();
            return;
        }

        configurationErrorShown = false;
        CurrentEntryIndex = 0;
        acceptedDigits.Clear();
        IsTransitioning = false;
        IsComplete = false;
        IsEntryActive = true;

        interactionController.SetInteractionEnabled(false);
        equationWorldRoot.SetActive(true);
        displayA.gameObject.SetActive(true);
        displayB.gameObject.SetActive(true);
        displayA.SetDigit(startingDigitA);
        displayB.SetDigit(startingDigitB);
        sharedInventory.SetCount(0);
        RefreshHud();

        if (!ValidateSegmentConservation(out string errorMessage))
        {
            HandleConservationFailure(errorMessage);
            return;
        }

        interactionController.SetInteractionEnabled(true);
    }

    public void ResetEquationModule()
    {
        CancelAdvance();

        if (interactionController != null)
        {
            interactionController.SetInteractionEnabled(false);
            interactionController.CancelCurrentDrag();
        }

        IsEntryActive = false;
        IsTransitioning = false;
        IsComplete = false;
        CurrentEntryIndex = 0;
        acceptedDigits.Clear();
        targetDigits.Clear();

        sharedInventory?.SetCount(0);
        equationWorldRoot?.SetActive(false);

        if (equationHUD != null)
        {
            int totalDigits =
                gameController?.LevelConfig?.CodeDigitCount ?? 0;
            equationHUD.SetEntryProgress(0, totalDigits);
            equationHUD.SetAcceptedDigits(acceptedDigits, totalDigits);
            equationHUD.ClearFeedback();
        }
    }

    public bool AttemptSubmit()
    {
        if (!CanAttemptSubmission())
        {
            return false;
        }

        if (!ValidateSegmentConservation(out string errorMessage))
        {
            HandleConservationFailure(errorMessage);
            return false;
        }

        bool validA = displayA.TryGetCurrentDigit(out int valueA);
        bool validB = displayB.TryGetCurrentDigit(out int valueB);

        if (!validA || !validB)
        {
            equationHUD.SetFeedback("INVALID DISPLAY SHAPE");
            RefreshCurrentValues(validA, valueA, validB, valueB);
            return false;
        }

        int total = valueA + valueB;

        if (!CodebreakerEquationMathUtility.IsValidEquation(
            valueA,
            valueB,
            CurrentTargetDigit))
        {
            equationHUD.SetFeedback(
                $"WRONG TOTAL: {total}\nTARGET: {CurrentTargetDigit}");
            RefreshCurrentValues(true, valueA, true, valueB);
            return false;
        }

        AcceptCurrentDigit(valueA, valueB);
        return true;
    }

    public bool TryGetDisplayValues(out int valueA, out int valueB)
    {
        valueA = -1;
        valueB = -1;

        if (displayA == null || displayB == null)
        {
            return false;
        }

        bool validA = displayA.TryGetCurrentDigit(out valueA);
        bool validB = displayB.TryGetCurrentDigit(out valueB);
        return validA && validB;
    }

    public bool ValidateReferences()
    {
        bool isValid = true;
        isValid &= ValidateAssigned(gameController, nameof(gameController));
        isValid &=
            ValidateAssigned(codeSequenceDisplay, nameof(codeSequenceDisplay));
        isValid &= ValidateAssigned(displayA, nameof(displayA));
        isValid &= ValidateAssigned(displayB, nameof(displayB));
        isValid &=
            ValidateAssigned(sharedInventory, nameof(sharedInventory));
        isValid &= ValidateAssigned(
            interactionController,
            nameof(interactionController));
        isValid &= ValidateAssigned(equationHUD, nameof(equationHUD));
        isValid &=
            ValidateAssigned(equationWorldRoot, nameof(equationWorldRoot));

        if (displayA != null && displayA == displayB)
        {
            Debug.LogError(
                "Codebreaker equation displays A and B must be different " +
                "objects.",
                this);
            isValid = false;
        }

        if (totalPhysicalSegments <= 0)
        {
            Debug.LogError(
                "Codebreaker equation totalPhysicalSegments must be " +
                "positive.",
                this);
            isValid = false;
        }

        if (!IsDigit(startingDigitA) || !IsDigit(startingDigitB))
        {
            Debug.LogError(
                $"Codebreaker equation starting digits must be 0-9; " +
                $"received A={startingDigitA}, B={startingDigitB}.",
                this);
            isValid = false;
        }
        else
        {
            int requiredSegments =
                CodebreakerEquationMathUtility.GetRequiredSegmentCount(
                    startingDigitA,
                    startingDigitB);

            if (requiredSegments != totalPhysicalSegments)
            {
                Debug.LogError(
                    $"Codebreaker equation starting state uses " +
                    $"{requiredSegments} segments for A={startingDigitA}, " +
                    $"B={startingDigitB}, tray=0; expected " +
                    $"{totalPhysicalSegments}.",
                    this);
                isValid = false;
            }
        }

        if (!IsFinite(successAdvanceDelaySeconds) ||
            successAdvanceDelaySeconds < 0f)
        {
            Debug.LogError(
                "Codebreaker equation success delay must be finite and " +
                "non-negative.",
                this);
            isValid = false;
        }

        if (interactionController != null)
        {
            isValid &= interactionController.ValidateReferences();
        }

        if (equationHUD != null)
        {
            isValid &= equationHUD.ValidateReferences();
        }

        CodebreakerLevelConfig levelConfig = gameController?.LevelConfig;

        if (levelConfig == null)
        {
            if (gameController != null)
            {
                Debug.LogError(
                    "Codebreaker equation controller cannot read the level " +
                    "configuration.",
                    this);
            }

            isValid = false;
        }
        else
        {
            IReadOnlyList<int> expectedDigits =
                levelConfig.ExpectedCodeDigits;

            for (int i = 0; i < expectedDigits.Count; i++)
            {
                int targetDigit = expectedDigits[i];

                if (!CodebreakerEquationMathUtility.HasSolution(
                    targetDigit,
                    totalPhysicalSegments))
                {
                    Debug.LogError(
                        $"Codebreaker target digit {targetDigit} at index " +
                        $"{i} has no A+B solution with " +
                        $"{totalPhysicalSegments} physical segments.",
                        this);
                    isValid = false;
                }
            }
        }

        return isValid;
    }

    public bool ValidateSegmentConservation(out string errorMessage)
    {
        if (displayA == null ||
            displayB == null ||
            sharedInventory == null ||
            interactionController == null)
        {
            errorMessage =
                "Segment conservation cannot be checked because references " +
                "are missing.";
            return false;
        }

        int countA = displayA.ActiveSegmentCount;
        int countB = displayB.ActiveSegmentCount;
        int trayCount = sharedInventory.StoredSegments;
        int inFlight = interactionController.InFlightSegmentCount;
        int actualTotal = countA + countB + trayCount + inFlight;

        if (actualTotal == totalPhysicalSegments)
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage =
            $"Segment conservation failed: A={countA}, B={countB}, " +
            $"tray={trayCount}, inFlight={inFlight}, " +
            $"expected={totalPhysicalSegments}, actual={actualTotal}.";
        return false;
    }

    private void AcceptCurrentDigit(int valueA, int valueB)
    {
        int acceptedIndex = CurrentEntryIndex;
        int acceptedTarget = CurrentTargetDigit;
        acceptedDigits.Add(acceptedTarget);
        EquationDigitAccepted?.Invoke(acceptedIndex, valueA, valueB);
        equationHUD.SetAcceptedDigits(acceptedDigits, targetDigits.Count);

        bool isFinalDigit =
            acceptedDigits.Count >= targetDigits.Count;

        if (isFinalDigit)
        {
            IsComplete = true;
            IsEntryActive = false;
            IsTransitioning = false;
            interactionController.SetInteractionEnabled(false);
            CancelAdvance();
            equationHUD.SetFeedback("CODE ACCEPTED");
            gameController.DefuseBomb();
            return;
        }

        IsTransitioning = true;
        interactionController.SetInteractionEnabled(false);
        equationHUD.SetFeedback($"DIGIT {acceptedTarget} ACCEPTED");
        advanceCoroutine = StartCoroutine(AdvanceAfterDelay());
    }

    private IEnumerator AdvanceAfterDelay()
    {
        float elapsed = 0f;

        while (elapsed < successAdvanceDelaySeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        advanceCoroutine = null;

        if (!IsEntryActive ||
            IsComplete ||
            gameController == null ||
            gameController.CurrentPhase != CodebreakerPhase.EquationEntry)
        {
            IsTransitioning = false;
            yield break;
        }

        CurrentEntryIndex++;
        IsTransitioning = false;
        RefreshHud();
        interactionController.SetInteractionEnabled(true);
    }

    private bool BuildTargetSequence()
    {
        targetDigits.Clear();
        CodebreakerLevelConfig levelConfig = gameController.LevelConfig;

        if (levelConfig == null)
        {
            Debug.LogError(
                "Codebreaker equation level configuration is missing.",
                this);
            return false;
        }

        if (!levelConfig.ValidateConfiguration(out string errorMessage))
        {
            Debug.LogError(errorMessage, this);
            return false;
        }

        IReadOnlyList<int> expectedDigits =
            levelConfig.ExpectedCodeDigits;
        bool usingDebugFallback = !codeSequenceDisplay.IsComplete;

        for (int i = 0; i < expectedDigits.Count; i++)
        {
            int expectedDigit = expectedDigits[i];

            if (codeSequenceDisplay.TryGetDigit(i, out int recoveredDigit))
            {
                if (recoveredDigit != expectedDigit)
                {
                    Debug.LogError(
                        $"Recovered code digit {recoveredDigit} at index " +
                        $"{i} does not match the configured digit " +
                        $"{expectedDigit}.",
                        this);
                    targetDigits.Clear();
                    return false;
                }

                targetDigits.Add(recoveredDigit);
                continue;
            }

            if (!usingDebugFallback)
            {
                Debug.LogError(
                    $"Recovered code is marked complete but digit {i} " +
                    "cannot be read.",
                    this);
                targetDigits.Clear();
                return false;
            }

            targetDigits.Add(expectedDigit);
        }

        if (usingDebugFallback && !debugFallbackWarningShown)
        {
            Debug.LogWarning(
                "Debug Equation Entry started before all code digits were " +
                "recovered. Missing targets use LevelConfig expected " +
                "digits without revealing them.",
                this);
            debugFallbackWarningShown = true;
        }

        return targetDigits.Count > 0;
    }

    private bool CanAttemptSubmission()
    {
        return configurationValid &&
            IsEntryActive &&
            !IsTransitioning &&
            !IsComplete &&
            interactionController != null &&
            !interactionController.IsDragging &&
            gameController != null &&
            !gameController.IsTerminalState &&
            gameController.CurrentPhase == CodebreakerPhase.EquationEntry;
    }

    private void RefreshHud()
    {
        if (equationHUD == null || targetDigits.Count == 0)
        {
            return;
        }

        equationHUD.SetEntryProgress(
            CurrentEntryIndex,
            targetDigits.Count);
        equationHUD.SetTargetDigit(CurrentTargetDigit);
        equationHUD.SetAcceptedDigits(
            acceptedDigits,
            targetDigits.Count);

        bool validA = displayA.TryGetCurrentDigit(out int valueA);
        bool validB = displayB.TryGetCurrentDigit(out int valueB);
        RefreshCurrentValues(validA, valueA, validB, valueB);

        if (!IsTransitioning && !IsComplete)
        {
            if (validA &&
                validB &&
                valueA + valueB == CurrentTargetDigit)
            {
                equationHUD.SetFeedback("READY - PRESS SPACE");
            }
            else
            {
                equationHUD.ClearFeedback();
            }
        }
    }

    private void RefreshCurrentValues(
        bool validA,
        int valueA,
        bool validB,
        int valueB)
    {
        equationHUD.SetCurrentValues(
            validA,
            valueA,
            validB,
            valueB,
            CurrentTargetDigit);
    }

    private void HandleBoardChanged()
    {
        if (IsEntryActive && !IsTransitioning && !IsComplete)
        {
            RefreshHud();
        }
    }

    private void HandlePhaseChanged(CodebreakerPhase phase)
    {
        switch (phase)
        {
            case CodebreakerPhase.CodeDiscovery:
                ResetEquationModule();
                break;
            case CodebreakerPhase.EquationEntry:
                BeginEquationEntry();
                break;
            case CodebreakerPhase.Defused:
            case CodebreakerPhase.Exploded:
                CancelAdvance();
                IsEntryActive = false;
                IsTransitioning = false;

                if (interactionController != null)
                {
                    interactionController.SetInteractionEnabled(false);
                    interactionController.CancelCurrentDrag();
                }

                break;
        }
    }

    private void HandleLevelStarted()
    {
        debugFallbackWarningShown = false;
        ResetEquationModule();
    }

    private void HandleConservationFailure(string errorMessage)
    {
        interactionController.SetInteractionEnabled(false);
        equationHUD.SetFeedback(ConservationError);
        Debug.LogError(errorMessage, this);
    }

    private void ShowConfigurationError()
    {
        interactionController?.SetInteractionEnabled(false);

        if (equationHUD != null)
        {
            equationHUD.SetFeedback(ConfigurationError);
        }

        if (!configurationErrorShown)
        {
            Debug.LogError(
                "Codebreaker equation module configuration is invalid.",
                this);
            configurationErrorShown = true;
        }
    }

    private void CancelAdvance()
    {
        if (advanceCoroutine != null)
        {
            StopCoroutine(advanceCoroutine);
            advanceCoroutine = null;
        }

        IsTransitioning = false;
    }

    private void SubscribeEvents()
    {
        if (eventsSubscribed ||
            gameController == null ||
            interactionController == null)
        {
            return;
        }

        gameController.PhaseChanged += HandlePhaseChanged;
        gameController.LevelStarted += HandleLevelStarted;
        interactionController.BoardChanged += HandleBoardChanged;
        eventsSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!eventsSubscribed)
        {
            return;
        }

        if (gameController != null)
        {
            gameController.PhaseChanged -= HandlePhaseChanged;
            gameController.LevelStarted -= HandleLevelStarted;
        }

        if (interactionController != null)
        {
            interactionController.BoardChanged -= HandleBoardChanged;
        }

        eventsSubscribed = false;
    }

    private bool ValidateAssigned(
        UnityEngine.Object reference,
        string fieldName)
    {
        if (reference != null)
        {
            return true;
        }

        Debug.LogError(
            $"CodebreakerEquationEntryController is missing {fieldName}.",
            this);
        return false;
    }

    private static bool IsDigit(int value)
    {
        return value >= 0 && value <= 9;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
