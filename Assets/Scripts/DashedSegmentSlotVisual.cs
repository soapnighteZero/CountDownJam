using UnityEngine;

public class DashedSegmentSlotVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SevenSegmentPiece segmentPiece;
    [SerializeField] private SpriteRenderer originalBackgroundRenderer;

    [Header("Appearance")]
    [SerializeField, Min(1)] private int dashCount = 5;
    [SerializeField]
    private Color emptySlotColor =
        new Color(0.72f, 0.82f, 0.88f, 0.22f);
    [SerializeField]
    private Color occupiedSlotColor =
        new Color(0.72f, 0.82f, 0.88f, 0.06f);
    [SerializeField]
    private Color validDropHighlightColor =
        new Color(0.45f, 1f, 0.62f, 0.45f);
    [SerializeField, Range(0.1f, 0.95f)]
    private float dashLengthRatio = 0.62f;
    [SerializeField, Range(0.1f, 1f)]
    private float dashThicknessRatio = 0.72f;

    private SpriteRenderer[] dashRenderers;
    private bool highlighted;

    private void Awake()
    {
        if (segmentPiece == null)
        {
            segmentPiece = GetComponent<SevenSegmentPiece>();
        }

        if (originalBackgroundRenderer == null)
        {
            Transform background = transform.Find("Background");

            if (background != null)
            {
                originalBackgroundRenderer =
                    background.GetComponent<SpriteRenderer>();
            }
        }

        if (segmentPiece == null || originalBackgroundRenderer == null)
        {
            Debug.LogError(
                "Dashed slot visual references are incomplete.",
                this
            );
            return;
        }

        BuildDashes();
        originalBackgroundRenderer.enabled = false;
    }

    private void BuildDashes()
    {
        int safeDashCount = Mathf.Max(1, dashCount);
        dashRenderers = new SpriteRenderer[safeDashCount];
        SpriteRenderer activeRenderer =
            segmentPiece.VisualObject == null
                ? null
                : segmentPiece.VisualObject.GetComponent<SpriteRenderer>();
        int dashSortingOrder = activeRenderer == null
            ? originalBackgroundRenderer.sortingOrder
            : Mathf.Min(
                originalBackgroundRenderer.sortingOrder,
                activeRenderer.sortingOrder - 1
            );

        GameObject container = new GameObject("DashedSlotGuide");
        container.transform.SetParent(transform, false);
        container.transform.localPosition =
            originalBackgroundRenderer.transform.localPosition;
        container.transform.localRotation =
            originalBackgroundRenderer.transform.localRotation;
        container.transform.localScale =
            originalBackgroundRenderer.transform.localScale;

        for (int i = 0; i < safeDashCount; i++)
        {
            GameObject dash = new GameObject($"Dash_{i + 1:00}");
            dash.transform.SetParent(container.transform, false);

            float normalizedPosition =
                (i + 0.5f) / safeDashCount - 0.5f;
            dash.transform.localPosition =
                new Vector3(normalizedPosition, 0f, 0f);
            dash.transform.localScale = new Vector3(
                dashLengthRatio / safeDashCount,
                dashThicknessRatio,
                1f
            );

            SpriteRenderer renderer =
                dash.AddComponent<SpriteRenderer>();
            renderer.sprite = originalBackgroundRenderer.sprite;
            renderer.sharedMaterial =
                originalBackgroundRenderer.sharedMaterial;
            renderer.sortingLayerID =
                originalBackgroundRenderer.sortingLayerID;
            renderer.sortingOrder = dashSortingOrder;
            dashRenderers[i] = renderer;
        }
    }

    private void LateUpdate()
    {
        if (dashRenderers == null || segmentPiece == null)
        {
            return;
        }

        Color color = highlighted && !segmentPiece.IsActive
            ? validDropHighlightColor
            : segmentPiece.IsActive
                ? occupiedSlotColor
                : emptySlotColor;

        for (int i = 0; i < dashRenderers.Length; i++)
        {
            dashRenderers[i].color = color;
        }
    }

    public void SetDropHighlight(bool isHighlighted)
    {
        highlighted = isHighlighted;
    }
}
