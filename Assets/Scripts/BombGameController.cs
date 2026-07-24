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

    [SerializeField, Min(0.1f)]
    private float secondsPerDigit = 3f;

    [SerializeField]
    private int[] passwordDigits = { 0, 9, 7 };

    [Header("Runtime Debug")]
    [SerializeField] private int currentCountdownDigit;
    [SerializeField] private int lockedCountdownDigit = -1;
    [SerializeField] private int currentPasswordIndex;
    [SerializeField] private int totalEnergy;
    [SerializeField] private float countdownTimer;
    [SerializeField] private bool isEditing;
    [SerializeField] private bool gameResolved;
    [SerializeField] private bool playerWon;

    private string statusMessage;

    public int CurrentCountdownDigit => currentCountdownDigit;
    public int LockedCountdownDigit => lockedCountdownDigit;
    public int CurrentPasswordIndex => currentPasswordIndex;
    public int TotalEnergy => totalEnergy;
    public float CountdownTimer => countdownTimer;
    public float SecondsPerDigit => secondsPerDigit;
    public bool IsEditing => isEditing;
    public bool GameResolved => gameResolved;
    public bool PlayerWon => playerWon;
    public string StatusMessage => statusMessage;
    public int PasswordLength =>
        passwordDigits == null ? 0 : passwordDigits.Length;
    public int CurrentRequiredDigit =>
        currentPasswordIndex >= 0 &&
        currentPasswordIndex < PasswordLength
            ? passwordDigits[currentPasswordIndex]
            : -1;

    public int GetPasswordDigit(int index)
    {
        return index >= 0 && index < PasswordLength
            ? passwordDigits[index]
            : -1;
    }

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
        if (interactionController != null)
        {
            interactionController.enabled = false;
        }

        if (!ValidateLevel())
        {
            gameResolved = true;
            playerWon = false;
            statusMessage = "LEVEL CONFIGURATION ERROR";
            return;
        }

        currentCountdownDigit = startingCountdownDigit;
        lockedCountdownDigit = -1;
        currentPasswordIndex = 0;
        totalEnergy = 0;
        countdownTimer = secondsPerDigit;
        isEditing = false;
        gameResolved = false;
        playerWon = false;

        display.SetDigit(currentCountdownDigit);

        ReportCountdownState();
    }

    private void Update()
    {
        if (gameResolved)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (isEditing)
        {
            if (keyboard != null &&
                keyboard.spaceKey.wasPressedThisFrame)
            {
                SubmitCurrentShape();
            }

            return;
        }

        if (keyboard != null &&
            keyboard.spaceKey.wasPressedThisFrame)
        {
            BeginEditing();
            return;
        }

        UpdateCountdown();
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

        if (passwordDigits.Length > startingCountdownDigit + 1)
        {
            Debug.LogError(
                "The password contains more digits than the " +
                "countdown can support.",
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

    private void UpdateCountdown()
    {
        countdownTimer -= Time.deltaTime;

        while (countdownTimer <= 0f && !gameResolved)
        {
            if (currentCountdownDigit <= 0)
            {
                HandleExplosion(
                    "The countdown reached zero before " +
                    "the password was completed."
                );

                return;
            }

            currentCountdownDigit--;
            countdownTimer += secondsPerDigit;

            display.SetDigit(currentCountdownDigit);

            ReportCountdownState();
        }
    }

    private void BeginEditing()
    {
        isEditing = true;
        lockedCountdownDigit = currentCountdownDigit;
        statusMessage =
            $"EDIT TO {CurrentRequiredDigit}  |  SPACE: SUBMIT";

        interactionController.enabled = true;

        Debug.Log(
            $"Countdown locked at {lockedCountdownDigit}. " +
            $"Edit the display into password digit " +
            $"{passwordDigits[currentPasswordIndex]}, " +
            $"then press Space to submit.",
            this
        );
    }

    private void SubmitCurrentShape()
    {
        bool isValidDigit =
            display.TryGetCurrentDigit(out int submittedDigit);

        int requiredDigit =
            passwordDigits[currentPasswordIndex];

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
        int gainedEnergy = lockedCountdownDigit;

        totalEnergy += gainedEnergy;
        currentPasswordIndex++;

        interactionController.enabled = false;
        isEditing = false;

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

        currentCountdownDigit = lockedCountdownDigit - 1;
        lockedCountdownDigit = -1;

        if (currentCountdownDigit < 0)
        {
            HandleExplosion(
                "No countdown digits remain before " +
                "the password is complete."
            );

            return;
        }

        countdownTimer = secondsPerDigit;

        display.SetDigit(currentCountdownDigit);

        ReportCountdownState();
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
        playerWon = true;
        isEditing = false;
        lockedCountdownDigit = -1;
        statusMessage = "BOMB DEFUSED";

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
        playerWon = false;
        isEditing = false;
        statusMessage = "BOOM - GAME OVER";

        DisableInteraction();

        Debug.LogError(
            $"BOOM! Game Over. {reason}",
            this
        );
    }

    private void ReportCountdownState()
    {
        int requiredDigit =
            passwordDigits[currentPasswordIndex];

        statusMessage =
            $"SPACE: LOCK  |  NEXT CODE: {requiredDigit}";

        Debug.Log(
            $"Countdown digit: {currentCountdownDigit}. " +
            $"Required password digit: {requiredDigit}. " +
            $"Password progress: {currentPasswordIndex + 1}/" +
            $"{passwordDigits.Length}. " +
            $"Current energy: {totalEnergy}. " +
            $"Press Space to lock this countdown digit.",
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
