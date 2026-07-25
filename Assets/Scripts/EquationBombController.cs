using UnityEngine;
using UnityEngine.InputSystem;

public class EquationBombController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SevenSegmentDisplay displayA;
    [SerializeField] private SevenSegmentDisplay displayB;
    [SerializeField] private SharedSegmentInventory sharedInventory;
    [SerializeField]
    private EquationSegmentInteractionController interactionController;
    [SerializeField] private CodeModuleController codeModule;

    [Header("Level Settings")]
    [SerializeField, Range(0, 9)] private int startingDigitA = 3;
    [SerializeField, Range(0, 9)] private int startingDigitB = 8;
    [SerializeField, Min(0)] private int startingSharedSegments;
    [SerializeField, Min(0.1f)] private float secondsPerPulse = 15f;
    [SerializeField, Min(0.1f)] private float masterFuseDuration = 90f;

    [Header("Runtime Debug")]
    [SerializeField] private float pulseTimer;
    [SerializeField] private float masterFuseRemaining;
    [SerializeField] private bool gameResolved;
    [SerializeField] private bool playerWon;

    private string statusMessage;

    public float PulseTimer => pulseTimer;
    public float MasterFuseRemaining => masterFuseRemaining;
    public float SecondsPerPulse => secondsPerPulse;
    public bool GameResolved => gameResolved;
    public bool PlayerWon => playerWon;
    public bool EquationSatisfied =>
        TryGetDisplayValues(out int valueA, out int valueB) &&
        valueA - valueB == 0;
    public bool CodeComplete =>
        codeModule != null && codeModule.IsComplete;
    public bool SystemReady => EquationSatisfied && CodeComplete;
    public string StatusMessage
    {
        get
        {
            if (gameResolved)
            {
                return statusMessage;
            }

            if (!TryGetDisplayValues(out int valueA, out int valueB))
            {
                return
                    "REPAIR INVALID COUNTDOWN DISPLAY BEFORE NEXT PULSE";
            }

            bool equationReady = valueA - valueB == 0;
            bool codeReady = CodeComplete;
            string targetCode = GetTargetCodeLabel();

            if (!equationReady && !codeReady)
            {
                return
                    $"CALIBRATE A - B = 0 AND BUILD CODE {targetCode}";
            }

            if (equationReady && !codeReady)
            {
                return
                    $"CALIBRATION READY - COMPLETE CODE {targetCode}";
            }

            if (!equationReady)
            {
                return "CODE ACCEPTED - CALIBRATE A - B = 0";
            }

            return "SYSTEM READY - PRESS SPACE TO DEFUSE";
        }
    }

    public bool TryGetDisplayValues(out int valueA, out int valueB)
    {
        valueA = -1;
        valueB = -1;

        bool validA =
            displayA != null &&
            displayA.TryGetCurrentDigit(out valueA);
        bool validB =
            displayB != null &&
            displayB.TryGetCurrentDigit(out valueB);

        return validA && validB;
    }

    private void Start()
    {
        startingDigitA = Mathf.Clamp(startingDigitA, 0, 9);
        startingDigitB = Mathf.Clamp(startingDigitB, 0, 9);
        startingSharedSegments = Mathf.Max(0, startingSharedSegments);

        if (!ValidateConfiguration())
        {
            ResolveConfigurationFailure();
            return;
        }

        displayA.SetDigit(startingDigitA);
        displayB.SetDigit(startingDigitB);
        sharedInventory.SetCount(startingSharedSegments);
        codeModule.ClearCode();
        pulseTimer = secondsPerPulse;
        masterFuseRemaining = masterFuseDuration;
        gameResolved = false;
        playerWon = false;
        statusMessage = string.Empty;
        interactionController.enabled = true;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null &&
            keyboard.spaceKey.wasPressedThisFrame)
        {
            AttemptDefuse();
        }

        if (gameResolved)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        masterFuseRemaining -= deltaTime;
        pulseTimer -= deltaTime;

        if (masterFuseRemaining <= 0f)
        {
            masterFuseRemaining = 0f;
            Explode("The master fuse reached zero.");
            return;
        }

        while (pulseTimer <= 0f && !gameResolved)
        {
            ExecuteCountdownPulse();

            if (!gameResolved)
            {
                pulseTimer += secondsPerPulse;
            }
        }
    }

    private bool ValidateConfiguration()
    {
        if (displayA == null ||
            displayB == null ||
            sharedInventory == null ||
            interactionController == null ||
            codeModule == null)
        {
            Debug.LogError(
                "Equation prototype is missing one or more required " +
                "controller references.",
                this
            );
            return false;
        }

        if (displayA == displayB)
        {
            Debug.LogError(
                "Display A and Display B must be different displays.",
                this
            );
            return false;
        }

        if (!codeModule.ValidateConfiguration())
        {
            Debug.LogError(
                "Code module configuration is invalid.",
                this
            );
            return false;
        }

        if (codeModule.ContainsDisplay(displayA) ||
            codeModule.ContainsDisplay(displayB))
        {
            Debug.LogError(
                "Countdown displays cannot also be code displays.",
                this
            );
            return false;
        }

        if (secondsPerPulse <= 0f || masterFuseDuration <= 0f)
        {
            Debug.LogError(
                "Pulse and master fuse durations must be greater than zero.",
                this
            );
            return false;
        }

        return true;
    }

    private void AttemptDefuse()
    {
        if (gameResolved)
        {
            return;
        }

        if (interactionController != null &&
            interactionController.IsDragging)
        {
            return;
        }

        if (SystemReady)
        {
            playerWon = true;
            gameResolved = true;
            statusMessage = "BOMB DEFUSED";
            DisableInteraction();
            Debug.Log(
                "Bomb defused with a valid equation and code.",
                this
            );
            return;
        }

        bool equationReady = EquationSatisfied;
        bool codeReady = CodeComplete;
        string reason = !equationReady && !codeReady
            ? "The equation and code were incomplete."
            : !equationReady
                ? "The equation was incomplete."
                : "The code was incomplete.";
        Explode($"Defuse attempted too early. {reason}");
    }

    private void ExecuteCountdownPulse()
    {
        if (!TryGetDisplayValues(out int currentA, out int currentB))
        {
            Explode("A display had an invalid segment shape at the pulse.");
            return;
        }

        if (currentA <= 0 || currentB <= 0)
        {
            Explode("A display reached zero before the bomb was defused.");
            return;
        }

        int nextA = currentA - 1;
        int nextB = currentB - 1;
        int currentSegmentCount =
            displayA.ActiveSegmentCount + displayB.ActiveSegmentCount;
        int requiredSegmentCount =
            displayA.GetRequiredSegmentCount(nextA) +
            displayB.GetRequiredSegmentCount(nextB);
        int segmentDifference =
            requiredSegmentCount - currentSegmentCount;

        if (segmentDifference > 0 &&
            !sharedInventory.TrySpend(segmentDifference))
        {
            Explode(
                "The shared inventory could not supply the next pulse."
            );
            return;
        }

        if (segmentDifference < 0)
        {
            sharedInventory.Add(-segmentDifference);
        }

        displayA.SetDigit(nextA);
        displayB.SetDigit(nextB);
    }

    private void ResolveConfigurationFailure()
    {
        gameResolved = true;
        playerWon = false;
        pulseTimer = 0f;
        masterFuseRemaining = 0f;
        statusMessage = "LEVEL CONFIGURATION ERROR";
        DisableInteraction();
        Debug.LogError("LEVEL CONFIGURATION ERROR", this);
    }

    private void Explode(string reason)
    {
        gameResolved = true;
        playerWon = false;
        statusMessage = "BOOM - GAME OVER";
        DisableInteraction();
        Debug.LogError($"BOOM! Game Over. {reason}", this);
    }

    private void DisableInteraction()
    {
        if (interactionController != null)
        {
            interactionController.enabled = false;
        }
    }

    private string GetTargetCodeLabel()
    {
        if (codeModule == null || codeModule.DigitCount == 0)
        {
            return "?";
        }

        string label = string.Empty;

        for (int i = 0; i < codeModule.DigitCount; i++)
        {
            int digit = codeModule.GetTargetDigit(i);
            label += digit >= 0 ? digit.ToString() : "?";
        }

        return label;
    }
}
