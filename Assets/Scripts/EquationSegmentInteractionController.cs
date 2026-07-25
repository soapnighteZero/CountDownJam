using UnityEngine;
using UnityEngine.InputSystem;

public class EquationSegmentInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private SharedSegmentInventory inventory;

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

        if (leftClicked && segment.IsActive)
        {
            if (segment.TryRemove())
            {
                inventory.Add(1);
            }
        }
        else if (rightClicked && !segment.IsActive)
        {
            if (!inventory.TrySpend(1))
            {
                return;
            }

            if (!segment.TryInstall())
            {
                inventory.Add(1);
            }
        }
    }
}
