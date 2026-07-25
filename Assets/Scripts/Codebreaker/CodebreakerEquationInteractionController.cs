using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CodebreakerEquationInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private SharedSegmentInventory inventory;

    private SevenSegmentPiece dragOrigin;
    private bool dragFromInventory;
    private GameObject dragGhost;
    private DashedSegmentSlotVisual highlightedSlot;

    public bool IsDragging => dragGhost != null;
    public bool InteractionEnabled { get; private set; }
    public int InFlightSegmentCount => IsDragging ? 1 : 0;

    public event Action BoardChanged;
    public event Action DragStateChanged;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        ValidateReferences();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (!InteractionEnabled ||
            mouse == null ||
            worldCamera == null ||
            inventory == null)
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

    public void SetInteractionEnabled(bool enabled)
    {
        if (!enabled)
        {
            CancelCurrentDrag();
        }

        InteractionEnabled = enabled;
    }

    public void CancelCurrentDrag()
    {
        if (!IsDragging)
        {
            ClearDropHighlight();
            return;
        }

        bool restoredBoard = false;

        if (dragFromInventory)
        {
            inventory?.Add(1);
            restoredBoard = true;
        }
        else if (dragOrigin != null && !dragOrigin.IsActive)
        {
            restoredBoard = dragOrigin.TryInstall();
        }

        FinishDrag(restoredBoard);
    }

    public bool ValidateReferences()
    {
        bool isValid = true;

        if (worldCamera == null)
        {
            Debug.LogError(
                "CodebreakerEquationInteractionController is missing " +
                "worldCamera.",
                this);
            isValid = false;
        }

        if (inventory == null)
        {
            Debug.LogError(
                "CodebreakerEquationInteractionController is missing " +
                "inventory.",
                this);
            isValid = false;
        }

        return isValid;
    }

    private void BeginDrag(Vector2 worldPosition)
    {
        if (!InteractionEnabled || IsDragging)
        {
            return;
        }

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        for (int i = 0; i < hits.Length; i++)
        {
            InventorySegmentToken token =
                GetHitComponent<InventorySegmentToken>(hits[i]);

            if (token == null ||
                !token.isActiveAndEnabled ||
                inventory.StoredSegments <= 0 ||
                !inventory.TrySpend(1))
            {
                continue;
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
            DragStateChanged?.Invoke();
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
            DragStateChanged?.Invoke();
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
                FinishDrag(true);
                return;
            }

            CancelCurrentDrag();
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

            if (dragFromInventory)
            {
                CancelCurrentDrag();
                return;
            }

            inventory.Add(1);
            FinishDrag(true);
            return;
        }

        CancelCurrentDrag();
    }

    private GameObject CreateGhost(GameObject sourceVisual)
    {
        if (sourceVisual == null)
        {
            Debug.LogError("Drag source has no visual object.", this);
            return null;
        }

        GameObject ghost = Instantiate(sourceVisual);
        ghost.name = "CodebreakerSegmentDragGhost";
        ghost.SetActive(true);
        ghost.transform.SetParent(null, true);

        Collider2D[] colliders =
            ghost.GetComponentsInChildren<Collider2D>(true);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        MonoBehaviour[] gameplayScripts =
            ghost.GetComponentsInChildren<MonoBehaviour>(true);

        for (int i = 0; i < gameplayScripts.Length; i++)
        {
            gameplayScripts[i].enabled = false;
        }

        SpriteRenderer[] renderers =
            ghost.GetComponentsInChildren<SpriteRenderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingOrder += 100;
            Color color = renderers[i].color;
            color.a = Mathf.Min(color.a, 0.88f);
            renderers[i].color = color;
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

            if (target.VisualObject != null)
            {
                dragGhost.transform.rotation =
                    target.VisualObject.transform.rotation;
            }
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
        if (highlightedSlot == null)
        {
            return;
        }

        highlightedSlot.SetDropHighlight(false);
        highlightedSlot = null;
    }

    private void FinishDrag(bool boardChanged)
    {
        bool wasDragging = IsDragging;
        ClearDropHighlight();

        if (dragGhost != null)
        {
            DestroyDragObject(dragGhost);
        }

        dragGhost = null;
        dragOrigin = null;
        dragFromInventory = false;

        if (wasDragging)
        {
            DragStateChanged?.Invoke();
        }

        if (boardChanged)
        {
            BoardChanged?.Invoke();
        }
    }

    private void OnDisable()
    {
        CancelCurrentDrag();
        InteractionEnabled = false;
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        float cameraDistance =
            Mathf.Abs(worldCamera.transform.position.z);
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                cameraDistance));
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

    private static void DestroyDragObject(UnityEngine.Object target)
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
