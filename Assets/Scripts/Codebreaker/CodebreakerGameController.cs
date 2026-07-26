using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CodebreakerGameController : MonoBehaviour
{
    private const string DiscoveryStatus =
        "RECOVER THE ACCESS CODE";
    private const string EquationStatus =
        "ENTER THE RECOVERED CODE THROUGH A + B";
    private const string DefusedResult =
        "BOMB DEFUSED\nPRESS R TO RESTART";
    private const string ExplodedResult =
        "BOOM\nGAME OVER\nPRESS R TO RESTART";

    [Header("Configuration")]
    [SerializeField] private CodebreakerLevelConfig levelConfig;

    [Header("Scene References")]
    [SerializeField] private GlobalBombTimer globalTimer;
    [SerializeField] private CodeSequenceDisplay codeSequenceDisplay;
    [SerializeField] private CodebreakerHUD hud;
    [SerializeField] private GameObject codeDiscoveryRoot;
    [SerializeField] private GameObject equationEntryRoot;

    [Header("Temporary Debug")]
    [SerializeField] private bool enableDebugControls = true;

    private bool configurationValid;
    private bool phaseInitialized;
    private bool timerEventsSubscribed;

    public CodebreakerPhase CurrentPhase { get; private set; }
    public CodebreakerLevelConfig LevelConfig => levelConfig;
    public int CurrentCodeIndex { get; private set; }
    public bool IsTerminalState =>
        CurrentPhase == CodebreakerPhase.Defused ||
        CurrentPhase == CodebreakerPhase.Exploded;
    private bool DebugControlsEnabled =>
        enableDebugControls && Debug.isDebugBuild;

    public event Action<CodebreakerPhase> PhaseChanged;
    public event Action LevelStarted;
    public event Action<int, int> DigitRegistered;

    private void OnEnable()
    {
        SubscribeToTimer();
    }

    private void Start()
    {
        StartLevel();
    }

    private void Update()
    {
        HandleDebugControls();
    }

    private void OnDisable()
    {
        UnsubscribeFromTimer();
    }

    private void OnDestroy()
    {
        UnsubscribeFromTimer();
    }

    public void StartLevel()
    {
        if (globalTimer != null)
        {
            globalTimer.StopTimer();
        }

        configurationValid = ValidateConfiguration();

        if (!configurationValid)
        {
            ShowConfigurationError();
            return;
        }

        SubscribeToTimer();

        CodebreakerTimeBreakdown breakdown;

        try
        {
            breakdown =
                CodebreakerTimeCalculator.CalculateBreakdown(levelConfig);
        }
        catch (Exception exception)
        {
            configurationValid = false;
            Debug.LogError(
                $"Codebreaker timer calculation failed: " +
                $"{exception.Message}",
                this);
            ShowConfigurationError();
            return;
        }

        LogTimerBreakdown(breakdown);

        CurrentCodeIndex = 0;
        codeSequenceDisplay.Initialize(levelConfig.CodeDigitCount);
        codeSequenceDisplay.Clear();
        hud.SetResult(string.Empty);
        hud.SetDebugHelpVisible(DebugControlsEnabled);
        SetPhase(CodebreakerPhase.CodeDiscovery);

        globalTimer.Initialize(breakdown.FinalSeconds);
        hud.SetTimer(breakdown.FinalSeconds);
        globalTimer.StartTimer();
        LevelStarted?.Invoke();
    }

    public void RestartLevel()
    {
        StartLevel();
    }

    public bool RevealNextCodeDigitForDebug()
    {
        if (!configurationValid ||
            IsTerminalState ||
            CurrentPhase != CodebreakerPhase.CodeDiscovery ||
            CurrentCodeIndex >= levelConfig.CodeDigitCount)
        {
            return false;
        }

        int digit = levelConfig.ExpectedCodeDigits[CurrentCodeIndex];
        return TryRevealCurrentCodeDigit(digit);
    }

    public bool TryRegisterDiscoveredDigit(int digit)
    {
        if (!configurationValid ||
            IsTerminalState ||
            CurrentPhase != CodebreakerPhase.CodeDiscovery ||
            digit < 0 ||
            digit > 9 ||
            CurrentCodeIndex < 0 ||
            CurrentCodeIndex >= levelConfig.CodeDigitCount)
        {
            return false;
        }

        int expectedDigit =
            levelConfig.ExpectedCodeDigits[CurrentCodeIndex];

        if (digit != expectedDigit)
        {
            Debug.LogError(
                $"CodebreakerGameController rejected discovered digit " +
                $"{digit} at code index {CurrentCodeIndex}; the authored " +
                $"level expects {expectedDigit}.",
                this);
            return false;
        }

        return TryRevealCurrentCodeDigit(digit);
    }

    public bool ApplyTimePenalty(float seconds, string reason)
    {
        if (float.IsNaN(seconds) ||
            float.IsInfinity(seconds) ||
            seconds < 0f)
        {
            Debug.LogError(
                "CodebreakerGameController rejected a non-finite or " +
                "negative time penalty.",
                this);
            return false;
        }

        if (!configurationValid || IsTerminalState || globalTimer == null)
        {
            return false;
        }

        if (!globalTimer.SubtractTime(seconds))
        {
            return false;
        }

        string penaltyReason = string.IsNullOrWhiteSpace(reason)
            ? "UNSPECIFIED"
            : reason;
        Debug.Log(
            $"Codebreaker time penalty: -{seconds:0.##} seconds. " +
            $"Reason: {penaltyReason}",
            this);
        return true;
    }

    public void EnterEquationEntryPhase()
    {
        if (!configurationValid || IsTerminalState)
        {
            return;
        }

        if (!levelConfig.ValidateConfiguration(out string errorMessage))
        {
            configurationValid = false;
            Debug.LogError(errorMessage, levelConfig);
            ShowConfigurationError();
            return;
        }

        SetPhase(CodebreakerPhase.EquationEntry);
    }

    public void DefuseBomb()
    {
        if (!configurationValid || IsTerminalState)
        {
            return;
        }

        globalTimer.StopTimer();
        SetPhase(CodebreakerPhase.Defused);
        hud.SetStatus(string.Empty);
        hud.SetResult(DefusedResult);
    }

    public void ExplodeBomb(string reason)
    {
        if (!configurationValid || IsTerminalState)
        {
            return;
        }

        globalTimer.StopTimer();
        SetPhase(CodebreakerPhase.Exploded);
        hud.SetStatus(string.IsNullOrWhiteSpace(reason)
            ? "EXPLOSION"
            : reason);
        hud.SetResult(ExplodedResult);
    }

    private bool ValidateConfiguration()
    {
        bool isValid = true;

        if (levelConfig == null)
        {
            Debug.LogError(
                "CodebreakerGameController is missing levelConfig.",
                this);
            isValid = false;
        }

        if (globalTimer == null)
        {
            Debug.LogError(
                "CodebreakerGameController is missing globalTimer.",
                this);
            isValid = false;
        }

        if (codeSequenceDisplay == null)
        {
            Debug.LogError(
                "CodebreakerGameController is missing " +
                "codeSequenceDisplay.",
                this);
            isValid = false;
        }

        if (hud == null)
        {
            Debug.LogError(
                "CodebreakerGameController is missing hud.",
                this);
            isValid = false;
        }

        if (codeDiscoveryRoot == null)
        {
            Debug.LogError(
                "CodebreakerGameController is missing codeDiscoveryRoot.",
                this);
            isValid = false;
        }

        if (equationEntryRoot == null)
        {
            Debug.LogError(
                "CodebreakerGameController is missing equationEntryRoot.",
                this);
            isValid = false;
        }

        if (codeSequenceDisplay != null)
        {
            isValid &= codeSequenceDisplay.ValidateReferences();
        }

        if (hud != null)
        {
            isValid &= hud.ValidateReferences();

            if (hud.CodeSequenceDisplay != codeSequenceDisplay)
            {
                Debug.LogError(
                    "CodebreakerHUD and CodebreakerGameController must " +
                    "reference the same CodeSequenceDisplay.",
                    this);
                isValid = false;
            }
        }

        if (levelConfig != null &&
            !levelConfig.ValidateConfiguration(out string errorMessage))
        {
            Debug.LogError(errorMessage, levelConfig);
            isValid = false;
        }

        return isValid;
    }

    private void ShowConfigurationError()
    {
        if (globalTimer != null)
        {
            globalTimer.StopTimer();
        }

        if (codeDiscoveryRoot != null)
        {
            codeDiscoveryRoot.SetActive(false);
        }

        if (equationEntryRoot != null)
        {
            equationEntryRoot.SetActive(false);
        }

        if (hud != null)
        {
            hud.SetStatus("LEVEL CONFIGURATION ERROR");
            hud.SetResult(string.Empty);
        }
    }

    private void SetPhase(CodebreakerPhase phase)
    {
        bool phaseChanged = !phaseInitialized || CurrentPhase != phase;
        CurrentPhase = phase;
        phaseInitialized = true;

        ApplyPhasePresentation(phase);

        if (phaseChanged)
        {
            PhaseChanged?.Invoke(phase);
        }
    }

    private void ApplyPhasePresentation(CodebreakerPhase phase)
    {
        bool discoveryActive =
            phase == CodebreakerPhase.CodeDiscovery;
        bool equationActive =
            phase == CodebreakerPhase.EquationEntry;

        codeDiscoveryRoot.SetActive(discoveryActive);
        equationEntryRoot.SetActive(equationActive);
        hud.SetPhase(phase);

        switch (phase)
        {
            case CodebreakerPhase.CodeDiscovery:
                hud.SetStatus(DiscoveryStatus);
                break;
            case CodebreakerPhase.EquationEntry:
                hud.SetStatus(EquationStatus);
                break;
        }
    }

    private void HandleDebugControls()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            RestartLevel();
            return;
        }

        if (!DebugControlsEnabled || IsTerminalState)
        {
            return;
        }

        if (keyboard.f1Key.wasPressedThisFrame)
        {
            RevealNextCodeDigitForDebug();
        }
        else if (keyboard.f2Key.wasPressedThisFrame)
        {
            EnterEquationEntryPhase();
        }
        else if (keyboard.f3Key.wasPressedThisFrame)
        {
            DefuseBomb();
        }
        else if (keyboard.f4Key.wasPressedThisFrame)
        {
            ExplodeBomb("DEBUG EXPLOSION");
        }
    }

    private void SubscribeToTimer()
    {
        if (timerEventsSubscribed || globalTimer == null)
        {
            return;
        }

        globalTimer.RemainingTimeChanged += HandleRemainingTimeChanged;
        globalTimer.TimerExpired += HandleTimerExpired;
        timerEventsSubscribed = true;
    }

    private void UnsubscribeFromTimer()
    {
        if (!timerEventsSubscribed || globalTimer == null)
        {
            return;
        }

        globalTimer.RemainingTimeChanged -= HandleRemainingTimeChanged;
        globalTimer.TimerExpired -= HandleTimerExpired;
        timerEventsSubscribed = false;
    }

    private void HandleRemainingTimeChanged(float seconds)
    {
        if (hud != null)
        {
            hud.SetTimer(seconds);
        }
    }

    private void HandleTimerExpired()
    {
        if (!IsTerminalState)
        {
            ExplodeBomb("TIME EXPIRED");
        }
    }

    private bool TryRevealCurrentCodeDigit(int digit)
    {
        if (CurrentCodeIndex < 0 ||
            CurrentCodeIndex >= levelConfig.CodeDigitCount)
        {
            return false;
        }

        int revealedIndex = CurrentCodeIndex;

        if (!codeSequenceDisplay.RevealDigit(revealedIndex, digit))
        {
            return false;
        }

        CurrentCodeIndex++;
        DigitRegistered?.Invoke(revealedIndex, digit);

        if (CurrentCodeIndex >= levelConfig.CodeDigitCount)
        {
            EnterEquationEntryPhase();
        }

        return true;
    }

    private void LogTimerBreakdown(
        CodebreakerTimeBreakdown breakdown)
    {
        string manualLabel = breakdown.UsedManualOverride
            ? "\nManual override: true"
            : string.Empty;

        Debug.Log(
            "Codebreaker timer estimate:\n" +
            $"Base: {breakdown.BaseSeconds:0.##}\n" +
            $"Discovery: {breakdown.DiscoverySeconds:0.##}\n" +
            $"Equation moves: " +
            $"{breakdown.EquationMovementSeconds:0.##}\n" +
            $"Code planning: {breakdown.CodePlanningSeconds:0.##}\n" +
            $"Transition: {breakdown.PhaseTransitionSeconds:0.##}\n" +
            $"Authored adjustment: " +
            $"{breakdown.AuthoredAdjustmentSeconds:0.##}\n" +
            $"Difficulty multiplier: " +
            $"{breakdown.DifficultyMultiplier:0.##}\n" +
            $"Final: {breakdown.FinalSeconds:0.##}" +
            manualLabel,
            this);
    }
}
