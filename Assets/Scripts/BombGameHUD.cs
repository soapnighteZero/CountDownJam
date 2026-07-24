using System.Text;
using TMPro;
using UnityEngine;

public class BombGameHUD : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private BombGameController gameController;
    [SerializeField]
    private SevenSegmentInteractionController interactionController;

    [Header("Text")]
    [SerializeField] private TMP_Text passwordText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text inventoryText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text resultText;

    private readonly StringBuilder passwordBuilder = new StringBuilder();

    private void Awake()
    {
        if (gameController == null)
        {
            gameController = FindFirstObjectByType<BombGameController>();
        }

        if (interactionController == null)
        {
            interactionController =
                FindFirstObjectByType<SevenSegmentInteractionController>();
        }

        if (gameController == null)
        {
            Debug.LogError("BombGameController was not found.", this);
        }

        if (interactionController == null)
        {
            Debug.LogError(
                "SevenSegmentInteractionController was not found.",
                this
            );
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (gameController == null || interactionController == null)
        {
            return;
        }

        UpdatePasswordText();

        energyText.text = $"ENERGY  {gameController.TotalEnergy}";
        inventoryText.text =
            $"SEGMENTS  {interactionController.StoredSegments}";
        instructionText.text = gameController.StatusMessage;

        if (gameController.GameResolved)
        {
            timerText.text = "COUNTDOWN STOPPED";
            resultText.gameObject.SetActive(true);
            resultText.text = gameController.PlayerWon
                ? $"BOMB DEFUSED\nENERGY  {gameController.TotalEnergy}"
                : "BOOM\nGAME OVER";
        }
        else
        {
            resultText.gameObject.SetActive(false);

            if (gameController.IsEditing)
            {
                timerText.text =
                    $"LOCKED AT  {gameController.LockedCountdownDigit}";
            }
            else
            {
                float remainingTime =
                    Mathf.Max(0f, gameController.CountdownTimer);
                timerText.text = $"NEXT PULSE  {remainingTime:F1}s";
            }
        }
    }

    private void UpdatePasswordText()
    {
        passwordBuilder.Clear();
        passwordBuilder.Append("PASSWORD  ");

        for (int i = 0; i < gameController.PasswordLength; i++)
        {
            if (i > 0)
            {
                passwordBuilder.Append("   ");
            }

            int digit = gameController.GetPasswordDigit(i);

            if (i < gameController.CurrentPasswordIndex)
            {
                passwordBuilder.Append("<color=#707070>");
                passwordBuilder.Append(digit);
                passwordBuilder.Append("</color>");
            }
            else if (i == gameController.CurrentPasswordIndex)
            {
                passwordBuilder.Append("<color=#FFD966>[");
                passwordBuilder.Append(digit);
                passwordBuilder.Append("]</color>");
            }
            else
            {
                passwordBuilder.Append(digit);
            }
        }

        passwordText.text = passwordBuilder.ToString();
    }
}
