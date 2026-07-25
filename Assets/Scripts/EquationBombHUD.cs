using TMPro;
using UnityEngine;

public class EquationBombHUD : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private EquationBombController gameController;
    [SerializeField] private SharedSegmentInventory inventory;

    [Header("Text")]
    [SerializeField] private TMP_Text equationText;
    [SerializeField] private TMP_Text fuseText;
    [SerializeField] private TMP_Text pulseText;
    [SerializeField] private TMP_Text inventoryText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text resultText;

    private void Awake()
    {
        if (gameController == null)
        {
            gameController =
                FindFirstObjectByType<EquationBombController>();
        }

        if (inventory == null)
        {
            inventory = FindFirstObjectByType<SharedSegmentInventory>();
        }

        ValidateReferences();

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (gameController == null || inventory == null)
        {
            return;
        }

        UpdateEquationText();

        if (fuseText != null)
        {
            fuseText.text =
                $"MASTER FUSE  " +
                $"{Mathf.Max(0f, gameController.MasterFuseRemaining):F1}s";
        }

        if (pulseText != null)
        {
            pulseText.text =
                $"NEXT PULSE  " +
                $"{Mathf.Max(0f, gameController.PulseTimer):F1}s";
        }

        if (inventoryText != null)
        {
            inventoryText.text =
                $"SPARE SEGMENTS  {inventory.StoredSegments}";
        }

        if (instructionText != null)
        {
            instructionText.text =
                $"{gameController.StatusMessage}\n" +
                "LEFT: REMOVE  |  RIGHT: INSTALL  |  SPACE: DEFUSE";
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(gameController.GameResolved);

            if (gameController.GameResolved)
            {
                resultText.text = gameController.PlayerWon
                    ? "BOMB DEFUSED"
                    : "BOOM\nGAME OVER";
            }
        }
    }

    private void UpdateEquationText()
    {
        if (equationText == null)
        {
            return;
        }

        bool valid =
            gameController.TryGetDisplayValues(
                out int valueA,
                out int valueB
            );

        string displayA = valueA >= 0 ? valueA.ToString() : "?";
        string displayB = valueB >= 0 ? valueB.ToString() : "?";
        string equation =
            $"A [{displayA}]  -  B [{displayB}]  =  0";

        if (!valid)
        {
            equationText.text = $"{equation}\nCURRENT: INVALID";
            return;
        }

        int currentResult = valueA - valueB;

        if (gameController.EquationSatisfied)
        {
            equationText.text =
                $"<color=#66FF88>{equation}\nCALIBRATED</color>";
        }
        else
        {
            equationText.text =
                $"{equation}\nCURRENT: {currentResult}";
        }
    }

    private void ValidateReferences()
    {
        if (gameController == null)
        {
            Debug.LogError(
                "EquationBombController was not found.",
                this
            );
        }

        if (inventory == null)
        {
            Debug.LogError(
                "SharedSegmentInventory was not found.",
                this
            );
        }

        if (equationText == null ||
            fuseText == null ||
            pulseText == null ||
            inventoryText == null ||
            instructionText == null ||
            resultText == null)
        {
            Debug.LogError(
                "EquationBombHUD is missing one or more TMP text references.",
                this
            );
        }
    }
}
