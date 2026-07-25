using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LayeredDigitPuzzleController : MonoBehaviour
{
    private const string ConfigurationErrorText =
        "LAYERED PUZZLE CONFIGURATION ERROR";
    private const string InstructionText =
        "COLOR = CURRENT SIGNAL\n" +
        "DOTS = LAYERS BELOW\n" +
        "REMOVE EXACTLY 4 LAYERS";

    [SerializeField] private LayeredDigitPuzzleConfig puzzleConfig;
    [SerializeField] private CodebreakerGameController gameController;
    [SerializeField] private LayeredSegmentStackView[] segmentViews =
        Array.Empty<LayeredSegmentStackView>();
    [SerializeField] private TMP_Text hitsLeftText;
    [SerializeField] private TMP_Text puzzleInstructionText;
    [SerializeField] private TMP_Text puzzleFeedbackText;
    [SerializeField] private GameObject interactionBlocker;

    private bool configurationValid;
    private bool initialized;
    private bool eventsSubscribed;
    private float failureResetAtUnscaledTime = -1f;

    public LayeredDigitPuzzleConfig PuzzleConfig => puzzleConfig;
    public int HitsRemaining { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsResolving { get; private set; }
    public bool IsSolved { get; private set; }

    public event Action<int> HitsRemainingChanged;
    public event Action<int, LayeredSegmentColor> PuzzleSolved;
    public event Action<string> PuzzleFailed;

    private void Awake()
    {
        InitializePuzzle();
    }

    private void OnEnable()
    {
        SubscribeToGameEvents();
    }

    private void Start()
    {
        if (!initialized)
        {
            InitializePuzzle();
        }

        if (configurationValid)
        {
            ResetPuzzle();
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
            LockInteraction();
            return;
        }

        if (gameController.CurrentCodeIndex >
            puzzleConfig.TargetCodeIndex)
        {
            LockAsAlreadyDiscovered();
            return;
        }

        if (failureResetAtUnscaledTime >= 0f &&
            Time.unscaledTime >= failureResetAtUnscaledTime)
        {
            ResetPuzzle();
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

    public void ResetPuzzle()
    {
        if (!configurationValid)
        {
            ShowConfigurationError();
            return;
        }

        for (int i = 0; i < segmentViews.Length; i++)
        {
            segmentViews[i].ResetStack();
        }

        HitsRemaining = puzzleConfig.HitBudget;
        IsResolving = false;
        IsSolved = false;
        failureResetAtUnscaledTime = -1f;
        UpdateHitsText();

        if (puzzleInstructionText != null)
        {
            puzzleInstructionText.text = InstructionText;
        }

        if (puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = string.Empty;
        }

        bool targetIsCurrent =
            gameController != null &&
            !gameController.IsTerminalState &&
            gameController.CurrentPhase ==
                CodebreakerPhase.CodeDiscovery &&
            gameController.CurrentCodeIndex ==
                puzzleConfig.TargetCodeIndex;

        IsPlaying = targetIsCurrent;
        SetInteractionBlocked(!IsPlaying);

        if (gameController != null &&
            gameController.CurrentCodeIndex >
                puzzleConfig.TargetCodeIndex)
        {
            LockAsAlreadyDiscovered();
        }
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
            gameController == null ||
            gameController.IsTerminalState ||
            gameController.CurrentPhase !=
                CodebreakerPhase.CodeDiscovery ||
            gameController.CurrentCodeIndex !=
                puzzleConfig.TargetCodeIndex)
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
        if (!configurationValid || IsSolved || HitsRemaining != 0)
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

        if (digit != puzzleConfig.ExpectedDigit ||
            resolvedColor != puzzleConfig.ExpectedColor)
        {
            FailPuzzle("UNEXPECTED AUTHORED RESULT");
            return;
        }

        if (!gameController.TryRegisterDiscoveredDigit(digit))
        {
            FailPuzzle("UNEXPECTED AUTHORED RESULT");
            return;
        }

        IsResolving = false;
        IsSolved = true;
        failureResetAtUnscaledTime = -1f;

        if (puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = $"DIGIT CONFIRMED: {digit}";
        }

        PuzzleSolved?.Invoke(digit, resolvedColor);
    }

    public bool ValidateReferences()
    {
        bool isValid = true;

        if (puzzleConfig == null)
        {
            Debug.LogError(
                "LayeredDigitPuzzleController is missing puzzleConfig.",
                this);
            isValid = false;
        }
        else if (!puzzleConfig.ValidateConfiguration(
                     out string configError))
        {
            Debug.LogError(configError, puzzleConfig);
            isValid = false;
        }

        if (gameController == null)
        {
            Debug.LogError(
                "LayeredDigitPuzzleController is missing gameController.",
                this);
            isValid = false;
        }

        if (hitsLeftText == null)
        {
            Debug.LogError(
                "LayeredDigitPuzzleController is missing hitsLeftText.",
                this);
            isValid = false;
        }

        if (puzzleInstructionText == null)
        {
            Debug.LogError(
                "LayeredDigitPuzzleController is missing " +
                "puzzleInstructionText.",
                this);
            isValid = false;
        }

        if (puzzleFeedbackText == null)
        {
            Debug.LogError(
                "LayeredDigitPuzzleController is missing " +
                "puzzleFeedbackText.",
                this);
            isValid = false;
        }

        if (segmentViews == null || segmentViews.Length != 7)
        {
            Debug.LogError(
                "LayeredDigitPuzzleController requires exactly seven " +
                "segment views.",
                this);
            isValid = false;
        }
        else
        {
            bool[] positionsSeen = new bool[7];

            for (int i = 0; i < segmentViews.Length; i++)
            {
                LayeredSegmentStackView view = segmentViews[i];

                if (view == null)
                {
                    Debug.LogError(
                        $"LayeredDigitPuzzleController has a null segment " +
                        $"view at index {i}.",
                        this);
                    isValid = false;
                    continue;
                }

                isValid &= view.ValidateReferences();
                int positionIndex = (int)view.Position;

                if (positionIndex < 0 ||
                    positionIndex >= positionsSeen.Length)
                {
                    Debug.LogError(
                        $"LayeredDigitPuzzleController has invalid segment " +
                        $"position {view.Position}.",
                        this);
                    isValid = false;
                    continue;
                }

                if (positionsSeen[positionIndex])
                {
                    Debug.LogError(
                        "LayeredDigitPuzzleController has duplicate " +
                        $"segment position {view.Position}.",
                        this);
                    isValid = false;
                }

                positionsSeen[positionIndex] = true;

                if (puzzleConfig != null &&
                    !puzzleConfig.TryGetStack(
                        view.Position,
                        out _))
                {
                    Debug.LogError(
                        $"{puzzleConfig.name} has no stack for position " +
                        $"{view.Position}.",
                        puzzleConfig);
                    isValid = false;
                }
            }

            for (int i = 0; i < positionsSeen.Length; i++)
            {
                if (!positionsSeen[i])
                {
                    Debug.LogError(
                        "LayeredDigitPuzzleController has no view for " +
                        $"position {(LayeredSegmentPosition)i}.",
                        this);
                    isValid = false;
                }
            }
        }

        if (puzzleConfig != null &&
            gameController != null &&
            gameController.LevelConfig != null)
        {
            if (puzzleConfig.TargetCodeIndex >=
                gameController.LevelConfig.CodeDigitCount)
            {
                Debug.LogError(
                    $"{puzzleConfig.name} targets code index " +
                    $"{puzzleConfig.TargetCodeIndex}, beyond the configured " +
                    "code length.",
                    puzzleConfig);
                isValid = false;
            }
            else
            {
                int levelExpectedDigit =
                    gameController.LevelConfig.ExpectedCodeDigits[
                        puzzleConfig.TargetCodeIndex];

                if (levelExpectedDigit != puzzleConfig.ExpectedDigit)
                {
                    Debug.LogError(
                        $"{puzzleConfig.name} expects digit " +
                        $"{puzzleConfig.ExpectedDigit}, but the level " +
                        $"expects {levelExpectedDigit} at code index " +
                        $"{puzzleConfig.TargetCodeIndex}.",
                        puzzleConfig);
                    isValid = false;
                }
            }
        }

        return isValid;
    }

    private void InitializePuzzle()
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

        for (int i = 0; i < segmentViews.Length; i++)
        {
            LayeredSegmentStackView view = segmentViews[i];
            puzzleConfig.TryGetStack(
                view.Position,
                out LayeredSegmentStackDefinition stack);
            view.Initialize(stack, this);
        }

        HitsRemaining = puzzleConfig.HitBudget;
        UpdateHitsText();
    }

    private void FailPuzzle(string reason)
    {
        IsPlaying = false;
        IsResolving = true;
        SetInteractionBlocked(true);

        if (puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = reason;
        }

        PuzzleFailed?.Invoke(reason);
        gameController.ApplyTimePenalty(
            puzzleConfig.FailureTimePenaltySeconds,
            $"{puzzleConfig.PuzzleId}: {reason}");

        if (gameController.IsTerminalState)
        {
            failureResetAtUnscaledTime = -1f;
            return;
        }

        failureResetAtUnscaledTime =
            Time.unscaledTime + puzzleConfig.FailureResetDelaySeconds;
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
        ResetPuzzle();
    }

    private void HandlePhaseChanged(CodebreakerPhase phase)
    {
        if (phase != CodebreakerPhase.CodeDiscovery)
        {
            LockInteraction();
        }
    }

    private void HandleDigitRegistered(int codeIndex, int digit)
    {
        if (puzzleConfig != null &&
            codeIndex == puzzleConfig.TargetCodeIndex)
        {
            LockAsAlreadyDiscovered();
        }
    }

    private void LockAsAlreadyDiscovered()
    {
        LockInteraction();

        if (!IsSolved && puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = "DIGIT ALREADY RECOVERED";
        }
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
        IsPlaying = false;
        IsResolving = false;
        IsSolved = false;
        SetInteractionBlocked(true);

        if (hitsLeftText != null)
        {
            hitsLeftText.text = "HITS LEFT --";
        }

        if (puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = ConfigurationErrorText;
        }
    }
}
