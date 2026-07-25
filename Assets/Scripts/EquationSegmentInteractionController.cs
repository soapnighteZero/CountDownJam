using UnityEngine;
using UnityEngine.InputSystem;

public class EquationSegmentInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private SharedSegmentInventory inventory;

    private SevenSegmentPiece dragOrigin;
    private bool dragFromInventory;
    private GameObject dragGhost;
    private DashedSegmentSlotVisual highlightedSlot;

    public bool IsDragging => dragGhost != null;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (worldCamera == null)
        {
            Debug.LogError(
                "No camera has been assigned and no Main Camera was found.",
                this
            );
        }

        if (inventory == null)
        {
            Debug.LogError(
                "SharedSegmentInventory has not been assigned.",
                this
            );
        }
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || worldCamera == null || inventory == null)
        {
            return;
        }

        Vector3 worldPosition =
            GetWorldPosition(mouse.position.ReadValue());

        if (!IsDragging)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                BeginDrag(worldPosition);
            }

            return;
        }

        dragGhost.transform.position = worldPosition;
        UpdateDropHighlight(worldPosition);

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            CompleteDrop(worldPosition);
        }
    }

    private void BeginDrag(Vector2 worldPosition)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        for (int i = 0; i < hits.Length; i++)
        {
            InventorySegmentToken token =
                GetHitComponent<InventorySegmentToken>(hits[i]);

            if (token == null || !token.isActiveAndEnabled)
            {
                continue;
            }

            if (!inventory.TrySpend(1))
            {
                return;
            }

            dragGhost = CreateGhost(token.VisualObject);

            if (dragGhost == null)
            {
                inventory.Add(1);
                return;
            }

            dragFromInventory = true;
            dragOrigin = null;
            dragGhost.transform.position = worldPosition;
            return;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            SevenSegmentPiece segment =
                GetHitComponent<SevenSegmentPiece>(hits[i]);

            if (segment == null || !segment.IsActive)
            {
                continue;
            }

            dragGhost = CreateGhost(segment.VisualObject);

            if (dragGhost == null)
            {
                return;
            }

            if (!segment.TryRemove())
            {
                DestroyDragObject(dragGhost);
                dragGhost = null;
                return;
            }

            dragFromInventory = false;
            dragOrigin = segment;
            dragGhost.transform.position = worldPosition;
            return;
        }
    }

    private void CompleteDrop(Vector2 worldPosition)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        for (int i = 0; i < hits.Length; i++)
        {
            SevenSegmentPiece target =
                GetHitComponent<SevenSegmentPiece>(hits[i]);

            if (target == null || target.IsActive)
            {
                continue;
            }

            if (target.TryInstall())
            {
                FinishDrag();
                return;
            }

            CancelDrag();
            return;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            InventoryDropZone dropZone =
                GetHitComponent<InventoryDropZone>(hits[i]);

            if (dropZone == null)
            {
                continue;
            }

            inventory.Add(1);
            FinishDrag();
            return;
        }

        CancelDrag();
    }

    private GameObject CreateGhost(GameObject sourceVisual)
    {
        if (sourceVisual == null)
        {
            Debug.LogError("Drag source has no visual object.", this);
            return null;
        }

        GameObject ghost = Instantiate(sourceVisual);
        ghost.name = "SegmentDragGhost";
        ghost.SetActive(true);
        ghost.transform.SetParent(null, true);

        foreach (Collider2D collider in
            ghost.GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = false;
            DestroyDragObject(collider);
        }

        foreach (SevenSegmentPiece piece in
            ghost.GetComponentsInChildren<SevenSegmentPiece>(true))
        {
            piece.enabled = false;
            DestroyDragObject(piece);
        }

        foreach (InventorySegmentToken token in
            ghost.GetComponentsInChildren<InventorySegmentToken>(true))
        {
            token.enabled = false;
            DestroyDragObject(token);
        }

        foreach (SpriteRenderer renderer in
            ghost.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.sortingOrder += 100;
            Color color = renderer.color;
            color.a = Mathf.Min(color.a, 0.9f);
            renderer.color = color;
        }

        return ghost;
    }

    private void UpdateDropHighlight(Vector2 worldPosition)
    {
        SevenSegmentPiece target = FindInactiveSegment(worldPosition);
        DashedSegmentSlotVisual nextHighlight =
            target == null
                ? null
                : target.GetComponent<DashedSegmentSlotVisual>();

        if (nextHighlight == highlightedSlot)
        {
            return;
        }

        ClearDropHighlight();
        highlightedSlot = nextHighlight;

        if (highlightedSlot != null)
        {
            highlightedSlot.SetDropHighlight(true);
            dragGhost.transform.rotation =
                target.VisualObject.transform.rotation;
        }
    }

    private SevenSegmentPiece FindInactiveSegment(Vector2 worldPosition)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        for (int i = 0; i < hits.Length; i++)
        {
            SevenSegmentPiece segment =
                GetHitComponent<SevenSegmentPiece>(hits[i]);

            if (segment != null && !segment.IsActive)
            {
                return segment;
            }
        }

        return null;
    }

    private void ClearDropHighlight()
    {
        if (highlightedSlot != null)
        {
            highlightedSlot.SetDropHighlight(false);
            highlightedSlot = null;
        }
    }

    private void CancelDrag()
    {
        if (!IsDragging)
        {
            return;
        }

        if (dragFromInventory)
        {
            inventory?.Add(1);
        }
        else if (dragOrigin != null && !dragOrigin.IsActive)
        {
            dragOrigin.TryInstall();
        }

        FinishDrag();
    }

    private void FinishDrag()
    {
        ClearDropHighlight();

        if (dragGhost != null)
        {
            DestroyDragObject(dragGhost);
        }

        dragGhost = null;
        dragOrigin = null;
        dragFromInventory = false;
    }

    private void OnDisable()
    {
        CancelDrag();
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        float cameraDistance =
            Mathf.Abs(worldCamera.transform.position.z);
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                cameraDistance
            )
        );
        worldPosition.z = 0f;
        return worldPosition;
    }

    private static T GetHitComponent<T>(Collider2D hit)
        where T : Component
    {
        T component = hit.GetComponent<T>();
        return component != null
            ? component
            : hit.GetComponentInParent<T>();
    }

    private static void DestroyDragObject(Object target)
    {
        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}
