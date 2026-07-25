using System.Text;
using TMPro;
using UnityEngine;

public class EquationBombHUD : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private EquationBombController gameController;
    [SerializeField] private SharedSegmentInventory inventory;
    [SerializeField] private CodeModuleController codeModule;

    [Header("Text")]
    [SerializeField] private TMP_Text equationText;
    [SerializeField] private TMP_Text fuseText;
    [SerializeField] private TMP_Text pulseText;
    [SerializeField] private TMP_Text inventoryText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text codeText;

    private readonly StringBuilder codeBuilder = new StringBuilder();

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

        if (codeModule == null)
        {
            codeModule = FindFirstObjectByType<CodeModuleController>();
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
        UpdateCodeText();

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
                $"SPARE  {inventory.StoredSegments}";
        }

        if (instructionText != null)
        {
            instructionText.text =
                $"{gameController.StatusMessage}\n" +
                "DRAG SEGMENTS  |  SPACE: DEFUSE";
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

    private void UpdateCodeText()
    {
        if (codeText == null || codeModule == null)
        {
            return;
        }

        codeBuilder.Clear();
        bool complete = codeModule.IsComplete;

        if (complete)
        {
            codeBuilder.Append("<color=#66FF88>");
        }

        codeBuilder.Append("CODE  ");

        for (int i = 0; i < codeModule.DigitCount; i++)
        {
            bool valid =
                codeModule.TryGetCurrentDigit(i, out int digit);
            bool correct =
                valid && digit == codeModule.GetTargetDigit(i);
            string color = correct
                ? "#66FF88"
                : valid
                    ? "#FFD166"
                    : "#8A929C";
            string value = valid ? digit.ToString() : "?";

            codeBuilder.Append($"<color={color}>[{value}]</color>");

            if (i < codeModule.DigitCount - 1)
            {
                codeBuilder.Append(' ');
            }
        }

        if (complete)
        {
            codeBuilder.Append("</color>");
        }

        codeBuilder.Append("\nTARGET  ");

        for (int i = 0; i < codeModule.DigitCount; i++)
        {
            codeBuilder.Append('[');
            int target = codeModule.GetTargetDigit(i);
            codeBuilder.Append(target >= 0 ? target.ToString() : "?");
            codeBuilder.Append(']');

            if (i < codeModule.DigitCount - 1)
            {
                codeBuilder.Append(' ');
            }
        }

        if (complete)
        {
            codeBuilder.Append("\n<color=#66FF88>CODE ACCEPTED</color>");
        }

        codeText.text = codeBuilder.ToString();
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

        if (codeModule == null)
        {
            Debug.LogError(
                "CodeModuleController was not found.",
                this
            );
        }

        if (equationText == null ||
            fuseText == null ||
            pulseText == null ||
            inventoryText == null ||
            instructionText == null ||
            resultText == null ||
            codeText == null)
        {
            Debug.LogError(
                "EquationBombHUD is missing one or more TMP text references.",
                this
            );
        }
    }
}
