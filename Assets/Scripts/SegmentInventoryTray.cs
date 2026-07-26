using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SegmentInventoryTray : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SharedSegmentInventory inventory;
    [SerializeField] private InventorySegmentToken tokenTemplate;
    [SerializeField] private Transform tokenContainer;
    [SerializeField] private TMP_Text countText;

    [Header("Layout")]
    [SerializeField, Min(1)] private int maximumVisibleTokens = 14;
    [SerializeField, Min(1)] private int columns = 7;
    [SerializeField] private Vector2 tokenSpacing =
        new Vector2(0.85f, 0.62f);
    [SerializeField] private Vector2 firstTokenLocalPosition =
        new Vector2(-2.55f, 0.35f);

    private readonly List<InventorySegmentToken> tokens =
        new List<InventorySegmentToken>();

    private void Awake()
    {
        if (inventory == null)
        {
            Debug.LogError(
                "SegmentInventoryTray has no inventory assigned.",
                this
            );
            return;
        }

        inventory.CountChanged += Refresh;
        inventory.CapacityChanged += Refresh;
        EnsureTokenPool();
        Refresh(inventory.StoredSegments);
    }

    private void EnsureTokenPool()
    {
        if (tokenTemplate == null || tokenContainer == null)
        {
            Debug.LogError(
                "SegmentInventoryTray token references are incomplete.",
                this
            );
            return;
        }

        tokenTemplate.gameObject.SetActive(false);
        int safeColumns = Mathf.Max(1, columns);

        for (int i = tokens.Count; i < maximumVisibleTokens; i++)
        {
            InventorySegmentToken token =
                Instantiate(tokenTemplate, tokenContainer);
            token.name = $"InventoryToken_{i + 1:00}";

            int column = i % safeColumns;
            int row = i / safeColumns;
            token.transform.localPosition = new Vector3(
                firstTokenLocalPosition.x + column * tokenSpacing.x,
                firstTokenLocalPosition.y - row * tokenSpacing.y,
                0f
            );
            token.transform.localRotation = Quaternion.identity;
            token.gameObject.SetActive(false);
            tokens.Add(token);
        }
    }

    private void Refresh(int ignoredValue)
    {
        int storedCount = inventory.StoredSegments;
        int visibleCount =
            Mathf.Min(Mathf.Max(0, storedCount), tokens.Count);

        for (int i = 0; i < tokens.Count; i++)
        {
            tokens[i].gameObject.SetActive(i < visibleCount);
        }

        if (countText != null)
        {
            countText.text =
                $"BUFFER {storedCount} / {inventory.Capacity}";
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.CountChanged -= Refresh;
            inventory.CapacityChanged -= Refresh;
        }
    }
}
