using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LayeredDigitPuzzleController : MonoBehaviour
{
    private const string ConfigurationErrorText =
        "LAYERED PUZZLE CONFIGURATION ERROR";
    private const string InstructionText =
        "<size=30><b>USE ALL 4 HITS TO LEAVE ONE GREEN DIGIT</b></size>\n" +
        "<size=18>CLICK A SEGMENT = REMOVE ONE LAYER   |   RED > YELLOW > GREEN > OFF   |   DOTS = LAYERS LEFT</size>";
    private const float NoDelayedAction = -1f;

    [SerializeField] private LayeredDigitPuzzleConfig[] puzzleConfigs =
        Array.Empty<LayeredDigitPuzzleConfig>();
    [SerializeField] private CodebreakerGameController gameController;
    [SerializeField] private LayeredSegmentStackView[] segmentViews =
        Array.Empty<LayeredSegmentStackView>();
    [SerializeField] private TMP_Text puzzleProgressText;
    [SerializeField] private TMP_Text hitsLeftText;
    [SerializeField] private TMP_Text puzzleInstructionText;
    [SerializeField] private TMP_Text puzzleFeedbackText;
    [SerializeField] private GameObject interactionBlocker;
    [SerializeField] private float successAdvanceDelaySeconds = 0.75f;

    private readonly Dictionary<int, LayeredDigitPuzzleConfig>
        puzzleByCodeIndex =
            new Dictionary<int, LayeredDigitPuzzleConfig>();
    private bool configurationValid;
    private bool initialized;
    private bool eventsSubscribed;
    private bool configurationErrorsLogged;
    private float failureResetAtUnscaledTime = NoDelayedAction;
    private float successAdvanceAtUnscaledTime = NoDelayedAction;

    public IReadOnlyList<LayeredDigitPuzzleConfig> PuzzleConfigs =>
        Array.AsReadOnly(
            puzzleConfigs ??
            Array.Empty<LayeredDigitPuzzleConfig>());
    public LayeredDigitPuzzleConfig ActivePuzzleConfig { get; private set; }
    public int ActivePuzzleIndex { get; private set; } = -1;
    public LayeredDigitPuzzleConfig PuzzleConfig => ActivePuzzleConfig;
    public int HitsRemaining { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsResolving { get; private set; }
    public bool IsSolved { get; private set; }

    public event Action<int> HitsRemainingChanged;
    public event Action<int, LayeredSegmentColor> PuzzleSolved;
    public event Action<string> PuzzleFailed;

    private void Awake()
    {
        InitializeSequence();
    }

    private void OnEnable()
    {
        SubscribeToGameEvents();

        if (initialized && configurationValid)
        {
            SynchronizeToGameState();
        }
    }

    private void Start()
    {
        if (!initialized)
        {
            InitializeSequence();
        }

        if (configurationValid)
        {
            SynchronizeToGameState();
        }
    }

    private void Update()
    {
        if (!configurationValid || gameController == null)
        {
            return;
        }

        if (gameController.IsTerminalState ||
            gameController.CurrentPhase !=
                CodebreakerPhase.CodeDiscovery)
        {
            CancelDelayedActions();
            LockInteraction();
            return;
        }

        if (successAdvanceAtUnscaledTime >= 0f)
        {
            if (Time.unscaledTime >= successAdvanceAtUnscaledTime)
            {
                CompleteSuccessAdvance();
            }

            return;
        }

        if (failureResetAtUnscaledTime >= 0f)
        {
            if (Time.unscaledTime >= failureResetAtUnscaledTime)
            {
                CompleteFailureReset();
            }

            return;
        }

        if (gameController.CurrentCodeIndex != ActivePuzzleIndex)
        {
            SynchronizeToGameState();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromGameEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromGameEvents();
    }

    private void OnValidate()
    {
        if (!IsFinite(successAdvanceDelaySeconds))
        {
            successAdvanceDelaySeconds = 0.75f;
        }
        else
        {
            successAdvanceDelaySeconds =
                Mathf.Max(0f, successAdvanceDelaySeconds);
        }
    }

    public void ResetPuzzle()
    {
        if (!configurationValid)
        {
            ShowConfigurationError();
            return;
        }

        if (ActivePuzzleConfig == null)
        {
            SynchronizeToGameState();
            return;
        }

        ResetActivePuzzle(clearFeedback: true);
    }

    public bool TryHitSegment(LayeredSegmentStackView segmentView)
    {
        if (!configurationValid ||
            !IsPlaying ||
            IsResolving ||
            IsSolved ||
            HitsRemaining <= 0 ||
            segmentView == null ||
            !ContainsView(segmentView) ||
            !segmentView.CanAdvance ||
            ActivePuzzleConfig == null ||
            gameController == null ||
            gameController.IsTerminalState ||
            gameController.CurrentPhase !=
                CodebreakerPhase.CodeDiscovery ||
            gameController.CurrentCodeIndex != ActivePuzzleIndex)
        {
            return false;
        }

        if (!segmentView.TryAdvanceOneLayer())
        {
            return false;
        }

        HitsRemaining--;
        UpdateHitsText();
        HitsRemainingChanged?.Invoke(HitsRemaining);

        if (HitsRemaining == 0)
        {
            IsPlaying = false;
            IsResolving = true;
            SetInteractionBlocked(true);
            ResolvePuzzle();
        }

        return true;
    }

    public void ResolvePuzzle()
    {
        if (!configurationValid ||
            IsSolved ||
            HitsRemaining != 0 ||
            ActivePuzzleConfig == null)
        {
            return;
        }

        IsPlaying = false;
        IsResolving = true;
        SetInteractionBlocked(true);

        var activeSegments = new List<LayeredSegmentPosition>(7);
        LayeredSegmentColor resolvedColor = LayeredSegmentColor.Gray;

        for (int i = 0; i < segmentViews.Length; i++)
        {
            LayeredSegmentStackView view = segmentViews[i];
            LayeredSegmentColor color = view.CurrentColor;

            if (color == LayeredSegmentColor.Gray)
            {
                continue;
            }

            activeSegments.Add(view.Position);

            if (resolvedColor == LayeredSegmentColor.Gray)
            {
                resolvedColor = color;
            }
            else if (resolvedColor != color)
            {
                FailPuzzle("COLOR MISMATCH");
                return;
            }
        }

        if (activeSegments.Count == 0)
        {
            FailPuzzle("ALL SEGMENTS EMPTY");
            return;
        }

        if (!SevenSegmentDigitUtility.TryGetDigit(
                activeSegments,
                out int digit))
        {
            FailPuzzle("INVALID DIGIT");
            return;
        }

        LayeredDigitPuzzleConfig solvedConfig = ActivePuzzleConfig;

        if (digit != solvedConfig.ExpectedDigit ||
            resolvedColor != solvedConfig.ExpectedColor)
        {
            FailPuzzle("UNEXPECTED AUTHORED RESULT");
            return;
        }

        if (gameController == null ||
            gameController.IsTerminalState ||
            gameController.CurrentPhase !=
                CodebreakerPhase.CodeDiscovery ||
            gameController.CurrentCodeIndex != ActivePuzzleIndex)
        {
            FailPuzzle("UNEXPECTED AUTHORED RESULT");
            return;
        }

        CancelFailureReset();
        IsResolving = false;
        IsSolved = true;

        if (puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = $"DIGIT CONFIRMED: {digit}";
        }

        ScheduleSuccessAdvance();

        if (!gameController.TryRegisterDiscoveredDigit(digit))
        {
            CancelSuccessAdvance();
            IsSolved = false;
            FailPuzzle("UNEXPECTED AUTHORED RESULT");
            return;
        }

        PuzzleSolved?.Invoke(digit, resolvedColor);
    }

    public bool ValidateReferences()
    {
        var errors = new List<string>();
        puzzleByCodeIndex.Clear();

        ValidateSceneReferences(errors);
        ValidatePuzzleSequence(errors);

        if (errors.Count > 0)
        {
            LogConfigurationErrors(errors);
            return false;
        }

        return true;
    }

    private void InitializeSequence()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        configurationValid = ValidateReferences();

        if (!configurationValid)
        {
            ShowConfigurationError();
            return;
        }

        SynchronizeToGameState();
    }

    private void ValidateSceneReferences(ICollection<string> errors)
    {
        if (gameController == null)
        {
            errors.Add(
                "LayeredDigitPuzzleController is missing gameController.");
        }

        if (puzzleProgressText == null)
        {
            errors.Add(
                "LayeredDigitPuzzleController is missing " +
                "puzzleProgressText.");
        }

        if (hitsLeftText == null)
        {
            errors.Add(
                "LayeredDigitPuzzleController is missing hitsLeftText.");
        }

        if (puzzleInstructionText == null)
        {
            errors.Add(
                "LayeredDigitPuzzleController is missing " +
                "puzzleInstructionText.");
        }

        if (puzzleFeedbackText == null)
        {
            errors.Add(
                "LayeredDigitPuzzleController is missing " +
                "puzzleFeedbackText.");
        }

        if (!IsFinite(successAdvanceDelaySeconds) ||
            successAdvanceDelaySeconds < 0f)
        {
            errors.Add(
                "LayeredDigitPuzzleController success advance delay must " +
                "be finite and non-negative.");
        }

        if (segmentViews == null || segmentViews.Length != 7)
        {
            errors.Add(
                "LayeredDigitPuzzleController requires exactly seven " +
                "segment views.");
            return;
        }

        bool[] positionsSeen = new bool[7];

        for (int i = 0; i < segmentViews.Length; i++)
        {
            LayeredSegmentStackView view = segmentViews[i];

            if (view == null)
            {
                errors.Add(
                    "LayeredDigitPuzzleController has a null segment " +
                    $"view at index {i}.");
                continue;
            }

            if (!view.ValidateReferences())
            {
                errors.Add(
                    $"Layered segment view {view.Position} has invalid " +
                    "scene references.");
            }

            int positionIndex = (int)view.Position;

            if (positionIndex < 0 ||
                positionIndex >= positionsSeen.Length)
            {
                errors.Add(
                    "LayeredDigitPuzzleController has invalid segment " +
                    $"position {view.Position}.");
                continue;
            }

            if (positionsSeen[positionIndex])
            {
                errors.Add(
                    "LayeredDigitPuzzleController has duplicate segment " +
                    $"position {view.Position}.");
            }

            positionsSeen[positionIndex] = true;
        }

        for (int i = 0; i < positionsSeen.Length; i++)
        {
            if (!positionsSeen[i])
            {
                errors.Add(
                    "LayeredDigitPuzzleController has no view for " +
                    $"position {(LayeredSegmentPosition)i}.");
            }
        }
    }

    private void ValidatePuzzleSequence(ICollection<string> errors)
    {
        CodebreakerLevelConfig levelConfig =
            gameController != null
                ? gameController.LevelConfig
                : null;

        if (levelConfig == null)
        {
            errors.Add(
                "LayeredDigitPuzzleController cannot validate puzzle " +
                "configs without a CodebreakerLevelConfig.");
            return;
        }

        int codeDigitCount = levelConfig.CodeDigitCount;

        if (puzzleConfigs == null)
        {
            errors.Add(
                "LayeredDigitPuzzleController has a null puzzleConfigs " +
                "array.");
            return;
        }

        if (puzzleConfigs.Length != codeDigitCount)
        {
            errors.Add(
                $"LayeredDigitPuzzleController has {puzzleConfigs.Length} " +
                $"puzzle configs, but the level has {codeDigitCount} " +
                "code digits.");
        }

        var puzzleIds = new HashSet<string>(
            StringComparer.Ordinal);
        var duplicatedCodeIndices = new HashSet<int>();
        int totalHitBudget = 0;

        for (int i = 0; i < puzzleConfigs.Length; i++)
        {
            LayeredDigitPuzzleConfig config = puzzleConfigs[i];

            if (config == null)
            {
                errors.Add(
                    "LayeredDigitPuzzleController has a null puzzle " +
                    $"config at array index {i}.");
                continue;
            }

            totalHitBudget += config.HitBudget;
            bool configValid =
                config.ValidateConfiguration(
                    out string configError);

            if (!configValid)
            {
                errors.Add(configError);
            }

            if (string.IsNullOrWhiteSpace(config.PuzzleId))
            {
                errors.Add($"{config.name} has a blank puzzleId.");
            }
            else if (!puzzleIds.Add(config.PuzzleId))
            {
                errors.Add(
                    $"Duplicate puzzleId {config.PuzzleId}.");
            }

            int targetIndex = config.TargetCodeIndex;

            if (targetIndex < 0 || targetIndex >= codeDigitCount)
            {
                errors.Add(
                    $"{config.name} targets code index {targetIndex}, " +
                    $"outside the valid range 0 through " +
                    $"{codeDigitCount - 1}.");
            }
            else if (puzzleByCodeIndex.ContainsKey(targetIndex))
            {
                if (duplicatedCodeIndices.Add(targetIndex))
                {
                    errors.Add(
                        $"Two puzzle configs target code index " +
                        $"{targetIndex}.");
                }
            }
            else
            {
                puzzleByCodeIndex.Add(targetIndex, config);
            }

            if (targetIndex >= 0 && targetIndex < codeDigitCount)
            {
                int levelDigit =
                    levelConfig.ExpectedCodeDigits[targetIndex];

                if (config.ExpectedDigit != levelDigit)
                {
                    errors.Add(
                        $"{config.name} expects " +
                        $"{config.ExpectedDigit} but the level code " +
                        $"expects {levelDigit}.");
                }
            }

            if (configValid)
            {
                ValidateSolverResult(config, errors);
            }
        }

        for (int codeIndex = 0;
            codeIndex < codeDigitCount;
            codeIndex++)
        {
            if (!puzzleByCodeIndex.ContainsKey(codeIndex))
            {
                errors.Add(
                    "LayeredDigitPuzzleController has no config for " +
                    $"code index {codeIndex}.");
            }
        }

        if (totalHitBudget != levelConfig.TotalDiscoveryHitBudget)
        {
            errors.Add(
                "Discovery hit budget mismatch: configs total " +
                $"{totalHitBudget} but level expects " +
                $"{levelConfig.TotalDiscoveryHitBudget}.");
        }

        if (string.Equals(
                levelConfig.LevelId,
                "prototype-tutorial",
                StringComparison.Ordinal) &&
            totalHitBudget != 12)
        {
            errors.Add(
                "Prototype tutorial discovery configs must total 12 " +
                $"hits, but total {totalHitBudget}.");
        }
    }

    private static void ValidateSolverResult(
        LayeredDigitPuzzleConfig config,
        ICollection<string> errors)
    {
        IReadOnlyList<LayeredDigitPuzzleSolution> solutions =
            LayeredDigitPuzzleSolver.FindValidSolutions(config);

        if (solutions.Count != 1)
        {
            errors.Add(
                $"{config.name} expected one unique solution but found " +
                $"{solutions.Count}.");
            return;
        }

        LayeredDigitPuzzleSolution solution = solutions[0];

        if (solution.Digit != config.ExpectedDigit ||
            solution.Color != config.ExpectedColor)
        {
            errors.Add(
                $"{config.name} unique solver result is " +
                $"{solution.Color.ToString().ToLowerInvariant()} digit " +
                $"{solution.Digit}, not " +
                $"{config.ExpectedColor.ToString().ToLowerInvariant()} " +
                $"digit {config.ExpectedDigit}.");
        }
    }

    private bool LoadPuzzleForCodeIndex(int codeIndex)
    {
        if (!configurationValid ||
            !puzzleByCodeIndex.TryGetValue(
                codeIndex,
                out LayeredDigitPuzzleConfig config))
        {
            return false;
        }

        CancelDelayedActions();
        ActivePuzzleConfig = config;
        ActivePuzzleIndex = codeIndex;

        for (int i = 0; i < segmentViews.Length; i++)
        {
            LayeredSegmentStackView view = segmentViews[i];

            if (!config.TryGetStack(
                    view.Position,
                    out LayeredSegmentStackDefinition stack))
            {
                Debug.LogError(
                    $"{config.name} has no stack for position " +
                    $"{view.Position}.",
                    config);
                configurationValid = false;
                ShowConfigurationError();
                return false;
            }

            view.Initialize(stack, this);
        }

        ResetActivePuzzle(clearFeedback: true);
        return true;
    }

    private void ResetActivePuzzle(bool clearFeedback)
    {
        CancelDelayedActions();

        for (int i = 0; i < segmentViews.Length; i++)
        {
            segmentViews[i].ResetStack();
        }

        HitsRemaining = ActivePuzzleConfig.HitBudget;
        IsResolving = false;
        IsSolved = false;
        UpdateHitsText();
        UpdateProgressText();

        if (puzzleInstructionText != null)
        {
            puzzleInstructionText.text = InstructionText;
        }

        if (clearFeedback && puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = string.Empty;
        }

        bool targetIsCurrent =
            gameController != null &&
            !gameController.IsTerminalState &&
            gameController.CurrentPhase ==
                CodebreakerPhase.CodeDiscovery &&
            gameController.CurrentCodeIndex == ActivePuzzleIndex;

        IsPlaying = targetIsCurrent;
        SetInteractionBlocked(!IsPlaying);
    }

    private void SynchronizeToGameState()
    {
        if (!configurationValid || gameController == null)
        {
            return;
        }

        if (gameController.IsTerminalState ||
            gameController.CurrentPhase !=
                CodebreakerPhase.CodeDiscovery)
        {
            CancelDelayedActions();
            LockInteraction();
            return;
        }

        int currentCodeIndex = gameController.CurrentCodeIndex;

        if (currentCodeIndex < 0 ||
            currentCodeIndex >=
                gameController.LevelConfig.CodeDigitCount)
        {
            CancelDelayedActions();
            LockInteraction();
            return;
        }

        if (!LoadPuzzleForCodeIndex(currentCodeIndex))
        {
            configurationValid = false;
            LogConfigurationErrorOnce(
                "LayeredDigitPuzzleController has no config for " +
                $"code index {currentCodeIndex}.");
            ShowConfigurationError();
        }
    }

    private void CompleteFailureReset()
    {
        failureResetAtUnscaledTime = NoDelayedAction;

        if (gameController == null ||
            gameController.IsTerminalState ||
            gameController.CurrentPhase !=
                CodebreakerPhase.CodeDiscovery)
        {
            LockInteraction();
            return;
        }

        if (gameController.CurrentCodeIndex != ActivePuzzleIndex)
        {
            SynchronizeToGameState();
            return;
        }

        ResetActivePuzzle(clearFeedback: true);
    }

    private void CompleteSuccessAdvance()
    {
        successAdvanceAtUnscaledTime = NoDelayedAction;

        if (gameController == null ||
            gameController.IsTerminalState ||
            gameController.CurrentPhase !=
                CodebreakerPhase.CodeDiscovery)
        {
            LockInteraction();
            return;
        }

        int nextCodeIndex = gameController.CurrentCodeIndex;

        if (nextCodeIndex < 0 ||
            nextCodeIndex >=
                gameController.LevelConfig.CodeDigitCount)
        {
            LockInteraction();
            return;
        }

        LoadPuzzleForCodeIndex(nextCodeIndex);
    }

    private void FailPuzzle(string reason)
    {
        IsPlaying = false;
        IsResolving = true;
        IsSolved = false;
        SetInteractionBlocked(true);
        CancelSuccessAdvance();

        if (puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = reason;
        }

        PuzzleFailed?.Invoke(reason);

        LayeredDigitPuzzleConfig failedConfig = ActivePuzzleConfig;

        gameController.ApplyTimePenalty(
            failedConfig.FailureTimePenaltySeconds,
            $"{failedConfig.PuzzleId}: {reason}");

        if (gameController.IsTerminalState ||
            gameController.CurrentPhase !=
                CodebreakerPhase.CodeDiscovery)
        {
            CancelFailureReset();
            return;
        }

        failureResetAtUnscaledTime =
            Time.unscaledTime +
            failedConfig.FailureResetDelaySeconds;
    }

    private bool ContainsView(LayeredSegmentStackView segmentView)
    {
        for (int i = 0; i < segmentViews.Length; i++)
        {
            if (segmentViews[i] == segmentView)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateHitsText()
    {
        if (hitsLeftText != null)
        {
            hitsLeftText.text = $"HITS LEFT {HitsRemaining}";
        }
    }

    private void UpdateProgressText()
    {
        if (puzzleProgressText == null ||
            gameController == null ||
            gameController.LevelConfig == null)
        {
            return;
        }

        puzzleProgressText.text =
            $"DIGIT {ActivePuzzleIndex + 1} OF " +
            $"{gameController.LevelConfig.CodeDigitCount}";
    }

    private void SubscribeToGameEvents()
    {
        if (eventsSubscribed || gameController == null)
        {
            return;
        }

        gameController.LevelStarted += HandleLevelStarted;
        gameController.PhaseChanged += HandlePhaseChanged;
        gameController.DigitRegistered += HandleDigitRegistered;
        eventsSubscribed = true;
    }

    private void UnsubscribeFromGameEvents()
    {
        if (!eventsSubscribed || gameController == null)
        {
            return;
        }

        gameController.LevelStarted -= HandleLevelStarted;
        gameController.PhaseChanged -= HandlePhaseChanged;
        gameController.DigitRegistered -= HandleDigitRegistered;
        eventsSubscribed = false;
    }

    private void HandleLevelStarted()
    {
        CancelDelayedActions();
        SynchronizeToGameState();
    }

    private void HandlePhaseChanged(CodebreakerPhase phase)
    {
        if (phase == CodebreakerPhase.CodeDiscovery)
        {
            SynchronizeToGameState();
            return;
        }

        CancelDelayedActions();
        LockInteraction();
    }

    private void HandleDigitRegistered(int codeIndex, int digit)
    {
        if (!configurationValid || gameController == null)
        {
            return;
        }

        if (codeIndex == ActivePuzzleIndex && IsSolved)
        {
            return;
        }

        CancelDelayedActions();
        LockInteraction();

        if (!gameController.IsTerminalState &&
            gameController.CurrentPhase ==
                CodebreakerPhase.CodeDiscovery &&
            gameController.CurrentCodeIndex <
                gameController.LevelConfig.CodeDigitCount)
        {
            LoadPuzzleForCodeIndex(
                gameController.CurrentCodeIndex);
        }
    }

    private void ScheduleSuccessAdvance()
    {
        CancelFailureReset();
        successAdvanceAtUnscaledTime =
            Time.unscaledTime + successAdvanceDelaySeconds;
    }

    private void CancelFailureReset()
    {
        failureResetAtUnscaledTime = NoDelayedAction;
    }

    private void CancelSuccessAdvance()
    {
        successAdvanceAtUnscaledTime = NoDelayedAction;
    }

    private void CancelDelayedActions()
    {
        CancelFailureReset();
        CancelSuccessAdvance();
    }

    private void LockInteraction()
    {
        IsPlaying = false;
        SetInteractionBlocked(true);
    }

    private void SetInteractionBlocked(bool blocked)
    {
        if (interactionBlocker != null)
        {
            interactionBlocker.SetActive(blocked);
        }
    }

    private void ShowConfigurationError()
    {
        CancelDelayedActions();
        ActivePuzzleConfig = null;
        ActivePuzzleIndex = -1;
        HitsRemaining = 0;
        IsPlaying = false;
        IsResolving = false;
        IsSolved = false;
        SetInteractionBlocked(true);

        if (puzzleProgressText != null)
        {
            puzzleProgressText.text = string.Empty;
        }

        if (hitsLeftText != null)
        {
            hitsLeftText.text = "HITS LEFT --";
        }

        if (puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = ConfigurationErrorText;
        }
    }

    private void LogConfigurationErrors(
        IReadOnlyList<string> errors)
    {
        if (configurationErrorsLogged)
        {
            return;
        }

        for (int i = 0; i < errors.Count; i++)
        {
            Debug.LogError(errors[i], this);
        }

        configurationErrorsLogged = true;
    }

    private void LogConfigurationErrorOnce(string errorMessage)
    {
        if (configurationErrorsLogged)
        {
            return;
        }

        Debug.LogError(errorMessage, this);
        configurationErrorsLogged = true;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
