using UnityEngine;
using UnityEngine.InputSystem;

public class SevenSegmentInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;

    [Header("Segment Inventory")]
    [SerializeField, Min(0)] private int storedSegments;

    public int StoredSegments => storedSegments;

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
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || worldCamera == null)
        {
            return;
        }

        bool leftClicked = mouse.leftButton.wasPressedThisFrame;
        bool rightClicked = mouse.rightButton.wasPressedThisFrame;

        if (!leftClicked && !rightClicked)
        {
            return;
        }

        Vector2 screenPosition = mouse.position.ReadValue();
        Vector2 worldPosition =
            worldCamera.ScreenToWorldPoint(screenPosition);

        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null)
        {
            return;
        }

        SevenSegmentPiece segment =
            hit.GetComponent<SevenSegmentPiece>();

        if (segment == null)
        {
            segment = hit.GetComponentInParent<SevenSegmentPiece>();
        }

        if (segment == null)
        {
            return;
        }

        if (leftClicked)
        {
            RemoveSegment(segment);
        }
        else if (rightClicked)
        {
            InstallSegment(segment);
        }
    }

    private void RemoveSegment(SevenSegmentPiece segment)
    {
        if (!segment.TryRemove())
        {
            return;
        }

        storedSegments++;

        Debug.Log(
            $"Removed {segment.gameObject.name}. " +
            $"Stored segments: {storedSegments}",
            segment
        );
    }

    private void InstallSegment(SevenSegmentPiece segment)
    {
        if (segment.IsActive)
        {
            return;
        }

        if (storedSegments <= 0)
        {
            Debug.Log(
                "Cannot install segment: inventory is empty.",
                this
            );

            return;
        }

        if (!segment.TryInstall())
        {
            return;
        }

        storedSegments--;

        Debug.Log(
            $"Installed {segment.gameObject.name}. " +
            $"Stored segments: {storedSegments}",
            segment
        );
    }
}