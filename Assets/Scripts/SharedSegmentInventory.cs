using UnityEngine;

public class SharedSegmentInventory : MonoBehaviour
{
    [SerializeField, Min(0)] private int storedSegments;

    public int StoredSegments => storedSegments;

    public void SetCount(int amount)
    {
        storedSegments = Mathf.Max(0, amount);
    }

    public void Add(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        storedSegments = amount > int.MaxValue - storedSegments
            ? int.MaxValue
            : storedSegments + amount;
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || amount > storedSegments)
        {
            return false;
        }

        storedSegments -= amount;
        return true;
    }
}
