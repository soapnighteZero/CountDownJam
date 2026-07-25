using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LayeredSegmentStackView :
    MonoBehaviour,
    IPointerClickHandler
{
    private const float ClickCooldownSeconds = 0.08f;

    [SerializeField] private LayeredSegmentPosition position;
    [SerializeField] private Image segmentImage;
    [SerializeField] private TMP_Text depthText;
    [SerializeField] private TMP_Text positionLabelText;

    private LayeredSegmentStackDefinition definition;
    private LayeredDigitPuzzleController owner;
    private float nextAllowedClickTime;

    public LayeredSegmentPosition Position => position;
    public int CurrentHitDepth { get; private set; }
    public int MaximumHits => definition?.MaximumHits ?? 0;
    public int RemainingDepth =>
        Mathf.Max(0, MaximumHits - CurrentHitDepth);
    public LayeredSegmentColor CurrentColor { get; private set; }
    public bool CanAdvance =>
        definition != null && CurrentHitDepth < MaximumHits;

    public void Initialize(
        LayeredSegmentStackDefinition stackDefinition,
        LayeredDigitPuzzleController puzzleOwner)
    {
        definition = stackDefinition;
        owner = puzzleOwner;

        if (definition == null)
        {
            Debug.LogError(
                $"LayeredSegmentStackView {position} received a null " +
                "definition.",
                this);
            return;
        }

        if (definition.Position != position)
        {
            Debug.LogError(
                $"LayeredSegmentStackView {position} received the " +
                $"{definition.Position} definition.",
                this);
            definition = null;
            return;
        }

        ResetStack();
    }

    public bool TryAdvanceOneLayer()
    {
        if (!CanAdvance)
        {
            return false;
        }

        CurrentHitDepth++;
        RefreshVisuals();
        return true;
    }

    public void ResetStack()
    {
        CurrentHitDepth = 0;
        nextAllowedClickTime = 0f;
        RefreshVisuals();
    }

    public bool ValidateReferences()
    {
        bool isValid = true;

        if (segmentImage == null)
        {
            Debug.LogError(
                $"LayeredSegmentStackView {position} is missing " +
                "segmentImage.",
                this);
            isValid = false;
        }

        if (depthText == null)
        {
            Debug.LogError(
                $"LayeredSegmentStackView {position} is missing depthText.",
                this);
            isValid = false;
        }

        if (positionLabelText == null)
        {
            Debug.LogError(
                $"LayeredSegmentStackView {position} is missing " +
                "positionLabelText.",
                this);
            isValid = false;
        }

        return isValid;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null ||
            eventData.button != PointerEventData.InputButton.Left ||
            owner == null ||
            Time.unscaledTime < nextAllowedClickTime)
        {
            return;
        }

        if (owner.TryHitSegment(this))
        {
            nextAllowedClickTime =
                Time.unscaledTime + ClickCooldownSeconds;
        }
    }

    private void RefreshVisuals()
    {
        if (definition == null)
        {
            CurrentColor = LayeredSegmentColor.Gray;
        }
        else
        {
            CurrentColor =
                definition.GetStateAfterHits(CurrentHitDepth);
        }

        if (segmentImage != null)
        {
            segmentImage.color = GetDisplayColor(CurrentColor);
            segmentImage.raycastTarget = true;
        }

        if (depthText != null)
        {
            depthText.text =
                RemainingDepth > 0 ? RemainingDepth.ToString() : string.Empty;
        }

        if (positionLabelText != null)
        {
            positionLabelText.text = position.ToString();
        }
    }

    private static Color GetDisplayColor(LayeredSegmentColor color)
    {
        switch (color)
        {
            case LayeredSegmentColor.Green:
                return new Color32(53, 232, 126, 255);
            case LayeredSegmentColor.Yellow:
                return new Color32(255, 222, 62, 255);
            case LayeredSegmentColor.Red:
                return new Color32(255, 76, 82, 255);
            default:
                return new Color32(58, 65, 74, 255);
        }
    }
}
