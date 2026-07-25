using System;
using UnityEngine;

public class SharedSegmentInventory : MonoBehaviour
{
    [SerializeField, Min(0)] private int storedSegments;

    public event Action<int> CountChanged;

    public int StoredSegments => storedSegments;

    public void SetCount(int amount)
    {
        int newCount = Mathf.Max(0, amount);

        if (newCount == storedSegments)
        {
            return;
        }

        storedSegments = newCount;
        CountChanged?.Invoke(storedSegments);
    }

    public void Add(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int newCount = amount > int.MaxValue - storedSegments
            ? int.MaxValue
            : storedSegments + amount;

        if (newCount == storedSegments)
        {
            return;
        }

        storedSegments = newCount;
        CountChanged?.Invoke(storedSegments);
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || amount > storedSegments)
        {
            return false;
        }

        if (amount == 0)
        {
            return true;
        }

        storedSegments -= amount;
        CountChanged?.Invoke(storedSegments);
        return true;
    }
}
