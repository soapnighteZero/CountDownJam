using UnityEngine;

public class SevenSegmentPiece : MonoBehaviour
{
    [SerializeField]
    private GameObject visual;

    [SerializeField]
    private bool isActive = true;

    public bool IsActive => isActive;

    private void Awake()
    {
        ApplyVisualState();
    }

    public void SetActiveState(bool active)
    {
        isActive = active;
        ApplyVisualState();
    }

    public void Toggle()
    {
        SetActiveState(!isActive);
    }

    private void ApplyVisualState()
    {
        if (visual == null)
        {
            Debug.LogError(
                $"Visual has not been assigned on {gameObject.name}.",
                this
            );

            return;
        }

        visual.SetActive(isActive);
    }
}