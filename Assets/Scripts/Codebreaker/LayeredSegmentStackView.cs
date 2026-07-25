using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LayeredSegmentStackView :
    MonoBehaviour,
    IPointerClickHandler
{
    private const float ClickCooldownSeconds = 0.08f;
    private static readonly Color DepthIndicatorColor =
        new Color32(172, 190, 208, 255);

    [SerializeField] private LayeredSegmentPosition position;
    [SerializeField] private Image segmentImage;
    [SerializeField] private Image[] depthIndicators =
        Array.Empty<Image>();
    [SerializeField] private TMP_Text positionLabelText;
    [SerializeField] private bool showDebugLabels = true;

    private LayeredSegmentStackDefinition definition;
    private LayeredDigitPuzzleController owner;
    private float nextAllowedClickTime;
    private bool runtimeConfigurationValid;
    private bool runtimeConfigurationErrorLogged;

    public LayeredSegmentPosition Position => position;
    public int CurrentHitDepth { get; private set; }
    public int MaximumHits => definition?.MaximumHits ?? 0;
    public int RemainingDepth =>
        Mathf.Max(0, MaximumHits - CurrentHitDepth);
    public LayeredSegmentColor CurrentColor { get; private set; }
    public bool CanAdvance =>
        runtimeConfigurationValid &&
        definition != null &&
        CurrentHitDepth < MaximumHits;

    public void Initialize(
        LayeredSegmentStackDefinition stackDefinition,
        LayeredDigitPuzzleController puzzleOwner)
    {
        definition = stackDefinition;
        owner = puzzleOwner;
        runtimeConfigurationValid = true;
        runtimeConfigurationErrorLogged = false;
        enabled = true;

        if (definition == null)
        {
            DisableForConfigurationError(
                $"LayeredSegmentStackView {position} received a null " +
                "definition.");
            return;
        }

        if (definition.Position != position)
        {
            DisableForConfigurationError(
                $"LayeredSegmentStackView {position} received the " +
                $"{definition.Position} definition.");
            return;
        }

        if (!definition.ValidateConfiguration(out string errorMessage))
        {
            DisableForConfigurationError(errorMessage);
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

        if (depthIndicators == null || depthIndicators.Length != 3)
        {
            Debug.LogError(
                $"LayeredSegmentStackView {position} requires exactly " +
                "three depth indicators.",
                this);
            isValid = false;
        }
        else
        {
            for (int i = 0; i < depthIndicators.Length; i++)
            {
                if (depthIndicators[i] == null)
                {
                    Debug.LogError(
                        $"LayeredSegmentStackView {position} is missing " +
                        $"depth indicator {i}.",
                        this);
                    isValid = false;
                    continue;
                }

                depthIndicators[i].raycastTarget = false;
                depthIndicators[i].color = DepthIndicatorColor;
            }
        }

        if (positionLabelText != null)
        {
            positionLabelText.raycastTarget = false;
        }

        if (segmentImage != null)
        {
            segmentImage.raycastTarget = true;
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
            !runtimeConfigurationValid ||
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
        if (!runtimeConfigurationValid)
        {
            ApplyDisabledVisualState();
            return;
        }

        if (definition == null)
        {
            DisableForConfigurationError(
                $"LayeredSegmentStackView {position} has no active " +
                "stack definition.");
            return;
        }

        CurrentColor =
            definition.GetStateAfterHits(CurrentHitDepth);

        int canonicalRemainingDepth;

        try
        {
            canonicalRemainingDepth =
                LayeredSegmentColorProgression.GetRemainingDepth(
                    CurrentColor);
        }
        catch (ArgumentOutOfRangeException)
        {
            DisableForConfigurationError(
                $"Stack {position} reached unsupported color " +
                $"{CurrentColor} at hit depth {CurrentHitDepth}.");
            return;
        }

        if (RemainingDepth != canonicalRemainingDepth)
        {
            DisableForConfigurationError(
                $"Stack {position} has remaining depth {RemainingDepth} " +
                $"at {CurrentColor}, but the canonical progression " +
                $"requires {canonicalRemainingDepth}.");
            return;
        }

        if (segmentImage != null)
        {
            segmentImage.color = GetDisplayColor(CurrentColor);
            segmentImage.raycastTarget = true;
        }

        if (depthIndicators != null)
        {
            for (int i = 0; i < depthIndicators.Length; i++)
            {
                Image indicator = depthIndicators[i];

                if (indicator == null)
                {
                    continue;
                }

                indicator.color = DepthIndicatorColor;
                indicator.raycastTarget = false;
                indicator.gameObject.SetActive(RemainingDepth >= i + 1);
            }
        }

        if (positionLabelText != null)
        {
            positionLabelText.text = position.ToString();
            positionLabelText.raycastTarget = false;
            positionLabelText.gameObject.SetActive(showDebugLabels);
        }
    }

    private void DisableForConfigurationError(string errorMessage)
    {
        runtimeConfigurationValid = false;
        enabled = false;

        if (!runtimeConfigurationErrorLogged)
        {
            Debug.LogError(errorMessage, this);
            runtimeConfigurationErrorLogged = true;
        }

        ApplyDisabledVisualState();
    }

    private void ApplyDisabledVisualState()
    {
        CurrentColor = LayeredSegmentColor.Gray;

        if (segmentImage != null)
        {
            segmentImage.color = GetDisplayColor(CurrentColor);
            segmentImage.raycastTarget = false;
        }

        if (depthIndicators != null)
        {
            for (int i = 0; i < depthIndicators.Length; i++)
            {
                Image indicator = depthIndicators[i];

                if (indicator == null)
                {
                    continue;
                }

                indicator.color = DepthIndicatorColor;
                indicator.raycastTarget = false;
                indicator.gameObject.SetActive(false);
            }
        }

        if (positionLabelText != null)
        {
            positionLabelText.raycastTarget = false;
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
