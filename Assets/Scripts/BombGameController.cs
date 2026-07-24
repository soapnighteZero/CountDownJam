using UnityEngine;
using UnityEngine.InputSystem;

public class BombGameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SevenSegmentDisplay display;
    [SerializeField]
    private SevenSegmentInteractionController interactionController;

    [Header("Level Settings")]
    [SerializeField, Range(0, 9)]
    private int startingCountdownDigit = 9;

    [SerializeField]
    private int[] passwordDigits = { 0, 9, 7 };

    [Header("Runtime Debug")]
    [SerializeField] private int currentCountdownDigit;
    [SerializeField] private int currentPasswordIndex;
    [SerializeField] private int totalEnergy;

    private bool gameResolved;

    private void Awake()
    {
        if (display == null)
        {
            display = FindFirstObjectByType<SevenSegmentDisplay>();
        }

        if (interactionController == null)
        {
            interactionController =
                FindFirstObjectByType<SevenSegmentInteractionController>();
        }
    }

    private void Start()
    {
        if (!ValidateLevel())
        {
            gameResolved = true;
            DisableInteraction();
            return;
        }

        currentCountdownDigit = startingCountdownDigit;
        currentPasswordIndex = 0;
        totalEnergy = 0;
        gameResolved = false;

        display.SetDigit(currentCountdownDigit);

        ReportCurrentObjective();
    }

    private void Update()
    {
        if (gameResolved)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            SubmitCurrentShape();
        }
    }

    private bool ValidateLevel()
    {
        if (display == null)
        {
            Debug.LogError(
                "SevenSegmentDisplay was not found.",
                this
            );

            return false;
        }

        if (interactionController == null)
        {
            Debug.LogError(
                "SevenSegmentInteractionController was not found.",
                this
            );

            return false;
        }

        if (passwordDigits == null || passwordDigits.Length == 0)
        {
            Debug.LogError(
                "Password sequence is empty.",
                this
            );

            return false;
        }

        for (int i = 0; i < passwordDigits.Length; i++)
        {
            if (passwordDigits[i] < 0 || passwordDigits[i] > 9)
            {
                Debug.LogError(
                    $"Password digit at index {i} is invalid: " +
                    $"{passwordDigits[i]}.",
                    this
                );

                return false;
            }
        }

        return true;
    }

    private void SubmitCurrentShape()
    {
        bool isValidDigit =
            display.TryGetCurrentDigit(out int submittedDigit);

        int requiredDigit = passwordDigits[currentPasswordIndex];

        if (!isValidDigit || submittedDigit != requiredDigit)
        {
            HandleWrongSubmission(
                isValidDigit,
                submittedDigit,
                requiredDigit
            );

            return;
        }

        HandleCorrectSubmission(submittedDigit);
    }

    private void HandleCorrectSubmission(int submittedDigit)
    {
        int gainedEnergy = currentCountdownDigit;

        totalEnergy += gainedEnergy;
        currentPasswordIndex++;

        Debug.Log(
            $"Correct submission: {submittedDigit}. " +
            $"Energy gained: {gainedEnergy}. " +
            $"Total energy: {totalEnergy}.",
            this
        );

        if (currentPasswordIndex >= passwordDigits.Length)
        {
            HandlePasswordCompleted();
            return;
        }

        currentCountdownDigit--;

        if (currentCountdownDigit < 0)
        {
            HandleExplosion(
                "Countdown ended before the password was completed."
            );

            return;
        }

        display.SetDigit(currentCountdownDigit);

        ReportCurrentObjective();
    }

    private void HandleWrongSubmission(
        bool isValidDigit,
        int submittedDigit,
        int requiredDigit
    )
    {
        string submittedDescription = isValidDigit
            ? submittedDigit.ToString()
            : "invalid segment shape";

        HandleExplosion(
            $"Required: {requiredDigit}, " +
            $"submitted: {submittedDescription}."
        );
    }

    private void HandlePasswordCompleted()
    {
        gameResolved = true;
        DisableInteraction();

        Debug.Log(
            $"PASSWORD COMPLETE. Bomb defused. " +
            $"Final energy: {totalEnergy}.",
            this
        );
    }

    private void HandleExplosion(string reason)
    {
        gameResolved = true;
        DisableInteraction();

        Debug.LogError(
            $"BOOM! Game Over. {reason}",
            this
        );
    }

    private void ReportCurrentObjective()
    {
        int requiredDigit = passwordDigits[currentPasswordIndex];

        Debug.Log(
            $"Countdown digit: {currentCountdownDigit}. " +
            $"Required password digit: {requiredDigit}. " +
            $"Password progress: {currentPasswordIndex + 1}/" +
            $"{passwordDigits.Length}. " +
            $"Current energy: {totalEnergy}.",
            this
        );
    }

    private void DisableInteraction()
    {
        if (interactionController != null)
        {
            interactionController.enabled = false;
        }
    }
}