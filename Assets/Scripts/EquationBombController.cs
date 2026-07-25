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

    [Header("Level Settings")]
    [SerializeField, Range(0, 9)] private int startingDigitA = 8;
    [SerializeField, Range(0, 9)] private int startingDigitB = 5;
    [SerializeField, Min(0)] private int startingSharedSegments;
    [SerializeField, Min(0.1f)] private float secondsPerPulse = 5f;
    [SerializeField, Min(0.1f)] private float masterFuseDuration = 30f;

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
                return "REPAIR INVALID DISPLAY BEFORE NEXT PULSE";
            }

            return valueA - valueB == 0
                ? "CALIBRATED - PRESS SPACE TO DEFUSE"
                : "MAKE A - B = 0";
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
            interactionController == null)
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

        if (EquationSatisfied)
        {
            playerWon = true;
            gameResolved = true;
            statusMessage = "BOMB DEFUSED";
            DisableInteraction();
            Debug.Log("Bomb defused with A - B = 0.", this);
            return;
        }

        Explode("Defuse attempted before A - B equalled zero.");
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
}
